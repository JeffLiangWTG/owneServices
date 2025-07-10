using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescPhase4LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestOriginStates()
	{
		goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
		var originState = Factory.New<RefCountryStates>();
		originState.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		AssertEquals(true, originState.MatchesFilter(lookups.OriginStates.CompleteFilter));
	}

	public void TestCPCListAdditionalFilter()
	{
		var expectedAdditionalFilter = new ZQuery(RefCusProcedureSchema.ZZ6_ProcedureCode, IT.Business.UniversalReferenceConstants.RefCusProcedureCodes.Transit)
			.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, string.Empty)
			.LiteralTextADO;
		var cpcList = (RefCusProcedureCollection)lookups.CPCList;
		AssertEquals("AdditionalFilter", expectedAdditionalFilter, cpcList.AdditionalFilter.LiteralTextADO);
	}

	public void TestCPCListDefaultFilters()
	{
		var cpcList = (RefCusProcedureCollection)lookups.CPCList;
		var defaultFilters = cpcList.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().ToArray();
		AssertEquals("Number of default filters", 2, defaultFilters.Length);

		var procedureCodeDefaultFilter = defaultFilters.SingleOrDefault(x => x.FilterName == Constants.ZZRefCusProcedureFilters.ProcedureCode);
		AssertNotNull("Expected default filter for ProcedureCode", procedureCodeDefaultFilter);
		var concessionDefaultFilter = defaultFilters.SingleOrDefault(x => x.FilterName == Constants.ZZRefCusProcedureFilters.Concession);
		AssertNotNull("Expected default filter for Concession", concessionDefaultFilter);

		CombineAssertions("Default filters", () =>
		{
			AssertEquals($"{nameof(procedureCodeDefaultFilter.Value)}", IT.Business.UniversalReferenceConstants.RefCusProcedureCodes.Transit, procedureCodeDefaultFilter.Value);
			Assert($"{nameof(procedureCodeDefaultFilter.IsRemovable)}", !procedureCodeDefaultFilter.IsRemovable);

			AssertEquals($"{nameof(concessionDefaultFilter.Value)}", "is blank", concessionDefaultFilter.Value);
			Assert($"{nameof(concessionDefaultFilter.IsRemovable)}", !concessionDefaultFilter.IsRemovable);
		});
	}

	public void TestCPCList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "80", "00", "", "", "", false);
		var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "80", "71", "", "", "IMP", false);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "40", "71", "", "", "", false);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "A", "80", "50", "444", "80", "EXP", false);
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Belgium, "A", "80", "71", "", "", "EXP", false);

		var cpcList = goodsItem.ITLookups.CPCList;
		AssertEquals("Number of CPCs", 2, cpcList.Count);
		Assert($"Contains {nameof(procedure1)}", cpcList.Contains(procedure1));
		Assert($"Contains {nameof(procedure2)}", cpcList.Contains(procedure2));
	}

	public void TestStatusList()
	{
		CombineAssertions(() =>
		{
			AssertType<EntryLineCustomsStatusList>("Type", lookups.StatusList);
			AssertSame("Cached", lookups.StatusList, lookups.StatusList);
		});
	}

	public void TestPortTaxRateList()
	{
		new ITUniversalReferenceTestDataHelper(Factory).SetupPortTaxRates();
		Factory.Save();

		AssertEquals("Codes", "A1, A2, A3", lookups.PortTaxRateList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		lookups = new NctsDepartureCargoDescPhase4Lookups(goodsItem);
	}
	NctsDepartureCargoDesc goodsItem;
	NctsDepartureCargoDescPhase4Lookups lookups;
}
