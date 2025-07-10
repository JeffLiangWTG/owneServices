using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobExRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJF_RX_NKRateCurrency_OnEmtyCurrency()
		{
			ExchangeRate exchangeRate = Factory.New<ExchangeRate>();
			exchangeRate.JF_RX_NKRateCurrency = string.Empty;
			AssertHasError(exchangeRate.JF_RX_NKRateCurrencyInfo, "Please enter a Currency.");
		}

		public void TestCheckJF_BaseRate_WithoutRateConfiguration()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "ALL", "SEA", preference: "TDR");
			Factory.Save();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("00001");
			shipment.JS_TransportMode = "AIR";
			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();
				orgContact.OC_OH = orgHeader.PK;
				OrgAddress orgAddress = orgHeader.MainAddress;
				job.JH_OC_LocalBillingContact = orgContact.PK;
				job.JH_OA_LocalChargesAddr = orgAddress.PK;

				var exchangeRate = job.ExchangeRates.AddNew();
				exchangeRate.JF_RX_NKRateCurrency = TestObjectCreator.AUD.Code;
				exchangeRate.JF_BaseRate = 1;
				AssertHasWarnings("Standard exchange rate will be used.", exchangeRate.JF_BaseRateInfo);
			}
		}

		public void CheckJF_CFXPercent()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("00001");
			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				var exchangeRate = job.ExchangeRates.AddNew();
				exchangeRate.JF_CFXMinimum = -1;
				AssertHasWarnings("CFX Minimum must be greater than or equal to zero.", exchangeRate.JF_BaseRateInfo);

				exchangeRate.JF_CFXMinimum = 0;
				AssertNoWarnings(exchangeRate.JF_BaseRateInfo);

				exchangeRate.JF_CFXMinimum = 1;
				AssertNoWarnings(exchangeRate.JF_BaseRateInfo);
			}
		}

		public void CheckJF_CFXMinimum()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("00001");
			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				var exchangeRate = job.ExchangeRates.AddNew();
				exchangeRate.JF_CFXPercent = -1;
				AssertHasWarnings("CFX Percent must be greater than or equal to zero.", exchangeRate.JF_CFXPercentInfo);

				exchangeRate.JF_CFXPercent = 0;
				AssertNoWarnings(exchangeRate.JF_BaseRateInfo);

				exchangeRate.JF_CFXPercent = 1;
				AssertNoWarnings(exchangeRate.JF_BaseRateInfo);
			}
		}

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
	}
}
