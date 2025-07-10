using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	public class IM413_414_415OperationProviderTest : DataProviderTestCase<IM413_414_415OperationProvider>
	{
		public void TestMsgType()
		{
			AssertEquals("Msg Type", ImportDeclarationTypeList.Codes.H7, Provider.MsgType);
		}

		public void TestDeclarationType()
		{
			AssertNull("Declaration Type", Provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			AssertEquals("Additional Declaration Type", SubStyleCodeList.Codes.NormalDeclaration, Provider.AdditionalDeclarationType);
		}

		public void TestLanguageCode()
		{
			AssertNull("Language Code", Provider.LanguageCode);
		}

		public void TestPreferredPaymentMethod()
		{
			SetUpTestData();
			bill.Header.AMA_PaymentMethod = "E";
			AssertEquals("E", Provider.PreferredPaymentMethod);
		}

		public void TestLRN()
		{
			SetUpTestData();
			bill.LocalReferenceNumber = "LRN1231";
			var provider = new IM413_414_415OperationProvider(messageSendingObject, true);
			AssertEquals("LRN", AISOutboundEDIMessage.LRNPlaceHolder, provider.LRN);
			provider = new IM413_414_415OperationProvider(messageSendingObject, false);
			AssertEquals("LRN", "LRN1231", provider.LRN);
		}

		void SetUpTestData()
		{
			if (messageSendingObject == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();

				messageSendingObject = new MessageSendingObject(bill);
				messageSendingObject.SubStyle = SubStyleCodeList.Codes.NormalDeclaration;
			}
		}
		MessageSendingObject messageSendingObject;
		AsycudaBill bill;

		protected override IM413_414_415OperationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413_414_415OperationProvider(messageSendingObject, true);
		}
	}
}
