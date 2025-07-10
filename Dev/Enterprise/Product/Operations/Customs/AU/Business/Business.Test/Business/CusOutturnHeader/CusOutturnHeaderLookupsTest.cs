using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderLookupsTest : Customs.Business.Testing.CusOutturnHeaderLookupsTest
	{
		public void TestVessels()
		{
			header.C6_VesselName = "Vessel1";
			header.C6_LloydsIMO = "Test1";
			var vessels = lookups.VesselNames;
			AssertEquals("Vessel1", vessels.FilterBusinessObjectDefaults["Vessel Name:Property"].Value);
			AssertEquals("Test1", vessels.FilterBusinessObjectDefaults["Lloyds Number:Property"].Value);
		}

		public void TestLloydsIMOList()
		{
			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "Vessel1";
			refVessel.RV_LloydsNumber = "Number1";
			Factory.Save();
			AssertEquals(0, lookups.LloydsIMOList.Count);

			header.C6_VesselName = "Vessel1";
			AssertEquals(1, lookups.LloydsIMOList.Count);
			AssertEquals("Number1", lookups.LloydsIMOList.CodesAsString);
		}

		public void TestCTOAddressOrgs()
		{
			AssertNotNull("nullness", lookups.CTOAddressOrgs);
			AssertEquals("type", typeof(SeaCTOAndDepotCollection), lookups.CTOAddressOrgs.GetType());
		}

		public void TestOutturnStatusList()
		{
			AssertNotNull("nullness", lookups.OutturnStatusList);
			AssertEquals("type", typeof(CMRBaseStatuses), lookups.OutturnStatusList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = CusOutturnHeader.New(Factory);
			lookups = header.Lookups;
		}

		CusOutturnHeader header;
		CusOutturnHeaderLookups lookups;
	}
}
