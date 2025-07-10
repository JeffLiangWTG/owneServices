using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Sailing
{
	[TestedType(typeof(DocVoyageDestination))]
	sealed class DocVoyageDestinationTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocVoyageDestination.New(Destination, Factory),
				DocVoyageDestination.New(Destination.Factory, Destination.PK)
			};
		}

		public void TestActualARV()
		{
			ZDateTime actualARV = new ZDateTime(2004, 04, 04);
			Destination.JB_A_ARV = actualARV;
			AssertEquals("ActualARV", actualARV, DestinationWrapper.ActualARV);
		}

		public void TestEstimatedARV()
		{
			ZDateTime estimatedARV = new ZDateTime(2004, 04, 04);
			Destination.JB_E_ARV = estimatedARV;
			AssertEquals("EstimatedARV", estimatedARV, DestinationWrapper.EstimatedARV);
		}

		public void TestIsTranshipment()
		{
			Destination.JB_IsTranshipment = ZBool.False;
			Assert("!IsTranshipment", !DestinationWrapper.IsTranshipment);

			Destination.JB_IsTranshipment = ZBool.True;
			Assert("IsTranshipment", DestinationWrapper.IsTranshipment);
		}

		public void TestVoyage()
		{
			AssertNull("Voyage", DestinationWrapper.Voyage);

			var voyage = Factory.New<JobVoyage>();
			Destination.JB_JV = voyage.PK;
			DestinationWrapper = DocVoyageDestination.New(Destination, Factory);
			AssertNotNull("Voyage", DestinationWrapper.Voyage);
			AssertEquals("Voyage is of type DocVoyage", typeof(DocVoyage), DestinationWrapper.Voyage.GetType());
		}

		public void TestNKPortOfDischarge()
		{
			AssertNull("NKPortOfLoading", DestinationWrapper.NKPortOfDischarge);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Destination.JB_RL_NKPortOfDischarge = uNLOCO.RL_Code;
			DestinationWrapper = DocVoyageDestination.New(Destination, Factory);
			AssertNotNull("UNLOCO", DestinationWrapper.NKPortOfDischarge);
			AssertEquals("UNLOCO is of type DocUNLOCO", typeof(DocUNLOCO), DestinationWrapper.NKPortOfDischarge.GetType());
		}

		#region Implementation

		VoyageDestination Destination;
		DocVoyageDestination DestinationWrapper;

		protected override void SetUp()
		{
			Destination = Factory.New<VoyageDestination>();
			DestinationWrapper = DocVoyageDestination.New(Destination, Factory);
			base.SetUp();
		}

		#endregion
	}
}
