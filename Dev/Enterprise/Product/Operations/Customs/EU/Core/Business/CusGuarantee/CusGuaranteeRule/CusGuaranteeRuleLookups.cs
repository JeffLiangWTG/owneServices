using System.Collections;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeRuleLookups : Customs.Business.CusGuaranteeRuleLookups
	{
		public CusGuaranteeRuleLookups(CusGuaranteeRule parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PermitRuleCodes
		{
			get
			{
				var codeList = PermitRuleCodesCore;
				var codeListCopy = default(CodeDescriptionPairList);
				if (ShouldRemoveRuleCodeTSP)
				{
					codeListCopy ??= new CodeDescriptionPairList(codeList);
					codeListCopy.RemoveCode(PermitRuleCodeList.Codes.TSP);
				}
				return codeListCopy ?? codeList;
			}
		}

		protected virtual Customs.Business.PermitRuleCodeList PermitRuleCodesCore => PermitHeader?.GetCountrySpecificInstruction()?.GetRuleCodeList(PermitHeader.CPH_Type, PermitHeader.CPH_SubType);

		protected virtual bool ShouldRemoveRuleCodeTSP => PermitHeader.CPH_Type != EUGuaranteeTypeList.Codes.TST;

		public override ICollection CPR_ValueFromList => Parent.CPR_RuleCode.ToString() switch
		{
			PermitRuleCodeList.Codes.INV => new RefCountryCollection(Factory),
			PermitRuleCodeList.Codes.LAP => Factory.GetCachedValue<LiabilityApplicablePercentageCodeList>(),
			PermitRuleCodeList.Codes.CUS => EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(Factory, CusPermitHeaderApplicationCodeList.Codes.Guarantee),
			_ => base.CPR_ValueFromList
		};

		public override ICollection CPR_ValueToList => (string)Parent.CPR_RuleCode switch
		{
			PermitRuleCodeList.Codes.INV => new RefCountryCollection(Factory),
			_ => new CodeDescriptionPairList(),
		};
	}
}
