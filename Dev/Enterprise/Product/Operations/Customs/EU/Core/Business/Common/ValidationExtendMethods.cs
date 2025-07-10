using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class ValidationExtendMethods
	{
		public static readonly ImmutableArray<string> GuaranteeTypesApplicable = ImmutableArray.Create(
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver, //0
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.ComprehensiveGuarantee, // 1
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeByGuarantor, //2
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher, //4
			EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.IndividualGuaranteeWithMultipleUsage // 9 
		);

		public static readonly ImmutableArray<string> GuaranteeTypesApplicableForRuleC085 = ImmutableArray.Create(
			GuaranteeTypesApplicable
					.Add(EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention) // B
					.ToArray()
		);

		public static void RuleTR301(ZString type, ZString number, ZPropertyInfo info)
		{
			if (!number.IsEmpty && GuaranteeTypesApplicable.Contains(type))
			{
				var maxLength = 17;
				if (type == EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.FlatRateVoucher)
				{
					maxLength = 24;
				}
				if (number.Length != maxLength)
				{
					var warning = Res.GetString("E663C5C1-C315-4B81-90EE-0D30464FEBA8", "Guarantee reference must be {0} characters long", maxLength.ToString());
					info.AddMessageError(warning);
				}
			}
		}
	}
}
