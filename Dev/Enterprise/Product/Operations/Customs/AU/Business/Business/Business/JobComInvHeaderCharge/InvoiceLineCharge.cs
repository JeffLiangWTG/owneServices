using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceLineCharge : Customs.Business.BaseInvoiceLineCharge, Integration.Customs.AU.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new JobComInvHeaderChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(this);
		}

		public override CargoWise.Types.ZString J7_RX_NKCurrency
		{
			get { return base.J7_RX_NKCurrency; }
			set
			{
				if (base.J7_RX_NKCurrency != value)
				{
					MarkInvoiceHeaderAsNeedingValidation();
				}
				base.J7_RX_NKCurrency = value;
			}
		}

		void MarkInvoiceHeaderAsNeedingValidation()
		{
			if (InvoiceLine != null && InvoiceLine.InvoiceHeader != null)
			{
				InvoiceLine.InvoiceHeader.MarkAsNeedingValidation();
			}
		}
	}
}
