using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestVessels()
		{
			header.CB_VesselName = "Vessel1";
			header.CB_LloydsIMO = "Test1";
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

			header.CB_VesselName = "Vessel1";
			AssertEquals(1, lookups.LloydsIMOList.Count);
			AssertEquals("Number1", lookups.LloydsIMOList.CodesAsString);
		}

		public void TestShippingLines()
		{
			AssertEquals("ShippingLinesType", typeof(ShippingProviderCollection), new CusSCAOceanBillLookups(Factory.New<CusSCAOceanBill>()).ShippingLines.GetType());
		}

		public void TestApplicationCodeList()
		{
			Assert("Application Code List contains CMR Item", new CusSCAOceanBillLookups(Factory.New<CusSCAOceanBill>()).ApplicationCodeList.ContainsCode(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusSCAOceanBill>();
			lookups = header.Lookups;
		}

		CusSCAOceanBill header;
		CusSCAOceanBillLookups lookups;
	}
}
