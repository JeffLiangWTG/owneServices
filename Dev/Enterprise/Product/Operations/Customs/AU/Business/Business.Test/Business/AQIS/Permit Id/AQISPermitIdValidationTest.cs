namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISPermitIdValidationTest : AQISSingleValueValidationTest
	{
		public override void TestCodeAgainstLookupList()
		{
			Assert("No list to test", true);
		}

		public override void TestNumberOfCodesEntered()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISPermitId permitId1 = invoiceLine.AQISPermitIds.AddNew();
			permitId1.Code = "1";
			AQISPermitId permitId2 = invoiceLine.AQISPermitIds.AddNew();
			permitId2.Code = "2";
			AQISPermitId permitId3 = invoiceLine.AQISPermitIds.AddNew();
			permitId3.Code = "3";
			AQISPermitId permitId4 = invoiceLine.AQISPermitIds.AddNew();
			permitId4.Code = "4";
			AQISPermitId permitId5 = invoiceLine.AQISPermitIds.AddNew();
			permitId5.Code = "5";
			AQISPermitId permitId6 = invoiceLine.AQISPermitIds.AddNew();
			permitId6.Code = "6";
			AQISPermitId permitId7 = invoiceLine.AQISPermitIds.AddNew();
			permitId7.Code = "7";
			AQISPermitId permitId8 = invoiceLine.AQISPermitIds.AddNew();
			permitId8.Code = "8";
			AQISPermitId permitId9 = invoiceLine.AQISPermitIds.AddNew();
			permitId9.Code = "9";
			AQISPermitId permitId10 = invoiceLine.AQISPermitIds.AddNew();
			permitId10.Code = "10";
			permitId10.Validation.ValidateAll();
			AssertNoErrors("Concern Type 1", permitId1.CodeInfo);
			AssertNoErrors("Concern Type 2", permitId2.CodeInfo);
			AssertNoErrors("Concern Type 3", permitId3.CodeInfo);
			AssertNoErrors("Concern Type 4", permitId4.CodeInfo);
			AssertNoErrors("Concern Type 5", permitId5.CodeInfo);
			AssertNoErrors("Concern Type 6", permitId6.CodeInfo);
			AssertNoErrors("Concern Type 7", permitId7.CodeInfo);
			AssertNoErrors("Concern Type 8", permitId8.CodeInfo);
			AssertNoErrors("Concern Type 9", permitId9.CodeInfo);
			AssertNoErrors("Concern Type 10", permitId10.CodeInfo);

			AQISPermitId permitId11 = invoiceLine.AQISPermitIds.AddNew();
			permitId11.Code = "11";
			AssertNoErrors("Concern Type 1", permitId1.CodeInfo);
			AssertNoErrors("Concern Type 2", permitId2.CodeInfo);
			AssertNoErrors("Concern Type 3", permitId3.CodeInfo);
			AssertNoErrors("Concern Type 4", permitId4.CodeInfo);
			AssertNoErrors("Concern Type 5", permitId5.CodeInfo);
			AssertNoErrors("Concern Type 6", permitId6.CodeInfo);
			AssertNoErrors("Concern Type 7", permitId7.CodeInfo);
			AssertNoErrors("Concern Type 8", permitId8.CodeInfo);
			AssertNoErrors("Concern Type 9", permitId9.CodeInfo);
			AssertNoErrors("Concern Type 10", permitId10.CodeInfo);
			AssertHasErrors("Concern Type 11", permitId11.CodeInfo);

			invoiceLine.AQISPermitIds.RemoveAndDelete(permitId2);
			permitId11.Validation.ValidateCode();
			AssertNoErrors("Concern Type 1", permitId1.CodeInfo);
			AssertNoErrors("Concern Type 3", permitId3.CodeInfo);
			AssertNoErrors("Concern Type 4", permitId4.CodeInfo);
			AssertNoErrors("Concern Type 5", permitId5.CodeInfo);
			AssertNoErrors("Concern Type 6", permitId6.CodeInfo);
			AssertNoErrors("Concern Type 7", permitId7.CodeInfo);
			AssertNoErrors("Concern Type 8", permitId8.CodeInfo);
			AssertNoErrors("Concern Type 9", permitId9.CodeInfo);
			AssertNoErrors("Concern Type 10", permitId10.CodeInfo);
			AssertNoErrors("Concern Type 11", permitId11.CodeInfo);
		}

		AQISPermitId aqisPermitId;
		public override AQISSingleValueBusinessObject BizObjToTest => aqisPermitId ?? (aqisPermitId = new AQISPermitId(Factory));
	}
}
