namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class SACJobComInvoiceLineValidationTest : ImportJobComInvoiceLineValidationTest
	{
		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new SACJobComInvoiceLineValidation(invoiceLine);
		}

		public void TestEmptyTariffIsIfTobaccoOrAlcoholIsDeclared()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrors("Tariff is mandatory for tobacco or alcohol", invoiceLine.JI_TariffInfo);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			invoiceLine.JI_Tariff = "";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrors("Tariff is mandatory for tobacco or alcohol", invoiceLine.JI_TariffInfo);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertNoErrors("Pre-condition", invoiceLine.JI_IsPackToBondForLineInfo);
			AssertEquals(typeof(IMDJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			IMDJobComInvoiceLineValidation iMDValidation = new IMDJobComInvoiceLineValidation(invoiceLine);
			invoiceLine.JI_IsPackToBondForLine = true;
			iMDValidation.ValidateJI_IsPackToBondForLine();
			AssertNoMessageErrors("Pack to Bond is allowed if not a SAC", invoiceLine.JI_IsPackToBondForLineInfo);

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals(typeof(SACJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
			SACJobComInvoiceLineValidation sacValidation = new SACJobComInvoiceLineValidation(invoiceLine);
			invoiceLine.JI_IsPackToBondForLine = true;
			sacValidation.ValidateJI_IsPackToBondForLine();
			AssertHasMessageErrors("Pack to Bond not allowed for SAC", invoiceLine.JI_IsPackToBondForLineInfo);
		}
	}
}
