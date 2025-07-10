using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	sealed class DeclarationSourceTest : TestCaseWithFactory
	{
		#region TestParentDescription

		public void TestParentDescription()
		{
			AssertEquals("", Details.ParentDescription);

			Declaration.JE_DeclarationReference = "DecRef";
			AssertEquals("DecRef", Details.ParentDescription);
		}

		#endregion

		#region TestVessel

		public void TestVessel()
		{
			AssertEquals("", Details.Vessel);

			Declaration.JE_VesselName = "vessel";
			AssertEquals("vessel", Details.Vessel);
		}

		#endregion

		#region TestVoyage

		public void TestVoyage()
		{
			AssertEquals("", Details.VoyageFlight);

			Declaration.JE_VoyageFlightNo = "voyage";
			AssertEquals("voyage", Details.VoyageFlight);
		}

		#endregion

		#region TestATD

		public void TestATD()
		{
			ZDateTime now = ZDateTime.Now;
			AssertEquals(ZDateTime.Empty, Details.ATD);

			Declaration.JE_ExportDate = now;
			AssertEquals(now, Details.ATD);
		}

		#endregion

		#region TestATA

		public void TestATA()
		{
			ZDateTime now = ZDateTime.Now;
			AssertEquals(ZDateTime.Empty, Details.ATD);

			Declaration.JE_ExportDate = now;
			AssertEquals(now, Details.ATD);
		}

		#endregion

		#region TestLoad

		public void TestLoad()
		{
			AssertEquals("", Details.Load);

			Declaration.JE_RL_NKPortOfLoading = "LOAD";
			AssertEquals("LOAD", Details.Load);
		}

		#endregion

		#region TestDischarge

		public void TestDischarge()
		{
			AssertEquals("", Details.Discharge);

			Declaration.JE_RL_NKPortOfArrival = "DISCH";
			AssertEquals("DISCH", Details.Discharge);
		}

		#endregion

		#region TestLegOrder

		public void TestLegOrder()
		{
			AssertEquals((byte)0, Details.LegOrder);
		}

		#endregion

		#region Implementation

		#region Declaration

		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
				}
				return declaration;
			}
		}

		BaseJobDeclaration declaration;

		#endregion

		#region Details

		ITransportDetails Details
		{
			get
			{
				if (details == null)
				{
					details = new DeclarationSource(Declaration);
				}
				return details;
			}
		}

		ITransportDetails details;

		#endregion

		#endregion
	}
}
