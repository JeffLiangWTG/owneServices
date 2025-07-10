using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RD415AdditionalInformationProviderTest : DataProviderTestCase<RD415AdditionalInformationProvider>
	{
		public void TestIRD415AdditionalInformation()
		{
			Assert("Should implement IRD415AdditionalInformation", Provider is IRD415AdditionalInformation);
		}

		public void TestPeriodDischarge()
		{
			SetUpTestData();
			sendingAction.PeriodForDischarge = 7;
			AssertEquals("7", Provider.PeriodDischarge);

			sendingAction.PeriodForDischarge = 30;
			AssertEquals("30", Provider.PeriodDischarge);
		}

		public void TestRateOfYield()
		{
			SetUpTestData();
			sendingAction.RateOfYield = "Test";
			AssertEquals("Test", Provider.RateOfYield);

			sendingAction.RateOfYield = "Rate of Yield";
			AssertEquals("Rate of Yield", Provider.RateOfYield);
		}

		protected override RD415AdditionalInformationProvider GetProvider()
		{
			SetUpTestData();
			return new RD415AdditionalInformationProvider(sendingAction);
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
