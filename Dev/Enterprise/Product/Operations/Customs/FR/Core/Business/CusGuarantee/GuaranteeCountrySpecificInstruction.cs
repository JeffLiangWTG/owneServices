using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
{
	public class GuaranteeCountrySpecificInstruction : EU.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<GuaranteeTypeList>();

		public override CodeDescriptionPairList GetSubTypeList(ZString typeCode) => Factory.GetCachedValue(key: "FR.CusGuaranteeHeaderLookups.PermitSubTypes" + typeCode, getValueDelegate: () =>
		{
			var result = new CodeDescriptionPairList();
			if (typeCode == GuaranteeTypeList.Codes.DEF)
			{
				result = new ExternalDefermentGuaranteeSubTypeList();
			}
			else if (typeCode == GuaranteeTypeList.Codes.COD)
			{
				result = NCTSGuaranteeSubTypeList;
			}
			return result;
		});

		public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType) => Factory.GetCachedValue($"ruleCodeList|{permitType}", () =>
	   {
		   PermitRuleCodeList result = new PermitRuleCodeList();

		   if (permitType != GuaranteeTypeList.Codes.ALT && permitType != GuaranteeTypeList.Codes.AI2)
		   {
			   result.RemoveCode(PermitRuleCodeList.Codes.CAN);
		   }

		   if (permitType != GuaranteeTypeList.Codes.COD && permitType != GuaranteeTypeList.Codes.DEF)
		   {
			   result.RemoveCode(PermitRuleCodeList.Codes.ENT);
		   }

		   if (permitType != GuaranteeTypeList.Codes.COD)
		   {
			   result.RemoveCode(PermitRuleCodeList.Codes.PCD);
			   result.RemoveCode(PermitRuleCodeList.Codes.PCP);
			   result.RemoveCode(PermitRuleCodeList.Codes.PCV);
		   }

		   return result;
	   });

		public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule() => Factory.GetCachedValue<PermitRuleCodeList>();

		protected override string PermitGuaranteeTypeCore => GuaranteeTypeList.Codes.COD;

		public override Customs.Business.PermitMatchingType GetMatchingType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case PermitRuleCodeList.Codes.MOD:
					return Customs.Business.PermitMatchingType.SingleValue;
				case PermitRuleCodeList.Codes.CAN:
					return Customs.Business.PermitMatchingType.SingleValue;
				case PermitRuleCodeList.Codes.ENT:
					return Customs.Business.PermitMatchingType.SingleValue;
				default:
					return base.GetMatchingType(ruleCode);
			}
		}

		public override ZString GetValueFromFieldType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case PermitRuleCodeList.Codes.MOD:
					return nameof(FieldType.TextDropEdit);
				case PermitRuleCodeList.Codes.CAN:
					return nameof(FieldType.TextDropEdit);
				case PermitRuleCodeList.Codes.ENT:
					return nameof(FieldType.TextDropEdit);
				default:
					return base.GetValueFromFieldType(ruleCode);
			}
		}
	}
}
