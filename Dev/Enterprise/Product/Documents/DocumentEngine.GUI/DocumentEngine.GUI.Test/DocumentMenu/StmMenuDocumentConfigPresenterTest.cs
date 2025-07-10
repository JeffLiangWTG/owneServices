using System;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class StmMenuDocumentConfigPresenterTest : TestCaseWithFactory
	{
		StmTemplateBase template;
		StmMenuTemplatePivotBase document;

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
{B}-[This is Generic Section 3.]
{A}-[#ConfigurableSection:GEN, Generic Section 4]
{B}-[This is Generic Section 4.]
{A}-[#ConfigurableSection:GEN, Generic Section 5]
{B}-[This is Generic Section 5.]
{A}-[#ConfigurableSection:GEN, Text on Top Level Business Object]
{B}-[<Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Document";

			document = documentCommand.Documents.AddNew();
			document.SI_DocumentTitle = "Test Document";
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;
		}

		public void TestPreviewConfigItemWithFieldFromTopLevelBusinessObject()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var configItem = config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Text on Top Level Business Object"));

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			var presenter = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m => m.GetSelectedConfigItems()).Returns(new[] { configItem });
			mock.Setup(m => m.ShowSectionPreview(It.IsAny<SectionPreviewManager>()))
				.Callback((SectionPreviewManager manager) =>
				{
					var containsTopLevelBO = false;

					foreach (var dataProvider in manager.DocDataProviders)
					{
						containsTopLevelBO = dataProvider.ParentBusinessObject == dummy;

						if (containsTopLevelBO)
						{ break; }
					}

					Assert("The section preview manager should contain data provider to the top level business object.", containsTopLevelBO);
				});

			mock.Raise(m => m.PreviewConfigItemButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestPreviewDocumentConfig_InvalidOriginalDataContext()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.S3_Description = document.DocumentTitle;
			template.SO_DataContext = "WrongValue";
			var configItem = config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Text on Top Level Business Object"));

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			temporaryConfig.S3_Description = "TempConfig";
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			var presenter = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m =>
			m.ShowError(
				"The DataContext parameter in the #config of the template has been entered incorrectly. Please check the value and then try generating the document again.",
				"DataContext is invalid"));

			mock.Raise(m => m.PreviewButtonClicked += null, view, EventArgs.Empty);

			mock.Verify(m => m.ShowPreview(It.IsAny<PrintTask>()), Times.Never());
		}

		[ExpectNoExceptions]
		public void TestPreviewConfigItemPassesDocumentType()
		{
			var docType = Factory.LoadTop1<RefDocType>(new ZQuery());
			var dummy = Factory.New<DummyBODocSupportableWithDocTypeCode>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_MenuName = "test";
			document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;
			document.SI_RT_DocType = docType.PK;
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			Factory.Save();

			var configItem = config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Text on Top Level Business Object"));
			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			var presenter = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItem });
			mock.Setup(m => m.ShowSectionPreview(It.IsAny<SectionPreviewManager>()))
				.Callback((SectionPreviewManager manager) =>
				{
					foreach (var dataProvider in manager.DocDataProviders)
					{
						if (dataProvider is IDocTypeCode)
						{
							AssertEquals(docType.RT_DocType, ((IDocTypeCode)dataProvider).DocTypeCode);
						}
					}
				});

			mock.Raise(m => m.PreviewConfigItemButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestPreviewConfigItemWithBODocDataProvidersIsNull()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var configItem = config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var dummy = Factory.New<DummyBODocSupportableForTest>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItem });

			mock.Raise(m => m.PreviewConfigItemButtonClicked += null, view, EventArgs.Empty);
			mock.Verify();
		}

		public void TestPreviewNonExistingConfigItem()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;

			var configItem = config.ConfigItems.AddNew();
			configItem.S4_PrintOrder = 1;
			configItem.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
			configItem.S4_SectionItemName = "I Don't Exist";

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var dummy = Factory.New<DummyBODocSupportableForTest>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			bool eventTriggered = false;
			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItem });
			mock.Setup(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()))
				.Callback(() =>
				{
					eventTriggered = true;
				});

			mock.Raise(m => m.PreviewConfigItemButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			Assert("Should trigger event to show error message", eventTriggered);
		}

		class DummyBODocSupportableForTest : DummyBODocSupportable
		{
			public DummyBODocSupportableForTest(BusinessObjectFactory factory, DataRow dataRow)
				: base(factory, dataRow)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return new DummyBODocSupportableDocumentSupporterForTest(this); }
			}

			class DummyBODocSupportableDocumentSupporterForTest : DummyBODocSupportableDocumentSupporter
			{
				public DummyBODocSupportableDocumentSupporterForTest(DummyBODocSupportable docDummyBusinessObject)
					: base(docDummyBusinessObject)
				{
				}

				protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext,
					IStmMenuItem commandBeingRun)
				{
					return null;
				}
			}
		}

		public void TestMoveMultipleConfigItemsDown()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 2"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 3"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 4"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 5"));

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var configItems = temporaryConfig.ConfigItems;
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			_ = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 1
2: BDY: Generic Section 2
3: BDY: Generic Section 3
4: BDY: Generic Section 4
5: BDY: Generic Section 5", GetConfigItemsDescription(temporaryConfig.ConfigItems));

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItems[0], configItems[1] });
			mock.Raise(m => m.MoveDownButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 3
2: BDY: Generic Section 1
3: BDY: Generic Section 2
4: BDY: Generic Section 4
5: BDY: Generic Section 5", GetConfigItemsDescription(temporaryConfig.ConfigItems));

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItems[1], configItems[3] });
			mock.Raise(m => m.MoveDownButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 3
2: BDY: Generic Section 2
3: BDY: Generic Section 1
4: BDY: Generic Section 5
5: BDY: Generic Section 4", GetConfigItemsDescription(temporaryConfig.ConfigItems));

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItems[3], configItems[4] });
			mock.Raise(m => m.MoveDownButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 3
2: BDY: Generic Section 2
3: BDY: Generic Section 1
4: BDY: Generic Section 5
5: BDY: Generic Section 4", GetConfigItemsDescription(temporaryConfig.ConfigItems));
		}

		public void TestMoveMultipleConfigItemsUp()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 2"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 3"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 4"));
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 5"));

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var configItems = temporaryConfig.ConfigItems;
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			var presenter = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 1
2: BDY: Generic Section 2
3: BDY: Generic Section 3
4: BDY: Generic Section 4
5: BDY: Generic Section 5", GetConfigItemsDescription(temporaryConfig.ConfigItems));

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItems[3], configItems[4] });
			mock.Raise(m => m.MoveUpButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 1
2: BDY: Generic Section 2
3: BDY: Generic Section 4
4: BDY: Generic Section 5
5: BDY: Generic Section 3", GetConfigItemsDescription(temporaryConfig.ConfigItems));

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItems[1], configItems[3] });
			mock.Raise(m => m.MoveUpButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 2
2: BDY: Generic Section 1
3: BDY: Generic Section 5
4: BDY: Generic Section 4
5: BDY: Generic Section 3", GetConfigItemsDescription(temporaryConfig.ConfigItems));

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new[] { configItems[0], configItems[1] });
			mock.Raise(m => m.MoveUpButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertMultilineASCIIEquals("The configs are not in the correct order.",
@"1: BDY: Generic Section 2
2: BDY: Generic Section 1
3: BDY: Generic Section 5
4: BDY: Generic Section 4
5: BDY: Generic Section 3", GetConfigItemsDescription(temporaryConfig.ConfigItems));
		}

		string GetConfigItemsDescription(StmMenuDocumentConfigItemCollection configItems)
		{
			var builder = new StringBuilder();
			var line = 1;

			foreach (var configItem in configItems.OfType<StmMenuDocumentConfigItem>())
			{
				builder.AppendLine(string.Format(
					"{0}: {1}: {2}",
					line++,
					configItem.S4_SectionType,
					configItem.S4_SectionItemName));
			}

			return builder.ToString();
		}

		public void TestMoveConfigItem()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			{
				var configItems = config.ConfigItems;
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 2"));
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 3"));
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 4"));
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 5"));
			}

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			var presenter = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			AssertEquals(5, temporaryConfig.ConfigItems.Count);
			var configItem1 = temporaryConfig.ConfigItems[0];
			var configItem2 = temporaryConfig.ConfigItems[1];
			var configItem3 = temporaryConfig.ConfigItems[2];
			var configItem4 = temporaryConfig.ConfigItems[3];
			var configItem5 = temporaryConfig.ConfigItems[4];

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(Array.Empty<StmMenuDocumentConfigItem>());
			mock.Raise(m => m.MoveDownButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new StmMenuDocumentConfigItem[] { configItem3 });
			mock.Setup(m => m.SelectConfigItems(configItem3));
			mock.Raise(m => m.MoveDownButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertEquals(1, configItem1.S4_PrintOrder);
			AssertEquals(2, configItem2.S4_PrintOrder);
			AssertEquals(3, configItem4.S4_PrintOrder);
			AssertEquals(4, configItem3.S4_PrintOrder);
			AssertEquals(5, configItem5.S4_PrintOrder);

			mock.Setup(m => m.GetSelectedConfigItems())
				.Returns(new StmMenuDocumentConfigItem[] { configItem2 });
			mock.Setup(m => m.SelectConfigItems(configItem2));
			mock.Raise(m => m.MoveUpButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			AssertEquals(1, configItem2.S4_PrintOrder);
			AssertEquals(2, configItem1.S4_PrintOrder);
			AssertEquals(3, configItem4.S4_PrintOrder);
			AssertEquals(4, configItem3.S4_PrintOrder);
			AssertEquals(5, configItem5.S4_PrintOrder);
		}

		[ExpectNoExceptions]
		public void TestDoNotAllowUserToChangeAnythingIfConfigIsReadOnly()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.ReadOnly = true;
			{
				var configItems = config.ConfigItems;
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
				configItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 2"));
			}

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			_ = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m =>
			m.ShowError(
				"Cannot edit this Document Config - 'System' Configurations are Read Only.",
				"Read Only"));
			mock.Raise(m => m.AddButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Setup(m =>
				m.ShowError(
					"Cannot edit this Document Config - 'System' Configurations are Read Only.",
					"Read Only"));
			mock.Raise(m => m.RemoveButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Setup(m =>
				m.ShowError(
					"Cannot edit this Document Config - 'System' Configurations are Read Only.",
					"Read Only"));
			mock.Raise(m => m.MoveDownButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Setup(m =>
				m.ShowError(
					"Cannot edit this Document Config - 'System' Configurations are Read Only.",
					"Read Only"));
			mock.Raise(m => m.MoveUpButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();
		}

		public void TestAddOneSectionPreviewThenAddAnotherSectionAndPreviewASecondTimeShouldBeUpdated()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.S3_Description = "TempConfigDescription";
			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			temporaryConfig.S3_Description = "TempConfigDescription";
			var dummy = Factory.New<DummyBODocSupportable>();

			Factory.Save();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			var presenter = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m => m.GetSelectedSections())
				.Returns(new[] { template.TemplateSections.Find("Generic Section 1") });
			mock.Raise(m => m.AddButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Setup(m => m.ShowPreview(It.IsAny<PrintTask>()))
				.Callback((PrintTask printTask) =>
				{
					AssertEquals(1, printTask.Count);
					AssertEquals(1, printTask[0].Count);

					var report = (Report)printTask[0][0];
					AssertMultilineASCIIEquals("There should be one strip in this report.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]",
						report.XlInterface.WorkSheets[0].ToString());
				});

			mock.Raise(m => m.PreviewButtonClicked += null, view, EventArgs.Empty);
			mock.Verify();

			mock.Setup(m => m.GetSelectedSections())
				.Returns(new[] { template.TemplateSections.Find("Generic Section 2") });
			mock.Raise(m => m.AddButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Setup(m => m.ShowPreview(It.IsAny<PrintTask>()))
				.Callback((PrintTask printTask) =>
				{
					AssertEquals(1, printTask.Count);
					AssertEquals(1, printTask[0].Count);

					var report = (Report)printTask[0][0];
					AssertMultilineASCIIEquals("There should be now two strips in this report.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[This is Generic Section 1.]
{A}-[#SectionBody]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]",
						report.XlInterface.WorkSheets[0].ToString());
				});

			mock.Raise(m => m.PreviewButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();
		}

		public void TestViewShouldShowPreviewErrorsDialogWhenThereAreErrorsOnTheDocumentConfig()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.S3_OverrideDataContext = "This is an invalid data context.";
			config.S3_Description = document.DocumentTitle;
			config.RunPreSaveValidation();
			AssertEquals("Pre-condition: There are errors on the document config.", true, config.HasErrors);

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			temporaryConfig.S3_Description = "TempConfigDescription";
			var dummy = Factory.New<DummyBODocSupportable>();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			_ = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m => m.ShowError("There are errors - can't preview.", "Errors"));
			mock.Raise(m => m.PreviewButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Verify(m => m.ShowPreview(It.IsAny<PrintTask>()), Times.Never());
		}

		public void TestViewShouldShowPreviewWhenThereAreNoErrorsOnTheDocumentConfig()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.S3_Description = document.DocumentTitle;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
			config.RunPreSaveValidation();
			AssertEquals("Pre-condition: There should be no errors on the document config.", false, config.HasErrors);

			Factory.Save();

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			temporaryConfig.S3_Description = "TempDescription";
			var dummy = Factory.New<DummyBODocSupportable>();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			_ = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Setup(m => m.ShowPreview(It.IsAny<PrintTask>()));
			mock.Raise(m => m.PreviewButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		public void TestShowMessageWhenDataContextIsInvalid()
		{
			var config = document.DocConfigs.AddNew();
			config.S3_SI = document.PK;
			config.S3_Description = document.DocumentTitle;
			config.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Generic Section 1"));
			Factory.Save();

			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);
			temporaryConfig.S3_OverrideDataContext = "GenericBasicLabel";
			temporaryConfig.S3_Description = "GenericTempConfig";
			var dummy = Factory.New<DummyBODocSupportableForInvalidContextTest>();

			var mock = new Mock<IStmMenuDocumentConfigForm>();
			var view = mock.Object;
			_ = new StmMenuDocumentConfigPresenter(view, temporaryConfig, dummy);

			mock.Raise(m => m.PreviewButtonClicked += null, view, EventArgs.Empty);
			mock.VerifyAll();

			mock.Verify(m => m.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			mock.Verify(m => m.ShowPreview(It.IsAny<PrintTask>()), Times.Never());

			AssertEquals("Cannot generate document with the selected data context.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class DummyBODocSupportableForInvalidContextTest : DummyBODocSupportable
		{
			public DummyBODocSupportableForInvalidContextTest(BusinessObjectFactory factory, DataRow dataRow)
				: base(factory, dataRow)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return new DummyBODocSupportableDocumentSupporterForInvalidContextTest(this); }
			}
		}

		class DummyBODocSupportableDocumentSupporterForInvalidContextTest : DummyBODocSupportableDocumentSupporter
		{
			public DummyBODocSupportableDocumentSupporterForInvalidContextTest(DummyBODocSupportableForInvalidContextTest docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return null;
			}
		}
	}
}
