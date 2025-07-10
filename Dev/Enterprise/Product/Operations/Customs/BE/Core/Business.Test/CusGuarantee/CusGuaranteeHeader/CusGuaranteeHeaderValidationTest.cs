using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CusGuaranteeHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMainAccessPersonName()
	{
		CombineAssertions(() =>
		{
			foreach (var subType in new EUNctsGuaranteeTypeList().GetAllCodes())
			{
				guaranteeHeader.CPH_SubType = subType;
				guaranteeHeader.MainAccessPersonName = ZString.Empty;

				if (ValidationExtendMethods.GuaranteeTypesApplicable.Contains(subType))
				{
					AssertHasMessageErrorContaining($"CPH_SubType = {subType} and MainAccessPersonName is empty", guaranteeHeader.MainAccessPersonNameInfo, MandatoryValidation.YouHaveNotEntered);
				}
				else
				{
					AssertNoMessageErrorContaining($"CPH_SubType = {subType} and MainAccessPersonName is empty", guaranteeHeader.MainAccessPersonNameInfo, MandatoryValidation.YouHaveNotEntered);
				}

				guaranteeHeader.MainAccessPersonName = "John Smith";
				AssertNoMessageErrorContaining($"CPH_SubType = {subType} and MainAccessPersonName is not empty", guaranteeHeader.MainAccessPersonNameInfo, MandatoryValidation.YouHaveNotEntered);
			}
		});
	}

	public void TestValidateMainAccessPersonName()
	{
		guaranteeHeader.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
		CombineAssertions(() =>
		{
			guaranteeHeader.MainAccessPersonName = ZString.Empty;
			guaranteeHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining("MainAccessPersonName is empty", guaranteeHeader.MainAccessPersonNameInfo, MandatoryValidation.YouHaveNotEntered);

			guaranteeHeader.MainAccessPersonName = "John Smith";
			guaranteeHeader.Validation.ValidateAll();
			AssertNoMessageErrorContaining("MainAccessPersonName is not empty", guaranteeHeader.MainAccessPersonNameInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		guaranteeHeader = Factory.New<CusGuaranteeHeader>();
	}
	CusGuaranteeHeader guaranteeHeader;
}
