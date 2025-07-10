using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ExchangeHedgeCollection : CusSupportingInfoCollection<ExchangeHedge>
	{
		public ExchangeHedgeCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.ExchangeHedge)
		{
		}

		public override bool ReadOnly => base.ReadOnly || (Master is JobComInvoiceHeader parent && parent.AnyLineHasLinkedInvoiceLine);
	}
}
