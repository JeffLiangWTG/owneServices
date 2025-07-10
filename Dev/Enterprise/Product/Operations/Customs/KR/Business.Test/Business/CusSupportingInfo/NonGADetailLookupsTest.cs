using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(NonGADetailLookups))]
	sealed class NonGADetailLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2024, 01, 01)]
		public void TestNonGAReasonTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.INGAR, "Import - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code_13A01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "13A01", "가축전염병 예방법 제31조, 동법 시행규칙 제31조에 따른 지정검역물에 해당하지 않음", new ZDateTime("2023-01-01"), new ZDateTime("2024-12-31"));
			var code_80Z01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "80Z01", "기타 세관장확인 수입요건 비대상 등 사유를 기재", new ZDateTime("2023-01-01"), new ZDateTime("2024-01-31"));
			var code_80Z02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "80Z02", "기타 세관장확인 수입요건 비대상 등 사유를 기재", new ZDateTime("2023-01-01"), new ZDateTime("2024-12-31"));
			Factory.Save();

			var nonGADetail = CreateNonGADetailData();
			var invoiceLine = nonGADetail.Parent as JobComInvoiceLine;
			AssertEquals("Pre-Condition", ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);
			SetNonGADetails("01", NonRequirementTypeCodeList.Codes.A);
			nonGADetail.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(0, nonGADetail.Lookups.NonGAReasonTypeList.Count);

			SetNonGADetails("13", NonRequirementTypeCodeList.Codes.A);
			nonGADetail.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(1, nonGADetail.Lookups.NonGAReasonTypeList.Count);
			Assert(nonGADetail.Lookups.NonGAReasonTypeList.Contains(code_13A01));

			SetNonGADetails("80", NonRequirementTypeCodeList.Codes.Z);
			nonGADetail.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(2, nonGADetail.Lookups.NonGAReasonTypeList.Count);
			Assert(nonGADetail.Lookups.NonGAReasonTypeList.Contains(code_80Z01));
			Assert(nonGADetail.Lookups.NonGAReasonTypeList.Contains(code_80Z02));

			var entryNum = invoiceLine.CusEntryLine.Header.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum.CE_IssueDate = new ZDateTime("2024-02-01");
			AssertEquals("Pre-Condition", new ZDateTime("2024-02-01"), invoiceLine.EffectiveAssessmentDate);
			nonGADetail.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(1, nonGADetail.Lookups.NonGAReasonTypeList.Count);
			Assert(nonGADetail.Lookups.NonGAReasonTypeList.Contains(code_80Z02));

			void SetNonGADetails(string procedure, string code)
			{
				nonGADetail.CSI_Procedure = procedure;
				nonGADetail.CSI_Code = code;
		}
		}

		public void TestImportNonGAMandatoryDocument()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.INGAR, "Import - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			var testData1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "13C02", "관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제2호에 따라 통합공고 제12조(요건면제) 제1항 각 호에 해당하여 요건면제확인서를 제출한 물품", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var testData2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "80C02", "관세법 제226조에 따른 세관장확인물품 및 확인방법 지정고시 제7조 제2항 제2호에 따라 통합공고 제12조(요건면제) 제1항 각 호에 해당하여 요건면제확인서를 제출한 물품", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "22B03", "고압가스 안전관리법 제17조 제1항, 동법 시행령 제15조 제1항 제5호에 따른 산업기계설비 등에 부착되어 수입하는 것", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateCusCodeListAttribute(testData1.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "요건면제수입확인(신청)서");
			helper.CreateCusCodeListAttribute(testData2.PK, Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode, "요건면제수입확인(신청)서");
			Factory.Save();

			var nonGADetail = CreateNonGADetailData();
			nonGADetail.CSI_Procedure = "13";
			nonGADetail.CSI_Code = "C";
			nonGADetail.CSI_Status = "02";
			AssertEquals("13C02", nonGADetail.NonGAReasonType);
			AssertEquals("요건면제수입확인(신청)서", nonGADetail.ImportNonGAMandatoryDocument);

			nonGADetail.CSI_Procedure = "80";
			nonGADetail.CSI_Code = "C";
			nonGADetail.CSI_Status = "02";
			AssertEquals("80C02", nonGADetail.NonGAReasonType);
			AssertEquals("요건면제수입확인(신청)서", nonGADetail.ImportNonGAMandatoryDocument);

			nonGADetail.CSI_Procedure = "22";
			nonGADetail.CSI_Code = "B";
			nonGADetail.CSI_Status = "03";
			AssertEquals("22B03", nonGADetail.NonGAReasonType);
			AssertEquals(ZString.Empty, nonGADetail.ImportNonGAMandatoryDocument);
		}

		[TestDate(2024, 01, 01)]
		public void TestOGARegulationCategoryList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "01", "약사법", new ZDateTime("2023-01-01"), new ZDateTime("2024-12-31"));
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "02", "마약법", new ZDateTime("2023-01-01"), new ZDateTime("2024-01-31"));
			Factory.Save();

			var nonGADetail = CreateNonGADetailData();
			var invoiceLine = nonGADetail.Parent as JobComInvoiceLine;
			AssertEquals("Pre-Condition", ZDateTime.Today, invoiceLine.EffectiveAssessmentDate);

			nonGADetail.Lookups.OGARegulationCategoryList.Load();
			AssertEquals(2, nonGADetail.Lookups.OGARegulationCategoryList.Count);
			Assert(nonGADetail.Lookups.OGARegulationCategoryList.Contains(code1));
			Assert(nonGADetail.Lookups.OGARegulationCategoryList.Contains(code2));

			var entryNum = invoiceLine.CusEntryLine.Header.EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Import;
			entryNum.CE_IssueDate = new ZDateTime("2024-02-01");
			AssertEquals("Pre-Condition", new ZDateTime("2024-02-01"), invoiceLine.EffectiveAssessmentDate);

			nonGADetail.Lookups.OGARegulationCategoryList.Load();
			AssertEquals(1, nonGADetail.Lookups.OGARegulationCategoryList.Count);
			Assert(nonGADetail.Lookups.OGARegulationCategoryList.Contains(code1));
			Assert(!nonGADetail.Lookups.OGARegulationCategoryList.Contains(code2));
		}

		public void TestCodeList()
		{
			var nonGADetail = Factory.New<NonGADetail>();
			var list = (CodeDescriptionPairList)nonGADetail.Lookups.CodeList;
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(NonRequirementTypeCodeList.Codes.A));
			Assert(list.ContainsCode(NonRequirementTypeCodeList.Codes.B));
			Assert(list.ContainsCode(NonRequirementTypeCodeList.Codes.C));
			Assert(list.ContainsCode(NonRequirementTypeCodeList.Codes.Z));
		}

		NonGADetail CreateNonGADetailData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return invoiceLine.NonGADetailCollection.AddNew();
		}
	}
}
