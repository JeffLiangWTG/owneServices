using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Macros;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class VisualizableDocumentCommandProviderTest : TestCaseWithUXmlSupport
	{
		public void TestGetCommand()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);

			var template = Factory.New<VisualizerTemplate>();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(tempFileName);
			}

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var dummy = Factory.New<Forwarding.IForwardingConsol>();

			Factory.Save();

			var provider = new VisualizableDocumentCommandProvider();

			var command = provider.GetCommand((BusinessObject)dummy, menuItem, null);

			AssertNotNull("command", command);

			command.Execute();

			AssertType("visualizer form has been shown", typeof(DocumentVisualizerForm), ZFormModaliser.LastFormShownDialogForTest);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCommand_ReopenForm()
		{
			var refreshDocumentCommand = new RefreshDocumentCommandForTest();
			var documentDataSource = new { Description = "test" };

			using (DataContextManagersSubstitution())
			using (DummyVisualizableDocumentSupporter.TempSetGetCustomCommandsImpl(_ => new ICommand[] { refreshDocumentCommand }))
			using (DummyVisualizableDocumentSupporter.TempSetGetDocDataObjectImpl((parent, dataContext, parameters) => new Either<string, object>(documentDataSource)))
			{
				ZFormModaliser.ShowDialogsInTest = true;

				var menuItem = Factory.New<VisualizerMenuItem>();
				menuItem.SU_MenuName = "Test Doc1";
				menuItem.SU_BusinessContext = nameof(BusinessContext.Test);

				var template = Factory.New<VisualizerTemplate>();
				template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateWithDocData.FilePath);

				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SU = menuItem.PK;
				pivot.SI_SO = template.PK;
				pivot.SI_DocumentTitle = "abracadabra";
				pivot.SI_DataStoreName = "7-11";

				var dummy = Factory.New<DummyWithUXmlSupport>();

				Factory.Save();

				var provider = new VisualizableDocumentCommandProvider();

				var command = provider.GetCommand(dummy, menuItem, null);

				AssertNotNull("command", command);

				var docVisualizerFormShowCount = 0;

				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					if (!(form is DocumentVisualizerForm))
					{
						return;
					}

					docVisualizerFormShowCount++;

					if (docVisualizerFormShowCount == 1)
					{
						refreshDocumentCommand.Invoke();
					}
				});

				command.Execute();

				AssertEquals("visualizer form has been shown twice", 2, docVisualizerFormShowCount);
			}
		}

		sealed class RefreshDocumentCommandForTest : CustomCommand
		{
			public override string Id => "CustomCommand";
			public override string Caption => "Custom Command";
			public override bool IsEnabled => true;
			public override bool IsVisible => true;

			public override bool Invoke()
			{
				var broker = documentInfo?.Services?.Resolve<IEventBroker>();
				broker?.Publish(new DocumentHardRefreshEvent(documentInfo.Document));
				return true;
			}
		}

		public void TestGetCommand_NoBusinessObject()
		{
			var provider = new VisualizableDocumentCommandProvider();

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);

			var template = Factory.New<VisualizerTemplate>();

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var command = provider.GetCommand(null, menuItem, null);

			AssertNull("command", command);
		}

		public void TestGetCommand_NoMenuItem()
		{
			var provider = new VisualizableDocumentCommandProvider();

			var dummy = Factory.New<DummyDocumentSupportable>();

			var command = provider.GetCommand(dummy, null, null);

			AssertNull("command", command);
		}
	}
}
