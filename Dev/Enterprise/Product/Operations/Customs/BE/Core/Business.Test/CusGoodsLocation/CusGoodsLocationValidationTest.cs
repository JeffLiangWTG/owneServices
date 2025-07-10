using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCGL_Qualifier()
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		var goodslocation = temporaryStorageHeader.GoodsLocation;
		var message = "The chosen Qualifier does not match the Location Type.";

		CombineAssertions("Type is A, qualifier must be U or V", () =>
		{
			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation;
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertNoMessageError($"qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNoMessageError($"qualifier = V => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertHasMessageError($"qualifier != V && != U => error", goodslocation.CGL_QualifierInfo, message);
		});

		CombineAssertions("Type is B, qualifier must be V", () =>
		{
			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNoMessageError($"qualifier = V => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertHasMessageError($"qualifier != V => error", goodslocation.CGL_QualifierInfo, message);
		});

		CombineAssertions("Type is C, qualifier must be V", () =>
		{
			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNoMessageError($"qualifier = V => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertHasMessageError($"qualifier != V => error", goodslocation.CGL_QualifierInfo, message);
		});

		CombineAssertions("Type is D, qualifier must be U, V, W, X or Y", () =>
		{
			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.Other;
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertNoMessageError($"qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNoMessageError($"qualifier = V => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertNoMessageError($"qualifier = W => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNoMessageError($"qualifier = X => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertNoMessageError($"qualifier = Y => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertHasMessageError($"qualifier = Z => error", goodslocation.CGL_QualifierInfo, message);
		});

		CombineAssertions("No issue with goodsLocation not linked to temporaryStorage", () =>
		{
			var goods = Factory.New<CusGoodsLocation>();
			goods.CGL_ParentID = Factory.New<JobDeclaration>().PK;
			goods.CGL_ParentTableCode = "JE";

			AssertNoExceptionThrown(() => goods.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation);
			AssertNoExceptionThrown(() => goods.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode);
			AssertNoExceptionThrown(() => goods.Validation.ValidateAll());
			AssertNoNotifications(goods.CGL_TypeInfo);

			var goodsWithInvalidParentID = Factory.New<CusGoodsLocation>();
			goodsWithInvalidParentID.CGL_ParentID = CargoWise.Types.ZGuid.Invalid;
			goodsWithInvalidParentID.CGL_ParentTableCode = "AMA";

			AssertNoExceptionThrown(() => goodsWithInvalidParentID.CGL_Type = CusGoodsLocationTypeList.Codes.DesignatedLocation);
			AssertNoExceptionThrown(() => goodsWithInvalidParentID.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode);
			AssertNoExceptionThrown(() => goodsWithInvalidParentID.Validation.ValidateAll());
			AssertNoNotifications(goodsWithInvalidParentID.CGL_TypeInfo);
		});
	}
}

