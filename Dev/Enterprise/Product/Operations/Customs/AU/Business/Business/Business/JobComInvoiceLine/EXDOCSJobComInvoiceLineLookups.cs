using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class EXDOCSJobComInvoiceLineLookups : JobComInvoiceLineLookups
	{
		public EXDOCSJobComInvoiceLineLookups(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override CodeDescriptionPairList EXDOCPermitAuthorityList
		{
			get { return Factory.GetCachedValue<EXDOCPermitTypeCodes>(); }
		}
	}
}
