namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public SACJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => base.Parent;

		protected override void CheckJZ_Calc_FOBAmount()
		{
			base.CheckJZ_Calc_FOBAmount();
			var deminimus = UniversalReferenceHelper.GetDeminimus(Parent.Factory);

			if (Parent.JZ_Calc_FOBAmountInLocalCurrency > deminimus)
			{
				Parent.JZ_Calc_FOBAmountInfo.AddMessageError("FOB amount over $" + deminimus.ToString() + " is not allowed for SAC entry.");
			}
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.JobDeclaration != null && Parent.JobDeclaration.IsSACWithLines && Parent.JZ_Calc_Balance < 0)
			{
				Parent.JZ_Calc_BalanceInfo.AddMessageError("Total of line prices should not exceed Invoice amount on invoice header");
			}
		}

		protected override bool IsBuyerRequired
		{
			get { return false; }
		}

		protected override bool IsSupplierRequired
		{
			get { return false; }
		}
	}
}
