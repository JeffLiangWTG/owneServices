using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusReconDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCRD_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(CusReconDeclaration.CRD_CustomsOfficeInfo, "XXX", "010");
		}

		public void TestCRD_DeclarationType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(CusReconDeclaration.CRD_DeclarationTypeInfo, new ZString[] { "F", "X" }, new ZString[] { "A", "B", "C", "D", "E" });
		}

		public void TestCRD_RefundCauseCode()
		{
			CusReconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.A;
			ValidationTestHelper.AssertFieldIsMandatory(CusReconDeclaration.CRD_RefundCauseCodeInfo, "You have not entered a Refund Cause");
			CusReconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			ValidationTestHelper.AssertFieldIsNotMandatory(CusReconDeclaration.CRD_RefundCauseCodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(CusReconDeclaration.CRD_RefundCauseCodeInfo, new ZString[] { "14", "XX" }, new ZString[] { "01", "02", "05", "06", "07", "08", "09", "10", "11", "12", "13" });
		}

		public void TestCRD_RefundReasonCode()
		{
			CusReconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._03;
			ValidationTestHelper.AssertFieldIsMandatory(CusReconDeclaration.CRD_RefundReasonCodeInfo, "You have not entered a Refund Reason");
			CusReconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._01;
			ValidationTestHelper.AssertFieldIsNotMandatory(CusReconDeclaration.CRD_RefundReasonCodeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(CusReconDeclaration.CRD_RefundReasonCodeInfo, new ZString[] { "04",  "XX" }, new ZString[] { "01", "02", "03" });
		}

		public void TestCRD_CustomsDivision()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsDepartment, "00", "미지정 및 해당과 없음", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(CusReconDeclaration.CRD_CustomsDivisionInfo, "XX", "00");
		}

		public void TestCRD_TaxOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.TaxOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(CusReconDeclaration.CRD_TaxOfficeInfo, "XXX", "010");
		}
		CusReconDeclaration CusReconDeclaration
		{
			get { return cusReconDeclaration ?? (cusReconDeclaration = Factory.New<CusReconDeclaration>()); }
		}
		CusReconDeclaration cusReconDeclaration;
	}
}
