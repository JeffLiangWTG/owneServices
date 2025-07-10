using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineChargeCollection<T> : JobComInvChargeCollection<T> where T : InvoiceLineCharge
	{
		public InvoiceLineChargeCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (((JobComInvoiceLine)Parent).IsExport)
			{
				((T)child).J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			}
		}
	}
}
