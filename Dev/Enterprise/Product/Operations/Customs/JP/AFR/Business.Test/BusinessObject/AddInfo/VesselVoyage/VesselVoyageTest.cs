using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(VesselVoyage))]
	class VesselVoyageTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<VesselVoyage>
	{
		public void TestProperties()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_RadioCallSign = "12345";
			vessel.RV_RN_NKCountryOfReg = "CN";

			var portOfLoading = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfLoading.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfLoading.RL_PortName = "LN123";
			portOfLoading.Code = "L123";

			var portOfDischarge = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfDischarge.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfDischarge.RL_PortName = "DN123";
			portOfDischarge.Code = "D123";

			Factory.Save();

			var vesselVoyage = Factory.New<VesselVoyage>();
			vesselVoyage.JP_VesselName = vessel.RV_Code;
			vesselVoyage.JP_VesselCallSign = vessel.RV_RadioCallSign;
			vesselVoyage.JP_VesselCountry = vessel.RV_RN_NKCountryOfReg;
			vesselVoyage.JP_PortOfLoadingCode = portOfLoading.Code;
			AssertEquals(vessel.RV_Code, vesselVoyage.JP_VesselName);
			AssertEquals(vessel.RV_RN_NKCountryOfReg, vesselVoyage.JP_VesselCountry);
			AssertEquals(vessel.RV_RadioCallSign, vesselVoyage.JP_VesselCallSign);

			AssertEquals(portOfLoading.Code, vesselVoyage.JP_PortOfLoadingCode);
			AssertEquals(portOfLoading.RL_PortName, vesselVoyage.JP_PortOfLoadingName);
		}

		protected override IEnumerable<VesselVoyage> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<JPAFRHeader>();
			var result = factory.New<VesselVoyage>();
			result.B7_ParentID = header.PK;
			result.B7_ParentTableCode = header.TablePrefix;
			result.JP_VesselName = "VJ";
			yield return result;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<VesselVoyage>();
	}
}
