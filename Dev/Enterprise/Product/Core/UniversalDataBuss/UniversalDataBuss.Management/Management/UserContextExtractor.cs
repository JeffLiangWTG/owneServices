using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Management;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management
{
	class UserContextExtractor : IUserContextExtractor
	{
		readonly ZBool requiresCodesMappedToTarget;
		readonly IRecipientBranchLocator branchLocator;

		public UserContextExtractor(IRecipientBranchLocator branchLocator, ZBool requiresCodesMappedToTarget)
		{
			this.requiresCodesMappedToTarget = requiresCodesMappedToTarget;
			this.branchLocator = branchLocator;
		}

		public bool TryGetUserContext(IEDIMessage message, ITopLevelDataObject topLevelDataObject, IXmlSessionTracker logger, out IUserContext userContext)
		{
			userContext = null;

			topLevelDataObject.SetCodesMappedToTarget();

			var dataContext = topLevelDataObject.DataContext;

			IGlbCompany companyFromMessage = null;

			if (dataContext != null && (!requiresCodesMappedToTarget || dataContext.CodesMappedToTarget) && !dataContext.CompanyCodeToImportInto.IsEmpty)
			{
				companyFromMessage = message.Factory.LoadFromNaturalKey<IGlbCompany>(GlbCompanySchema.GC_Code, dataContext.CompanyCodeToImportInto);
			}

			if (dataContext != null && companyFromMessage == null && branchLocator.UseEventBranchToDecideImportCompany && !dataContext.EventBranchCode.IsEmpty)
			{
				var branch = message.Factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, dataContext.EventBranchCode);
				companyFromMessage = branch?.Company;
			}

			if (companyFromMessage == null)
			{
				var messageBranch = message.Factory.Load<IGlbBranch>(message.EM_GB);
				companyFromMessage = messageBranch?.Company;
			}

			if (companyFromMessage == null)
			{
				logger.LogBoth(LogType.Error, UniversalXmlUserContextLogging.NoCompanyFound());
				return false;
			}

			if (!companyFromMessage.GC_IsActive)
			{
				logger.LogBoth(LogType.Error, UniversalXmlUserContextLogging.CompanyIsInactive(companyFromMessage.GC_Code));
				return false;
			}

			var user = GetUserForEnvironment(message);

			var foundDepartment = TryGetActiveDepartment(new DepartmentLocator(message.Factory, requiresCodesMappedToTarget, logger), message, dataContext);
			if (!foundDepartment.active)
			{
				foundDepartment.ReportActiveState(logger, UniversalXmlUserContextLogging.InvalidDepartment(foundDepartment.department), out var failMessage);
				if (failMessage)
				{
					return false;
				}
			}
			var departmentPK = foundDepartment.department.PK.ToGuid();

			/*
			 * We are setting the DataProviderDetails after getting the user,
			 * otherwise the CodesMappedToTarget flag changes the sender id. (And therefore the user we import under)
			 * This is/was an unintented implementation detail that caused unintented/emergent behaviour.
			 */
			if (dataContext != null && dataContext.CompanyCodeToImportInto.IsEmpty && dataContext.CodesMappedToTarget)
			{
				dataContext.SetCompanyAndDataProviderDetails(companyFromMessage);
			}

			var branchPK = branchLocator.GetBranchPK(message, topLevelDataObject, companyFromMessage, logger);
			if (!branchPK.IsValid)
			{
				//no need to log this, logging done in IRecipientBranchLocator
				return false;
			}

			userContext = new UserContext(user, branchPK.ToGuid(), departmentPK);

			return true;
		}

		(bool active, IGlbDepartment department) TryGetActiveDepartment(IDepartmentLocator departmentLocator, IEDIMessage message, IDataContextDataObject contextDataObject)
		{
			return departmentLocator.TryGetActiveDepartment(message, contextDataObject);
		}

		public static IUser GetUserForEnvironment(IEDIMessage message)
		{
			if (!Env.CurrentUser.IsSystemAccount)
			{
				// If the message is being processed in the context of an operator user, we don't want to change
				// context. See CS00630860 for an example of EDIMessages being used for module -> module communication.
				return Env.CurrentUser;
			}

			if (message.EM_ECC_CommunicationPartyConfig.IsValid)
			{
				var partyConfig = message.Factory.Load<IEDICommunicationPartyConfig>(message.EM_ECC_CommunicationPartyConfig);
				var party = partyConfig.Party;
				if (party != null && party.ECP_GS_SecurityProxy.IsValid)
				{
					var staff = message.Factory.Load<IGlbStaff>(party.ECP_GS_SecurityProxy);
					if (staff != null)
					{
						return staff;
					}
				}
			}

			var senderCode = message?.Interchange?.EI_From ?? ZString.Empty;

			return eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value.GetStaffFromRecipient(senderCode, message.Factory);
		}
	}

	public static class UniversalXmlUserContextLogging
	{
		public static string NoCompanyFound() => Res.GetString("0b702ecc-7756-4448-af45-b1c0850bb2a7", "No company is found on message.");

		public static string CompanyIsInactive(string companyCode) => Res.GetString("d18fed03-315d-4a7e-ae65-3a11bd76e638", "Message Rejected as Company '{0}' is inactive.", companyCode);

		public static string TargetingOnlyActiveBranch(string companyCode, string branchCode) => Res.GetString("ffbd38af-5e2d-404f-b5e9-dbb32142a8f2", "Targeting Branch '{0}', Company '{1}' – the only one active branch of recipient company.", branchCode, companyCode);

		public static string BranchIsInactive(string branchCode) => Res.GetString("06DBBCC3-F0C8-42D7-A1B6-938D7BE741EA", "Message Rejected as Branch '{0}' is inactive.", branchCode);

		public static string EventBranchIsNotActiveOnDataContextCompany(string companyCode, string branchCode) => Res.GetString("9CB89E5F-CDA9-4C94-BAEB-1EC6D28234E4", "Event Branch '{0}' is not an active Branch from Company '{1}'.", branchCode, companyCode);

		public static string CompanyHasNoActiveBranches(string companyCode) => Res.GetString("3efaaa6b-dd6f-4dc9-b06c-7d6f40d7f0f2", "Message Rejected as Company '{0}' has no active branches.", companyCode);

		public static string TargetingFirstActiveBranch(string companyCode, string branchCode) => Res.GetString("04a4933c-3504-4f0b-b854-1e0cb5ad86fa", "Targeting Branch '{0}', Company '{1}' – defaulted to first active branch of recipient company.", branchCode, companyCode);
		public static string InvalidDepartment(IGlbDepartment department)
		{
			return department != null
				? Res.GetString("EA9FE52A-F977-48D8-AB7D-C1D341FCE0CC",
					"Department '{0}' used for message is inactive", department.GE_Code)
				: Res.GetString("36E26FB0-5F25-49F0-9AEE-63ECBA7B747C", "Message Rejected due to no department available for message");
		}
	}
}
