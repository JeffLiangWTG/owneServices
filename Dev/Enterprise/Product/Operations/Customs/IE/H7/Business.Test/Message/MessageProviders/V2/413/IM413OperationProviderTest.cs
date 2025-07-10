using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	public class IM413OperationProviderTest : DataProviderTestCase<IM413OperationProvider>
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
			AssertEquals("Preferred Payment Method", string.Empty, Provider.PreferredPaymentMethod);
		}

		public void TestLRN()
		{
			SetUpTestData();
			bill.LocalReferenceNumber = "LRN1231";
			var provider = new IM413OperationProvider(messageSendingObject);
			AssertEquals("LRN", "LRN1231", provider.LRN);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertNull("Customs Registration Number", Provider.CustomsRegistrationNumber);
		}

		public void TestDetailsAmended()
		{
			AssertNull("Details Amended", Provider.DetailsAmended);
		}

		public void TestMRN()
		{
			SetUpTestData();
			AssertEquals("MRN", "MRN001", Provider.MRN);
		}

		void SetUpTestData()
		{
			if (messageSendingObject == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
				bill.MovementReferenceNumberSetter("MRN001");

				messageSendingObject = new MessageSendingObject(bill);
				messageSendingObject.SubStyle = SubStyleCodeList.Codes.NormalDeclaration;
			}
		}
		MessageSendingObject messageSendingObject;
		AsycudaBill bill;

		protected override IM413OperationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413OperationProvider(messageSendingObject);
		}
	}
}
