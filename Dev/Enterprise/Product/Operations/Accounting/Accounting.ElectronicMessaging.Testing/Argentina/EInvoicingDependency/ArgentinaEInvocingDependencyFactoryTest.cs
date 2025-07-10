using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	public class ArgentinaEInvocingDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestArgentinaEInvocingDependencyFactory()
		{
			var argentinaDependencies = new ArgentinaEInvoicingDependencyFactory() as IArgentinaEInvoicingDependencyFactory;
			AssertType<AuthRequestBuilder>(argentinaDependencies.GetAuthRequestBuilder());
			AssertType<ComprobanteCAERequestBuilder>(argentinaDependencies.GetComprobanteCAERequestBuilder());
			AssertType<ItemDetailEInvoiceXmlBuilder>(argentinaDependencies.GetItemDetailEInvoiceXmlBuilder());
			AssertType<ComplianceSequenceRetriever>(argentinaDependencies.GetComplianceSequenceRetriever());
		}
	}
}
