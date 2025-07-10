using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceHeaderLookups : JobComInvoiceHeaderLookups
	{
		public EMCSJobComInvoiceHeaderLookups(EMCSJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override GroupHeaderCollection GetNewGroupHeaderCollection()
		{
			return new EMCSGroupHeaderCollection((EMCSJobComInvoiceHeader)Invoice);
		}
	}
}
