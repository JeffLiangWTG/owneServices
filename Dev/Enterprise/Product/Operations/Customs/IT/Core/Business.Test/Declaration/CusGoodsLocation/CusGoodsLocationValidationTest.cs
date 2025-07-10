using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusGoodsLocationValidationTest : TestCaseWithFactory
{
	public void TestValidateAdditionalIdentifier_PlaceCode_MandatoryValidation()
	{
		var goodsLocation = (CusGoodsLocation)Factory.New<JobDeclaration>().GoodsLocation;
		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_Type = "B";

		goodsLocation.CGL_AdditionalIdentifier = "";
		AssertHasMessageErrorContaining(goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

		goodsLocation.CGL_AdditionalIdentifier = "90808F";
		AssertNoMessageErrorContaining(goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_AdditionalIdentifier = "";
		AssertNoMessageErrorContaining(goodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestValidateAdditionalIdentifier_PlaceCode_ListValidation()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holder.PK, "ALE1")
			.AddLocRule("90815M");

		var goodsLocation = (CusGoodsLocation)Factory.New<JobDeclaration>().GoodsLocation;
		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_Type = "B";
		goodsLocation.Address.IdentificationHolderPK = holder.PK;

		goodsLocation.CGL_AdditionalIdentifier = "90808F";
		AssertHasMessageErrorContaining(goodsLocation.CGL_AdditionalIdentifierInfo, ListValidation.InvalidCodeMessageError);

		goodsLocation.CGL_AdditionalIdentifier = "90815M";
		AssertNoMessageErrorContaining(goodsLocation.CGL_AdditionalIdentifierInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCGL_Qualifier_RequiredIfTypeIsFilled()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var goodsLocation = declaration.GoodsLocation;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = "T";
			goodsLocation.Validation.ValidateCGL_Qualifier();
			AssertHasMessageErrorContaining("Qualifier is empty and Type is filled", goodsLocation.CGL_QualifierInfo, qualifierAndTypeAreRequiredMessage);

			goodsLocation.CGL_Qualifier = "Q";
			goodsLocation.CGL_Type = ZString.Empty;
			goodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrorContaining("Qualifier Filled and Type is empty", goodsLocation.CGL_QualifierInfo, qualifierAndTypeAreRequiredMessage);

			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = ZString.Empty;
			goodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrorContaining(goodsLocation.CGL_QualifierInfo, qualifierAndTypeAreRequiredMessage);

			goodsLocation.CGL_Qualifier = "Q";
			goodsLocation.CGL_Type = "R";
			goodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrorContaining(goodsLocation.CGL_QualifierInfo, qualifierAndTypeAreRequiredMessage);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = "R";
			goodsLocation.Validation.ValidateCGL_Qualifier();
			AssertNoMessageErrorContaining(goodsLocation.CGL_QualifierInfo, qualifierAndTypeAreRequiredMessage);
		}
	}

	public void TestCGL_Type_RequiredIfQualifierIsFilled()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var goodsLocation = declaration.GoodsLocation;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			goodsLocation.CGL_Qualifier = "Q";
			goodsLocation.CGL_Type = ZString.Empty;
			goodsLocation.Validation.ValidateCGL_Type();
			AssertHasMessageErrorContaining("Qualifier Filled and Type is empty", goodsLocation.CGL_TypeInfo, qualifierAndTypeAreRequiredMessage);

			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = "T";
			goodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrorContaining("Qualifier is empty and Type is filled", goodsLocation.CGL_TypeInfo, qualifierAndTypeAreRequiredMessage);

			goodsLocation.CGL_Qualifier = ZString.Empty;
			goodsLocation.CGL_Type = ZString.Empty;
			goodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrorContaining(goodsLocation.CGL_TypeInfo, qualifierAndTypeAreRequiredMessage);

			goodsLocation.CGL_Qualifier = "Q";
			goodsLocation.CGL_Type = "R";
			goodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrorContaining(goodsLocation.CGL_TypeInfo, qualifierAndTypeAreRequiredMessage);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			goodsLocation.CGL_Qualifier = "Q";
			goodsLocation.CGL_Type = ZString.Empty;
			goodsLocation.Validation.ValidateCGL_Type();
			AssertNoMessageErrorContaining(goodsLocation.CGL_TypeInfo, qualifierAndTypeAreRequiredMessage);
		}
	}

	public void TestCheckCGL_CustomsOffice_RuleCN0394()
	{
		var goodsLocation = declaration.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsLocation.CGL_CustomsOfficeInfo, "[CN0394] Customs Office required when qualifier is 'V'.");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	const string qualifierAndTypeAreRequiredMessage = "Qualifier and Type field must be both filled or both empty";
}
