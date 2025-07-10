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
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;

	public class AutoRateHLSPrintingStrategyTest : TestCaseWithFactory
	{
		public void TestAddAutoRates_MergesApplicableCharges()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();

			Factory.Save();

			var freightChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));

			var autoRateInfo1 = new AutoRateInfo(Factory) { ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia, ChargeUnit = Constants.Weight.Kilograms };
			autoRateInfo1.AddFlatPaymentBasis(1.0034m, shipment.RatingAdapter.OperationalJobCode);
			var autoRateInfo2 = new AutoRateInfo(Factory) { ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia };
			autoRateInfo2.AddFlatPaymentBasis(1.0155, shipment.RatingAdapter.OperationalJobCode);

			var autoRateInfoCollection = new AutoRateInfoCollection(Factory) { autoRateInfo1, autoRateInfo2 };
			var autoRateHSLPrintingStrategy = new AutoRateHLSPrintingStrategy(shipment, null);
			var interactor = new LoggerDecorator();
			var result = autoRateHSLPrintingStrategy.AddAutoRates(interactor, autoRateInfoCollection, CostSell.Cost, new[] { shipment.RatingAdapter.OperationalJobCode });

			Assert("Printing should not change Job", !job.HasChanges);
			AssertEquals("Printing should not change shipment billing", 0, result.CreatedCharges.Count());
			var shipmentCAREvent = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code)).Any();
			AssertEquals("Printing should not create CAR event", false, shipmentCAREvent);

			AssertContainsExactElementsInAnyOrder
			(
				"AutoRatedInfosForJobRevenue should have unmerged AutoRateInfos",
				new[] { 1.00m, 1.02m },
				job.AutoRatedInfosForJobRevenue.Select(autoRateInfo => (decimal)autoRateInfo.Amount)
			);

			AssertContainsExactElementsInAnyOrder
			(
				"AutoRatedInfoGroupByChargeForJobRevenue should have merged AutoRateInfos",
				new[] { 2.02m },
				job.AutoRatedInfoGroupByChargeForJobRevenue.Select(autoRateInfo => (decimal)autoRateInfo.Amount)
			);
		}

		public void TestAddAutoRates_WhenChargeCodeIsNull_ShouldNotThrowException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var charge = job.Charges.AddNew();

			charge.JR_E6 = ZGuid.NewZGuid(); // set IsApportioned to true
			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.StopFromAutorating;

			var autoRateInfo = new AutoRateInfo(Factory) { Currency = Constants.CurrencyCodes.Australia };
			var autoRateInfoCollection = new AutoRateInfoCollection(Factory) { autoRateInfo };
			var autoRateHLSPrintingStrategy = new AutoRateHLSPrintingStrategy(shipment, job);
			var interactor = new LoggerDecorator();

			AssertNull(charge.ChargeCode);
			AssertNull(autoRateInfo.ChargeCode);
			AssertNoExceptionThrown(() => autoRateHLSPrintingStrategy.AddAutoRates(interactor, autoRateInfoCollection, CostSell.Cost, new[] { shipment.RatingAdapter.OperationalJobCode }));
		}
	}
}
