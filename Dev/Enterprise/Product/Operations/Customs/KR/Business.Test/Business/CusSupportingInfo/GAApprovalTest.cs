using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GAApproval))]
	sealed class GAApprovalTest : CusSupportingInfoTest<GAApproval>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return SetInvoiceLineData().GAApprovalDataCollection.AddNew();
		}

		protected override IEnumerable<GAApproval> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().GAApprovalDataCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (GAApproval)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.GAApproval;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<GAApproval>();
			AssertEquals(typeof(GAApprovalValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			var invoiceLine = SetInvoiceLineData();
			var supportingInfo = invoiceLine.GAApprovalDataCollection.AddNew();
			AssertEquals(invoiceLine, supportingInfo.Parent);
			AssertEquals(1, supportingInfo.Parent.GetType().FindInterfaces(new System.Reflection.TypeFilter((type, criteria) => type.ToString().Equals(criteria.ToString())), "Enterprise.Customs.KR.Business.ILineOrProduct").Length);
		}

		public void TestMaxLength()
		{
			var invoiceLine = SetInvoiceLineData();
			var gaApproval = invoiceLine.GAApprovalDataCollection.AddNew();
			AssertEquals(1, gaApproval.CSI_SubTypeInfo.MaxLength);
			AssertEquals(2, gaApproval.CSI_ProcedureInfo.MaxLength);
			AssertEquals(1, gaApproval.CSI_CodeInfo.MaxLength);
			AssertEquals(300, gaApproval.CSI_DescriptionInfo.MaxLength);
			AssertEquals(18, gaApproval.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals(20, gaApproval.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals(200, gaApproval.CSI_AdditionalDescriptionInfo.MaxLength);

			invoiceLine.Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertEquals(2, gaApproval.CSI_SubTypeInfo.MaxLength);
			AssertEquals(2, gaApproval.CSI_ProcedureInfo.MaxLength);
			AssertEquals(1, gaApproval.CSI_CodeInfo.MaxLength);
			AssertEquals(50, gaApproval.CSI_DescriptionInfo.MaxLength);
			AssertEquals(20, gaApproval.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals(22, gaApproval.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals(60, gaApproval.CSI_AdditionalDescriptionInfo.MaxLength);

			var partPivot = Factory.New<CusClassPartPivot>();
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			gaApproval = partPivot.GAApprovalDataCollection.AddNew();
			AssertEquals(1, gaApproval.CSI_SubTypeInfo.MaxLength);
			AssertEquals(2, gaApproval.CSI_ProcedureInfo.MaxLength);
			AssertEquals(1, gaApproval.CSI_CodeInfo.MaxLength);
			AssertEquals(300, gaApproval.CSI_DescriptionInfo.MaxLength);
			AssertEquals(18, gaApproval.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals(20, gaApproval.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals(200, gaApproval.CSI_AdditionalDescriptionInfo.MaxLength);

			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(2, gaApproval.CSI_SubTypeInfo.MaxLength);
			AssertEquals(2, gaApproval.CSI_ProcedureInfo.MaxLength);
			AssertEquals(1, gaApproval.CSI_CodeInfo.MaxLength);
			AssertEquals(50, gaApproval.CSI_DescriptionInfo.MaxLength);
			AssertEquals(20, gaApproval.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals(22, gaApproval.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals(60, gaApproval.CSI_AdditionalDescriptionInfo.MaxLength);
		}

		[TestDate(2022, 08, 11)]
		public void TestValueChange()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code_13 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "13", "가축전염병 예방법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_13.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.D);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "21", "액화석유가스의 안전관리 및 사업법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			var code_29 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "29", "폐기물의 국가 간 이동 및 그 처리에 관한 법률", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_29.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.A);

			var code_32 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "32", "외국환거래법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_32.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.A);
			var code_34 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "34", "방위사업법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_34.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.E);

			var code_53 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "53", "원자력안전법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_53.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.E);
			var code_55 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "55", "총포･도검･화약류 등의 안전관리에 관한 법률", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_55.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.A);
			var code_57 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "57", "문화재보호법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_57.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.A);

			var code_69 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "69", "마약류 관리에 관한 법률", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_69.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.A);
			var code_71 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "71", "야생생물 보호 및 관리에 관한 법률", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_71.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.A);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var invoice = declaration.Invoices[0];
			var invoiceLine = invoice.JobComInvoiceLines[0];
			var gAApproval = invoiceLine.GAApprovalDataCollection.AddNew();

			gAApproval.CSI_Procedure = "42";
			gAApproval.CSI_Code = "B";
			gAApproval.CSI_ReferenceNumber = "AAA";
			gAApproval.CSI_ReferenceNumber2 = "BBB";
			gAApproval.CSI_AdditionalDescription = "CCC";

			gAApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;

			AssertEquals(true, gAApproval.CSI_LineNoInfo.ReadOnly);

			#region CSI_Procedure
			gAApproval.CSI_Status = "13";
			gAApproval.CSI_Procedure = "42";

			AssertEquals("13", gAApproval.CSI_Status);

			gAApproval.CSI_Procedure = "75";
			AssertEquals(ZString.Empty, gAApproval.CSI_Status);
			#endregion
			#region CSI_SubType
			AssertEquals(new ZDateTime(2022, 08, 11), gAApproval.CSI_DateOfIssue);
			AssertEquals(true, gAApproval.CSI_DateOfIssueInfo.ReadOnly);

			AssertEquals("NO", gAApproval.CSI_ReferenceNumber);
			AssertEquals(true, gAApproval.CSI_ReferenceNumberInfo.ReadOnly);

			AssertEquals("", gAApproval.CSI_ReferenceNumber2);
			AssertEquals(true, gAApproval.CSI_ReferenceNumber2Info.ReadOnly);

			AssertEquals(false, gAApproval.CSI_AdditionalDescriptionInfo.ReadOnly);

			invoice.Entries[0].EntryNumber = "1163921400096X";
			invoice.Entries[0].CusEntryNumber.CE_IssueDate = new ZDateTime(2022, 08, 12);
			AssertEquals("CE_IssueDate is set by a message processor. It should be able to return the date without resetting SubType", new ZDateTime(2022, 08, 12), gAApproval.CSI_DateOfIssue);

			gAApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;

			AssertEquals(new ZDateTime(2022, 08, 12), gAApproval.CSI_DateOfIssue);
			AssertEquals(true, gAApproval.CSI_DateOfIssueInfo.ReadOnly);

			gAApproval.CSI_Status = "13";
			gAApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;

			AssertEquals("13", gAApproval.CSI_Status);

			gAApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;
			AssertEquals(ZString.Empty, gAApproval.CSI_Status);

			AssertEquals(ZDateTime.Empty, gAApproval.CSI_DateOfIssue);
			AssertEquals(false, gAApproval.CSI_DateOfIssueInfo.ReadOnly);

			AssertEquals(ZString.Empty, gAApproval.CSI_ReferenceNumber);
			AssertEquals(false, gAApproval.CSI_ReferenceNumberInfo.ReadOnly);

			AssertEquals("", gAApproval.CSI_ReferenceNumber2);
			AssertEquals(false, gAApproval.CSI_ReferenceNumber2Info.ReadOnly);

			AssertEquals("", gAApproval.CSI_AdditionalDescription);
			AssertEquals(true, gAApproval.CSI_AdditionalDescriptionInfo.ReadOnly);

			AssertEquals(ZString.Empty, gAApproval.CSI_Status);
			#endregion

			#region CSI_Code
			AssertEquals("B", gAApproval.CSI_Code);
			AssertEquals(false, gAApproval.CSI_CodeInfo.ReadOnly);

			AssertProcedure(gAApproval, gAApproval, "29", "A", true);
			AssertProcedure(gAApproval, gAApproval, "32", "A", true);
			AssertProcedure(gAApproval, gAApproval, "55", "A", true);
			AssertProcedure(gAApproval, gAApproval, "57", "A", true);
			AssertProcedure(gAApproval, gAApproval, "69", "A", true);
			AssertProcedure(gAApproval, gAApproval, "71", "A", true);

			AssertProcedure(gAApproval, gAApproval, "13", "D", true);

			AssertProcedure(gAApproval, gAApproval, "34", "E", true);
			AssertProcedure(gAApproval, gAApproval, "53", "E", true);

			AssertProcedure(gAApproval, gAApproval, "21", "E", false);
			gAApproval.CSI_Code = "";
			AssertProcedure(gAApproval, gAApproval, "21", "", false);
			#endregion

			void AssertProcedure(GAApproval gAApproval, CusSupportingInfo row, ZString procedure, ZString code, ZBool infoReadonly)
			{
				gAApproval.CSI_Procedure = procedure;
				AssertEquals(code, row.CSI_Code);
				AssertEquals(infoReadonly, row.CSI_CodeInfo.ReadOnly);
			}
		}

		[TestDate(2022, 08, 11)]
		public void TestAdditionalDescriptionWhenSubTypeChanges()
		{
			var invoiceLine = SetInvoiceLineData();
			var gaApproval = invoiceLine.GAApprovalDataCollection.AddNew();
			gaApproval.CSI_Procedure = "42";

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
			gaApproval.CSI_AdditionalDescription = "조건 대상이 아닙니다";

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;
			AssertEquals("User-entered data is reserved when readonly stays the same", "조건 대상이 아닙니다", gaApproval.CSI_AdditionalDescription);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;
			AssertEquals("ReadOnly changes, cleared out as it is not relevant", ZString.Empty, gaApproval.CSI_AdditionalDescription);
		}

		public void TestRelevantValuesWhenParentIsDifferent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];
			invoiceLine.CusEntryLine.Header.EntryNumber = "AAA111";

			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			var pivot = orgSupplierPart.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;

			var gaApproval1 = invoiceLine.GAApprovalDataCollection.AddNew();
			gaApproval1.CSI_SubType = RequirementTypeCodeList.Codes._1;
			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = new ZDateTime(2022, 10, 10);

			var gaApproval2 = pivot.GAApprovalDataCollection.AddNew();
			gaApproval2.CSI_SubType = RequirementTypeCodeList.Codes._1;

			AssertEquals(true, gaApproval1.Parent.IsIssueDateRelevant);
			AssertEquals(true, gaApproval1.Parent.IsReferenceNumberRelevant);
			AssertEquals(new ZDateTime(2022, 10, 10), gaApproval1.CSI_DateOfIssue);
			AssertEquals("NO", gaApproval1.CSI_ReferenceNumber);
			AssertEquals(true, gaApproval1.Parent.IsValidationEnabled);

			AssertEquals(false, gaApproval2.Parent.IsIssueDateRelevant);
			AssertEquals(false, gaApproval2.Parent.IsReferenceNumberRelevant);
			AssertEquals(ZDateTime.Empty, gaApproval2.CSI_DateOfIssue);
			AssertEquals(string.Empty, gaApproval2.CSI_ReferenceNumber);
			AssertEquals(true, gaApproval2.Parent.IsValidationEnabled);
		}

		public void TestNonGAReasonType()
		{
			var invoiceLine = SetInvoiceLineData();
			var gaApproval = invoiceLine.GAApprovalDataCollection.AddNew();
			gaApproval.CSI_Procedure = "01";
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;
			gaApproval.CSI_Status = "13";
			Factory.Save();

			AssertEquals(false, gaApproval.NonGAReasonTypeInfo.ReadOnly);
			AssertEquals("01213", gaApproval.NonGAReasonType);

			gaApproval.CSI_Procedure = "";
			AssertNullOrEmpty(gaApproval.NonGAReasonType);
			AssertNullOrEmpty(gaApproval.CSI_Status);
			AssertEquals(true, gaApproval.NonGAReasonTypeInfo.ReadOnly);

			gaApproval.CSI_Procedure = "01";
			AssertNullOrEmpty(gaApproval.NonGAReasonType);
			AssertNullOrEmpty(gaApproval.CSI_Status);
			AssertEquals(false, gaApproval.NonGAReasonTypeInfo.ReadOnly);

			gaApproval.CSI_Status = "13";
			AssertEquals("01213", gaApproval.NonGAReasonType);

			gaApproval.CSI_SubType = "";
			AssertNullOrEmpty(gaApproval.NonGAReasonType);
			AssertNullOrEmpty(gaApproval.CSI_Status);
			AssertEquals(true, gaApproval.NonGAReasonTypeInfo.ReadOnly);
		}

		public void TestExportNonGAMandatoryDocument()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ENGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			var testData1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "69102", "마약류 관리에 관한 법률 제2조 제3호 마목 단서에 따른 신체적 또는 정신적 의존성을 야기하지 아니하는 제제", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var testData2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "53201", "원자력안전법, 핵물질 수출입요건확인요령제8조제1항에 따른 다음 1,2를 모두 만족하는 경우(1. 사용허가 면제대상이거나 사용신고면제대상인 경우와 2. 보고 면제대상인 경우)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var testData3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "13101", "가축전염병 예방법 제31조, 동법 시행규칙 제31조에 따른 지정검역물에 해당하지 않음", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateCusCodeListAttribute(testData1.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "향정신성의약품 제외인정 신청서");
			helper.CreateCusCodeListAttribute(testData2.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "핵물질 수출입요건확인면제(신청)서");
			Factory.Save();

			var invoiceline = SetInvoiceLineData();
			var gaApproval = invoiceline.GAApprovalDataCollection.AddNew();
			gaApproval.CSI_Procedure = "69";
			gaApproval.CSI_SubType = "1";
			gaApproval.CSI_Status = "02";
			AssertEquals("69102", gaApproval.NonGAReasonType);
			AssertEquals(gaApproval.NonGAReasonType, testData1.ZZD_Code);
			AssertEquals(1, testData1.Attributes.Count);
			AssertEquals("향정신성의약품 제외인정 신청서", gaApproval.ExportNonGAMandatoryDocument);
			AssertEquals(gaApproval.ExportNonGAMandatoryDocument, testData1.Attributes[0].ZZE_Value);

			gaApproval.CSI_Procedure = "53";
			gaApproval.CSI_SubType = "2";
			gaApproval.CSI_Status = "01";
			AssertEquals("53201", gaApproval.NonGAReasonType);
			AssertEquals(gaApproval.NonGAReasonType, testData2.ZZD_Code);
			AssertEquals(1, testData2.Attributes.Count);
			AssertEquals("핵물질 수출입요건확인면제(신청)서", gaApproval.ExportNonGAMandatoryDocument);
			AssertEquals(gaApproval.ExportNonGAMandatoryDocument, testData2.Attributes[0].ZZE_Value);

			gaApproval.CSI_Procedure = "13";
			gaApproval.CSI_SubType = "1";
			gaApproval.CSI_Status = "01";
			AssertEquals("13101", gaApproval.NonGAReasonType);
			AssertEquals(gaApproval.NonGAReasonType, testData3.ZZD_Code);
			AssertEquals(0, testData3.Attributes.Count);
			AssertEquals(ZString.Empty, gaApproval.ExportNonGAMandatoryDocument);
		}

		public void TestCSI_CodeForMessageType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code_13 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "13", "가축전염병 예방법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(code_13.PK, Constants.ZZ.CodeListAttributeNames.RequiredExportDocumentType, RequirementDocumentTypeCodeList.Codes.D);
			Factory.Save();

			var exportInvoiceLine = SetInvoiceLineData();
			Assert(exportInvoiceLine.IsExport);
			var gAApproval = exportInvoiceLine.GAApprovalDataCollection.AddNew();
			gAApproval.CSI_Procedure = code_13.ZZD_Code;
			AssertEquals("D", gAApproval.CSI_Code);

			var importInvoiceLine = SetInvoiceLineData(KRJobMessageTypeList.Codes.Import);
			Assert(importInvoiceLine.IsImport);
			gAApproval = importInvoiceLine.GAApprovalDataCollection.AddNew();
			gAApproval.CSI_Procedure = code_13.ZZD_Code;
			AssertEquals(ZString.Empty, gAApproval.CSI_Code);
		}

		JobComInvoiceLine SetInvoiceLineData(string messageType = KRJobMessageTypeList.Codes.Export)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoice = declaration.Invoices.AddNew();
			return invoice.InvoiceLines.AddNew();
		}
	}
}
