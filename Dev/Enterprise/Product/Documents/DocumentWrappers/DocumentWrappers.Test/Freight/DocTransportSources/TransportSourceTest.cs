using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class TransportSourceTest : TestCaseWithFactory
	{
		#region TestParentDescription

		public void TestParentDescription()
		{
			AssertEquals("", Details.ParentDescription);

			Consol.JK_UniqueConsignRef = "UCREF";
			AssertEquals("UCREF", Details.ParentDescription);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(Core.Constants.TransportModes.Sea, Details.TransportMode);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(Core.Constants.TransportModes.Air, Details.TransportMode);
		}

		#endregion

		#region TestTransportType

		public void TestTransportType()
		{
			Transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			AssertEquals(Core.Constants.TransportPlanningType.MainVessel, Details.TransportType);

			Transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			AssertEquals(Core.Constants.TransportPlanningType.OnForwarding, Details.TransportType);
		}

		#endregion

		#region TestTransportTypeDescription

		public void TestTransportTypeDescription()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			Transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			AssertEquals("Main Vessel", Details.TransportTypeDescription);

			Transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			AssertEquals("On-forwarding Vessel", Details.TransportTypeDescription);
		}

		#endregion

		#region TestVessel

		public void TestVessel()
		{
			AssertEquals("", Details.Vessel);

			Transport.JW_Vessel = "AOEU";
			AssertEquals("AOEU", Details.Vessel);
		}

		#endregion

		#region TestVoyageFlight

		public void TestVoyageFlight()
		{
			AssertEquals("", Details.VoyageFlight);

			Transport.JW_VoyageFlight = "SNTH";
			AssertEquals("SNTH", Details.VoyageFlight);
		}

		#endregion

		#region TestLoad

		public void TestLoad()
		{
			AssertEquals("", Details.Load);

			Transport.JW_RL_NKLoadPort = "NLAMS";
			AssertEquals("NLAMS", Details.Load);
		}

		#endregion

		#region TestDischarge

		public void TestDischarge()
		{
			AssertEquals("", Details.Discharge);

			Transport.JW_RL_NKDiscPort = "NLAMS";
			AssertEquals("NLAMS", Details.Discharge);
		}

		#endregion

		#region TestLegOrder

		public void TestLegOrder()
		{
			Transport.JW_LegOrder = 1;
			AssertEquals(1, (int)Details.LegOrder);

			Transport.JW_LegOrder = 2;
			AssertEquals(2, (int)Details.LegOrder);
		}

		#endregion

		#region TestETD

		public void TestETD()
		{
			AssertEquals(ZDateTime.Empty, Details.ETD);

			Transport.JW_ETD = ZDateTime.Now;
			AssertEquals(Transport.JW_ETD, Details.ETD);
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			AssertEquals(ZDateTime.Empty, Details.ETA);

			Transport.JW_ETA = ZDateTime.Now;
			AssertEquals(Transport.JW_ETA, Details.ETA);
		}

		#endregion

		#region TestATD

		public void TestATD()
		{
			AssertEquals(ZDateTime.Empty, Details.ATD);

			Transport.JW_ATD = ZDateTime.Now;
			AssertEquals(Transport.JW_ATD, Details.ATD);
		}

		#endregion

		#region TestATA

		public void TestATA()
		{
			AssertEquals(ZDateTime.Empty, Details.ATA);

			Transport.JW_ATA = ZDateTime.Now;
			AssertEquals(Transport.JW_ATA, Details.ATA);
		}

		#endregion

		#region TestLCLReceivalCommences

		public void TestLCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLReceivalCommences);

			Transport.JW_DepotReceivalCommences = ZDateTime.Now;
			AssertEquals(Transport.JW_DepotReceivalCommences, Details.LCLReceivalCommences);
		}

		#endregion

		#region TestLCLCutOff

		public void TestLCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLCutOff);

			Transport.JW_DepotCutOff = ZDateTime.Now;
			AssertEquals(Transport.JW_DepotCutOff, Details.LCLCutOff);
		}

		#endregion

		#region TestLCLAvailabilityDate

		public void TestLCLAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLAvailabilityDate);

			Transport.JW_DepotAvailabilityDate = ZDateTime.Now;
			AssertEquals(Transport.JW_DepotAvailabilityDate, Details.LCLAvailabilityDate);
		}

		#endregion

		#region TestLCLStorageDate

		public void TestLCLStorageDate()
		{
			AssertEquals(ZDateTime.Empty, Details.LCLStorageDate);

			Transport.JW_DepotStorageDate = ZDateTime.Now;
			AssertEquals(Transport.JW_DepotStorageDate, Details.LCLStorageDate);
		}

		#endregion

		#region TestFCLReceivalCommences

		public void TestFCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLReceivalCommences);

			Transport.JW_TerminalReceivalCommences = ZDateTime.Now;
			AssertEquals(Transport.JW_TerminalReceivalCommences, Details.FCLReceivalCommences);
		}

		#endregion

		#region TestFCLCutOff

		public void TestFCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLCutOff);

			Transport.JW_TerminalCutOff = ZDateTime.Now;
			AssertEquals(Transport.JW_TerminalCutOff, details.FCLCutOff);
		}

		#endregion

		#region TestFCLAvailabilityDate

		public void TestFCLAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLAvailabilityDate);

			Transport.JW_TerminalAvailabilityDate = ZDateTime.Now;
			AssertEquals(Transport.JW_TerminalAvailabilityDate, Details.FCLAvailabilityDate);
		}

		#endregion

		#region TestFCLStorageDate

		public void TestFCLStorageDate()
		{
			AssertEquals(ZDateTime.Empty, Details.FCLStorageDate);

			Transport.JW_TerminalStorageDate = ZDateTime.Now;
			AssertEquals(Transport.JW_TerminalStorageDate, Details.FCLStorageDate);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			AssertEquals(ZGuid.Empty, Details.Carrier);

			OrgHeader org = Factory.New<OrgHeader>();
			Transport.CarrierPK = org.PK;
			AssertEquals(org.PK, Details.Carrier);
		}

		#endregion

		#region Implementation

		#region Consol

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<CommonConsol>();
				}
				return consol;
			}
		}

		CommonConsol consol;

		#endregion

		#region Transport

		Transport Transport
		{
			get
			{
				if (transport == null)
				{
					transport = Consol.Transports[0];
				}
				return transport;
			}
		}

		Transport transport;

		#endregion

		#region Details

		ITransportDetails Details
		{
			get
			{
				if (details == null)
				{
					details = new TransportSource(Transport);
				}
				return details;
			}
		}

		ITransportDetails details;

		#endregion

		#endregion
	}
}
