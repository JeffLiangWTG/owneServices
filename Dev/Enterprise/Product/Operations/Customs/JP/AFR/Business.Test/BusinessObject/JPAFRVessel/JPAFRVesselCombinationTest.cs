using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRVesselCombinationTest : TestCaseWithFactory
	{
		public void TestDefaultCallSignAndNationalityIfNeeded()
		{
			header.VesselCombination.DefaultCallSignAndNationalityIfNeeded("VSSL2");
			AssertEquals("RCS2", header.JPH_RadioCallSign);
			AssertEquals("GB", header.JPH_RN_NKCountryOfReg);
		}

		public void TestOnVesselSelected()
		{
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, "VSSL1"));
			header.VesselCombination.OnVesselSelected(vessel);
			AssertEquals("VSSL1", header.JPH_VesselName);
			AssertEquals("RCS1", header.JPH_RadioCallSign);
			AssertEquals("AU", header.JPH_RN_NKCountryOfReg);
		}

		public void TestVessel()
		{
			header.JPH_VesselName = "VSSL1";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, "VSSL1"));
			AssertEquals(vessel, header.VesselCombination.Vessel);
		}

		public void TestVessels()
		{
			header.JPH_VesselName = "VSSL1";
			var vessels = header.VesselCombination.Vessels;
			AssertEquals("VSSL1", vessels.FilterBusinessObjectDefaults["Vessel Name:Property"].Value);
			AssertEquals("RCS1", vessels.FilterBusinessObjectDefaults["Radio Call Sign:Property"].Value);
			AssertEquals("AU", vessels.FilterBusinessObjectDefaults["Country of Registration:Property"].Value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<JPAFRHeader>();

			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VSSL1";
			vessel1.RV_RadioCallSign = "RCS1";
			vessel1.RV_RN_NKCountryOfReg = "AU";

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VSSL2";
			vessel2.RV_RadioCallSign = "RCS2";
			vessel2.RV_RN_NKCountryOfReg = "GB";
		}
		JPAFRHeader header;
	}
}
