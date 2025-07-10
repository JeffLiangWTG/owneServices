using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(DocumentVisualizerForm))]
	[CaptureMemoryDumpForDisposableLeak]
	sealed class DocumentVisualizerFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var parent = Factory.New<Forwarding.IForwardingConsol>();
			var menuItem = Factory.New<VisualizerMenuItem>();

			var template = Factory.New<VisualizerTemplate>();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(tempFileName);
			}

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();
			pivot.SI_DataStoreName = "TestDocument";
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pack = new DocumentInfoPack(services, (BusinessObject)parent, null, menuItem.SU_MenuName, new[] { pivot });

			return DocumentVisualizerForm.CreateView(pack);
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		#endregion
	}
}
