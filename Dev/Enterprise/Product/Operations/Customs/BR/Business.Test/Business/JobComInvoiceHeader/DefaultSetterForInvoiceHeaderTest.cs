using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class DefaultSetterForInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestSetDefaultsForNewElement_ShouldBringImporterFromDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Imp001";
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			AssertEquals(importer.PK, invoice.JZ_OH_Buyer);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			AssertEquals(ZGuid.Empty, invoice.JZ_OH_Buyer);
		}
	}
}
