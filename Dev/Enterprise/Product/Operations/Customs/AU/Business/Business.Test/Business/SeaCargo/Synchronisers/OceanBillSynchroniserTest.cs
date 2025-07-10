using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OceanBillSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniser()
		{
			var consol = CreateFCLConsol();
			var transport = consol.Transports[0];

			var oceanBill = Factory.New<CusSCAOceanBill>();

			var now = ZDateTime.Now;

			var synchroniser = new OceanBillSynchroniser(oceanBill, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			consol.JK_MasterBillNum = TestOceanBill;
			AssertEquals("OceanBill CB_OceanBill", TestOceanBill, oceanBill.CB_OceanBill);

			transport.JW_Vessel = TestRV_NKVessel;
			AssertEquals("OceanBill CB_VesselName", TestRV_NKVessel, oceanBill.CB_VesselName);

			transport.JW_VoyageFlight = TestVoyage;
			AssertEquals("OceanBill CB_Voyage", TestVoyage, oceanBill.CB_Voyage);

			transport.JW_ETD = now;
			AssertEquals("OceanBill CB_DateOfDeparture", now, oceanBill.CB_DateOfDeparture);

			transport.JW_ETA = now.AddDays(1);
			AssertEquals("OceanBill CB_DateOfArrival", now.AddDays(1), oceanBill.CB_DateOfArrival);

			transport.JW_RL_NKLoadPort = TestRL_NKPortOfLoading;
			AssertEquals("OceanBill CB_RL_NKPortOfLoading", TestRL_NKPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);

			consol.JK_RL_NKLoadPort = TestRL_NKPortOfLoading;
			transport.JW_RL_NKLoadPort = "";
			AssertEquals("OceanBill CB_RL_NKPortOfLoading", "", oceanBill.CB_RL_NKPortOfLoading);

			oceanBill.CB_RL_NKPortOfDischarge = "";
			consol.JK_RL_NKDischargePort = TestRL_NKPortOfDischarge;
			AssertEquals("OceanBill CB_RL_NKPortOfDischarge", TestRL_NKPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);

			consol.JK_RL_NKDischargePort = "";
			oceanBill.CB_RL_NKPortOfDischarge = "";
			transport.JW_RL_NKLoadPort = TestRL_NKPortOfLoading;
			transport.JW_RL_NKDiscPort = TestRL_NKPortOfDischarge;
			AssertEquals("OceanBill CB_RL_NKPortOfDischarge", TestRL_NKPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);

			consol.SetDefaultShippingLineAddress(TestOH_ShippingLine);
			AssertEquals("OceanBill CB_OH_ShippingLine", TestOH_ShippingLine, oceanBill.CB_OH_ShippingLine);

			oceanBill.CB_RL_NKPortOfLoading = "";
			transport.JW_RL_NKLoadPort = "ITSPE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals("OceanBill CB_RL_NKPortOfLoading", TestRL_NKPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "AUBNE";
			AssertEquals("OceanBill CB_RL_NKPortOfLoading", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);

			transport2.JW_RL_NKDiscPort = "NZABL";
			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "NZABL";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals("OceanBill CB_RL_NKPortOfLoading", "NZABL", oceanBill.CB_RL_NKPortOfLoading);
		}

		public void TestOceanBillContainers()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();

			OceanBillSynchroniser synchroniser = new OceanBillSynchroniser(oceanBill, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			CommonContainer newContainer1 = consol.Containers.AddNew();
			AssertEquals("OceanBill Container Count", 1, oceanBill.Containers.Count);
			CommonContainer newContainer2 = consol.Containers.AddNew();
			AssertEquals("OceanBill Container Count", 2, oceanBill.Containers.Count);
			consol.Containers.Remove(newContainer1);
			AssertEquals("OceanBill Container Count", 1, oceanBill.Containers.Count);
		}

		public void TestSynchroniserReadOnlyState()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			OceanBillSynchroniser synchroniser = new OceanBillSynchroniser(oceanBill, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			Assert(!oceanBill.OverrideFreightDefaults);
			AssertEquals("ReadOnly CB_OceanBillInfo", true, oceanBill.CB_OceanBillInfo.ReadOnly);
			AssertEquals("ReadOnly CB_VesselNameInfo", true, oceanBill.CB_VesselNameInfo.ReadOnly);
			AssertEquals("ReadOnly CB_VoyageInfo", true, oceanBill.CB_VoyageInfo.ReadOnly);
			AssertEquals("ReadOnly CB_RL_NKPortOfLoadingInfo", true, oceanBill.CB_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals("ReadOnly CB_RL_NKPortOfDischargeInfo", true, oceanBill.CB_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals("ReadOnly CB_OH_ShippingLineInfo", true, oceanBill.CB_OH_ShippingLineInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals("ReadOnly CB_OceanBillInfo", false, oceanBill.CB_OceanBillInfo.ReadOnly);
			AssertEquals("ReadOnly CB_VesselNameInfo", false, oceanBill.CB_VesselNameInfo.ReadOnly);
			AssertEquals("ReadOnly CB_VoyageInfo", false, oceanBill.CB_VoyageInfo.ReadOnly);
			AssertEquals("ReadOnly CB_RL_NKPortOfLoadingInfo", false, oceanBill.CB_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals("ReadOnly CB_RL_NKPortOfDischargeInfo", false, oceanBill.CB_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals("ReadOnly CB_OH_ShippingLineInfo", false, oceanBill.CB_OH_ShippingLineInfo.ReadOnly);
		}

		public void TestDeletingContainerNotInCollection()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();

			OceanBillSynchroniser synchroniser = new OceanBillSynchroniser(oceanBill, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			CommonContainer newContainer1 = consol.Containers.AddNew();
			AssertEquals("OceanBill Container Count", 1, oceanBill.Containers.Count);
			CusSCAContainer sCAContainer = oceanBill.Containers[0];
			oceanBill.Containers.Remove(sCAContainer);
			newContainer1.Delete();
			AssertEquals("Container should still be deleted", true, sCAContainer.IsDeleted);
		}
	}
}
