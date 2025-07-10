namespace Enterprise.Customs.KR.Business
{
	public class D87JobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public D87JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCargoManagementNo();
		}

		protected override void CheckJZ_IncoTerm()
		{
		}

		protected override void CheckJZ_MarksAndNumbersIsWesternEuropeanIfRequired()
		{
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
		}

		protected override void CheckJZ_OH_Buyer()
		{
		}

		protected override void CheckJZ_OH_Supplier()
		{
		}

		protected override void CheckJZ_InvoiceAmount() { }

		protected override void CheckJZ_RX_NKInvoice_Currency() { }

		public void ValidateCargoManagementNo()
		{
			ValidateCalculatedProperty(Parent.JZ_ImportCargoManagementNumberInfo);
		}
		protected void CheckCargoManagementNo() { }

		protected JobDeclaration declaration => Parent.JobDeclaration;
	}
}
