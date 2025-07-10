using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GAApprovalLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2024, 01, 01)]
		public void TestNonGAReasonTypeList_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ENGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code_13101 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "13101", "가축전염병 예방법 제31조, 동법 시행규칙 제31조에 따른 지정검역물에 해당하지 않음1", new ZDateTime("2023-01-01"), new ZDateTime("2024-12-31"));
			var code_13102 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "13102", "가축전염병 예방법 제31조, 동법 시행규칙 제31조에 따른 지정검역물에 해당하지 않음2", new ZDateTime("2023-01-01"), new ZDateTime("2024-01-31"));
			var code_13901 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "13901", "기타 세관장확인 수출요건 비대상 등 사유를 기재", new ZDateTime("2023-01-01"), new ZDateTime("2024-01-31"));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var gaApproval = CreateGAApprovalData(declaration, KRJobMessageTypeList.Codes.Export);
			CreateCusEntryHeader(declaration, (JobComInvoiceLine)gaApproval.Parent);
			AssertEquals("Pre-Condition", ZDateTime.Today, gaApproval.Parent.DeclarationDate);
			SetGAApprovalData("13", RequirementTypeCodeList.Codes._1);
			gaApproval.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(2, gaApproval.Lookups.NonGAReasonTypeList.Count);
			AssertNonGAReasonTypeList(code_13101);
			AssertNonGAReasonTypeList(code_13102);

			SetGAApprovalData("13", RequirementTypeCodeList.Codes._9);
			gaApproval.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(1, gaApproval.Lookups.NonGAReasonTypeList.Count);
			AssertNonGAReasonTypeList(code_13901);

			var entryNum = declaration.CustomsEntryHeaders[0].EntryNumbers.AddNew();
			entryNum.CE_EntryType = KRJobMessageTypeList.Codes.Export;
			entryNum.CE_IssueDate = new ZDateTime("2024-02-01");
			AssertEquals("Pre-Condition", new ZDateTime("2024-02-01"), gaApproval.Parent.DeclarationDate);
			gaApproval.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(0, gaApproval.Lookups.NonGAReasonTypeList.Count);
			SetGAApprovalData("13", RequirementTypeCodeList.Codes._1);
			gaApproval.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(1, gaApproval.Lookups.NonGAReasonTypeList.Count);
			AssertNonGAReasonTypeList(code_13101);

			void SetGAApprovalData(string procedure, string subType)
			{
				gaApproval.CSI_Procedure = procedure;
				gaApproval.CSI_SubType = subType;
			}

			void AssertNonGAReasonTypeList(RefCusCodeList codeList)
			{
				var code = gaApproval.Lookups.NonGAReasonTypeList.Cast<ZZRefCusCodeListCombined>().Single(x => x.ZZD_Code == codeList.ZZD_Code);
				AssertNotNull(code);
				AssertEquals(codeList.ZZD_Description, code.ZZD_Description);
			}
		}

		public void TestNonGAReasonTypeList_Import()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.INGAR, "Import - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code_13C01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "13C01", "요건면제수입확인(신청)서1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var code_13C02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "13C02", "요건면제수입확인(신청)서2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var code_13Z03 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.INGAR, "13Z03", "안전인증(확인), 공급자적합성확인 번호 기재", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var gaApproval = pivot.GAApprovalDataCollection.AddNew();
			AssertEquals("Pre-Condition", ZDateTime.Today, gaApproval.Parent.DeclarationDate);
			SetGAApprovalData("13", NonRequirementTypeCodeList.Codes.C);
			gaApproval.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(2, gaApproval.Lookups.NonGAReasonTypeList.Count);
			AssertNonGAReasonTypeList(code_13C01);
			AssertNonGAReasonTypeList(code_13C02);

			SetGAApprovalData("13", NonRequirementTypeCodeList.Codes.Z);
			gaApproval.Lookups.NonGAReasonTypeList.Load();
			AssertEquals(1, gaApproval.Lookups.NonGAReasonTypeList.Count);
			AssertNonGAReasonTypeList(code_13Z03);

			void SetGAApprovalData(string procedure, string subType)
			{
				gaApproval.CSI_Procedure = procedure;
				gaApproval.CSI_SubType = subType;
			}

			void AssertNonGAReasonTypeList(RefCusCodeList codeList)
			{
				var code = gaApproval.Lookups.NonGAReasonTypeList.Cast<ZZRefCusCodeListCombined>().Single(x => x.ZZD_Code == codeList.ZZD_Code);
				AssertNotNull(code);
				AssertEquals(codeList.ZZD_Description, code.ZZD_Description);
			}
		}

		public void TestSubTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var gaApproval = CreateGAApprovalData(declaration, KRJobMessageTypeList.Codes.Export);
			var invoiceLine = gaApproval.Parent as JobComInvoiceLine;
			var list = gaApproval.Lookups.SubTypeList;
			Assert(invoiceLine.IsExport);
			AssertEquals(typeof(RequirementTypeCodeList), gaApproval.Lookups.SubTypeList.GetType());

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			Assert(invoiceLine.IsImport);
			AssertEquals(typeof(CommodityUsageCodeList), gaApproval.Lookups.SubTypeList.GetType());

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			gaApproval = pivot.GAApprovalDataCollection.AddNew();
			Assert(gaApproval.Parent.IsExport);
			AssertEquals(typeof(RequirementTypeCodeList), gaApproval.Lookups.SubTypeList.GetType());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert(gaApproval.Parent.IsImport);
			AssertEquals(typeof(CommodityUsageCodeList), gaApproval.Lookups.SubTypeList.GetType());
		}

		[TestDate(2024, 01, 01)]
		public void TestOGARegulationCategoryList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var code01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "01", "약사법", new ZDateTime("2023-01-01"), new ZDateTime("2024-12-31"));
			var code02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "02", "마약법", new ZDateTime("2023-01-01"), new ZDateTime("2024-01-31"));
			Factory.Save();

			AssertOGARegulationCategoryListWhenParentIsInvoiceLine(KRJobMessageTypeList.Codes.Export);
			AssertOGARegulationCategoryListWhenParentIsInvoiceLine(KRJobMessageTypeList.Codes.Import);
			AssertOGARegulationCategoryListWhenParentIsPivot(ClassificationTypeList.Codes.HTE);
			AssertOGARegulationCategoryListWhenParentIsPivot(ClassificationTypeList.Codes.HTI);

			void AssertOGARegulationCategoryListWhenParentIsInvoiceLine(string messageType)
			{
				var declaration = Factory.New<JobDeclaration>();
				var gaApproval = CreateGAApprovalData(declaration, messageType);
				CreateCusEntryHeader(declaration, (JobComInvoiceLine)gaApproval.Parent);
				AssertEquals(ZDateTime.Today, gaApproval.Parent.DeclarationDate);

				gaApproval.Lookups.OGARegulationCategoryList.Load();
				AssertEquals(2, gaApproval.Lookups.OGARegulationCategoryList.Count);
				Assert(gaApproval.Lookups.OGARegulationCategoryList.Contains(code01));
				Assert(gaApproval.Lookups.OGARegulationCategoryList.Contains(code02));

				var entryNum = declaration.CustomsEntryHeaders[0].EntryNumbers.AddNew();
				entryNum.CE_EntryType = messageType;
				entryNum.CE_IssueDate = new ZDateTime("2024-02-01");
				AssertEquals(new ZDateTime("2024-02-01"), gaApproval.Parent.DeclarationDate);
				gaApproval.Lookups.OGARegulationCategoryList.Load();
				AssertEquals(1, gaApproval.Lookups.OGARegulationCategoryList.Count);
				Assert(gaApproval.Lookups.OGARegulationCategoryList.Contains(code01));
			}

			void AssertOGARegulationCategoryListWhenParentIsPivot(string childType)
			{
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_ChildType = childType;

				var gaApproval = pivot.GAApprovalDataCollection.AddNew();
				gaApproval.Lookups.OGARegulationCategoryList.Load();
				AssertEquals(2, gaApproval.Lookups.OGARegulationCategoryList.Count);
				Assert(gaApproval.Lookups.OGARegulationCategoryList.Contains(code01));
				Assert(gaApproval.Lookups.OGARegulationCategoryList.Contains(code02));
			}
		}

		public void TestCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var gaApproval = CreateGAApprovalData(declaration, KRJobMessageTypeList.Codes.Export);
			var list = gaApproval.Lookups.CodeList;
			AssertNotNull(gaApproval.Parent as JobComInvoiceLine);
			Assert(gaApproval.Parent.IsExport);
			foreach (var code in list)
			{
				Assert(new RequirementDocumentTypeCodeList().ContainsCode(code));
			}

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			list = gaApproval.Lookups.CodeList;
			AssertNotNull(gaApproval.Parent as JobComInvoiceLine);
			Assert(gaApproval.Parent.IsImport);
			foreach (var code in list)
			{
				Assert(new ImportRequirementTypeCodeList().ContainsCode(code));
			}
		}

		GAApproval CreateGAApprovalData(JobDeclaration declaration, string messageType)
		{
			declaration.JE_MessageType = messageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return invoiceLine.GAApprovalDataCollection.AddNew();
		}

		void CreateCusEntryHeader(JobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;
		}
	}
}
