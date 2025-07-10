using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Sailing
{
	[TestedType(typeof(DocVessel))]
	sealed class DocVesselTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocVessel.New(Vessel, Factory),
				DocVessel.New(Vessel.Factory, Vessel.RV_Code)
			};
		}

		public void TestCode()
		{
			AssertEquals("TESTRVCODE", VesselWrapper.Code);
			Vessel.RV_Code = "VESSEL TEST 123";
			AssertEquals("VESSEL TEST 123", VesselWrapper.Code);
		}

		public void TestIsActive()
		{
			Assert(VesselWrapper.IsActive);
			Vessel.RV_IsActive = ZBool.False;
			AssertEquals(ZBool.False, VesselWrapper.IsActive);
		}

		public void TestLloydsNumber()
		{
			AssertEquals("", VesselWrapper.LloydsNumber);
			Vessel.RV_LloydsNumber = "LloydXX";
			AssertEquals("LloydXX", VesselWrapper.LloydsNumber);
		}

		public void TestNetRegisterTon()
		{
			AssertEquals(0, VesselWrapper.NetRegisterTon);
			Vessel.RV_NetRegisterTon = 10;
			AssertEquals(10, VesselWrapper.NetRegisterTon);
		}

		public void TestOrganisation()
		{
			AssertNull(VesselWrapper.Organisation);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			Vessel.RV_OH = org.PK;
			AssertEquals("TESTORG", VesselWrapper.Organisation.Code);
		}

		public void TestCountry()
		{
			AssertNull(VesselWrapper.Country);
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery());
			Vessel.RV_RN_NKCountryOfReg = country.Code;
			AssertEquals(country.RN_Desc, Vessel.CountryOfReg.Description);
		}

		public void TestVesselType()
		{
			AssertEquals("CV", VesselWrapper.VesselType);
			Vessel.RV_VesselType = "XX";
			AssertEquals("XX", VesselWrapper.VesselType);
		}

		public void TestToString()
		{
			ZString code = new ZString("Code");
			Vessel.RV_Code = code;
			AssertEquals("ToString()", code, VesselWrapper.ToString());
		}

		#region Implementation

		RefVessel Vessel;
		DocVessel VesselWrapper;
		protected override void SetUp()
		{
			Vessel = Factory.New<RefVessel>();
			Vessel.RV_Code = "TESTRVCODE";
			VesselWrapper = DocVessel.New(Vessel, Factory);
			base.SetUp();
		}

		#endregion
	}
}
