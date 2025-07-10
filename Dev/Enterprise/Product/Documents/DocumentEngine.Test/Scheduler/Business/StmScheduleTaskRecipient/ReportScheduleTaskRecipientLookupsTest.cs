using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Scheduler.Business;
using Enterprise.Scheduler.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class ReportScheduleTaskRecipientLookupsTest : StmScheduleTaskRecipientLookupsTest
	{
		protected override void TestDeliveryRecipientTypesCore()
		{
			AssertEquals("Count", 4, Recipient.Lookups.DeliveryRecipientTypes.Count);
			AssertEquals("GetCodeFromDescription(\"Contact\")", ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("Contact"));
			AssertEquals("GetCodeFromDescription(\"Group\")", ScheduledReportDeliveryRecipientConstants.RecipientType.Group, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("Group"));
			AssertEquals("GetCodeFromDescription(\"Staff\")", ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("Staff"));
			AssertEquals("GetCodeFromDescription(\"eDoc\")", ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc, Recipient.Lookups.DeliveryRecipientTypes.GetCodeFromDescription("eDoc"));
		}

		protected override void TestAttachmentTypesCore()
		{
			AssertEquals(11, Recipient.Lookups.AttachmentTypes.Count);
			Assert("Attachment types should contain XLS.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain TIF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain CSV.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
			Assert("Attachment types should contain HTML.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			Assert("Attachment types should contain TXT_SEMI.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
			Assert("Attachment types should contain TXT_COMM.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
			Assert("Attachment types should contain TXT_PIPE.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));

			ReportScheduleTask task = Factory.NewWithValidTestData<ReportScheduleTask>();
			Recipient.S6_S5 = task.PK;
			task.S5_ParentID = new CargoWise.Types.ZGuid("5f796ea9-5ea1-4684-b1d5-9e7c57e6c97e"); //1-Stop Vessel Arrival Report

			AssertEquals(13, Recipient.Lookups.AttachmentTypes.Count);
			Assert("Attachment types should contain XLS.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain TIF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain CSV.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
			Assert("Attachment types should contain CS2.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.CsvWithHeadings));
			Assert("Attachment types should contain XML.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xml));
			Assert("Attachment types should contain HTML.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			Assert("Attachment types should contain TXT_SEMI.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
			Assert("Attachment types should contain TXT_COMM.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
			Assert("Attachment types should contain TXT_PIPE.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));

			task.S5_ParentID = CargoWise.Types.ZGuid.Empty;

			AssertEquals(11, Recipient.Lookups.AttachmentTypes.Count);
			Assert("Attachment types should contain XLS.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain TIF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain CSV.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Csv));
			Assert("Attachment types should contain HTML.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
			Assert("Attachment types should contain TXT_SEMI.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
			Assert("Attachment types should contain TXT_COMM.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
			Assert("Attachment types should contain TXT_PIPE.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));

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

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				report.SetScheduleTask(task);
				task.S5_ParentID = reportCommand.PK;
				Assert(task.DisableXLSXExport);

				Assert("Attachment types should not contain XLSX.", !Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			}

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DisableCSVExport]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			{
				report.SetScheduleTask(task);
				task = Factory.NewWithValidTestData<ReportScheduleTask>();
				task.S5_ParentID = reportCommand.PK;
				Assert(task.DisableCSVExport);

				Recipient.S6_S5 = task.PK;
				AssertEquals(10, Recipient.Lookups.AttachmentTypes.Count);
				Assert("Attachment types should contain XLS.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
				Assert("Attachment types should contain XLSX.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
				Assert("Attachment types should contain PDF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
				Assert("Attachment types should contain PDF/A.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
				Assert("Attachment types should contain TIF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
				Assert("Attachment types should contain HTML.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
				Assert("Attachment types should contain HTMF.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
				Assert("Attachment types should contain TXT_SEMI.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Semi));
				Assert("Attachment types should contain TXT_COMM.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Comm));
				Assert("Attachment types should contain TXT_PIPE.", Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Txt_Pipe));
			}
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

		void AssertAttachmentTypeShouldOnlyContainTemplateFormatItself(string templateName, bool expectedXls, bool expectedXlsx)
		{
			var task = Factory.NewWithValidTestData<ReportScheduleTask>();
			Recipient.S6_S5 = task.PK;
			var reportCommand = Factory.New<ReportCommand>();
			var document = reportCommand.Documents.AddNew();
			var template = Factory.New<StmTemplateBase>();
			var excelTemplate = new ExcelTemplateForUnitTesting(templateName, TestFilesSubFolder.ReportTestFiles);
			template.SO_Template = excelTemplate.GetAsByteArray();
			document.SI_SU = reportCommand.PK;
			document.SI_SO = template.PK;

			using var pack = new DocumentPack(reportCommand);
			using var report = Report.NewForTesting(pack);
			report.SetScheduleTask(task);
			task.S5_ParentID = reportCommand.PK;
			AssertEquals($"Template: {templateName}: Attachment types should {(expectedXls ? "" : "not")} contain XLS.", expectedXls, Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			AssertEquals($"Template: {templateName}: Attachment types should {(expectedXlsx ? "" : "not")} contain XLSX.", expectedXlsx, Recipient.Lookups.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
		}

		public void TestPrinters()
		{
			StmPrintQueue printer1 = Factory.New<StmPrintQueue>();
			StmPrintQueue printer2 = Factory.New<StmPrintQueue>();

			printer1.SQ_DisplayName = "x";
			printer2.SQ_DisplayName = "y";

			Env.Security.GetPrintQueueCheckPoint(printer2.PK.ToGuid(), null).IsAllowed = false;

			AssertEquals("Count", 1, Recipient.Lookups.Printers.Count);
			AssertEquals("Count", "x", Recipient.Lookups.Printers[0].Code);
		}

		public void TestPrintersWithParent()
		{
			PrintersWithParentCore(false, 1);
		}

		public void TestPrintersWithParent_IfParentAlreadyInTheList()
		{
			PrintersWithParentCore(true, 2);
		}

		void PrintersWithParentCore(bool allowPrinting, int printersCount)
		{
			var printer1 = Factory.New<StmPrintQueue>();
			printer1.SQ_DisplayName = "A printer";
			var printerParent = Factory.New<StmPrintQueue>();
			printerParent.SQ_DisplayName = "Parent name";
			printerParent.SQ_AllowPrinting = allowPrinting;
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			recipient.S6_SQ = printerParent.PK;
			var lookups = new ReportScheduleTaskRecipientLookups(recipient);

			AssertEquals("Printers count", printersCount, lookups.Printers.Count);
			AssertEquals("PrintersWithParent count", 2, lookups.PrintersWithParent.Count);
			AssertEquals("Printer name", "A printer", lookups.PrintersWithParent[0].Code);
			AssertEquals("Parent printer name", "Parent name", lookups.PrintersWithParent[1].Code);
		}

		new ReportScheduleTaskRecipient Recipient
		{
			get { return (ReportScheduleTaskRecipient)base.Recipient; }
		}

		protected override StmScheduleTaskRecipient NewStmScheduleTaskRecipient()
		{
			return Factory.New<ReportScheduleTaskRecipient>();
		}

		protected override int ExpectedNotifyModes
		{
			get { return 5; }
		}
	}
}
