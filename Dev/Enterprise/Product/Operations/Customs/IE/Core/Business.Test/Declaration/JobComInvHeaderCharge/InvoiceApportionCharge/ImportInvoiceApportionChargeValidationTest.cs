namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportInvoiceApportionChargeValidationTest : EU.Business.Declaration.Testing.InvoiceApportionChargeValidationTest
	{
		public void TestCheckJ7_IsIncludedInITOTMandatory()
		{
			var messageError = "EXW cannot include this charge in lines.";
			invoice.JZ_IncoTerm = "EXW";
			invoiceApportionCharge.J7_ChargeType = "AD";
			invoiceApportionCharge.J7_IsIncludedInITOT = true;

			CombineAssertions(() =>
			{
				invoiceApportionCharge.Validation.ValidateJ7_IsIncludedInITOT();
				AssertHasMessageError("Has message error for the not default charge type", invoiceApportionCharge.J7_IsIncludedInITOTInfo, messageError);

				invoiceApportionCharge.J7_ChargeType = "BB";
				invoiceApportionCharge.Validation.ValidateJ7_IsIncludedInITOT();
				AssertNoMessageError("No message error for the default charge type", invoiceApportionCharge.J7_IsIncludedInITOTInfo, messageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceApportionCharge = invoice.GroupCharges.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		InvoiceApportionCharge invoiceApportionCharge;
	}
}
