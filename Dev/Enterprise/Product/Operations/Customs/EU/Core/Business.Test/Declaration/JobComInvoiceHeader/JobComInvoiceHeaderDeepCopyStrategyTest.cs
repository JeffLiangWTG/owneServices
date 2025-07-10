using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class JobComInvoiceHeaderDeepCopyStrategyTest : TestCaseWithFactory
	{
		public void TestLinkInvoiceToDeclarationForEU()
		{
			var declarationOrg = Factory.New<JobDeclaration>();
			declarationOrg.JE_MessageType = "EXP";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declarationOrg.JE_OH_Supplier = supplier.PK;
			declarationOrg.JE_ApplicationCode = "BLT";
			var invoice1 = declarationOrg.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "123";
			invoice1.JZ_InvoiceDate = ZDateTime.Today;

			AssertEquals("Count", 1, invoice1.SupportingDocuments.Count);
			var supportingDocument1 = invoice1.SupportingDocuments[0];
			AssertEquals("Default Document", "N380", supportingDocument1.CSI_Code);

			var declarationCopy = declarationOrg.TemplateCopy() as JobDeclaration;
			var invoice2 = declarationCopy.Invoices[0];
			AssertEquals("Count", 1, invoice2.SupportingDocuments.Count);
			var supportingDocument2 = invoice2.SupportingDocuments[0];
			AssertEquals("Default Document", "N380", supportingDocument2.CSI_Code);
		}
	}
}
