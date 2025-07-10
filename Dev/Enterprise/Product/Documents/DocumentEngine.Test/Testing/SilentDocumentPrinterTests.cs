using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class SilentDocumentPrinterTests : TestCaseWithFactory
	{
		public void TestSupportSpecificLanguageForLegacyDocument_WithTranslateLegacyDocument()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Save Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[TranslateLegacyDocument]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[<DateTimeAsString('<Z0_Date>', 'dd-MMM-yy')>]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Date = new ZDateTime(2021, 11, 23);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;
			var queue = Factory.New<StmPrintQueue>();
			Factory.Save();

			AssertSpecificLanguage(dummy, documentCommand, queue.PK, "", "23-NOV-21");
			AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "", "23-NOV-21");
			AssertSpecificLanguage(dummy, documentCommand, queue.PK, "PL-PL", "23-LIS-21");
			AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "PL-PL", "23-LIS-21");
			AssertSpecificLanguage(dummy, documentCommand, queue.PK, "AR-AE", "‏23-نوفمبر-21");
			AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "AR-AE", "‏23-نوفمبر-21");
		}

		public void TestSupportSpecificLanguageForLegacyDocument_WithoutTranslateLegacyDocument()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Save Document",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#DocumentHeader]
{B}-[<DateTimeAsString('<Z0_Date>', 'dd-MMM-yy')>]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Date = new ZDateTime(2021, 11, 23);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;
			var queue = Factory.New<StmPrintQueue>();
			Factory.Save();

			var defaultLanguageSettings = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, Order = 1 }
			};

			GlbBranch.CurrentBranch.OrgProxy.OH_Language = "ZH-CN";

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguageSettings))
			{
				AssertSpecificLanguage(dummy, documentCommand, queue.PK, "", "23-NOV-21");
				AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "", "23-NOV-21");
				AssertSpecificLanguage(dummy, documentCommand, queue.PK, "PL-PL", "23-LIS-21");
				AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "PL-PL", "23-LIS-21");
			}
		}

		public void TestSupportSpecificLanguageForDocBuilder()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Date = new ZDateTime(2021, 11, 23);
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
$@"{{A}}-[#Config]
{{A}}-[DataContext=.DummyBODocSupportable]
{{A}}-[Name=Test]
{{A}}-[#ConfigurableSection:GEN, Test Date]
{{B}}-[<DateTimeAsString('<Z0_Date>', 'dd-MMM-yy')>]
{{A}}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Date"));
			var queue = Factory.New<StmPrintQueue>();
			Factory.Save();

			AssertSpecificLanguage(dummy, documentCommand, queue.PK, "", "23-NOV-21");
			AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "", "23-NOV-21");
			AssertSpecificLanguage(dummy, documentCommand, queue.PK, "PL-PL", "23-LIS-21");
			AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "PL-PL", "23-LIS-21");
			AssertSpecificLanguage(dummy, documentCommand, queue.PK, "AR-AE", "‏23-نوفمبر-21");
			AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "AR-AE", "‏23-نوفمبر-21");

			var defaultLanguageSettings = new DocumentDeliveryDefaultLanguagesCollection
			{
				new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, Order = 1 }
			};

			GlbBranch.CurrentBranch.OrgProxy.OH_Language = "ZH-CN";

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguageSettings))
			{
				AssertSpecificLanguage(dummy, documentCommand, queue.PK, "", "2021-11-23");
				AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "", "2021-11-23");
				AssertSpecificLanguage(dummy, documentCommand, queue.PK, "PL-PL", "23-LIS-21");
				AssertSpecificLanguage(dummy, documentCommand, ZGuid.Empty, "PL-PL", "23-LIS-21");
			}
		}

		public void TestSilentDocumentPrinter_Paper()
		{
			SetUpObjects();

			var queue = Factory.New<StmPrintQueue>();
			Factory.Save();

			var cmd = Factory.Load<DocumentCommand>(menuItem.PK);
			var silentPrinter = new SilentDocumentPrinter(Factory, dummyBODocSupportable, cmd);
			silentPrinter.Print(queue.PK, 1, false);
			Factory.Save();
			AssertPrintQueue(false);
		}

		public void TestSilentDocumentPrinter_Edocs()
		{
			SetUpObjects();

			var cmd = Factory.Load<DocumentCommand>(menuItem.PK);
			var silentPrinter = new SilentDocumentPrinter(Factory, dummyBODocSupportable, cmd);
			silentPrinter.Print(ZGuid.Empty, 0, true);
			Factory.Save();
			AssertPrintQueue(true);
		}

		void SetUpObjects()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				dummyBODocSupportable = Factory.New<DummyBODocSupportable>();
				AssertPrintQueueEmpty(dummyBODocSupportable.PK);
				menuItem = Factory.New<StmMenuItem>();
				menuItem.SU_BusinessContext = "Customs"; // anything will do 
				menuItem.SU_DocumentDirection = "ANY";
				menuItem.SU_ContactType = ContactType.NoContactType.Code;
				var template = Factory.New<StmTemplate>();
				template.SO_DataContext = "None";
				template.SO_Name = "BOO";
				template.SO_Template = resourceRetriever.GetBytes("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls");
				var pivot = Factory.New<StmMenuTemplatePivot>();
				pivot.SI_SU = menuItem.PK;
				pivot.SI_SO = template.PK;
				Factory.Save();
			}
		}

		void AssertPrintQueueEmpty(ZGuid parentId)
		{
			ZQuery q = new ZQuery();
			q.AddToFilter(StmPrintJobSchema.SP_ParentGuid, parentId);
			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(q);
			AssertNull(printJob);
		}

		void AssertPrintQueue(bool shouldBeQueuedForEdocs)
		{
			ZQuery q = new ZQuery();
			q.AddToFilter(StmPrintJobSchema.SP_ParentGuid, dummyBODocSupportable.PK);
			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(q);
			AssertNotNull(printJob);
			AssertContains("DummyBODocSupportable", printJob.SP_EmailAttachments);
			if (shouldBeQueuedForEdocs)
			{
				AssertEquals("DDS", printJob.SP_JobType);
			}
			else
			{
				AssertEquals("PRN", printJob.SP_JobType);
			}
		}

		void AssertSpecificLanguage(DummyBODocSupportable documentSupportable, DocumentCommand documentCommand, ZGuid queuePK, string language, string expectResult)
		{
			var silentPrinter = new SilentDocumentPrinter(Factory, documentSupportable, documentCommand);
			if (queuePK != ZGuid.Empty)
			{
				silentPrinter.Print(queuePK, 1, false, language: language);
			}
			else
			{
				silentPrinter.Print(queuePK, 0, true, language: language);
			}
			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, documentSupportable.PK));
			using (var excelInterface = new ExcelInterface())
			{
				AssertEquals(1, printJobs.Length);
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
				AssertMultilineASCIIEquals("Date should be formatted correctly.", $"{{B}}-[{expectResult}]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}

			printJobs[0].Delete();
			printJobs[0].Factory.Save();
		}

		DummyBODocSupportable dummyBODocSupportable;
		StmMenuItem menuItem;
	}
}
