using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class AccessCodePinRuleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCPR_Description()
	{
		foreach (var guaranteeHeaderType in new[] { EUGuaranteeTypeList.Codes.TRA, EUGuaranteeTypeList.Codes.IMP })
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = guaranteeHeaderType;

			var guaranteeRule = guaranteeHeader.AdditionalAccessCodes.AddNew();
			CombineAssertions(() =>
			{
				foreach (var subType in new EUNctsGuaranteeTypeList().GetAllCodes())
				{
					guaranteeHeader.CPH_SubType = subType;
					guaranteeRule.CPR_Description = CargoWise.Types.ZString.Empty;

					if (ValidationExtendMethods.GuaranteeTypesApplicable.Contains(subType))
					{
						AssertHasMessageErrorContaining($"Header CPH_Type is {guaranteeHeaderType} and CPH_SubType = {subType} and CPR_Description is empty", guaranteeRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
					}
					else
					{
						AssertNoMessageErrorContaining($"Header CPH_Type is {guaranteeHeaderType} and CPH_SubType = {subType} and CPR_Description is empty", guaranteeRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
					}

					guaranteeRule.CPR_Description = "John Smith";
					AssertNoMessageErrorContaining($"Header CPH_Type is {guaranteeHeaderType} and CPH_SubType = {subType} and CPR_Description is not empty", guaranteeRule.CPR_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}
	}
}
