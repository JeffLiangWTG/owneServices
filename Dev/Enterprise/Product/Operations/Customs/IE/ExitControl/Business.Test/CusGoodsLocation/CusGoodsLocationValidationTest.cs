using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCGL_AdditionalIdentifier()
		{
			(var arrivalGoodsLocation, var report, _) = CusGoodsLocationTest.GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			arrivalGoodsLocation.CGL_AdditionalIdentifier = "!@#";
			AssertNoMessageErrorContaining(arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, ListValidation.InvalidCodeMessageError);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			arrivalGoodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
			AssertHasMessageErrorContaining(arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, ListValidation.InvalidCodeMessageError);
			arrivalGoodsLocation.CGL_AdditionalIdentifier = "AUSYD";
			AssertNoMessageErrorContaining(arrivalGoodsLocation.CGL_AdditionalIdentifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCGL_Type()
		{
			(var arrivalGoodsLocation, var report, _) = CusGoodsLocationTest.GetNewBusinessObject(Factory);
			report.CER_Type = ExitReportTypeList.Codes.Presentation;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(arrivalGoodsLocation.CGL_TypeInfo, "X", CusGoodsLocationTypeList.Codes.AuthorizedPlace);
		}
	}
}
