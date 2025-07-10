using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.Common;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415DepositRefundDetailsProviderTest : DataProviderTestCase<RD415DepositRefundDetailsProvider>
	{
		public void TestIDepositRefundDetails()
		{
			Assert("Should implement IDepositRefundDetails", Provider is IDepositRefundDetails);
		}

		public void TestImportedGoodsDischarged()
		{
			SetUpTestData();
			sendingAction.ImportedGoodsDischarged = true;
			AssertEquals(true, Provider.ImportedGoodsDischarged);

			sendingAction.ImportedGoodsDischarged = false;
			AssertEquals(false, Provider.ImportedGoodsDischarged);
		}

		public void TestOutstandingbalanceOnImportedGoods()
		{
			SetUpTestData();
			sendingAction.OutstandingBalance = 300;
			AssertEquals(300m, Provider.OutstandingbalanceOnImportedGoods);

			sendingAction.OutstandingBalance = 450.55;
			AssertEquals(450.55m, Provider.OutstandingbalanceOnImportedGoods);
		}

		public void TestAmountOfDepositRefundClaim()
		{
			SetUpTestData();
			sendingAction.AmountOfDepositRefund = 300;
			AssertEquals(300m, Provider.AmountOfDepositRefundClaim);

			sendingAction.AmountOfDepositRefund = 450.55;
			AssertEquals(450.55m, Provider.AmountOfDepositRefundClaim);
		}

		public void TestPayerEORIForRefund()
		{
			SetUpTestData();
			sendingAction.PayerEori = "IE432009";
			AssertEquals("IE432009", Provider.PayerEORIForRefund);

			sendingAction.PayerEori = "P123";
			AssertEquals("P123", Provider.PayerEORIForRefund);
		}

		protected override RD415DepositRefundDetailsProvider GetProvider()
		{
			SetUpTestData();
			return new RD415DepositRefundDetailsProvider(sendingAction);
		}

		void SetUpTestData()
		{
			if (sendingAction == null)
			{
				sendingAction = new DepositRefundApplicationMessageSendingAction(Factory.New<CusEntryHeader>());
			}
		}

		DepositRefundApplicationMessageSendingAction sendingAction;
	}
}
