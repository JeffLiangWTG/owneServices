using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceLineValidation : InvoicingLineBaseValidation
	{
		public InvoiceLineValidation(InvoiceLine parent)
			: base(parent)
		{
		}

		#region Overrides

		protected override void CheckAL_Desc()
		{
			base.CheckAL_Desc();
			MandatoryValidation.CheckEntered(Parent.AL_DescInfo);
		}

		#endregion

		#region Transaction Line

		InvoiceLine fTransactionInvoiceLine;

		public InvoiceLine TransactionInvoiceLine
		{
			get
			{
				if (fTransactionInvoiceLine == null)
				{
					fTransactionInvoiceLine = (InvoiceLine)Parent;
				}
				return fTransactionInvoiceLine;
			}
		}

		#endregion
	}
}
