using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class DeferredPaymentProviderTest : DataProviderTestCase<DeferredPaymentProvider>
	{
		public void TestIDeferredPayment()
		{
			Assert("Should implement IDeferredPayment", Provider is IDeferredPayment);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", GetProvider().CcQualifier);
		}

		public void TestDeferredPayment()
		{
			SetUpTestData();
			AssertEquals("DeferredPayment", "1234", Provider.DeferredPayment);
		}

		protected override DeferredPaymentProvider GetProvider()
		{
			SetUpTestData();
			return new DeferredPaymentProvider(declaration);
		}

		void SetUpTestData()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_DefermentAccountNumber = "1234";
		}

		JobDeclaration declaration;
	}
}
