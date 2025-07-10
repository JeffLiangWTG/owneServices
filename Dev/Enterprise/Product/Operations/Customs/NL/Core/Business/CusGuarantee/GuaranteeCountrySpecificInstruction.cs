using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
{
	public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => new(Factory.GetCachedValue<EUGuaranteeTypeList>());

	public override CodeDescriptionPairList GetSubTypeList(ZString typeCode) => Factory.GetCachedValue(key: "NL.CusGuaranteeHeaderLookups.PermitSubTypes" + typeCode,
		getValueDelegate: () => (string)typeCode switch
		{
			EUGuaranteeTypeList.Codes.IMP => new ImportGuaranteeSubTypeList(),
			_ => base.GetSubTypeList(typeCode),
		});

	public override ZString GetValueFromFieldType(ZString ruleCode) => (string)ruleCode switch
	{
		PermitRuleCodeList.Codes.OFF => (ZString)nameof(FieldType.TextCodeFindBox),
		_ => base.GetValueFromFieldType(ruleCode),
	};

	public override PermitMatchingType GetMatchingType(ZString ruleCode) => (string)ruleCode switch
	{
		PermitRuleCodeList.Codes.OFF => PermitMatchingType.SingleValue,
		_ => base.GetMatchingType(ruleCode),
	};

	public override ICollection GetLookupList(SharedCusPermitHeader permitHeader, ZString ruleCode) => (string)ruleCode switch
	{
		PermitRuleCodeList.Codes.OFF => Factory.GetCachedValue("NL.GuaranteeCountrySpecificInstruction.CustomsOffice." + permitHeader?.CPH_OH_PermitHolder, () =>
		{
			ZString attValue = EuOfficeCodesTypes.Codes.OfficeOfGuarantee;
			var cusOfficeCode = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Netherlands, ZString.Empty);
			cusOfficeCode.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", attValue, false));
			cusOfficeCode.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, false));
			return cusOfficeCode;
		}),
		_ => base.GetLookupList(permitHeader, ruleCode),
	};

	public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType) => Factory.GetCachedValue<PermitRuleCodeList>();

	public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule() => Factory.GetCachedValue<PermitRuleCodeList>();
}
