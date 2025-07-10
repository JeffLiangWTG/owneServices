using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DocumentViewExTest : TestCaseWithUXmlSupport
	{
		#region TestCreateDocument

		public void TestCreateDocument()
		{
			using (DataContextManagersSubstitution())
			{
				var worksheet = DummyWorksheet.Parse(
					@"#Config:Name=""Test Document"":DataContext=""UXML""
#End
#Body
	Code: <Code>
	Description: <Description>
#End");

				var dummy = Factory.New<DummyWithUXmlSupport>();
				dummy.Z0_Code = "XXX";
				dummy.Z0_Description = "You don't want to know";

				var pivot = Factory.New<VisualizerMenuTemplatePivot>();

				var menuItem = Factory.New<VisualizerMenuItem>();
				var template = Factory.New<VisualizerTemplate>();

				pivot.SI_SU = menuItem.PK;
				pivot.SI_SO = template.PK;
				pivot.SI_DataStoreName = "TestDocData";

				var builder = new XlsFileBuilder(worksheet);
				var xls = builder.Build();

				using (var templateStream = new MemoryStream())
				{
					xls.Save(templateStream);

					template.SO_Template = templateStream.ToArray();
				}

				Factory.Save();

				var services = new ServiceContainer();
				services.Register<IDocumentSecurityService>(new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy));
				services.Register<IPageViewBuildService>(() => new PageViewBuildService());
				services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
				services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
				services.Register<IUserNotificationService>(() => new UserNotificationService());
				services.Register<IDocumentToolsService>(() => new DocumentToolsService());
				services.Register<IEventBroker>(new EventBroker());
				services.Register<IConsoleService>(new ConsoleService());
				services.Register<IResourceProvider>(new DummyResourcesProvider());
				services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

				var pack = new DocumentInfoPack(services, dummy, null, menuItem.SU_MenuName, new[] { pivot });

				using (var form = DocumentVisualizerForm.CreateView(pack))
				{
					var content = GetContent(form);

					AssertMultilineASCIIEquals("document UI element",
@"Filling rectangle with Vertical gradient 0 and 0
Drawing text 'Code: XXX'
Filling rectangle with Vertical gradient 0 and 0
Drawing text 'Description: You don't want to know'
",
						content);
				}
			}
		}

		public void TestTheExceptionOfConstructionReported()
		{
			ErrorReporter.Clear();
			try
			{
				new DocumentViewEx(null, null);
			}
			catch
			{
			}

			AssertContains("Error initializing DocumentViewEx", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		string GetContent(DocumentVisualizerForm form)
		{
			var documentView = (DocumentView)form.Controls.Find("documentView", true).Single();

			var stringBuilder = new StringBuilder();

			Action<string> log = message => stringBuilder.AppendLine(message);

			var dummyCanvas = new DummyCanvas(log);

			foreach (var elem in documentView.PageViews.Single().Elements.Where(elem => elem.IsVisible))
			{
				elem.Paint(dummyCanvas, false);
			}

			return stringBuilder.ToString();
		}

		#endregion

		#region TestEmptyView

		[SnailTest]
		[ExpectNoExceptions]
		public void TestEmptyViewWithNoTabDoesNotThrowAnException()
		{
			var worksheet = DummyWorksheet.Parse(
				@"#Config:Name=""Test Document"":DataContext=""UXML"":NumberOfCopies=0
#End
#Body
	Code: <Code>
	Description: <Description>
#End");

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "XXX";
			dummy.Z0_Description = "You don't want to know";

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			var template = Factory.New<VisualizerTemplate>();

			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DataStoreName = "TestDocData";

			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			using (var templateStream = new MemoryStream())
			{
				xls.Save(templateStream);

				template.SO_Template = templateStream.ToArray();
			}

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy));
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pack = new DocumentInfoPack(services, dummy, null, menuItem.SU_MenuName, new[] { pivot });
			Assert("prerequisite: there's no document infos since there's no copies to print", !pack.DocumentInfos.Any());

			using (var form = DocumentVisualizerForm.CreateView(pack))
			{
				form.Closing += (s, e) => form.PerformClickForTest();
				// form.Shown += (s, e) =>
				// {
				// 	//Task.Delay(500).ContinueWith(task => form.Invoke((MethodInvoker)(() => form.Close())));
				// };
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		#endregion
	}
}
