using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRateOverrideReasonCodeList()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertType<RateOverrideReasonList>(charge.Lookups.RateOverrideReasonCodeList);
		}

		[TestDate(2025, 5, 31)]
		public void TestNationalFeeTypeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: eun);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.FrenchNationalTaxCode, "French National Tax Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.FrenchNationalTaxCode, "aaa", "aaa Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.FrenchNationalTaxCode, "bbb", "bbb Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.BrettsBirthday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.FrenchNationalTaxCode, "ccc", "ccc Desc.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var charge = entryHeader.Charges.AddNew();
			var nationalFeeTypeCodeList = charge.Lookups.NationalFeeTypeCodeList;

			AssertEquals("Code aaa is retrieved", true, nationalFeeTypeCodeList.ContainsCode("aaa"));
			AssertEquals("CEI_DateForDuty is empty, we will use today's date to calculate NationalFeeTypeCodeList, code bbb is not retrieved", false, nationalFeeTypeCodeList.ContainsCode("bbb"));
			AssertEquals("DataGroupingCode is EUN instead of FR, code ccc is not retrieved", false, nationalFeeTypeCodeList.ContainsCode("ccc"));

			entryInstruction.CEI_DateForDuty = ZDateTime.BrettsBirthday.AddDays(-3);
			nationalFeeTypeCodeList = charge.Lookups.NationalFeeTypeCodeList;
			AssertEquals("CEI_DateForDuty is not empty, we will use CEI_DateForDuty to calculate NationalFeeTypeCodeList, code bbb is retrieved", true, nationalFeeTypeCodeList.ContainsCode("bbb"));
		}
	}
}
