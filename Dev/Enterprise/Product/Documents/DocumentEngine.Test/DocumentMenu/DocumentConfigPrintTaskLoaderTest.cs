using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentConfigPrintTaskLoaderTest : TestCaseWithFactory
	{
		public void TestLoadWithInvalidBODocDataProvider()
		{
			var dummySupport = Factory.New<DummyBODocSupportableInvalidWrapper>();
			documentCommand.Parent = dummySupport;
			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));

			Factory.Save();

			var documentPack = new DocumentPack(documentCommand);
			var loader = new DocumentConfigDocumentPackLoader(documentPack);
			loader.Load(documentConfig, documentCommand.Parent);

			AssertEquals(0, documentPack.Count);
		}

		public void TestAccessingFieldFromTopLevelBusinessObject()
		{
			dummy.Z0_VarCharMax = "Hello World";

			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 3"));

			Factory.Save();

			using (var printTask = new PrintTask())
			{
				var documentPack = new DocumentPack(documentCommand);
				var loader = new DocumentConfigDocumentPackLoader(documentPack);
				loader.Load(documentConfig, documentCommand.Parent);

				printTask.Add(documentPack);
				printTask.Run(deliveryInstructions);
			}

			var printJobs = DeliveryTestHelper.GetPrintJobs(deliveryInstructions.DeliveryGroups[0]);
			AssertEquals(1, printJobs.Length);

			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("Field from top level business object should be printed.", "{B}-[Hello World]", excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestReportNameAppliesOverrideDocumentNameForDocBuilder()
		{
			dummy.Z0_VarCharMax = "Hello World";

			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_OverrideEmailSubject = "<Z0_VarCharMax>";
			documentConfig.S3_SI = document.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand))
			{
				var loader = new DocumentConfigDocumentPackLoader(documentPack);
				loader.Load(documentConfig, documentCommand.Parent);

				var report = documentPack[0] as Report;
				AssertNotNull(report);
				AssertEquals("Hello World", report.Name);
			}
		}

		public void TestLoadWithDocumentSupporterThatReliesOnMenuItemToGetGetBODocDataProviders()
		{
			var mocker = new MockRepository(MockBehavior.Default);
			var menuItem = Factory.New<StmMenuItem>();

			using (var documentPack = new DocumentPack(menuItem))
			{
				var dummy = Factory.New<DummyBusinessObject>();
				var documentSupporter = new DocumentSupporterThatNeedsMenuItemToGetDocumentWrappers(dummy);
				var documentConfig = mocker.Create<IDocumentConfig>();
				var documentSupportable = mocker.Create<IDocumentSupportable>();

				documentSupportable.Setup(m => m.DocumentSupporter).Returns(documentSupporter);
				documentConfig.Setup(m => m.OverrideDataContext).Returns(string.Empty);

				var loader = new DocumentConfigDocumentPackLoader(documentPack);
				AssertNoExceptionThrown(() => { loader.Load(documentConfig.Object, documentSupportable.Object); });
			}
		}

		public void TestLoadWithOneItemInDocumentConfig()
		{
			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));

			Factory.Save();

			using (var printTask = new PrintTask())
			{
				var documentPack = new DocumentPack(documentCommand);
				var loader = new DocumentConfigDocumentPackLoader(documentPack);
				loader.Load(documentConfig, documentCommand.Parent);

				printTask.Add(documentPack);
				printTask.Run(deliveryInstructions);
			}

			var printJobs = DeliveryTestHelper.GetPrintJobs(deliveryInstructions.DeliveryGroups[0]);
			AssertEquals(1, printJobs.Length);

			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be 1 strip.", "{B}-[This is Generic Section 1.]", excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestLoadWithOneDocumentConfigItemAndFilters_EvaluatesFalse()
		{
			AssertLoadWithOneConfigItemWithFilter("1 == 2", false);
		}

		public void TestLoadWithOneDocumentConfigItemAndFilters_EvaluatesTrue()
		{
			AssertLoadWithOneConfigItemWithFilter("1 == 1", true);
		}

		void AssertLoadWithOneConfigItemWithFilter(string filter, bool filterEvaluationExpectedResult)
		{
			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
			documentConfig.ConfigItems[0].S4_FilterList = filter;

			Factory.Save();

			using (var printTask = new PrintTask())
			{
				var documentPack = new DocumentPack(documentCommand);
				var loader = new DocumentConfigDocumentPackLoader(documentPack);
				loader.Load(documentConfig, documentCommand.Parent);

				printTask.Add(documentPack);
				printTask.Run(deliveryInstructions);
			}

			var printJobs = DeliveryTestHelper.GetPrintJobs(deliveryInstructions.DeliveryGroups[0]);
			AssertEquals(1, printJobs.Length);

			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				AssertEquals(filterEvaluationExpectedResult, "{B}-[This is Generic Section 1.]" == excelInterface.WorkSheets[0].ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenLoadingWithDocumentComandInDifferentFactory()
		{
			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));

			var command = new BusinessObjectFactory().New<StmMenuItem>();
			var documentPack = new DocumentPack(command);
			var loader = new DocumentConfigDocumentPackLoader(documentPack);
			loader.Load(documentConfig, documentCommand.Parent);
		}

		public void TestDocumentDirectionIsPresevered()
		{
			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));

			documentCommand.SU_DocumentDirection = nameof(DocumentDirection.ARV);

			Factory.Save();

			var documentPack = new DocumentPack(documentCommand);
			var loader = new DocumentConfigDocumentPackLoader(documentPack);
			loader.Load(documentConfig, documentCommand.Parent);

			AssertEquals(1, documentPack.Count);
			var report = documentPack[0] as Report;
			AssertEquals(documentCommand.SU_DocumentDirection, report.Direction.ToString());
		}

		public void TestDocTypeCode()
		{
			var docType = Factory.LoadTop1<RefDocType>(new ZQuery());

			var dummy1 = Factory.New<DummyBODocSupportableWithDocTypeCode>();
			documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy1;
			documentCommand.SU_MenuName = "test";
			document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;
			document.SI_RT_DocType = docType.PK;

			deliveryInstructions = GetDeliveryInstructions();
			var documentConfig = document.DocConfigs.AddNew();
			documentConfig.S3_SI = document.PK;
			documentConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));

			Factory.Save();

			var documentPack = new DocumentPack(documentCommand);
			var loader = new DocumentConfigDocumentPackLoader(documentPack);
			loader.Load(documentConfig, documentCommand.Parent);

			var report = documentPack[0] as Report;
			foreach (var dataProvider in report.DataProviderList.AllDataProviders)
			{
				if (dataProvider is IDocTypeCode)
				{
					AssertEquals(docType.RT_DocType, ((IDocTypeCode)dataProvider).DocTypeCode);
				}
			}
		}

		#region Implementation

		DeliveryInstructions GetDeliveryInstructions()
		{
			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Language = Enterprise.Core.Constants.Languages.English;
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "unit.test@cargowise.com";
			recipient.AttachmentType = AttachmentTypeList.Codes.Xls;

			return deliveryInstructions;
		}

		class DocumentSupporterThatNeedsMenuItemToGetDocumentWrappers : DocumentSupporter
		{
			public DocumentSupporterThatNeedsMenuItemToGetDocumentWrappers(BusinessObject parent)
				: base(parent)
			{
			}

			public override BusinessContext BusinessContext
			{
				get { throw new NotImplementedException(); }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new NotImplementedException(); }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				var anyPropertyOnMenuItem = commandBeingRun.SU_MenuName;

				return Array.Empty<DocumentWrapper>();
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				throw new NotImplementedException();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[<Z0_VarCharMax>]
{A}-[#EndOfReport]");

			dummy = Factory.New<DummyBODocSupportable>();
			documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			deliveryInstructions = GetDeliveryInstructions();
		}

		StmTemplateBase template;
		DummyBODocSupportable dummy;
		DocumentCommand documentCommand;
		StmMenuTemplatePivotBase document;
		DeliveryInstructions deliveryInstructions;

		#endregion
	}
}
