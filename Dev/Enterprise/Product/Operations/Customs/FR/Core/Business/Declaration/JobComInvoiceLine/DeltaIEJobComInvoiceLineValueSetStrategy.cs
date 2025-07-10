namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceLineValueSetStrategy : JobComInvoiceLineValueSetStrategy
	{
		public DeltaIEJobComInvoiceLineValueSetStrategy(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override void OnProcedureChanged()
		{
			base.OnProcedureChanged();
			invoiceLine.Declaration?.ValueSetStrategy?.DefaultJE_CustomsGuaranteeNumber();
		}
	}
}
