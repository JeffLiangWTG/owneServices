using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public abstract class IM413AndIM415DeclarationProviderTest<T> : DataProviderTestCase<T> where T : IM413AndIM415DeclarationProvider
	{
		public void TestConstructorOptions()
		{
			SetUpTestData();
			bill.LocalReferenceNumber = "LRN1231";
			var providerGenerateLRN = new IM413AndIM415DeclarationProvider(messageSendingObject, generateNewLRN: true);
			AssertEquals("LRN Placeholder", AISOutboundEDIMessage.LRNPlaceHolder, providerGenerateLRN.LRN);
			var providerDoNotGenerateLRN = new IM413AndIM415DeclarationProvider(messageSendingObject, generateNewLRN: false);
			AssertEquals("bill.LocalReferenceNumber", "LRN1231", providerDoNotGenerateLRN.LRN);
		}

		public void TestLRN()
		{
			SetUpTestData();
			bill.LocalReferenceNumber = "LRN1231";
			AssertEquals("LRN Placeholder", ExpectedLRN, Provider.LRN);
		}

		protected abstract string ExpectedLRN { get; }

		public void TestAdditionalDeclarationType()
		{
			AssertEquals(SubStyleCodeList.Codes.NormalDeclaration, Provider.AdditionalDeclarationType);
		}

		public void TestPreferredPaymentMethod()
		{
			AssertEquals(string.Empty, Provider.PreferredPaymentMethod);
			bill.Header.AMA_PaymentMethod = PaymentMethodList.Codes.E;
			AssertEquals("E", Provider.PreferredPaymentMethod);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals(string.Empty, Provider.CustomsOfficeLodgement);
			bill.Header.AMA_CustomsOffice = "TestOffice";
			AssertEquals("TestOffice", Provider.CustomsOfficeLodgement);
		}

		public void TestDeferredPayment()
		{
			AssertEquals(string.Empty, Provider.DeferredPayment);
			bill.Header.AMA_PaymentAccountNumber = "001234";
			AssertEquals("001234", Provider.DeferredPayment);
		}

		public void TestParties()
		{
			AssertType<PartiesProvider>(Provider.Parties);
		}

		protected void SetUpTestData()
		{
			if (messageSendingObject == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();

				messageSendingObject = new MessageSendingObject(bill);
				messageSendingObject.SubStyle = SubStyleCodeList.Codes.NormalDeclaration;
			}
		}
		protected MessageSendingObject messageSendingObject;
		protected AsycudaBill bill;
	}

	public class IM413AndIM415DeclarationProviderTest : IM413AndIM415DeclarationProviderTest<IM413AndIM415DeclarationProvider>
	{
		protected override string ExpectedLRN => AISOutboundEDIMessage.LRNPlaceHolder;

		protected override IM413AndIM415DeclarationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415DeclarationProvider(messageSendingObject, generateNewLRN: true);
		}
	}
}
