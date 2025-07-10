using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public partial class ProfessionalServicesQuoteLookups : IncidentMainLookups
	{
		public ProfessionalServicesQuoteLookups(ProfessionalServicesQuote parent) : base(parent)
		{
		}

		public static class ProfessionalServiceQuoteStatusCodes
		{
			public const string Invoice = "INV";
			public const string Quote = "QUO";
			public const string WaitForCustomer = "W_C";
			public const string WaitForOrder = "W_O";
			public const string WaitForPayment = "W_P";
			public const string WaitForSomeone = "W_S";
			public const string WorkInProgress = "WIP";
		}

		protected override void PopulateStatusList(CodeDescriptionPairList statusList)
		{
			statusList.AddPair(IncidentConstants.IncidentStatus.Cancelled, "Cancelled");
			statusList.AddPair(IncidentConstants.IncidentStatus.Closed, "Closed");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.Invoice, "Invoice");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.Quote, "Quote");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.WaitForCustomer, "Wait for Customer");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.WaitForOrder, "Wait for Order");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.WaitForPayment, "Wait for Payment");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.WaitForSomeone, "Wait for Someone");
			statusList.AddPair(ProfessionalServiceQuoteStatusCodes.WorkInProgress, "Work in Progress");
		}

		CodeDescriptionPairList GetWorkItemTypeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair("CTM", "Customisation (subject to Maintenance & Upgrade Fee)");
			result.AddPair("CTW", "Customisation (without Maintenance & Upgrade Fee)");
			result.AddPair("ENH", "Enhancement on a partially funded basis");
			result.AddPair("SRV", "Services charged on an hourly basis, i.e. data fixes, consultancy");
			result.AddPair("GRA", "Graphics");

			return result;
		}

		public override OrgHeaderCollection Clients
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}
	}
}

