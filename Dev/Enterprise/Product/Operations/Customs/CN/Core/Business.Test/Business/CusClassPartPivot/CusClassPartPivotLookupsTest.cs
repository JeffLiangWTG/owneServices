using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOriginStateList()
		{
			Pivot.CI_RN_NKCountryOfOrigin = "CA";
			var caStates1 = Lookups.OriginStateList;
			Pivot.CI_RN_NKCountryOfOrigin = "US";
			var usStates1 = Lookups.OriginStateList;
			Pivot.CI_RN_NKCountryOfOrigin = "CA";
			var caStates2 = Lookups.OriginStateList;
			Pivot.CI_RN_NKCountryOfOrigin = "US";
			var usStates2 = Lookups.OriginStateList;
			AssertSame("Should have cached OriginStateList for CA", caStates1, caStates2);
			AssertSame("Should have cached OriginStateList for US", usStates1, usStates2);
		}

		public void TestUNDGSubs()
		{
			var iatSubstance = Factory.New<UNDGSubstance>();
			iatSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			var imoSubstance = Factory.New<UNDGSubstance>();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubs = Lookups.UNDGSubs;
			AssertNull("Non-IMO substance should not be found", undgSubs.FindByPK(iatSubstance.PK));
			AssertNotNull("IMO substance should be found", undgSubs.FindByPK(imoSubstance.PK));
		}

		CusClassPartPivot Pivot => pivot ?? (pivot = Factory.New<CusClassPartPivot>());
		CusClassPartPivot pivot;

		CusClassPartPivotLookups Lookups => Pivot.Lookups;
	}
}
