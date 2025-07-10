using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common;

sealed class ErrorInterchangeHandler
{
	public ErrorInterchangeHandler(NACCSMessageParentFinder parentFinder)
	{
		this.parentFinder = Argument.NotNull(parentFinder, nameof(parentFinder));
	}

	readonly NACCSMessageParentFinder parentFinder;

	public EDIInterchangeUnpackerResult CreateXERMessage(Messaging.Business.EDIInterchange interchange, BusinessObject messageParent)
	{
		var factory = interchange.Factory;
		messageParent ??= parentFinder.FindParentFromHubID(factory, interchange.eHubID);

		var branch = (messageParent as IBranchProvider)?.Branch ?? GetBranchFromAttribute(interchange) ?? GetFirstActiveBranch(factory);
		var branchPk = branch?.PK ?? interchange.EI_GB;

		using (DisposableEnvironment.ForBranch(branchPk.ToGuid()))
		{
			var message = interchange.Factory.New<EDIMessage>();
			message.MessageNumberStrategy = new MessageNumberStrategy(message);
			message.EM_ReceiveTransmit = interchange.EI_ReceiveTransmit;
			message.EM_MessageData = interchange.EI_BodyData;
			message.EM_MessageType = interchange.EI_InterchangeType;
			message.EM_LinkedObject = messageParent;
			message.EM_Status = EDIMessage.Status.Queued;

			interchange.EI_GB = branchPk;
			interchange.ContainedMessages.Add(message);

			return new EDIInterchangeUnpackerResult(new[] { message });
		}
	}

	GlbBranch GetBranchFromAttribute(Messaging.Business.EDIInterchange interchange)
	{
		GlbBranch result = null;

		var headerText = interchange.EI_HeaderText;
		var attributes = xTMessaging.Shared.Utils.GetHeaderTextDictionary(headerText);

		if (attributes.TryGetValue(Constants.DirectxT.CompanyCodeAttribute, out var companyCode) && !string.IsNullOrWhiteSpace(companyCode))
		{
			var company = LoadCompany(interchange.Factory, companyCode);
			result = company?.FirstActiveBranch;
		}

		return result;
	}

	GlbBranch GetFirstActiveBranch(BusinessObjectFactory factory)
	{
		return GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Japan, factory).FirstOrDefault();
	}

	GlbCompany LoadCompany(BusinessObjectFactory factory, string companyCode)
	{
		var query = new ZQuery(GlbCompanySchema.GC_Code, companyCode);
		query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
		query.AddToFilter(GlbCompanySchema.GC_IsActive, true);

		return factory.LoadTop1<GlbCompany>(query);
	}
}
