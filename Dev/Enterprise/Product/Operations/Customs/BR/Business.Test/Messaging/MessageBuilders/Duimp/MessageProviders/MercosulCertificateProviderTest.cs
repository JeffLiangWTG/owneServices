using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class MercosulCertificateProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(MercosulCertificateProvider.New(null));
			AssertType<MercosulCertificateProvider>(MercosulCertificateProvider.New(Factory.New<MercosulForeignDeclaration>()));
		}

		public void TestProperties()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var mercosulForeignDeclaration = invoiceLine.MercosulForeignDeclarations.AddNew();
			mercosulForeignDeclaration.CSI_SubType = CertificateTypeList.Codes.CCPTC;
			mercosulForeignDeclaration.CSI_Code = "TEST_CERT";
			mercosulForeignDeclaration.CSI_Quantity3 = 1.52658m;

			var dataProvider = MercosulCertificateProvider.New(mercosulForeignDeclaration);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be", CertificateTypeList.Codes.CCPTC, dataProvider.Type);
				AssertEquals("Number should be", "TEST_CERT", dataProvider.Number);
				AssertEquals("Quantity should be", "1.52658", dataProvider.Quantity);
			});
		}
	}
}
