using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business
{
	public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
		{
		}

		protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetAdditionalIdentifierRequirementRule()
		{
			if(Parent is CusGoodsLocation goodsLocation && goodsLocation.Parent is Declaration.CusEntryInstruction instruction && instruction.IsUCC5)
			{
				return new Dictionary<string, (ZString, ZPropertyInfo)> { };
			}
			else
			{
				var result = base.GetAdditionalIdentifierRequirementRule();
				result.Remove(CusGoodsLocationQualifierList.Codes.UnLocode);
				return result;
			}
		}
		protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule()
		{
			var rules = base.GetCustomsOfficeRequirementRule();

			rules[CusGoodsLocationQualifierList.Codes.UnLocode] = (ValidationRuleCodeConstants.Codes.C0061, Parent.CGL_CustomsOfficeInfo);

			return rules;
		}
		protected override bool ShouldPerformCGL_AdditionalInfoListValidationCore => false;
	}
}
