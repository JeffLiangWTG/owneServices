using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISDocumentValidationTest : TestCaseWithFactory
	{
		public void TestCheckNumberAndType()
		{
			AQISDocument document = new AQISDocument(Factory);
			AssertNoErrors("Type", document.TypeInfo);
			AssertNoErrors("Number", document.NumberInfo);

			document.Number = "Num";
			document.Validation.ValidateType();
			AssertHasErrors("Type", document.TypeInfo);
			AssertNoErrors("Number", document.NumberInfo);

			document.Number = ZString.Empty;
			document.Validation.ValidateType();
			AssertNoErrors("Type", document.TypeInfo);
			AssertNoErrors("Number", document.NumberInfo);

			document.Type = "TT";
			document.Validation.ValidateNumber();
			AssertNoErrors("Type", document.TypeInfo);
			AssertHasErrors("Number", document.NumberInfo);

			document.Type = ZString.Empty;
			document.Validation.ValidateNumber();
			AssertNoErrors("Type", document.TypeInfo);
			AssertNoErrors("Number", document.NumberInfo);

			document.Number = "Num";
			document.Type = "TT";
			AssertNoErrors("Type", document.TypeInfo);
			AssertNoErrors("Number", document.NumberInfo);
		}

		public void TestTypeListValidation()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var documentType = CMRAqisDocumentType.New(Factory);
				documentType.QD_AQISDocumentType = "Code";
				documentType.QD_AQISDocumentDescription = "Description";
				Factory.Save();

				var document = new AQISDocument(Factory);
				ValidationTestHelper.AssertInvalidCodeMessageError(document.TypeInfo, "TTT", "Code");
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var universalHelper = new UniversalReferenceTestDataHelper(Factory);
				universalHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRDT, "AQIS Document Code Type", Core.Constants.CountryCodes.Australia);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRDT, "CODE", "Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				var document = new AQISDocument(Factory);
				ValidationTestHelper.AssertInvalidCodeMessageError(document.TypeInfo, "XXX", "CODE");
			}
		}

		public void TestNumberOfDocumentsEntered()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISDocument document1 = invoiceLine.AQISDocuments.AddNew();
			document1.Number = "123/123";
			document1.Type = "One";

			AQISDocument document2 = invoiceLine.AQISDocuments.AddNew();
			document2.Number = "456";
			document2.Type = "Two";

			AQISDocument document3 = invoiceLine.AQISDocuments.AddNew();
			document3.Number = "789";
			document3.Type = "Thr";

			AQISDocument document4 = invoiceLine.AQISDocuments.AddNew();
			document4.Number = "321";
			document4.Type = "Four";

			AQISDocument document5 = invoiceLine.AQISDocuments.AddNew();
			document5.Number = "654";
			document5.Type = "Five";

			AQISDocument document6 = invoiceLine.AQISDocuments.AddNew();
			document6.Number = "987";
			document6.Type = "Six";

			AQISDocument document7 = invoiceLine.AQISDocuments.AddNew();
			document7.Number = "987";
			document7.Type = "Sev";

			AQISDocument document8 = invoiceLine.AQISDocuments.AddNew();
			document8.Number = "987";
			document8.Type = "Eig";

			AQISDocument document9 = invoiceLine.AQISDocuments.AddNew();
			document9.Number = "987";
			document9.Type = "Nine";

			AQISDocument document10 = invoiceLine.AQISDocuments.AddNew();
			document10.Number = "987";
			document10.Type = "Ten";

			document10.Validation.ValidateAll();

			AssertEquals("Document 1 has no errors", false, document1.HasRowErrors);
			AssertEquals("Document 2 has no errors", false, document2.HasRowErrors);
			AssertEquals("Document 3 has no errors", false, document3.HasRowErrors);
			AssertEquals("Document 4 has no errors", false, document4.HasRowErrors);
			AssertEquals("Document 5 has no errors", false, document5.HasRowErrors);
			AssertEquals("Document 6 has no errors", false, document6.HasRowErrors);
			AssertEquals("Document 7 has no errors", false, document7.HasRowErrors);
			AssertEquals("Document 8 has no errors", false, document8.HasRowErrors);
			AssertEquals("Document 9 has no errors", false, document9.HasRowErrors);
			AssertEquals("Document 10 has no errors", false, document10.HasRowErrors);

			AQISDocument document11 = invoiceLine.AQISDocuments.AddNew();
			document11.Number = "987";
			document11.Type = "Ele";
			Factory.Save();

			document11.Validation.ValidateAll();
			AssertEquals("Document 1 has no errors", false, document1.HasRowErrors);
			AssertEquals("Document 2 has no errors", false, document2.HasRowErrors);
			AssertEquals("Document 3 has no errors", false, document3.HasRowErrors);
			AssertEquals("Document 4 has no errors", false, document4.HasRowErrors);
			AssertEquals("Document 5 has no errors", false, document5.HasRowErrors);
			AssertEquals("Document 6 has no errors", false, document6.HasRowErrors);
			AssertEquals("Document 7 has no errors", false, document7.HasRowErrors);
			AssertEquals("Document 8 has no errors", false, document8.HasRowErrors);
			AssertEquals("Document 9 has no errors", false, document9.HasRowErrors);
			AssertEquals("Document 10 has no errors", false, document10.HasRowErrors);
			AssertEquals("Document 11 has errors", true, document11.HasRowErrors);

			invoiceLine.AQISDocuments.RemoveAndDelete(document2);
			document11.Validation.ValidateAll();
			AssertEquals("Document 1 has no errors", false, document1.HasRowErrors);
			AssertEquals("Document 3 has no errors", false, document3.HasRowErrors);
			AssertEquals("Document 4 has no errors", false, document4.HasRowErrors);
			AssertEquals("Document 5 has no errors", false, document5.HasRowErrors);
			AssertEquals("Document 6 has no errors", false, document6.HasRowErrors);
			AssertEquals("Document 7 has no errors", false, document7.HasRowErrors);
			AssertEquals("Document 8 has no errors", false, document8.HasRowErrors);
			AssertEquals("Document 9 has no errors", false, document9.HasRowErrors);
			AssertEquals("Document 10 has no errors", false, document10.HasRowErrors);
			AssertEquals("Document 11 has no errors", false, document11.HasRowErrors);

			document4.Number = "321,456";
			document5.Number = "321/456";
			AssertEquals("Document 4 has errors", true, document4.NumberInfo.HasErrors());
			AssertEquals("Document 5 has no errors", false, document5.NumberInfo.HasErrors());
		}
	}
}
