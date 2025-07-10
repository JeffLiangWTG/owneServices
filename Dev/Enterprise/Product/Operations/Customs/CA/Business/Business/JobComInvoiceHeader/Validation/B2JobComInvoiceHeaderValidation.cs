using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	class B2JobComInvoiceHeaderValidation : CommonImportJobComInvoiceHeaderValidation
	{
		public B2JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		#region CheckJZ_InvoiceNumber

		protected override void CheckJZ_InvoiceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo);
		}

		#endregion

		#region CheckJZ_IncoTerm

		protected override bool IncoTermRequired
		{
			get { return false; }
		}

		#endregion

		protected override void CheckJZ_Calc_CIFAmount()
		{
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
		}

		protected override void CheckJZ_ValuationDateOverrideIsValidZDateTimeRange()
		{
			var limits = new TypeValidationLimits() { PastYearsBeforeWarning = 4 };
			TypeValidation.CheckValidZDateTimeRange(Parent.JZ_ValuationDateOverrideInfo, limits);
		}
	}
}
