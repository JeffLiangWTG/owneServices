using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocERATest : TestCaseWithFactory
	{
		public void TestVoyage()
		{
			AssertEquals(Data.Voyage, ERA.VoyageNo);
			Assert(!ERA.VoyageNo.IsEmpty);
		}

		public void TestVesselName()
		{
			AssertEquals(Data.VesselName, ERA.VesselName);
			Assert(!ERA.VesselName.IsEmpty);
		}

		public void TestFinalDestination()
		{
			AssertEquals(Data.PortOfFinalDischarge, ERA.FinalDestination.Code);
		}

		public void TestDischarge()
		{
			AssertEquals(Data.PortOfDischarge, ERA.DischargePort.Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Consol = Factory.New<CommonConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUBNE";
			Consol.JK_RL_NKDischargePort = "USLAX";

			Transport1 = Consol.Transports[0];
			Transport1.JW_RL_NKLoadPort = "AUSYD";
			Transport1.JW_RL_NKDiscPort = "SGSIN";
			Transport1.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			Transport1.JW_VoyageFlight = "1234";

			Transport2 = Consol.Transports[0];
			Transport2.JW_RL_NKLoadPort = "SGSIN";
			Transport2.JW_RL_NKDiscPort = "USSEA";

			Container = Consol.Containers.AddNew();

			ERA = DocERA.New(Container, Factory);
			Data = (IPRAMessagingData)ERA.WrappedObject;
		}

		CommonConsol Consol;
		Transport Transport1;
		Transport Transport2;
		CommonContainer Container;
		DocERA ERA;
		IPRAMessagingData Data;

		#endregion
	}
}
