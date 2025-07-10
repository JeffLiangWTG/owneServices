using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class CusLineTariffDetailCollection : Customs.Business.CusLineTariffDetailCollection<CusLineTariffDetail>
	{
		public CusLineTariffDetailCollection(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CusLineTariffDetail)child).ZG_MethodOfPayment = ((CusLineTariffDetail)child).Parent is JobComInvoiceLine invoiceline ? invoiceline.Declaration.JE_PaymentMethod : ZString.Empty;
		}
	}
}
