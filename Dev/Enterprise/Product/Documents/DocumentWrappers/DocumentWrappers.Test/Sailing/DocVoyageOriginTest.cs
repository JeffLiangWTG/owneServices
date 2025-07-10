using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Sailing
{
	[TestedType(typeof(DocVoyageOrigin))]
	sealed class DocVoyageOriginTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocVoyageOrigin.New(Origin, Factory)
			};
		}

		public void TestToString()
		{
			AssertEquals("ToString()", ZString.Empty, OriginWrapper.ToString());
		}

		public void TestActualDEP()
		{
			ZDateTime actualDEP = new ZDateTime(2004, 04, 04);
			Origin.JA_A_DEP = actualDEP;
			AssertEquals("ActualDEP", actualDEP, OriginWrapper.ActualDEP);
		}

		public void TestEstimatedDEP()
		{
			ZDateTime estimatedDEP = new ZDateTime(2004, 04, 04);
			Origin.JA_E_DEP = estimatedDEP;
			AssertEquals("EstimatedDEP", estimatedDEP, OriginWrapper.EstimatedDEP);
		}

		public void TestVoyage()
		{
			AssertNull("Voyage", OriginWrapper.Voyage);

			var voyage = Factory.New<JobVoyage>();
			Origin.JA_JV = voyage.PK;
			OriginWrapper = DocVoyageOrigin.New(Origin, Factory);
			AssertNotNull("Voyage", OriginWrapper.Voyage);
			AssertEquals("Voyage is of type DocVoyage", typeof(DocVoyage), OriginWrapper.Voyage.GetType());
		}

		public void TestNKPortOfLoading()
		{
			AssertNull("NKPortOfLoading", OriginWrapper.NKPortOfLoading);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Origin.JA_RL_NKPortOfLoading = uNLOCO.RL_Code;
			OriginWrapper = DocVoyageOrigin.New(Origin, Factory);
			AssertNotNull("UNLOCO", OriginWrapper.NKPortOfLoading);
			AssertEquals("UNLOCO is of type DocUNLOCO", typeof(DocUNLOCO), OriginWrapper.NKPortOfLoading.GetType());
		}

		public void TestBerth()
		{
			AssertEquals("No Berth", "", OriginWrapper.Berth);
			Origin.JA_Berth = "BERTH A";
			AssertEquals("Berth A", "BERTH A", OriginWrapper.Berth);
		}

		#region Implementation

		VoyageOrigin Origin;
		DocVoyageOrigin OriginWrapper;
		protected override void SetUp()
		{
			Origin = Factory.New<VoyageOrigin>();
			OriginWrapper = DocVoyageOrigin.New(Origin, Factory);
			base.SetUp();
		}

		#endregion
	}
}
