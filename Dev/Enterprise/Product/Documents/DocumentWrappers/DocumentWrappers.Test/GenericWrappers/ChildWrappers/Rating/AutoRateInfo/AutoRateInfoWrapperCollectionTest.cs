using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Test
{
	[TestedType(typeof(AutoRateInfoWrapperCollection))]
	sealed class AutoRateInfoWrapperCollectionTest : GenericWrapperCollectionTest<AutoRateInfoWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var autoRateInfo = new AutoRateInfo(Factory) { InvoiceLineDescription = "TestInvoiceLineDescription" };
			autoRateInfo.AddFlatPaymentBasis(100m, "S00001234", "AUD");
			return new AutoRateInfoWrapper(autoRateInfo, Factory);
		}

		protected override AutoRateInfoWrapperCollection GetNewDocumentWrapperCollection()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var collection = new AutoRateInfoWrapperCollection(job, Factory);
			collection.Add(GetNewWrapperToAddToTheCollection());
			return collection;
		}

		public void TestAutoRateInfoWrapperCollection()
		{
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_IsDebtor = true;

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			clientRate.TH_OH = localClient.PK;
			var rateEntry = clientRate.AddRateEntry("AIR", "LSE", "NZAKL", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("BAF");
			rateLine1.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateLine1.Calculator).BaseRate = 60m;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment.JS_ActualWeight = 20m;
			shipment.ConsignorPK = localClient.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = localClient.MainAddress.PK;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_ActualWeight = 15m;
			consignment.HVC_WeightUQ = Constants.Weight.Kilograms;

			var item1 = consignment.Items.AddNew();
			item1.HVI_ActualWeight = 10m;
			var item2 = consignment.Items.AddNew();
			item2.HVI_ActualWeight = 5m;

			var job = new Job.Loader(shipment).TryCreate();
			job.PlugInData = shipment;
			job.LocalChargesPK = localClient.PK;

			Factory.Save();

			var collection = new AutoRateInfoWrapperCollection(job, Factory);
			AssertEquals(2, collection.Count);

			var wrapper = collection[0];
			AssertEquals(60m, wrapper.Amount);
			AssertEquals("HVLV Item HVI000000000000001", wrapper.AutoRatedForString);
			AssertEquals("Base Rate NZD 60.00", wrapper.CalculationSingleLineDescriptionWithoutChargeCode);
			AssertEquals("Bunker Adjustment Factor", wrapper.InvoiceLineDescription);
		}
	}
}
