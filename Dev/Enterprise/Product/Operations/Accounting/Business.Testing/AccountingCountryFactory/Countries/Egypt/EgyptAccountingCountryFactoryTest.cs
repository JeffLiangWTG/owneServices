using CargoWise.Accounting.eInvoicing.Egypt;
using CargoWise.Application;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Moq;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class EgyptAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestGetXmlDocumentSignatureBuilder()
		{
			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Egypt) as IEInvoicingSignatureBuilderProvider;
			var mockSignatureAlgorithm = new Mock<ISignatureAlgorithm>();
			var builder = provider.GetXmlDocumentSignatureBuilder(mockSignatureAlgorithm.Object);

			AssertType<EgyptSignatureBuilder>("XML document signature builder should be of type EgyptSignatureBuilder", builder);
		}
	}
}
