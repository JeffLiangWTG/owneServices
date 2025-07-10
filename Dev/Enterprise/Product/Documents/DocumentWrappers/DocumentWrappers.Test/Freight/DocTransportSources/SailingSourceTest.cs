using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class SailingSourceTest : TestCaseWithFactory
	{
		#region TestParentDescription

		public void TestParentDescription()
		{
			AssertEquals("", Details.ParentDescription);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			Sailing.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			AssertEquals(Core.Constants.TransportModes.Sea, Details.TransportMode);

			Sailing.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModes.Air, Details.TransportMode);
		}

		#endregion

		#region TestTransportType

		public void TestTransportType()
		{
			AssertEquals("", Details.TransportType);
		}

		#endregion

		#region TestTransportTypeDescription

		public void TestTransportTypeDescription()
		{
			AssertEquals("", Details.TransportTypeDescription);
		}

		#endregion

		#region TestVessel

		public void TestVessel()
		{
			AssertEquals("", Details.Vessel);

			Sailing.Voyage.JV_RV_NKVessel = "AOEU";
			AssertEquals("AOEU", Details.Vessel);
		}

		#endregion

		#region TestVoyageFlight

		public void TestVoyageFlight()
		{
			AssertEquals("", Details.VoyageFlight);

			Sailing.Voyage.JV_VoyageFlight = "SNTH";
			AssertEquals("SNTH", Details.VoyageFlight);
		}

		#endregion

		#region TestLoad

		public void TestLoad()
		{
			AssertEquals("AUBNE", Details.Load);

			Sailing.Origin.JA_RL_NKPortOfLoading = "NLAMS";
			AssertEquals("NLAMS", Details.Load);
		}

		#endregion

		#region TestDischarge

		public void TestDischarge()
		{
			AssertEquals("SGSIN", Details.Discharge);

			Sailing.Destination.JB_RL_NKPortOfDischarge = "NLAMS";
			AssertEquals("NLAMS", Details.Discharge);
		}

		#endregion

		#region TestLegOrder

		public void TestLegOrder()
		{
			AssertEquals(0, (int)Details.LegOrder);
		}

		#endregion

		#region TestETD

		public void TestETD()
		{
			AssertEquals(ZDateTime.Empty, Details.ETD);

			Sailing.Origin.JA_E_DEP = ZDateTime.Now;
			AssertEquals(Sailing.Origin.JA_E_DEP, Details.ETD);
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			AssertEquals(ZDateTime.Empty, Details.ETA);

			Sailing.Destination.JB_E_ARV = ZDateTime.Now;
			AssertEquals(Sailing.Destination.JB_E_ARV, Details.ETA);
		}

		#endregion

		#region TestATD

		public void TestATD()
		{
			AssertEquals(ZDateTime.Empty, Details.ATD);

			Sailing.Origin.JA_A_DEP = ZDateTime.Now;
			AssertEquals(Sailing.Origin.JA_A_DEP, Details.ATD);
		}

		#endregion

		#region TestATA

		public void TestATA()
		{
			AssertEquals(ZDateTime.Empty, Details.ATA);

			Sailing.Destination.JB_A_ARV = ZDateTime.Now;
			AssertEquals(Sailing.Destination.JB_A_ARV, Details.ATA);
		}

		#endregion

		#region TestLCLReceivalCommences

		public void TestLCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLReceivalCommences);

			Sailing.JX_DepotReceivalCommences = ZDateTime.Now;
			AssertEquals(Sailing.JX_DepotReceivalCommences, Details.LCLReceivalCommences);
		}

		#endregion

		#region TestLCLCutOff

		public void TestLCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLCutOff);

			Sailing.JX_DepotCutOff = ZDateTime.Now;
			AssertEquals(Sailing.JX_DepotCutOff, Details.LCLCutOff);
		}

		#endregion

		#region TestLCLAvailabilityDate

		public void TestLCLAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLAvailabilityDate);

			Sailing.JX_DepotAvailabilityDate = ZDateTime.Now;
			AssertEquals(Sailing.JX_DepotAvailabilityDate, Details.LCLAvailabilityDate);
		}

		#endregion

		#region TestLCLStorageDate

		public void TestLCLStorageDate()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLStorageDate);

			Sailing.JX_DepotStorageDate = ZDateTime.Now;
			AssertEquals(Sailing.JX_DepotStorageDate, Details.LCLStorageDate);
		}

		#endregion

		#region TestFCLReceivalCommences

		public void TestFCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLReceivalCommences);

			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Now;
			AssertEquals(Sailing.Origin.JA_ReceivalCommences, Details.FCLReceivalCommences);
		}

		#endregion

		#region TestFCLCutOff

		public void TestFCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLCutOff);

			Sailing.Origin.JA_CutOff = ZDateTime.Now;
			AssertEquals(Sailing.Origin.JA_CutOff, details.FCLCutOff);
		}

		#endregion

		#region TestFCLAvailabilityDate

		public void TestFCLAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLAvailabilityDate);

			Sailing.Destination.JB_AvailabilityDate = ZDateTime.Now;
			AssertEquals(Sailing.Destination.JB_AvailabilityDate, Details.FCLAvailabilityDate);
		}

		#endregion

		#region TestFCLStorageDate

		public void TestFCLStorageDate()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLStorageDate);

			Sailing.Destination.JB_StorageDate = ZDateTime.Now;
			AssertEquals(Sailing.Destination.JB_StorageDate, Details.FCLStorageDate);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			AssertEquals(ZGuid.Empty, Details.Carrier);

			OrgHeader org = Factory.New<OrgHeader>();
			Sailing.Voyage.JV_OH_Line = org.PK;
			AssertEquals(org.PK, Details.Carrier);
		}

		#endregion

		#region Implementation

		#region Sailing

		JobSailing Sailing
		{
			get
			{
				if (sailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
					voyage.GenerateSailings();
					sailing = voyage.Sailings[0];
				}
				return sailing;
			}
		}

		JobSailing sailing;

		#endregion

		#region Details

		ITransportDetails Details
		{
			get
			{
				if (details == null)
				{
					details = new SailingSource(Sailing);
				}
				return details;
			}
		}

		ITransportDetails details;

		#endregion

		#endregion
	}
}
