using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	using System.Linq;
	using Enterprise.Core;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.Services.OperationalActions.Support.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(ExchangeRatesActionMethodApplicator))]
	internal class ExchangeRatesActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestUpdateFromSailingSchedule()
		{
			Settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.SailingSchedule;
			const string expectedLog =
				"INFO: [HL Shipping Bill of Lading S0001] does not have a job\n" +
				"INFO: [HL Shipping Bill of Lading S0002] does not have any exchange rates to update\n" +
				"WARNING: [HL Shipping Bill of Lading S0003] could not be updated because it does not have an appropriate exchange rates source\n" +
				"INFO: [HL Shipping Bill of Lading S0004] exchange rates were updated from [HL VesselName/VoyageNo]:\n" +
				"WARNING:   \u2022 Exchange rate for 'AUD' does not exist in [HL VesselName/VoyageNo]\n" +
				"WARNING:   \u2022 Exchange rate for 'USD' does not exist in [HL VesselName/VoyageNo]\n" +
				"INFO: [HL Shipping Bill of Lading S0005] exchange rates were updated from [HL VesselName/VoyageNo]:\n" +
				"INFO:   \u2022 Updated exchange rate for 'AUD' from 0.100 to 10.000\n" +
				"INFO:   \u2022 Exchange rate for 'USD' remains unchanged at 20.000\n" +
				"INFO:   \u2022 Exchange rate for 'RUB' remains unchanged at 21.000\n" +
				"INFO:   \u2022 Exchange rate for 'USD' remains unchanged at 20.000"; //one extra exchange rate (we have two: cost and sell);
			CommonShipment shipment1 = CreateShipment("S0001", null, null);
			CommonShipment shipment2 = CreateShipment("S0002", true, null);
			CommonShipment shipment3 = CreateShipment("S0003", false, null);
			CommonShipment shipment4 = CreateShipment("S0004", false, true);
			CommonShipment shipment5 = CreateShipment("S0005", false, false);

			//set cfx directly on ex rates:
			foreach (ExchangeRate r in ((Job)shipment4.Job).ExchangeRates)
			{
				r.OrgType = ExchangeRateOrgTypeEnum.Debtor;
				r.JF_CFXPercent = 10m;
			}

			foreach (ExchangeRate r in ((Job)shipment5.Job).ExchangeRates)
			{
				r.OrgType = ExchangeRateOrgTypeEnum.Debtor;
				r.JF_CFXPercent = 20m;
			}

			SetVoyageExchangeRate(shipment5.Sailing.Voyage, "RUB", 21);
			Charge charge = ((Job)shipment5.Job).Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostAmt = 10m;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSSellAmt = 10m;
			charge.RevenueExchangeRate?.SetBuyRate_ForTestOnly(1m);
			Factory.Save();
			ApplyApplicator(new BusinessObject[] { shipment1, shipment2, shipment3, shipment4, shipment5 }, expectedLog);
			AssertExchangeRate(shipment4, "AUD", BuyRate1 * 0.9m, BuyRate1);
			AssertExchangeRate(shipment4, "USD", BuyRate2 * 0.9m, BuyRate2);
			AssertExchangeRate(shipment5, "AUD", ExRate1 * 0.8m, ExRate1);
			AssertExchangeRate(shipment5, "USD", ExRate2 * 0.8m, ExRate2);
		}

		public void TestUpdateFromCurrencyFile()
		{
			Settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.CurrencyFile;
			const string expectedLog = "INFO: [HL Shipping Bill of Lading S0001] does not have a job\n" + "INFO: [HL Shipping Bill of Lading S0002] does not have any exchange rates to update\n" + "INFO: [HL Shipping Bill of Lading S0003] exchange rates were updated from the current buy rates:\n" + "WARNING:   \u2022 Exchange rate for 'AUD' does not exist in the current buy rates\n" + "INFO:   \u2022 Updated exchange rate for 'USD' from 20.000 to 0.750" + "";
			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "AUD"), null);
			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "USD"), 0.75m);
			CommonShipment shipment1 = CreateShipment("S0001", null, null);
			CommonShipment shipment2 = CreateShipment("S0002", true, null);
			CommonShipment shipment3 = CreateShipment("S0003", false, false);

			Factory.Save();
			ApplyApplicator(new BusinessObject[] { shipment1, shipment2, shipment3 }, expectedLog);
		}

		public void TestUpdateJobChargeWhileUpdatingExchangeRate()
		{
			Settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.CurrencyFile;
			const string expectedLog = "INFO: [HL Shipping Bill of Lading S0001] exchange rates were updated from the current buy rates:\n" + "WARNING:   \u2022 Exchange rate for 'AUD' does not exist in the current buy rates\n" + "INFO:   \u2022 Updated exchange rate for 'USD' from 1.000 to 0.750" + "";

			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "USD"), 0.75m);
			CommonShipment shipment = CreateShipment("S0001", false, false);

			Charge charge = ((Job)shipment.Job).Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 10m;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10m;
			charge.RevenueExchangeRate?.SetBuyRate_ForTestOnly(1m);
			Factory.Save();

			AssertEquals(charge.JR_LocalCostAmt, 10m);

			var newFactory = new BusinessObjectFactory();
			var newShipment = newFactory.Load<CommonShipment>(shipment.PK);
			ApplyApplicator(new BusinessObject[] { newShipment }, expectedLog);
			var jobcharge = ((Job)newShipment.Job).Charges[0];

			AssertEquals(13.33m, jobcharge.JR_LocalSellAmt);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestUpdateFromExchangeRateRegistryConfiguration()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "ALL", "ALL", preference: "HDR");
			GlbCompany.CurrentCompany.Factory.Save();

			Settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.BillingJobExRateRegistry;
			const string expectedLog = "INFO: [HL Shipment S0001] exchange rates were updated from Job Billing Exchange Rate Configuration:\n" + "WARNING:   \u2022 Exchange rate for 'AUD' does not exist in Job Billing Exchange Rate Configuration\n" + "INFO:   \u2022 Updated exchange rate for 'USD' from 20.000 to 0.600" + "";
			var departureDate = ZDateTime.Today.AddDays(-5);
			CommonShipment shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";
			CreateJob(shipment, false);
			shipment.JS_E_DEP = departureDate;
			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "AUD"), null);
			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "USD"), departureDate, 0.60m);
			Factory.Save();
			ApplyApplicator(new BusinessObject[] { shipment }, expectedLog);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestUpdateFromExchangeRateRegistryWithCharges()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "ALL", "ALL");
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("SHP", "ALL", "ALL", exRateType: "SEL");
			GlbCompany.CurrentCompany.Factory.Save();

			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "USD"), ZDateTime.Today, 1.2m);
			SetCurrentExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "USD"), ZDateTime.Today, 1.2m, Constants.ExchangeRateTypes.Code.SellRate);
			Factory.Save();

			Settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.BillingJobExRateRegistry;
			const string expectedLog = @"INFO: [HL Shipment S0001] exchange rates were updated from Job Billing Exchange Rate Configuration:
INFO:   • Updated exchange rate for 'USD' from 1.500 to 1.200
INFO:   • Updated exchange rate for 'USD' from 1.500 to 1.200";

			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S0001", "USLAX", "AUSYD");
			shipment.JS_E_DEP = ZDateTime.Today;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = objectCreator.Debtor.PK;
			shipment.JS_OH_DeliveryAgent = objectCreator.ABIGAS.PK;
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0, objectCreator.Agent, 0);
			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job.AgentCollectPK = ZGuid.Empty;
			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRate.JF_BaseRate = 1.5m;
			exRate.JF_OrgType = "CRD";

			exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRate.JF_BaseRate = 1.5m;
			exRate.JF_OrgType = "DEB";
			Factory.Save();

			var charge = objectCreator.CreateCharge(job, objectCreator.FRT, 0m, 0m);
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.JR_OSSellAmt = 100m;

			AssertEquals(1.5m, charge.JR_OSSellExRate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newShipment = newFactory.Load<CommonShipment>(shipment.PK);
			ApplyApplicator(new BusinessObject[] { newShipment }, expectedLog);

			AssertNoExceptionThrown(() => newFactory.Save());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExchangeRatesActionMethodApplicator(Settings);
		}

		CommonShipment CreateShipment(string shipmentNumber, bool? isJobEmpty, bool? isSailingEmpty)
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Freight.Integration.Agency.IBillOfLading>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			if (isJobEmpty != null)
			{
				CreateJob(shipment, isJobEmpty ?? false);
			}

			if (isSailingEmpty != null)
			{
				CreateSailing(shipment, isSailingEmpty ?? false);
			}

			return shipment;
		}

		Job CreateJob(CommonShipment shipment, bool isEmpty)
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = shipment.JS_UniqueConsignRef;
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			if (!isEmpty)
			{
				ExchangeRate rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = Currency1.RX_Code;
				rate.JF_BaseRate = BuyRate1;
				rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = Currency2.RX_Code;
				rate.JF_BaseRate = BuyRate2;
			}

			return job;
		}

		JobSailing CreateSailing(CommonShipment shipment, bool isEmpty)
		{
			var vessel = RefVessel.LookupVesselByName("VesselName", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "VesselName";
			}

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VoyageNo";
			voyage.JV_OH_Line = principal.PK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			shipment.JS_JX = sailing.PK;
			if (!isEmpty)
			{
				VoyageExRate rate = sailing.Voyage.ExRates.AddNew();
				rate.E8_RX_NKExCurrency = Currency1.RX_Code;
				rate.E8_VoyageExchangeRate = ExRate1;
				rate = sailing.Voyage.ExRates.AddNew();
				rate.E8_RX_NKExCurrency = Currency2.RX_Code;
				rate.E8_VoyageExchangeRate = ExRate2;
			}

			return sailing;
		}

		RefCurrency currency1;
		RefCurrency Currency1
		{
			get
			{
				if (currency1 == null)
				{
					currency1 = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
					currency1.ExchangeRates[0].RE_SellRate = ExRate1;
				}

				return currency1;
			}
		}

		RefCurrency currency2;
		RefCurrency Currency2
		{
			get
			{
				if (currency2 == null)
				{
					currency2 = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
					currency2.ExchangeRates[0].RE_SellRate = ExRate2;
				}

				return currency2;
			}
		}

		readonly Decimal BuyRate1 = 0.1m;
		readonly Decimal BuyRate2 = 20m;
		readonly Decimal ExRate1 = 10m;
		readonly Decimal ExRate2 = 20m;
		ExchangeRatesSettings settings;
		ExchangeRatesSettings Settings
		{
			get
			{
				return settings ?? (settings = new ExchangeRatesSettings(new ExRateSourceType[] { ExRateSourceType.Voyage }));
			}
		}

		void SetVoyageExchangeRate(JobVoyage voyage, ZString currency, ZDecimal rate)
		{
			VoyageExRate rateToUpdate = null;
			foreach (VoyageExRate voyageExRate in voyage.ExRates)
			{
				if (voyageExRate.E8_RX_NKExCurrency == currency)
				{
					rateToUpdate = voyageExRate;
					break;
				}
			}

			if (rateToUpdate == null)
			{
				rateToUpdate = voyage.ExRates.AddNew();
				rateToUpdate.E8_RX_NKExCurrency = currency;
			}

			rateToUpdate.E8_VoyageExchangeRate = rate;
		}

		void SetCurrentExchangeRate(RefCurrency currency, decimal? exRate)
		{
			SetCurrentExchangeRate(currency, ZDateTime.Today, exRate);
		}

		void SetCurrentExchangeRate(RefCurrency currency, ZDateTime day, decimal? exRate, string exRateType = Constants.ExchangeRateTypes.Code.BuyRate)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			filter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, day.AddDays(1));
			filter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, day);
			var exchangeRates = currency.ExchangeRates.Find(filter);
			foreach (RefExchangeRate rate in exchangeRates)
			{
				rate.Delete();
			}

			if (exRate.HasValue)
			{
				RefExchangeRate newRate = currency.ExchangeRates.AddNew();
				newRate.RE_ExRateType = exRateType;
				newRate.RE_StartDate = day;
				newRate.RE_ExpiryDate = day.AddDays(1);
				newRate.RE_SellRate = exRate.Value;
			}
		}

		void AssertExchangeRate(IJobInvoicingPlugIn parent, string currencyCode, decimal? expectedSellRate, decimal? expectedBuyRate)
		{
			Job job = new Job.Loader(parent).Load();
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(parent.Factory, currencyCode);
			ExchangeRate rate = job.ExchangeRates.FindByRefCurrency(currency);
			decimal? actualSellRate = (rate == null ? null : (decimal?)rate.JF_SellRate);
			decimal? actualBuyRate = (rate == null ? null : (decimal?)rate.JF_BaseRate);
			AssertEquals(string.Format("Expected Sell Rate ({0}, {1}):", parent.JobNumber, currencyCode), expectedSellRate, actualSellRate);
			AssertEquals(string.Format("Expected Buy Rate ({0}, {1}):", parent.JobNumber, currencyCode), expectedBuyRate, actualBuyRate);
		}
	}
}
