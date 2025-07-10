using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class RD415AmountsHeldDepositProviderTest : DataProviderTestCase<RD415AmountsHeldDepositProvider>
	{
		public void TestIAmountsHeldDeposit()
		{
			Assert("Should implement IAmountsHeldDeposit", Provider is IAmountsHeldDeposit);
		}

		public void TestCustomsDutyHeldDeposit()
		{
			SetUpTestData();
			sendingAction.CustomsDuty = 1200;
			AssertEquals(1200m, Provider.CustomsDutyHeldDeposit);

			sendingAction.CustomsDuty = 612.75;
			AssertEquals(612.75m, Provider.CustomsDutyHeldDeposit);
		}

		public void TestVATHeldDeposit()
		{
			SetUpTestData();
			sendingAction.Vat = 230;
			AssertEquals(230m, Provider.VATHeldDeposit);

			sendingAction.Vat = 460.55;
			AssertEquals(460.55m, Provider.VATHeldDeposit);
		}

		public void TestOtherDutiesHeldDeposit()
		{
			SetUpTestData();
			sendingAction.OtherDuties = 250;
			AssertEquals(250m, Provider.OtherDutiesHeldDeposit);

			sendingAction.OtherDuties = 852.25;
			AssertEquals(852.25m, Provider.OtherDutiesHeldDeposit);
		}

		protected override RD415AmountsHeldDepositProvider GetProvider()
		{
			SetUpTestData();
			return new RD415AmountsHeldDepositProvider(sendingAction);
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
