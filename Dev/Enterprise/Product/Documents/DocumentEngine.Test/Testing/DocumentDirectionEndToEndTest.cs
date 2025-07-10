using System.Data;
using System.IO;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentDirectionEndToEndTest : TestCaseWithFactory
	{
		public void TestDocumentDirectionCanBeAccessedInContstructorWhenUsingADocumentPack()
		{
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TestDocumentDirectionDataSource";
					workSheet[2, 0] = "#SectionBody";
					workSheet[3, 1] = "<DocumentDirectionSavedInConstructor>";
					workSheet[4, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				var menuItem = Factory.New<DocumentCommand>();
				menuItem.SU_MenuName = "My Document";
				menuItem.SU_DocumentDirection = nameof(DocumentDirection.DEP);

				var template = Factory.New<StmTemplateBase>();
				template.SO_Template = excelTemplate.GetAsByteArray();
				template.SO_Name = "My Template";
				template.SO_DataContext = nameof(DataContext.None);

				var menuTemplateLink = Factory.New<StmMenuTemplatePivotBase>();
				menuTemplateLink.SI_SU = menuItem.PK;
				menuTemplateLink.SI_SO = template.PK;
				menuTemplateLink.SI_DocumentTitle = "My Document Title";

				var businessObject = Factory.New<DummyBODocSupportable>();

				using (var documentPack = new DocumentPack(menuItem, businessObject, new RuntimeOptions.UserControlProviderList(), null))
				{
					var uiProvider = new PrintTaskForcePreviewTestingUIProvider();
					using (new PrintTaskUIProviderFactory.OverriderForTesting(uiProvider))
					{
						try
						{
							PrintTask printTask = new PrintTask();
							printTask.Add(documentPack);
							printTask.Run(EnvProxy.Instance.Security.None);
						}
						finally
						{
							ExceptionReporterTestListener.Instance.Clear();
						}
					}

					AssertMultilineASCIIEquals("uiProvider.LastErrors", null, uiProvider.LastErrors);
					AssertMultilineASCIIEquals("uiProvider.LastSheetRendered", "{B}-[DEP]", uiProvider.LastSheetRendered);
				}
			}
		}

		class DummyBODocSupportable : DummyBusinessObject, IDocumentSupportable
		{
			public DummyBODocSupportable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return new DummyBODocSupportableDocumentSupporter(this); }
			}
		}

		class DummyBODocSupportableDocumentSupporter : DocumentSupporter
		{
			public DummyBODocSupportableDocumentSupporter(DummyBODocSupportable docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
				Parent = docDummyBusinessObject;
			}
			readonly DummyBODocSupportable Parent;

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				return new DataContext[] { DataContext.None };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint { get { return Env.Security.None; } }

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return new DocumentWrapper[] { new DummyDocumentWrapper(this.Parent) };
			}
		}

		class DummyDocumentWrapper : DocBaseWrapperBaseWithImageSupport
		{
			public DummyDocumentWrapper(DummyBusinessObject boToWrap)
				: base(boToWrap, boToWrap.Factory)
			{
				this.DocumentDirectionSavedInConstructor = this.DocumentDirection;
			}

			public ZString DocumentDirectionSavedInConstructor
			{
				get;
				private set;
			}
		}
	}
}
