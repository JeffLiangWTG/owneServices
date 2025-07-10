using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Sailing
{
	[TestedType(typeof(DocVoyage))]
	sealed class DocVoyageTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocVoyage.New(Voyage, Factory)
			};
		}

		public void TestToString()
		{
			AssertEquals("ToString()", ZString.Empty, VoyageWrapper.ToString());
		}

		public void TestCode()
		{
			ZString code = new ZString("Code");
			Voyage.JV_VoyageFlight = code;
			AssertEquals("Code", code, VoyageWrapper.Code);
		}

		public void TestDescription()
		{
			ZString description = new ZString("Description");
			Voyage.JV_RV_NKVessel = description;
			AssertEquals("Description", description, VoyageWrapper.Description);
		}

		public void TestAirSeaRoad()
		{
			ZString airSeaRoad = new ZString("AAA");
			Voyage.JV_AirSeaRoad = airSeaRoad;
			AssertEquals("AirSeaRoad", airSeaRoad, VoyageWrapper.AirSeaRoad);
		}

		public void TestVoyageFlight()
		{
			ZString voyageFlight = new ZString("VoyFlight");
			Voyage.JV_VoyageFlight = voyageFlight;
			AssertEquals("VoyageFlight", voyageFlight, VoyageWrapper.VoyageFlight);
		}

		public void TestLine()
		{
			AssertNull("Line", VoyageWrapper.Line);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Voyage.JV_OH_Line = header.PK;
			AssertNotNull("Line", VoyageWrapper.Line);
			AssertEquals("Line is of type DocOrgranistaion", typeof(DocOrganisation), VoyageWrapper.Line.GetType());
		}

		public void TestNKVessel()
		{
			AssertNull("NKVessel", VoyageWrapper.NKVessel);

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			AssertNotNull("NKVessel", VoyageWrapper.NKVessel);
			AssertEquals("Line is of type DocVessel", typeof(DocVessel), VoyageWrapper.NKVessel.GetType());
		}

		public void TestVesselConsortiumCode()
		{
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			AssertEquals("VesselConsortiumCode", ZString.Empty, VoyageWrapper.VesselConsortiumCode);

			var consortium = Factory.NewWithValidTestData<RefCarrierConsortium>();
			consortium.RG_Code = "VES";
			consortium.OrgHeaders.Add(Factory.LoadTop1<OrgHeader>(new ZQuery()));

			vessel.RV_RG = consortium.PK;
			Voyage.JV_RV_NKVessel = vessel.RV_FK;

			AssertEquals("VesselConsortiumCode", consortium.RG_Code, VoyageWrapper.VesselConsortiumCode);
		}

		#region Implementation

		JobVoyage Voyage;
		DocVoyage VoyageWrapper;

		protected override void SetUp()
		{
			Voyage = Factory.New<JobVoyage>();
			VoyageWrapper = DocVoyage.New(Voyage, Factory);
			base.SetUp();
		}

		#endregion
	}
}
