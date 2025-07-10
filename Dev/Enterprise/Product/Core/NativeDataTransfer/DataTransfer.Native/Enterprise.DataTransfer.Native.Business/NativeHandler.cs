using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Retrieve;
using Enterprise.DataTransfer.Native.Business.Update;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business
{
	public enum ServiceAction
	{
		Update,
		Retrieve
	}

	public static class NativeHandler
	{
		public static IHandler GetHandler(ServiceAction action)
		{
			switch (action)
			{
				case ServiceAction.Update:
					return new UpdateHandler(new FactoryProvider());
				case ServiceAction.Retrieve:
					return new RetrieveHandler();
				default:
					throw new NotSupportedException();
			}
		}

		public static IDisposable SetUserContext(BusinessObjectFactory factory, DataContextWrapper dataContext, ILogger logger, IEDIMessage message = null)
			=> SetUserContext(factory, dataContext, logger.Log, message);

		public static IDisposable SetUserContext(BusinessObjectFactory factory, DataContextWrapper dataContext, IXmlImportLogger logger, IEDIMessage message = null)
			=> SetUserContext(factory, dataContext, logger.Log, message);

		static IDisposable SetUserContext(BusinessObjectFactory factory, DataContextWrapper dataContext, Action<LogType, string> logger, IEDIMessage message = null)
		{
			var branchPK = Env.Instance.CurrentBranchPK;

			var companyCode = dataContext?.Company?.Code;
			if (!string.IsNullOrEmpty(companyCode) && companyCode != Env.CurrentCompany.Code)
			{
				var company = factory.LoadFromNaturalKey<IGlbCompany>(GlbCompanySchema.GC_Code, companyCode);
				var branch = company?.GetActiveBranches()?.FirstOrDefault();
				if (branch != null)
				{
					branchPK = branch.PK.ToGuid();
					logger(LogType.Information, Res.GetString("54B59230-D1CA-42D4-A454-038AC31C13C8", "Targeting Branch '{0}', Company '{1}' from Data Context – defaulted to first active Branch of Company.", branch.GB_Code, companyCode));
				}
				else if (company == null)
				{
					logger(LogType.Warning, Res.GetString("4F99FAAC-EF08-4D9A-8CA9-90E16DC71413", "Data Context Company '{0}' does not exist. The default Company for Native XML will be used.", companyCode));
				}
				else
				{
					logger(LogType.Warning, Res.GetString("E1D08DDE-D3DA-4FF4-A01A-9B8DB138ED1F", "Data Context Company '{0}' has no active Branches. The default Company for Native XML will be used.", companyCode));
				}
			}
			if (message != null && message.EM_ECC_CommunicationPartyConfig.IsValid)
			{
				var partyConfig = factory.Load<IEDICommunicationPartyConfig>(message.EM_ECC_CommunicationPartyConfig);
				var party = partyConfig.Party;
				if (party != null && party.ECP_GS_SecurityProxy.IsValid)
				{
					var ediClientProxyStaff = factory.Load<IGlbStaff>(party.ECP_GS_SecurityProxy);
					if (ediClientProxyStaff != null)
					{
						return Env.SetTemporaryUserContext(ediClientProxyStaff.LoginName, branchPK, Env.Instance.CurrentDepartment.PK);
					}
				}
			}

			var senderCode = message?.Interchange?.EI_From ?? string.Empty;
			if (!string.IsNullOrEmpty(senderCode))
			{
				var interchangeSenderProxyStaff = eAdaptorRegistry.Instance.InterchangeSenderProxyUsers.Value.GetStaffFromRecipient(senderCode, factory);
				if (interchangeSenderProxyStaff != null)
				{
					return Env.SetTemporaryUserContext(interchangeSenderProxyStaff.LoginName, branchPK, Env.Instance.CurrentDepartment.PK);
				}
			}

			var user = (Globals.IsUserInteractive || !GlbStaff.CurrentUser.GS_IsSystemAccount) ? Env.CurrentUser.LoginName : User.InterchangeUserName;
			return Env.SetTemporaryUserContext(user, branchPK, Env.Instance.CurrentDepartment.PK);
		}
	}
}
