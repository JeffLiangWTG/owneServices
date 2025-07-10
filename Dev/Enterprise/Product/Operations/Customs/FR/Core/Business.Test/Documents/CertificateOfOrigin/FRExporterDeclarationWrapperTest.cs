using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class FRExporterDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			var supplierAddress = supplier.MainAddress;
			supplierAddress.FillWithValidTestData();
			supplierAddress.OA_City = "Creteil";
			var wrapper = (EU.Business.Documents.CertificateOfOrigin.IExporterDeclaration)new FRExporterDeclarationWrapper(declaration);
			AssertEquals("supplier city", "", wrapper.Place);

			declaration.JE_OH_Supplier = supplier.PK;
			wrapper = new FRExporterDeclarationWrapper(declaration);
			AssertEquals("supplier city", "Creteil", wrapper.Place);
		}
	}
}
