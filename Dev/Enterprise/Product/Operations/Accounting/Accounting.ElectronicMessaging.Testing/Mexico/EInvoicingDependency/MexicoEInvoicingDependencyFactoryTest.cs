using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	public class MexicoEInvoicingDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestMexicoEInvocingDependencyFactory()
		{
			var mexicoDependencies = new MexicoEInvoicingDependencyFactory() as IMexicoEInvoicingDependencyFactory;
			AssertType<CFDiCancellationBuilder>(mexicoDependencies.GetCFDiCancellationBuilder());
			AssertType<CFDiRelacionadosBuilder>(mexicoDependencies.GetCFDiRelacionadosBuilder());
			AssertType<CompanyCredential>(mexicoDependencies.GetCompanyCredential());
			AssertType<CertificateHelper>(mexicoDependencies.GetCertificateHelper());
			AssertType<CFDiObtenerPDFBuilder>(mexicoDependencies.GetCFDiObtenerPDFBuilder());
			AssertNotNull(mexicoDependencies.GetEInvoiceHelper());
		}
	}
}
