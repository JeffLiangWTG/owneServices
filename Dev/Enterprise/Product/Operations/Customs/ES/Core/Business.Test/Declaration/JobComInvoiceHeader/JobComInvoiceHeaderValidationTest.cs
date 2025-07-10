using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class JobComInvoiceHeaderValidationTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
	{
		public void TestParent()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZString.Empty;
			var invoice = declaration.Invoices.AddNew();
			return invoice;
		}

		protected override Type GetTypeForTest()
		{
			return typeof(JobComInvoiceHeaderValidation);
		}

		public void TestCheckChildrenPreviousDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var previousDoc = invoiceLine.PreviousDocuments.AddNew();
			SetDataForPreviousDocument(previousDoc, "AA", 2);

			var previousDoc1 = invoice.PreviousDocuments.AddNew();
			SetDataForPreviousDocument(previousDoc1, "BB", 3);

			CombineAssertions(() =>
			{
				invoice.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoice, "Header's previous document will not be declared as all invoice lines have its own previous document for Box 40");

				previousDoc1.Delete();
				invoice.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoice, "Header's previous document will not be declared as all invoice lines have its own previous document for Box 40");

				var previousDoc2 = declaration.PreviousDocuments.AddNew();
				SetDataForPreviousDocument(previousDoc2, "CC", 4);

				invoice.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoice, "Header's previous document will not be declared as all invoice lines have its own previous document for Box 40");

				previousDoc2.Delete();
				invoice.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoice, "Header's previous document will not be declared as all invoice lines have its own previous document for Box 40");
			});
		}

		public void TestOnePreviousDocumentPerEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var previousDoc = invoice.PreviousDocuments.AddNew();
			SetDataForPreviousDocument(previousDoc, "AA", 1);
			var previousDoc2 = invoice.PreviousDocuments.AddNew();
			SetDataForPreviousDocument(previousDoc2, "BB", 2);

			CombineAssertions(() =>
			{
				invoice.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoice, "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");

				previousDoc2.Delete();
				invoice.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoice, "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
			});
		}

		public void TestRequireJZ_IncoTermPlaceMandatory()
		{
			var invoice = (JobComInvoiceHeader)GetInvoiceHeader();
			ValidationTestHelper.AssertFieldIsNotMandatory(invoice.JZ_IncoTermPlaceInfo);
		}

		void SetDataForPreviousDocument(PreviousDocument doc, string code, ZShort lineNo)
		{
			doc.CSI_Code = code;
			doc.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
			doc.CSI_LineNo = lineNo;
		}
	}
}
