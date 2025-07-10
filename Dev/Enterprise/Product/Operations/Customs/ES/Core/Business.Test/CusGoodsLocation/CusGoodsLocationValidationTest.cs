using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;

namespace Enterprise.Customs.ES.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCGL_Qualifier()
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		var goodslocation = temporaryStorageHeader.GoodsLocation;
		var message = "The chosen Qualifier does not match the Location Type.";

		CombineAssertions("Type is B, qualifier must be U", () =>
		{
			goodslocation.CGL_Type = CusGoodsLocationTypeList.Codes.AuthorizedPlace;
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			AssertNoMessageError("qualifier = U => no error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertHasMessageError("qualifier != U && != Y => error", goodslocation.CGL_QualifierInfo, message);
			goodslocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertNoMessageError("qualifier = Y => no error", goodslocation.CGL_QualifierInfo, message);
		});
	}
}
