using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoProcessorJobForConsolTest : TestCaseWithFactory
	{
		public void TestSendChildren()
		{
			var consol = Factory.New<ForwardingConsol>();
			Assert(new AirCargoProcessorJobForConsol(consol).SendChildren);
		}

		public void TestNewProcessorJobWithNewConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.FillWithValidTestData();
			consol.MasterBillMAWB = "mawb123";
			consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.FillWithValidTestData();
			shipment1.JS_HouseBill = "hawb1";
			shipment1.JS_RL_NKOrigin = "GBLHR";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_INCO = Core.Constants.IncoTerms.DeliveredExShip;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.FillWithValidTestData();
			shipment2.JS_HouseBill = "hawb2";
			shipment2.JS_RL_NKOrigin = "GBLHR";
			shipment2.JS_RL_NKDestination = "AUSYD";
			shipment2.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;

			Factory.Save();

			using (var processorJob = new AirCargoProcessorJobForConsol(consol))
			{
				AssertNotNull(processorJob.MasterBill);
				AssertEquals(2, processorJob.MasterBill.ChildBills.Count);
				AssertEquals("should be PO", "PO", processorJob.MasterBill.ChildBills[0].CS_FreightPrepaidCollect);
				AssertEquals("should be CC", "CC", processorJob.MasterBill.ChildBills[1].CS_FreightPrepaidCollect);
			}
		}

		public void TestNewProcessorJobWithExistingConsolAndNewShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.FillWithValidTestData();
			consol.MasterBillMAWB = "mawb123";
			consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consol.JK_PrepaidCollect = Core.Constants.AWB.PPDCollect.Collect;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.FillWithValidTestData();
			shipment1.JS_HouseBill = "hawb1";
			shipment1.JS_RL_NKOrigin = "GBLHR";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_INCO = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			Factory.Save();

			int hawbsInDb = Factory.GetDatabaseCount(typeof(CusHAWB));

			using (var processorJob = new AirCargoProcessorJobForConsol(consol))
			{
				Factory.Save();
				AssertEquals(hawbsInDb + 1, Factory.GetDatabaseCount(typeof(CusHAWB)));
				AssertNotNull(processorJob.MasterBill);
				AssertEquals(1, processorJob.MasterBill.ChildBills.Count);
				AssertEquals("should be PO", "PO", processorJob.MasterBill.ChildBills[0].CS_FreightPrepaidCollect);
			}

			var shipment2 = consol.Shipments.AddNew();
			shipment2.FillWithValidTestData();
			shipment2.JS_HouseBill = "hawb2";
			shipment2.JS_RL_NKOrigin = "GBLHR";
			shipment2.JS_RL_NKDestination = "AUSYD";
			Factory.Save();

			using (var processorJob2 = new AirCargoProcessorJobForConsol(consol))
			{
				Factory.Save();
				AssertEquals(hawbsInDb + 2, Factory.GetDatabaseCount(typeof(CusHAWB)));
				AssertNotNull(processorJob2.MasterBill);
				AssertEquals(2, processorJob2.MasterBill.ChildBills.Count);
			}
		}

		public void TestHasErrors()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.FillWithValidTestData();
			consol.MasterBillMAWB = "mawb123";
			consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.JS_HouseBill = "hawb1";
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "AUSYD";

			Factory.Save();

			using (var processorJob = new AirCargoProcessorJobForConsol(consol))
			{
				Factory.Save();
				processorJob.MasterBill.RunPreSaveValidation();
				AssertEquals(processorJob.MasterBill.HasMessageErrors, !processorJob.IsAcceptable);
			}
		}

		public void TestOnlyOneMasterBillCreated()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.FillWithValidTestData();
			consol.MasterBillMAWB = "mawb123";
			consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.JS_HouseBill = "hawb1";
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "AUSYD";

			Factory.Save();

			var consolInAnotherFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

			using (var processorJob1 = new AirCargoProcessorJobForConsol(consol))
			using (var processorJob2 = new AirCargoProcessorJobForConsol(consolInAnotherFactory))
			{
				AssertNotNull("processorJob1 creates the MasterBill.", processorJob1.MasterBill);
				AssertEquals("processorJob2 finds the MasterBill.", processorJob1.MasterBill.PK, processorJob2.MasterBill.PK);
			}
		}

		public void TestOnlyOneHouseBillPerShipmentCreated()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.FillWithValidTestData();
			consol.MasterBillMAWB = "mawb123";
			consol.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.JS_HouseBill = "hawb1";
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "AUSYD";

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;

			Factory.Save();

			var consolInAnotherFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

			using (var processorJob1 = new AirCargoProcessorJobForConsol(consol))
			using (var processorJob2 = new AirCargoProcessorJobForConsol(consolInAnotherFactory))
			{
				AssertEquals(mawb.PK, processorJob1.MasterBill.PK);
				AssertEquals(mawb.PK, processorJob2.MasterBill.PK);

				AssertEquals(1, processorJob1.MasterBill.ChildBills.Count);
				AssertEquals(1, processorJob2.MasterBill.ChildBills.Count);
				AssertEquals(processorJob1.MasterBill.ChildBills[0].PK, processorJob2.MasterBill.ChildBills[0].PK);
			}
		}
	}
}
