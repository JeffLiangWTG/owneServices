using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.Business.Testing;

class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
{
	public void TestGetTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.Netherlands);
		AssertContainsExactElementsInAnyOrder("Type List", new ZString[] { "COD", "IMP", "TRA", "TST", "ZZZ" }, typeList.GetAllCodes());
	}

	public void TestGetSubTypeList()
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(EUGuaranteeTypeList.Codes.IMP);
		AssertContainsExactElementsInAnyOrder("Sub Type List", new ZString[] { "1", "2", "3", "4", "5", "8", "0", "B", "C", "D", "E", "F", "G", "H", "I", "J", "R" }, subTypeList.GetAllCodes());
	}

	public void TestValueFromAndValueToFieldType_OFF()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Value From Field Type", nameof(FieldType.TextCodeFindBox), guaranteeCountrySpecificInstruction.GetValueFromFieldType(PermitRuleCodeList.Codes.OFF));
			AssertEquals("Value To Field Type", nameof(FieldType.Text), guaranteeCountrySpecificInstruction.GetValueToFieldType(PermitRuleCodeList.Codes.OFF));
		});
	}

	public void TestGetRuleCodeListForModule()
	{
		var guarantee = Factory.New<CusGuaranteeHeader>();
		var listrule = guarantee.CountrySpecificInstruction.GetRuleCodeListForModule().GetAllCodes();
		AssertEquals(true, listrule.Contains(PermitRuleCodeList.Codes.OFF));
	}

	public void TestGetLookupList()
	{
		var guarantee = Factory.New<CusGuaranteeHeader>();
		var cusOfficeCodeCollection = (CustomsOfficeCodeCollection)guarantee.CountrySpecificInstruction.GetLookupList(guarantee, PermitRuleCodeList.Codes.OFF);
		string key = Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
		Assert("Key " + key + " should be in collection", cusOfficeCodeCollection.FilterBusinessObjectDefaults.ContainsDefaultFor(key));
		var filterBOD = cusOfficeCodeCollection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(f => f.FilterName == Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue & f.PropertyName == "Property");
		AssertNotNull(Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue + "must be added as a filter " + PermitRuleCodeList.Codes.OFF, filterBOD);
		var attValue = EuOfficeCodesTypes.Codes.OfficeOfGuarantee;
		AssertEquals("Value should be " + attValue, attValue, filterBOD.Value);
		AssertEquals("Filter is removable", false, filterBOD.IsRemovable);

		filterBOD = cusOfficeCodeCollection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(f => f.FilterName == Customs.Universal.Constants.ZZRefCusCodeListFilters.AttributeValue & f.PropertyName == "ComparisonOperator");
		AssertEquals("Comparison operator should be equal", (ZString)ModuleTextFilter.ComparisonConstants.Exact, filterBOD.Value);
	}

	public void TestGetMatchingType()
	{
		var guarantee = Factory.New<CusGuaranteeHeader>();
		AssertEquals(Customs.Business.PermitMatchingType.SingleValue, guaranteeCountrySpecificInstruction.GetMatchingType(PermitRuleCodeList.Codes.OFF));
	}

	protected override void SetUp()
	{
		base.SetUp();
		guaranteeCountrySpecificInstruction = new GuaranteeCountrySpecificInstruction(Factory);
	}
	GuaranteeCountrySpecificInstruction guaranteeCountrySpecificInstruction;
}
