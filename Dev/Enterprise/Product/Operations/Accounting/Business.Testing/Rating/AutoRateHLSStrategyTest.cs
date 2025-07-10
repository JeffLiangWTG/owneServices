namespace Enterprise.Accounting.Business.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Core;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.Rating.Business.Testing;
	using Enterprise.ZArchitecture.Schema;

	public class AutoRateHLSStrategyTest : TestCaseWithFactory
	{
		public void TestAddAutoRates_MergesApplicableCharges()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			var freightChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var info1 = new AutoRateInfo(Factory) { ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia, ChargeUnit = Constants.Weight.Kilograms };
			info1.AddFlatPaymentBasis(90m, shipment.RatingAdapter.OperationalJobCode);
			var info2 = new AutoRateInfo(Factory) { ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia };
			info2.AddFlatPaymentBasis(450m, shipment.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory) { info1, info2 };
			var strategy = new AutoRateHLSStrategy(shipment, null);
			var interactor = new LoggerDecorator();
			var result = strategy.AddAutoRates(interactor, collection, CostSell.Cost, new [] { shipment.RatingAdapter.OperationalJobCode });
			AssertEquals("Expected the two infos to be merged into one charge on adding", 1, result.CreatedCharges.Count());
			var mergedCharge = result.CreatedCharges.First().Charge as JobCharge;
			AssertNotNull(mergedCharge);
			AssertEquals(540m, mergedCharge.JR_LocalCostAmt);
		}

		public void TestAddAutoRates_MergesCharges_WhenAutorateRevenue_SavesProviderToCostAccount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			new Job.Loader(shipment).TryCreate();
			var freightChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));

			var provider1 = Factory.NewWithValidTestData<OrgHeader>();
			provider1.OH_IsCreditor = true;
			var info1 = new AutoRateInfo(Factory) { ProviderPK = provider1.PK, IsCost = false, ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia, ChargeUnit = Constants.Weight.Kilograms };
			info1.AddFlatPaymentBasis(90m, shipment.RatingAdapter.OperationalJobCode);
			var info2 = new AutoRateInfo(Factory) { ProviderPK = provider1.PK, IsCost = false, ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia };
			info2.AddFlatPaymentBasis(450m, shipment.RatingAdapter.OperationalJobCode);

			var provider2 = Factory.NewWithValidTestData<OrgHeader>();
			provider2.OH_IsCreditor = true;
			var info3 = new AutoRateInfo(Factory) { ProviderPK = provider2.PK, IsCost = false, ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia, ChargeUnit = Constants.Weight.Kilograms };
			info3.AddFlatPaymentBasis(190m, shipment.RatingAdapter.OperationalJobCode);
			var info4 = new AutoRateInfo(Factory) { ProviderPK = provider2.PK, IsCost = false, ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia };
			info4.AddFlatPaymentBasis(550m, shipment.RatingAdapter.OperationalJobCode);

			var collection = new AutoRateInfoCollection(Factory) { info1, info2, info3, info4 };
			var strategy = new AutoRateHLSStrategy(shipment, null);
			var interactor = new LoggerDecorator();
			var result = strategy.AddAutoRates(interactor, collection, CostSell.Revenue, new[] { shipment.RatingAdapter.OperationalJobCode });

			CombineAssertions(() =>
			{
				AssertEquals("Expected infos 1 and 2 to be merged, and infos 3 and 4 to be merged", 2, result.CreatedCharges.Count());

				var mergedCharges = result.CreatedCharges.Select(x => x.Charge).Cast<JobCharge>().OrderBy(x => x.JR_LocalSellAmt).ToList();

				AssertNotNull("First merged charge", mergedCharges[0]);
				AssertEquals("First merged charge cost", 540m, mergedCharges[0].JR_LocalSellAmt);
				AssertEquals("First merged charge provider", provider1.PK, mergedCharges[0].JR_OH_CostAccount);

				AssertNotNull("second merged charge", mergedCharges[1]);
				AssertEquals("First merged charge cost", 740m, mergedCharges[1].JR_LocalSellAmt);
				AssertEquals("second merged charge provider", provider2.PK, mergedCharges[1].JR_OH_CostAccount);
			});
		}

		public void TestAddAutoRates_WhenChargeCodeIsNull_ShouldNotThrowException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var charge = job.Charges.AddNew();

			charge.JR_E6 = ZGuid.NewZGuid(); // set IsApportioned to true
			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.StopFromAutorating;

			var info = new AutoRateInfo(Factory) { Currency = Constants.CurrencyCodes.Australia };
			var collection = new AutoRateInfoCollection(Factory) { info };
			var strategy = new AutoRateHLSStrategy(shipment, job);
			var interactor = new LoggerDecorator();

			AssertNull(charge.ChargeCode);
			AssertNull(info.ChargeCode);
			AssertNoExceptionThrown(() => strategy.AddAutoRates(interactor, collection, CostSell.Cost, new[] { shipment.RatingAdapter.OperationalJobCode }));
		}
	}
}
