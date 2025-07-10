
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(NonGADetailValidation))]
	sealed class NonGADetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			nonGADetail.CSI_Code = "";
			AssertHasMessageErrorContaining(nonGADetail.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			nonGADetail.CSI_Code = "X";
			AssertHasMessageErrorContaining(nonGADetail.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			nonGADetail.CSI_Code = NonRequirementTypeCodeList.Codes.A;
			AssertNoMessageErrors(nonGADetail.CSI_CodeInfo);

			nonGADetail.CSI_Code = NonRequirementTypeCodeList.Codes.B;
			AssertNoMessageErrors(nonGADetail.CSI_CodeInfo);

			nonGADetail.CSI_Code = NonRequirementTypeCodeList.Codes.C;
			AssertNoMessageErrors(nonGADetail.CSI_CodeInfo);

			nonGADetail.CSI_Code = "X";
			AssertHasMessageErrorContaining(nonGADetail.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			nonGADetail.CSI_Code = NonRequirementTypeCodeList.Codes.Z;
			AssertNoMessageErrors(nonGADetail.CSI_CodeInfo);
		}

		public void TestCheckCSI_Procedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var cusCode_01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "01", "약사법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var cusCode_02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "02", "마약법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			nonGADetail.CSI_Procedure = "";
			AssertHasMessageErrorContaining(nonGADetail.CSI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

			nonGADetail.CSI_Procedure = "XX";
			AssertHasMessageErrorContaining(nonGADetail.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

			nonGADetail.CSI_Procedure = cusCode_01.ZZD_Code;
			AssertNoMessageErrors(nonGADetail.CSI_ProcedureInfo);

			nonGADetail.CSI_Procedure = cusCode_02.ZZD_Code;
			AssertNoMessageErrors(nonGADetail.CSI_ProcedureInfo);
		}

		public void TestCheckCSI_Status()
		{
			nonGADetail.CSI_Status = "";
			AssertHasMessageErrorContaining(nonGADetail.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);

			nonGADetail.CSI_Status = "02";
			AssertNoMessageErrors(nonGADetail.CSI_StatusInfo);
		}

		public void TestCkeckCSI_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.INGAR, "Import - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "72C02", "(관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제2호에 따라 통합공고 제12조(요건면제) 제1항 각 호에 해당하여 요건면제확인서를 제출한 물품)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "72Z01", "(기타 세관장확인 수입요건 비대상 등 사유를 기재)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateCusCodeListAttribute(codeList1.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "향정신성의약품 제외인정 신청서");
			helper.CreateCusCodeListAttribute(codeList2.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "핵물질 수출입요건확인면제(신청)서");
			Factory.Save();

			nonGADetail.CSI_Code = NonRequirementTypeCodeList.Codes.C;
			nonGADetail.CSI_Procedure = "72";
			nonGADetail.CSI_Status = "";
			nonGADetail.CSI_Description = "";
			AssertNoMessageErrorContaining("NonGAReasonType is Empty", nonGADetail.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			nonGADetail.CSI_Status = "02";
			nonGADetail.CSI_Description = "";
			AssertHasMessageErrorContaining("Mandatory Doc When Exempt is true", nonGADetail.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			nonGADetail.CSI_Description = "향정신성의약품 제외인정 신청서";
			AssertNoMessageErrors(nonGADetail.CSI_DescriptionInfo);

			nonGADetail.CSI_Description = "핵물질 수출입요건확인면제(신청)서";
			AssertNoMessageErrors(nonGADetail.CSI_DescriptionInfo);

			nonGADetail.CSI_Status = "03";
			nonGADetail.CSI_Description = "";
			AssertNoMessageErrors("Mandatory Doc When Exempt is false", nonGADetail.CSI_DescriptionInfo);
		}

		public void TestCheckNonGAReasonType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.INGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "72C02", "(관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제2호에 따라 통합공고 제12조(요건면제) 제1항 각 호에 해당하여 요건면제확인서를 제출한 물품)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			nonGADetail.CSI_Code = "";
			nonGADetail.CSI_Procedure = "";
			nonGADetail.Validation.ValidateNonGAReasonType();
			AssertNoMessageErrors(nonGADetail.NonGAReasonTypeInfo);

			nonGADetail.CSI_Procedure = "01";
			AssertNoMessageErrors(nonGADetail.NonGAReasonTypeInfo);

			nonGADetail.CSI_Procedure = "72";
			nonGADetail.CSI_Code = NonRequirementTypeCodeList.Codes.C;
			nonGADetail.CSI_Status = "00";
			nonGADetail.Validation.ValidateNonGAReasonType();
			AssertHasMessageErrorContaining(nonGADetail.NonGAReasonTypeInfo, ListValidation.InvalidCodeMessageError);

			nonGADetail.CSI_Status = "02";
			nonGADetail.Validation.ValidateNonGAReasonType();
			AssertNoMessageErrors(nonGADetail.NonGAReasonTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			nonGADetail = invoiceline.NonGADetailCollection.AddNew();
		}
		NonGADetail nonGADetail;
	}
}
