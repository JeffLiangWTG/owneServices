using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAOceanBillSynchroniserTest : SynchroniserTestCase
	{
		// much of this is tested in the end 2 end test in CusSCAHouseSynchroniserTest
		public void TestCusSCAOceanBillSynchroniser()
		{
			var helper = new CusSCATestHelper();
			var consol = helper.Consol;
			var oceanBill = helper.OceanBill;
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "8080", canada);
			var cCN = consol.Numbers.AddNew();
			cCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			cCN.CE_EntryNum = "8010CCN";
			var c1 = helper.Container1;
			var c2 = helper.Container2;
			oceanBill.EnableAndSynchronise();

			AssertEquals("CCN", "8010CCN", oceanBill.OriginalCCN);
			AssertEquals("Application Code", Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, oceanBill.CB_ApplicationCode);
			AssertEquals("Master bill", CusSCATestHelper.MasterBillNum, oceanBill.CB_OceanBill);
			AssertEquals("2 containers", 2, oceanBill.Containers.Count);

			consol.JK_MasterBillNum = "NEWMASTER";
			var c3 = helper.Container3;
			AssertEquals("Master bill", "NEWMASTER", oceanBill.CB_OceanBill);
			AssertEquals("now 3 containers", 3, oceanBill.Containers.Count);

			var house = oceanBill.HouseBills.AddNew();
			house.CA_OverrideFreightDefaults = true;

			consol.JK_MasterBillNum = "NEWMASTER2";
			AssertEquals("Synchronising should have stopped since a house now is not syncronised", "NEWMASTER", oceanBill.CB_OceanBill);
		}
	}
}
