using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class AutoRatePrintingStrategyTest : TestCaseWithFactory
	{
		public void TestAddAutoRates_ReturnsResultButDoesNotChangeParentJob()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			var shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			var freightChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var info1 = new AutoRateInfo(Factory) { ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia, ChargeUnit = Constants.Weight.Kilograms };
			info1.AddFlatPaymentBasis(90m, shipment.RatingAdapter.OperationalJobCode);
			var info2 = new AutoRateInfo(Factory) { ChargeCode = freightChargeCode, Currency = Constants.CurrencyCodes.Australia };
			info2.AddFlatPaymentBasis(450m, shipment.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory)
			{ info1, info2 };
			var strategy = new AutoRatePrintingStrategy(shipment, null);
			var interactor = new LoggerDecorator();
			var result = strategy.AddAutoRates(interactor, collection, CostSell.Cost, new [] { shipment.RatingAdapter.OperationalJobCode });
			AssertEquals("Expected each info to appear on the job", 2, shipmentJob.AutoRatedInfosForJobRevenue.Count);
			Assert("Job should be unchanged", !shipmentJob.HasChanges);
			AssertEquals("Expected no charges to be returned as Printing should not change parent billing tab", 0, result.CreatedCharges.Count());
			var carEventsOnShipment = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TransactionApprovalActioned.Code)).Any();
			AssertEquals("As no billing charges were added no CAR event should have been generated on the shipment", false, carEventsOnShipment);
		}
	}
}
