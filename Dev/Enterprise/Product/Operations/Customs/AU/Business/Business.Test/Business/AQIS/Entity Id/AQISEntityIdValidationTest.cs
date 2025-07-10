using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISEntityIdValidationTest : AQISSingleValueValidationTest
	{
		public override void TestCodeAgainstLookupList()
		{
			var entityId = new AQISEntityId(Factory);
			AssertEquals("No Message Error", false, entityId.CodeInfo.HasMessageErrors());

			entityId.Code = "AAA";
			AssertEquals("Message Error", true, entityId.CodeInfo.HasMessageErrors());
		}

		public override void TestNumberOfCodesEntered()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISEntityId entityId1 = invoiceLine.AQISEntityIds.AddNew();
			entityId1.Code = "1";
			AQISEntityId entityId2 = invoiceLine.AQISEntityIds.AddNew();
			entityId2.Code = "2";
			AQISEntityId entityId3 = invoiceLine.AQISEntityIds.AddNew();
			entityId3.Code = "3";
			AQISEntityId entityId4 = invoiceLine.AQISEntityIds.AddNew();
			entityId4.Code = "4";
			AQISEntityId entityId5 = invoiceLine.AQISEntityIds.AddNew();
			entityId5.Code = "5";
			AQISEntityId entityId6 = invoiceLine.AQISEntityIds.AddNew();
			entityId6.Code = "6";
			AQISEntityId entityId7 = invoiceLine.AQISEntityIds.AddNew();
			entityId7.Code = "7";
			AQISEntityId entityId8 = invoiceLine.AQISEntityIds.AddNew();
			entityId8.Code = "8";
			AQISEntityId entityId9 = invoiceLine.AQISEntityIds.AddNew();
			entityId9.Code = "9";
			AQISEntityId entityId10 = invoiceLine.AQISEntityIds.AddNew();
			entityId10.Code = "10";
			entityId10.Validation.ValidateAll();
			AssertNoErrors("Concern Type 1", entityId1.CodeInfo);
			AssertNoErrors("Concern Type 2", entityId2.CodeInfo);
			AssertNoErrors("Concern Type 3", entityId3.CodeInfo);
			AssertNoErrors("Concern Type 4", entityId4.CodeInfo);
			AssertNoErrors("Concern Type 5", entityId5.CodeInfo);
			AssertNoErrors("Concern Type 6", entityId6.CodeInfo);
			AssertNoErrors("Concern Type 7", entityId7.CodeInfo);
			AssertNoErrors("Concern Type 8", entityId8.CodeInfo);
			AssertNoErrors("Concern Type 9", entityId9.CodeInfo);
			AssertNoErrors("Concern Type 10", entityId10.CodeInfo);

			AQISEntityId entityId11 = invoiceLine.AQISEntityIds.AddNew();
			entityId11.Code = "11";
			AssertNoErrors("Concern Type 1", entityId1.CodeInfo);
			AssertNoErrors("Concern Type 2", entityId2.CodeInfo);
			AssertNoErrors("Concern Type 3", entityId3.CodeInfo);
			AssertNoErrors("Concern Type 4", entityId4.CodeInfo);
			AssertNoErrors("Concern Type 5", entityId5.CodeInfo);
			AssertNoErrors("Concern Type 6", entityId6.CodeInfo);
			AssertNoErrors("Concern Type 7", entityId7.CodeInfo);
			AssertNoErrors("Concern Type 8", entityId8.CodeInfo);
			AssertNoErrors("Concern Type 9", entityId9.CodeInfo);
			AssertNoErrors("Concern Type 10", entityId10.CodeInfo);
			AssertHasErrors("Concern Type 11", entityId11.CodeInfo);

			invoiceLine.AQISEntityIds.RemoveAndDelete(entityId2);
			entityId11.Validation.ValidateCode();
			AssertNoErrors("Concern Type 1", entityId1.CodeInfo);
			AssertNoErrors("Concern Type 3", entityId3.CodeInfo);
			AssertNoErrors("Concern Type 4", entityId4.CodeInfo);
			AssertNoErrors("Concern Type 5", entityId5.CodeInfo);
			AssertNoErrors("Concern Type 6", entityId6.CodeInfo);
			AssertNoErrors("Concern Type 7", entityId7.CodeInfo);
			AssertNoErrors("Concern Type 8", entityId8.CodeInfo);
			AssertNoErrors("Concern Type 9", entityId9.CodeInfo);
			AssertNoErrors("Concern Type 10", entityId10.CodeInfo);
			AssertNoErrors("Concern Type 11", entityId11.CodeInfo);
		}

		AQISEntityId aqisEntityId;
		public override AQISSingleValueBusinessObject BizObjToTest => aqisEntityId ?? (aqisEntityId = new AQISEntityId(Factory));
	}
}
