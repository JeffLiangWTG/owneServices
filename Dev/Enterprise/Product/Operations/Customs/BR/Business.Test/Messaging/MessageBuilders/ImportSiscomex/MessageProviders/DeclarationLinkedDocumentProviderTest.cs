using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationLinkedDocumentProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationLinkedDocumentProvider()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			oDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			oDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var oInvoiceLine = oDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var previousDoc = oInvoiceLine.PreviousDocuments.AddNew();
			previousDoc.CSI_ReferenceNumber = "00000000";
			previousDoc.CSI_Code = ImportPreviousDocumentList.Codes.DI;

			var linkedDoc = new DeclarationLinkedDocumentProvider(previousDoc);

			AssertEquals("ReferenceNumber should be", "00000000", linkedDoc.ReferenceNumber);
		}

		public void TestEquals()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			oDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			oDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var oInvoiceLine = oDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var previousDoc1 = oInvoiceLine.PreviousDocuments.AddNew();
			previousDoc1.CSI_ReferenceNumber = "00000000";
			previousDoc1.CSI_Code = ImportPreviousDocumentList.Codes.DI;

			var previousDoc2 = oInvoiceLine.PreviousDocuments.AddNew();
			var linkedDoc1 = new DeclarationLinkedDocumentProvider(previousDoc1);
			var linkedDoc2 = new DeclarationLinkedDocumentProvider(previousDoc2);

			Assert("Not Equals", !linkedDoc1.Equals(null));
			Assert("Not Equals", !linkedDoc1.Equals(previousDoc1));
			Assert("Not Equals", !linkedDoc1.Equals(linkedDoc2));
		}
	}
}
