using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EXPGAApprovalValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_DateOfIssue()
		{
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;

			gaApproval.CSI_DateOfIssue = ZDateTime.Empty;
			AssertHasMessageErrorContaining(gaApproval.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_DateOfIssue = ZDateTime.Today;
			AssertNoMessageErrors(gaApproval.CSI_DateOfIssueInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
			gaApproval.CSI_DateOfIssue = ZDateTime.Empty;
			AssertNoMessageErrors(gaApproval.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_Code()
		{
			gaApproval.CSI_Code = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_Code = "Z";
			AssertHasMessageErrorContaining(gaApproval.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			gaApproval.CSI_Code = RequirementDocumentTypeCodeList.Codes.B;
			AssertNoMessageErrors(gaApproval.CSI_CodeInfo);

			gaApproval.CSI_Code = RequirementDocumentTypeCodeList.Codes.C;
			AssertNoMessageErrors(gaApproval.CSI_CodeInfo);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			gaApproval = declaration.Invoices[0].JobComInvoiceLines[0].GAApprovalDataCollection.AddNew();

			gaApproval.CSI_Code = "";
			AssertNoMessageErrorContaining(gaApproval.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_Code = "Z";
			AssertHasMessageErrorContaining(gaApproval.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			gaApproval.CSI_Code = ImportRequirementTypeCodeList.Codes._1;
			AssertNoMessageErrors(gaApproval.CSI_CodeInfo);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;

			gaApproval.CSI_ReferenceNumber = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_ReferenceNumber = "82455884";
			AssertNoMessageErrors(gaApproval.CSI_ReferenceNumberInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;

			gaApproval.CSI_ReferenceNumber = "";
			AssertNoMessageErrors(gaApproval.CSI_ReferenceNumberInfo);
		}

		public void TestCheckCSI_Description()
		{
			gaApproval.CSI_Description = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_Description = "수입식품 서류명";
			AssertNoMessageErrors(gaApproval.CSI_DescriptionInfo);
		}

		public void TestCheckCSI_AdditionalDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ENGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "69102", "마약류 관리에 관한 법률 제2조 제3호 마목 단서에 따른 신체적 또는 정신적 의존성을 야기하지 아니하는 제제", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "53201", "원자력안전법, 핵물질 수출입요건확인요령제8조제1항에 따른 다음 1,2를 모두 만족하는 경우(1. 사용허가 면제대상이거나 사용신고면제대상인 경우와 2. 보고 면제대상인 경우)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateCusCodeListAttribute(codeList1.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "향정신성의약품 제외인정 신청서");
			helper.CreateCusCodeListAttribute(codeList2.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "핵물질 수출입요건확인면제(신청)서");
			Factory.Save();

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;
			gaApproval.CSI_AdditionalDescription = "";
			AssertNoMessageErrors(gaApproval.CSI_AdditionalDescriptionInfo);

			gaApproval.CSI_Procedure = "69";
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
			gaApproval.CSI_AdditionalDescription = "";
			AssertHasMessageErrorContaining("NonGAReasonType is Empty", gaApproval.CSI_AdditionalDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_Status = "02";
			gaApproval.CSI_AdditionalDescription = "";
			AssertHasMessageErrorContaining("Mandatory Doc When Exempt is true", gaApproval.CSI_AdditionalDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_AdditionalDescription = "발급요청 보류";
			AssertNoMessageErrors(gaApproval.CSI_AdditionalDescriptionInfo);

			gaApproval.CSI_Procedure = "53";
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;
			gaApproval.CSI_Status = "01";

			gaApproval.CSI_AdditionalDescription = "";
			AssertHasMessageErrorContaining("Mandatory Doc When Exempt is true", gaApproval.CSI_AdditionalDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_AdditionalDescription = "요건승인번호 미기재";
			AssertNoMessageErrors(gaApproval.CSI_AdditionalDescriptionInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._4;
			gaApproval.CSI_AdditionalDescription = "";
			AssertHasMessageErrorContaining("NonGAReasonType is Empty", gaApproval.CSI_AdditionalDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			gaApproval.CSI_Status = "01";
			gaApproval.CSI_AdditionalDescription = "";
			AssertNoMessageErrors("Mandatory Doc When Exempt is false and CSI_SubType is not '9'", gaApproval.CSI_AdditionalDescriptionInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._9;
			gaApproval.CSI_Status = "01";
			gaApproval.CSI_AdditionalDescription = "";
			AssertHasMessageErrorContaining("CSI_SubType is '9'", gaApproval.CSI_AdditionalDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_AdditionalDescription = "발급요청 미신청";
			AssertNoMessageErrors(gaApproval.CSI_AdditionalDescriptionInfo);
		}

		public void TestCheckNonGAReasonType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ENGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "13101", "가축전염병 예방법 제31조, 동법 시행규칙 제31조에 따른 지정검역물에 해당하지 않음", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
			gaApproval.CSI_Procedure = "";
			gaApproval.CSI_SubType = "";

			var validation = (EXPGAApprovalValidation)gaApproval.Validation;
			validation.ValidateNonGAReasonType();
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_Procedure = "01";
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._4;
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._9;
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);

			gaApproval.CSI_Procedure = "13";
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;

			gaApproval.CSI_Status = "00";
			AssertHasMessageErrorContaining(gaApproval.NonGAReasonTypeInfo, ListValidation.InvalidCodeMessageError);

			gaApproval.CSI_Status = "01";
			AssertNoMessageErrors(gaApproval.NonGAReasonTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			gaApproval = invoiceline.GAApprovalDataCollection.AddNew();
		}
		JobDeclaration declaration;
		GAApproval gaApproval;
	}
}
