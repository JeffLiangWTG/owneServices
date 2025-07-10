using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccEPaymentQuoteLookups : AutoAccEPaymentQuoteLookups
	{
		public AccEPaymentQuoteLookups(AutoAccEPaymentQuote parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StatusCodeList => EPaymentStatusCodes.Quote.CodesList;

		public CodeDescriptionPairList StatusDescriptionCodeList => EPaymentStatusCodes.Quote.CodeListWithShortDescription;

		public CodeDescriptionPairList ProviderCodeList => EPaymentProviderCodes.CodesList;

		public OrgHeaderCollection OrgHeaders => new OrgHeaderCollection(Factory);
	}
}
