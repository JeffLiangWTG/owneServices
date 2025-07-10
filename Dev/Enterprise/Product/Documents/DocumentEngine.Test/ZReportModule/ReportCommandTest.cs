using System;
using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(ReportCommand))]
	sealed class ReportCommandTest : EnterpriseBusinessObjectTestCase
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestAttachmentTypesWithoutColumnHeaders()
		{
			var reportCommand = Factory.New<ReportCommand>();
			var attachmentTypes = reportCommand.AttachmentTypes;
			Assert(!reportCommand.DisableCSVExport);

			AssertEquals(11, attachmentTypes.Count);
			Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain CSV.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
			Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			Assert("Attachment types should contain TXT_SEMI.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
			Assert("Attachment types should contain TXT_COMM.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
			Assert("Attachment types should contain TXT_PIPE.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
		}

		public void TestAttachmentTypesWithColumnHeaders()
		{
			var reportCommand = Factory.LoadTop1<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Staff Profile Report"));
			var attachmentTypes = reportCommand.AttachmentTypes;
			Assert(!reportCommand.DisableCSVExport);

			AssertEquals(13, attachmentTypes.Count);
			Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain CSV.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
			Assert("Attachment types should contain CS2.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.CsvWithHeadings));
			Assert("Attachment types should contain XML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xml));
			Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			Assert("Attachment types should contain TXT_SEMI.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
			Assert("Attachment types should contain TXT_COMM.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
			Assert("Attachment types should contain TXT_PIPE.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
		}

		public void TestAttachmentTypeWithDisableXLSXExport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableXLSXExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Assert("DisableXLSXExport config should have the value of true", reportCommand.DisableXLSXExport);

			var attachmentTypes = reportCommand.AttachmentTypes;
			Assert("Attachment types should not contain XLSX.", !attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
		}

		public void TestAttachmentTypeWithDisableCSVExport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			Assert(reportCommand.DisableCSVExport);

			var attachmentTypes = reportCommand.AttachmentTypes;
			AssertEquals(10, attachmentTypes.Count);
			Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			Assert("Attachment types should contain TXT_SEMI.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
			Assert("Attachment types should contain TXT_COMM.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
			Assert("Attachment types should contain TXT_PIPE.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentTypeShouldOnlyContainTemplateFormatItself()
		{
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsChartAndDISABLEXLSXEXPORT.xls", true, false);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsNoChartAndDISABLEXLSXEXPORT.xls", true, false);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsChartAndNoDISABLEXLSXEXPORT.xls", true, false);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsNoChartAndNoDISABLEXLSXEXPORT.xls", true, true);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsxChartAndDISABLEXLSXEXPORT.xlsx", false, false);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsxNoChartAndDISABLEXLSXEXPORT.xlsx", true, false);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsxChartAndNoDISABLEXLSXEXPORT.xlsx", false, true);
			AssertAttachmentTypeShouldOnlyContainTemplateFormatItself("XlsxNoChartAndNoDISABLEXLSXEXPORT.xlsx", true, true);
		}

		public void AssertAttachmentTypeShouldOnlyContainTemplateFormatItself(string templateName, bool expectedXls, bool expectedXlsx)
		{
			var reportCommand = Factory.New<ReportCommand>();
			var document = reportCommand.Documents.AddNew();
			var template = Factory.New<StmTemplateBase>();
			var excelTemplate = new ExcelTemplateForUnitTesting(templateName, TestFilesSubFolder.ReportTestFiles);
			template.SO_Template = excelTemplate.GetAsByteArray();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;
			AssertEquals($"Template: {templateName}: Attachment types should {(expectedXls ? "" : "not")} contain XLS.", expectedXls, reportCommand.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			AssertEquals($"Template: {templateName}: Attachment types should {(expectedXlsx ? "" : "not")} contain XLSX.", expectedXlsx, reportCommand.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
		}

		public void TestIsApplicable_TaxFrameworkReport()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;

			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = companyB.PK;

			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfig.ETC_ParentId = companyA.PK;

			Factory.Save();

			var report = Factory.New<ReportCommand>();
			Assert("Precondition: report.IsApplicable = true", report.IsApplicable);

			report.SU_FilterList = nameof(DocumentFilters.TF) + "=Y";
			Assert("Precondition: report.IsApplicable = false", !report.IsApplicable);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA.PK.ToGuid(), Guid.Empty))
			{
				Assert("report.IsApplicable = true since company has tax configuration setup", report.IsApplicable);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchB.PK.ToGuid(), Guid.Empty))
			{
				Assert("report.IsApplicable = false since company does not have tax configuration setup", !report.IsApplicable);
			}
		}

		public void TestIsApplicableEnableReportSetups()
		{
			var report = Factory.New<ReportCommand>();
			AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				report.SU_FilterList = nameof(DocumentFilters.ERS) + "=Y";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				AccountingMasterFilesRegistry.Instance.EnableReportSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				report.SU_FilterList = nameof(DocumentFilters.ERS) + "=N";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestIsApplicable_ByComplianceSequenceModuleVisibility()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.China;
			item.SubType = "TXA";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			item.OriginalRule = OriginalRuleCodes.AllTransactions;
			item.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var report = Factory.New<ReportCommand>();
			AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				report.SU_FilterList = nameof(DocumentFilters.CMP) + "=Y";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				report.SU_FilterList = nameof(DocumentFilters.CMP) + "=N";
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				report.SU_FilterList = nameof(DocumentFilters.CMP) + "=";
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestIsApplicable_ByComplianceSubTypeIncludeAPLedger()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AP";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.China;
			item.SubType = "TXA";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			item.OriginalRule = OriginalRuleCodes.AllTransactions;
			item.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var report = Factory.New<ReportCommand>();
			AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				report.SU_FilterList = DocumentFilters.COMAP + "=Y";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.COMAP + "!=Y";
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.COMAP + "=";
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestIsApplicable_ByComplianceSubTypeNotIncludeAPLedger()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AP";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.China;
			item.SubType = "TXA";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			item.OriginalRule = OriginalRuleCodes.AllTransactions;
			item.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var report = Factory.New<ReportCommand>();
			AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				report.SU_FilterList = DocumentFilters.COMNOAP + "=Y";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.COMNOAP + "!=Y";
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.COMNOAP + "=";
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals("Precondition: report.IsApplicable", false, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestInvalidFiltersDontCauseExceptions()
		{
			var report = Factory.New<ReportCommand>();
			AssertEquals("Precondition: report.IsApplicable", true, report.IsApplicable);

			report.SU_FilterList = "SomeSillyCrud";
			AssertEquals("report.IsApplicable with Silly Filter", false, report.IsApplicable);

			report.SU_FilterList = nameof(DocumentFilters.MSGBKRCTY) + "=IMPUS";
			AssertEquals("report.IsApplicable with Recognised Key not valid in Reports", false, report.IsApplicable);

			report.SU_FilterList = nameof(DocumentFilters.CTY) + "=" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("report.IsApplicable with Recognised Key valid in Reports", true, report.IsApplicable);
		}

		public void TestIsApplicableWhenFilterExistsButItsNotPublished()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var report = Factory.New<ReportCommand>();
				report.SU_IsSystemDefined = true;
				report.SU_IsPublished = false;
				AssertEquals("Report.IsApplicable when not published", false, report.IsApplicable);

				report.SU_IsPublished = true;
				AssertEquals("Report.IsApplicable when published", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "=AU";
				AssertEquals("Report.IsApplicable with CTY=AU Filter", true, report.IsApplicable);

				report.SU_IsPublished = false;
				AssertEquals("Report.IsApplicable when not published with CTY=AU Filter", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "=SG";
				AssertEquals("Report.IsApplicable when not published with with CTY=SG Filter", false, report.IsApplicable);
			}
		}

		public void TestIsApplicable_ByCountryCode()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				var report = Factory.New<ReportCommand>();
				AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "=AU";
				AssertEquals("Should be applicable with CTY=AU Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "!=AU";
				AssertEquals("Should not be applicable with CTY!=AU Filter", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "=SG";
				AssertEquals("Should not be applicable with CTY=SG Filter", false, report.IsApplicable);

				GlbCompany.CurrentCompany.SetCountry("SG");
				AssertEquals("Should be applicable with CTY=SG Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "!=SG=US=GB";
				GlbCompany.CurrentCompany.SetCountry("SG");
				AssertEquals("With this multiple filter, report Should NOT be applicable for SG", false, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("US");
				AssertEquals("With this multiple filter, report Should NOT be applicable for US", false, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("GB");
				AssertEquals("With this multiple filter, report Should NOT be applicable for GB", false, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertEquals("With this multiple filter, report Should be applicable for other countries - eg AU", true, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("HK");
				AssertEquals("With this multiple filter, report Should be applicable for other countries - eg HK", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTY + "=US=HK";
				GlbCompany.CurrentCompany.SetCountry("SG");
				AssertEquals("With this multiple filter, report Should NOT be applicable for other countries - eg SG", false, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("US");
				AssertEquals("With this multiple filter, report Should be applicable for US", true, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("GB");
				AssertEquals("With this multiple filter, report Should NOT be applicable for other countries - eg GB", false, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("AU");
				AssertEquals("With this multiple filter, report Should NOT be applicable for other countries - eg AU", false, report.IsApplicable);
				GlbCompany.CurrentCompany.SetCountry("HK");
				AssertEquals("With this multiple filter, report Should be applicable for HK", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.BKRCTY + "=US";
				GlbCompany.CurrentCompany.SetCountry("PR");
				AssertEquals("Should be applicable with PR", true, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		public void TestIsApplicable_ByPublishedAndSystem()
		{
			var report = Factory.New<ReportCommand>();
			AssertEquals("Precondition, is applicable", true, report.IsApplicable);
			report.SU_IsSystemDefined = true;
			report.SU_IsPublished = false;
			AssertEquals("Should not be applicable with system unpublished flag", false, report.IsApplicable);
		}

		public void TestIsApplicable_ByCompanyCode()
		{
			var currentCompanyCode = GlbCompany.CurrentCompany.GC_Code;

			try
			{
				GlbCompany.CurrentCompany.GC_Code = "EDI";

				var report = Factory.New<ReportCommand>();
				AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CO + "=EDI";
				AssertEquals("Should be applicable with CO=EDI Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CO + "!=EDI";
				AssertEquals("Should not be applicable with CO!=EDI Filter", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CO + "=XYZ";
				AssertEquals("Should not be applicable with CO=XYZ Filter", false, report.IsApplicable);

				GlbCompany.CurrentCompany.GC_Code = "XYZ";
				AssertEquals("Should be applicable with CO=XYZ Filter", true, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_Code = currentCompanyCode;
			}
		}

		public void TestIsApplicable_ByLoginName()
		{
			var currentLoginName = GlbStaff.CurrentUser.GS_LoginName;

			try
			{
				var report = Factory.New<ReportCommand>();
				report.SU_FilterList = DocumentFilters.LoginName + "=CWSupport";

				GlbStaff.CurrentUser.GS_LoginName = "TEST";
				AssertEquals(false, report.IsApplicable);

				GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
				AssertEquals(true, report.IsApplicable);

				GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
				AssertEquals(true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.LoginName + "!=CWSupport";

				GlbStaff.CurrentUser.GS_LoginName = "TEST";
				AssertEquals(true, report.IsApplicable);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = currentLoginName;
			}
		}

		public void TestIsApplicable_ByNcts()
		{
			var helper = new Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Norway, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			Factory.Save();

			var euMembersProvider = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
			AssertEquals("PreReq - check that Germany is an NCTS country", true, euMembersProvider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Germany));
			AssertEquals("PreReq - check that Norway (non-EU)  is an NCTS country", true, euMembersProvider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Norway));
			AssertEquals("PreReq - check that Aus is not an NCTS country", false, euMembersProvider.IsCountryEuOrCtCountry(Core.Constants.CountryCodes.Australia));

			var originalCountry = GlbCompany.CurrentCompany.Country.RN_Code;

			try
			{
				RunTestIsApplicable_ByNcts(Core.Constants.CountryCodes.Germany, true, false);
				RunTestIsApplicable_ByNcts(Core.Constants.CountryCodes.Norway, true, false);
				RunTestIsApplicable_ByNcts(Core.Constants.CountryCodes.Australia, false, false);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestIsApplicable_ByEconomicGroupingCode()
		{
			var currentEconomicGroupingCode = GlbCompany.CurrentCompany.Country.RN_EconomicGrouping;

			try
			{
				GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;

				var report = Factory.New<ReportCommand>();
				AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTYEG + "=EUN";
				AssertEquals("Should be applicable with CTYEG=EUN Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTYEG + "!=EUN";
				AssertEquals("Should not be applicable with CTYEG!=EUN Filter", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.CTYEG + "=XYZ";
				AssertEquals("Should not be applicable with CTYEG=XYZ Filter", false, report.IsApplicable);

				GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = "XYZ";
				AssertEquals("Should be applicable with CTYEG=XYZ Filter", true, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = currentEconomicGroupingCode;
			}
		}

		public void TestIsApplicable_EUGB()
		{
			var currentEconomicGroupingCode = GlbCompany.CurrentCompany.Country.RN_EconomicGrouping;
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

				var report = Factory.New<ReportCommand>();
				AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.EUGB + "=N";
				AssertEquals("Should not be applicable with EUGB=N Filter", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.EUGB + "=Y";
				AssertEquals("Should be applicable with EUGB=Y Filter", true, report.IsApplicable);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
				AssertEquals("Should be applicable with EUGB=Y Filter as login country is in EU", true, report.IsApplicable);

				GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = "";
				AssertEquals("Should not be applicable with EUGB=Y Filter as neither login country not in EU nor it is in GB", false, report.IsApplicable);
			}
			finally
			{
				GlbCompany.CurrentCompany.Country.RN_EconomicGrouping = currentEconomicGroupingCode;
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountryCode;
			}
		}

		public void TestIsApplicable_ByIsComplianceDocumentModuleEnabled()
		{
			var report = Factory.New<ReportCommand>();
			AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				report.SU_FilterList = DocumentFilters.IsComplianceDocumentModuleEnabled + "=Y";
				AssertEquals("Only the filter is true, needs the registry setting to be true as well", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.IsComplianceDocumentModuleEnabled + "=N";
				AssertEquals("Pre-condition: both registry and filter need to be true", false, report.IsApplicable);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				report.SU_FilterList = DocumentFilters.IsComplianceDocumentModuleEnabled + "=Y";
				AssertEquals("Should be applicable with IsComplianceDocumentModuleEnabled=Y", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.IsComplianceDocumentModuleEnabled + "=N";
				AssertEquals("Only the registry is true, needs the filter setting to be true as well", false, report.IsApplicable);
			}
		}

		public void TestIsApplicable_TSD()
		{
			var report = Factory.New<ReportCommand>();
			AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

			var mockSettings = new Mock<Enterprise.Integration.Customs.Shared.ITemporaryStorageSettings>();
			mockSettings.Setup(x => x.IsUsingUCC5).Returns(false);
			mockSettings.Setup(x => x.IsUsingUCC6).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				report.SU_FilterList = DocumentFilters.TSD + "=Y";
				AssertEquals("Should not be applicable when both UCC5 and UCC6 are disabled", expected: false, report.IsApplicable);

				mockSettings.Setup(x => x.IsUsingUCC6).Returns(true);
				AssertEquals("Should be applicable when UCC5 is disabled and UCC6 is enabled", expected: true, report.IsApplicable);

				mockSettings.Setup(x => x.IsUsingUCC5).Returns(true);
				AssertEquals("Should be applicable when both UCC5 and UCC6 are enabled", expected: true, report.IsApplicable);

				mockSettings.Setup(x => x.IsUsingUCC6).Returns(false);
				AssertEquals("Should be applicable when UCC5 is enabled and UCC6 is disabled", expected: true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.TSD + "=N";
				AssertEquals("Should not be applicable when filter is N", expected: false, report.IsApplicable);

				mockSettings.Setup(x => x.IsUsingUCC6).Returns(true);
				AssertEquals("Should not be applicable when filter is N", expected: false, report.IsApplicable);

				mockSettings.Setup(x => x.IsUsingUCC5).Returns(false);
				AssertEquals("Should not be applicable when filter is N", expected: false, report.IsApplicable);

				mockSettings.Setup(x => x.IsUsingUCC6).Returns(false);
				AssertEquals("Should not be applicable when filter is N", expected: false, report.IsApplicable);
			}
		}

		public void TestIsApplicable_CSHB()
		{
			AssertEquals(false, GlbCompany.CurrentCompany.GC_IsGSTCashBasis);

			var report = Factory.New<ReportCommand>();
			AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

			report.SU_FilterList = DocumentFilters.CSHB + "=Y";
			AssertEquals("Should not be applicable with CSHB=Y", false, report.IsApplicable);

			report.SU_FilterList = DocumentFilters.CSHB + "=N";
			AssertEquals("Should not be applicable with CSHB=N", false, report.IsApplicable);

			GlbCompany.CurrentCompany.GC_IsGSTCashBasis = true;
			report.SU_FilterList = DocumentFilters.CSHB + "=Y";
			AssertEquals("Should be applicable with CSHB=Y", true, report.IsApplicable);

			report.SU_FilterList = DocumentFilters.CSHB + "=N";
			AssertEquals("Should not be applicable with CSHB=N", false, report.IsApplicable);
		}

		public void TestIsApplicable_SupportsMacro()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Guid.Empty))
			{
				var report = Factory.New<ReportCommand>();
				report.SU_MenuName = "test";
				report.SU_FilterList = $"\"<LoginName>\" == \"{Env.CurrentUser.LoginName}\"";
				AssertEquals("IsApplicable should be true", true, report.IsApplicable);

				report.SU_FilterList = "\"<InvalidMacro>\" == \"Value\"";
				AssertEquals("IsApplicable should be false", false, report.IsApplicable);
			}
		}

		public void TestIsApplicable_HasHVLVClearance()
		{
			var currentHasHVLVClearance = HVLVDataRegistry.HasHVLVClearance;

			try
			{
				HVLVDataRegistry.HasHVLVClearance = false;

				var report = Factory.New<ReportCommand>();
				AssertEquals("Should be applicable without Filter", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.HasHVLVClearance + "=Y";
				AssertEquals("Only the filter is true, needs the registry setting to be true as well", false, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.HasHVLVClearance + "=N";
				AssertEquals("Both registry and filter need to be true", false, report.IsApplicable);

				HVLVDataRegistry.HasHVLVClearance = true;

				report.SU_FilterList = DocumentFilters.HasHVLVClearance + "=Y";
				AssertEquals("Should be applicable with HasHVLVClearance=Y", true, report.IsApplicable);

				report.SU_FilterList = DocumentFilters.HasHVLVClearance + "=N";
				AssertEquals("Only the registry is true, needs the filter setting to be true as well", false, report.IsApplicable);
			}
			finally
			{
				HVLVDataRegistry.HasHVLVClearance = currentHasHVLVClearance;
			}
		}

		public void TestFilterList()
		{
			var command = Factory.New<ReportCommand>();
			AssertEquals("HasErrors", false, command.SU_FilterListInfo.HasErrors());

			command.SU_FilterList = "CTY-HideThisMenuItem";
			AssertEquals("HasErrors", true, command.SU_FilterListInfo.HasErrors());
			AssertEquals("Filter format is incorrect, you must enter a Code, followed by the '=' sign, and followed by the expected value. e.g. MOD=SEA", command.SU_FilterListInfo.GetErrors().GetFirstMessage());

			command.SU_FilterList = "BAD=FFF";
			AssertEquals("HasErrors", true, command.SU_FilterListInfo.HasErrors());
			Assert("Starts with ''BAD' code is incorrect'", command.SU_FilterListInfo.GetErrors().GetFirstMessage().StartsWith("'BAD' code is incorrect"));
			Assert("contains 'MOD'", command.SU_FilterListInfo.GetErrors().GetFirstMessage().IndexOf(nameof(DocumentFilters.MOD)) > 0);

			command.SU_FilterList = "CTY=HideThisMenuItem";
			AssertEquals("HasErrors", false, command.SU_FilterListInfo.HasErrors());
		}

		public void TestReportPrintAreaSetNew()
		{
			var printAreaErrors = "";

			var filter = new ZQuery();
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep");

			var menuItemCollection = new StmMenuItemCollection(Factory, filter);
			menuItemCollection.Load();

			foreach (StmMenuItem menuItem in menuItemCollection)
			{
				filter = new ZQuery();
				filter.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, SQLComparisonOperator.Equal, menuItem.PK);
				var pivots = Factory.Load<StmMenuTemplatePivot>(filter);
				if (pivots.Length > 0)
				{
					var templatePivot = pivots[0];
					var menuTemplate = Factory.Load<StmTemplate>(templatePivot.SI_SO);

					var excelFile = new XlsFile();
					using (var templateStream = new MemoryStream(menuTemplate.SO_Template, 0, menuTemplate.SO_Template.Length, false))
					{
						excelFile.Open(templateStream);
						TXlsNamedRange printArea = excelFile.GetNamedRange(((char)InternalNameRange.Print_Area).ToString(), 1);
						if (printArea != null)
						{
							printAreaErrors += "Business Context:" + menuItem.SU_BusinessContext + ", Report: " + menuItem.SU_MenuName
								+ " has the print area set to " + printArea.RangeFormula + System.Environment.NewLine;
						}
					}
				}
			}

			AssertEquals("Should not be any Print Areas set", "", printAreaErrors);
		}

		public void TestSchedules()
		{
			var command1 = Factory.New<ReportCommand>();

			var command1Task1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			command1Task1.S5_ParentID = command1.PK;

			var command1Task2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			command1Task2.S5_ParentID = command1.PK;

			var command2 = Factory.New<ReportCommand>();

			var command2Task1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			command2Task1.S5_ParentID = command2.PK;

			AssertEquals(2, command1.Schedules.Count);
			AssertEquals(1, command2.Schedules.Count);

			command1.Delete();
			AssertEquals(true, command1Task1.IsDeleted);
			AssertEquals(true, command1Task2.IsDeleted);
			AssertEquals(false, command2Task1.IsDeleted);

			command2.Delete();
			AssertEquals(true, command2Task1.IsDeleted);
		}

		public void TestValidationIsReportCommandValidation()
		{
			var report = Factory.New<ReportCommand>();
			Assert("Should be ReportCommandValidation", report.Validation is ReportCommandValidation);
		}

		public void TestReport()
		{
			var report = Factory.New<ReportCommand>();
			AssertNull("Should be null by default", report.GetReport());
		}

		public void TestReportsCanOnlyHaveASingleTemplate()
		{
			var report = Factory.NewWithValidTestData<ReportCommand>();
			report.SU_BusinessContext = "RepSystemReports";

			var pivot1 = report.Documents.AddNew();
			AssertNoRowErrors(pivot1);

			var pivot2 = report.Documents.AddNew();
			AssertNoRowErrors(pivot1);
			AssertHasRowError(pivot2, "Reports can only have a single template.");
		}

		public void TestSU_Calc_IsWebSupportable_ReadOnly()
		{
			var systemReport = Factory.New<ReportCommand>();
			systemReport.SU_BusinessContext = "ShipmentReport";
			systemReport.SU_IsPublished = true;
			systemReport.SU_IsSystemDefined = true;
			systemReport.SU_MenuName = "System Shipment Report";

			systemReport.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", true, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", true, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.SU_IsSystemDefined = false;
			systemReport.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemReport.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemReport.SU_Calc_IsWebSupportableInfo.ReadOnly);
		}

		DeliveryInstructions testInstructions;
		string tempPath;

		protected override void SetUp()
		{
			base.SetUp();

			tempPath = Env.TempPath + @"DocTest_" + Guid.NewGuid().ToString() + "\\";
			testInstructions = new DeliveryInstructions();
			testInstructions.Destination = DeliveryInstructionDestination.Disk;
			testInstructions.OutputDirectory = tempPath;

			if (Directory.Exists(tempPath))
			{
				try
				{
					Directory.Delete(tempPath, true);
				}
				catch
				{
					System.Threading.Thread.Sleep(5000);
					Directory.Delete(tempPath, true);
				}
			}

			Directory.CreateDirectory(tempPath);
		}

		protected override void TearDown()
		{
			base.TearDown();

			try
			{
				Directory.Delete(tempPath, true);
			}
			catch
			{
				//ignore, it will be cleaned next time
			}
		}

		void RunTestIsApplicable_ByNcts(string country, bool expectedWhenYes, bool expectedWhenNo)
		{
			GlbCompany.CurrentCompany.SetCountry(country);
			var report = Factory.New<ReportCommand>();
			AssertEquals(country + " should be applicable without Filter", true, report.IsApplicable);
			report.SU_FilterList = DocumentFilters.NCTS + "=Y";
			AssertEquals(country + " should be applicable with NCTS=Y filter", expectedWhenYes, report.IsApplicable);
			report.SU_FilterList = DocumentFilters.NCTS + "=N";
			AssertEquals(country + " should not be applicable with NCTS=N filter", expectedWhenNo, report.IsApplicable);
		}

		public void TestGetWarningBeforeBeingDeleted()
		{
			var command = Factory.New<ReportCommand>();
			AssertEquals(string.Empty, command.GetWarningBeforeBeingDeleted());

			var command1 = Factory.New<ReportCommand>();
			var command1Task1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			command1Task1.S5_ParentID = command1.PK;
			var command1Task2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			command1Task2.S5_ParentID = command1.PK;

			AssertEquals(2, command1.Schedules.Count);
			AssertEquals("This report has been scheduled, all related schedules will be deleted.", command1.GetWarningBeforeBeingDeleted());
		}
	}
}
