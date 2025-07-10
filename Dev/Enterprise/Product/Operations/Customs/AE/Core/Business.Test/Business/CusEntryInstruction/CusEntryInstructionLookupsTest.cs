using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEntryInstructionLookups))]
sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDeclarationPurposeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var otherCodeType = "TEST";
		helper.CreateNewOrGetExistingCusCodeType(AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose);
		helper.CreateNewOrGetExistingCusCodeType(otherCodeType, otherCodeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, "1", "Commercial Samples for Exhibition", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, "4", "Others", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, otherCodeType, "5", "For test 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.SouthAfrica, AEConstants.RefCusCodeList.CodeTypes.DelarationPurpose, "6", "For test 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.SouthAfrica, otherCodeType, "7", "For test 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CurrencyCodes.UnitedArabEmirates);
		Factory.Save();

		Declaration.JE_ApplicationCode = Core.Constants.CurrencyCodes.UnitedArabEmirates;
		var cachedList = Lookups.DeclarationPurposeList;
		CombineAssertions(() =>
		{
			AssertEquals("DeclarationPurposeList", "1, 4", cachedList.CodesAsString);
			AssertSame(cachedList, Lookups.DeclarationPurposeList);
		});
	}

	public void TestTradeTypeList()
		=> AssertSame(Factory.GetCachedValue<TradeTypeList>(), Lookups.TradeTypeList);

	CusEntryInstructionLookups Lookups => lookups ??= Declaration.CustomsEntryInstructions.AddNew().Lookups;
	CusEntryInstructionLookups lookups;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
