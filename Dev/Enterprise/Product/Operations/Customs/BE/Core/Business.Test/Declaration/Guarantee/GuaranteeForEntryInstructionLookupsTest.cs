using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class GuaranteeForEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestBondTypesList()
	{
		var guarantee = Factory.New<CusEntryInstruction>().Guarantees.AddNew();
		var bondTypes = guarantee.Lookups.BondTypeList;
		bondTypes.Sort();
		AssertEquals("0, 1, 2, 3, 5, 8, C, I, R", bondTypes.CodesAsString);
	}

	public void TestFacilityCollection()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE349000", "MENEN-LAR DAE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BEANR433A", "433A 433A 2030 ANTWERPEN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BEZAVVV00067004", "BPOST 709 BEDRIJVENZONE MACHELEN CARGO 1830 MACHELEN", ZDateTime.Today.AddDays(1), ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var guarantee = Factory.New<CusEntryInstruction>().Guarantees.AddNew();
		var facList = guarantee.Lookups.FacilityCollection;
		facList.Load();
		AssertContainsExactElementsInAnyOrder(new string[] { "BEANR433A" }, facList.Select(x => x.ZZD_Code));
	}
}
