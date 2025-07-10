using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCGL_Qualifier()
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var goodslocation = temporaryStorageHeader.GoodsLocation;

		goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
		goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertNoNotifications("Type is A, qualifier = Y => no error", goodslocation.CGL_QualifierInfo);

		goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertNoNotifications("Type is B, qualifier = Y => no error", goodslocation.CGL_QualifierInfo);

		goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
		goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertNoNotifications("Type is C, qualifier = Y => no error", goodslocation.CGL_QualifierInfo);
	}

	public void TestCGL_AdditionalIdentifier()
	{
		const string expectedNumericErrorMessage = "Only numeric characters are allowed.";

		var holder = Factory.NewWithValidTestData<OrgHeader>();

		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holder.PK, "TST1")
			.AddLocRule("908088");

		var header = Factory.New<TemporaryStorageHeader>();
		var goodsLocation = header.GoodsLocation;
		var goodsLocationAddress = goodsLocation.Address;
		goodsLocationAddress.IdentificationHolderPK = holder.PK;

		goodsLocationAddress.AuthorisationNumber = "TST1";

		goodsLocation.AdditionalIdentifier = "908088";
		goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
		AssertNoNotifications("Valid code", goodsLocation.AdditionalIdentifierInfo);

		goodsLocation.AdditionalIdentifier = "00000";
		goodsLocation.Address.IdentificationHolderPK = ZGuid.BrettsGuid;
		goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
		AssertHasMessageErrorContaining("Invalid code", goodsLocation.AdditionalIdentifierInfo, ListValidation.InvalidCodeMessageError.ToString());

		goodsLocation.AdditionalIdentifier = "00000";
		goodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
		goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
		AssertNoNotifications("Invalid code", goodsLocation.AdditionalIdentifierInfo);

		goodsLocation.AdditionalIdentifier = "12345";
		goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
		AssertNoMessageError(goodsLocation.AdditionalIdentifierInfo, expectedNumericErrorMessage);

		goodsLocation.AdditionalIdentifier = "";
		goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
		AssertHasMessageErrorContaining("Empty Place ID", goodsLocation.AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

		goodsLocation.AdditionalIdentifier = "ABC123";
		goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
		AssertHasMessageError("Non-numeric Place ID", goodsLocation.AdditionalIdentifierInfo, expectedNumericErrorMessage);
	}
}
