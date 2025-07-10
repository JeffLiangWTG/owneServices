
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Business.Testing
{
	[TestedType(typeof(VisualizerMenuCustomisation))]
	sealed class VisualizerMenuCustomisationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMenus()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(Dummy, Factory, "Consol");
			AssertEquals("Visualizer menus", typeof(VisualizerMenuItemCollection), menuCustomisation.Menus.GetType());

			var newMenuItem = menuCustomisation.Menus.AddNew();
			AssertEquals("Menu items have proper businesscontext", "Consol", newMenuItem.SU_BusinessContext);
		}

		public void TestAvailableTemplates()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(Dummy, Factory, "Consol");
			AssertEquals("Visualizer templates", typeof(VisualizerTemplateCollection), menuCustomisation.AvailableTemplates.GetType());
		}

		public void TestAddNewTemplate()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				var menuCustomisation = new VisualizerMenuCustomisation(Dummy, Factory, "Consol");
				var systemTemplates = menuCustomisation
					.AvailableTemplates
					.Cast<VisualizerTemplate>()
					.ToArray();

				var result = menuCustomisation.AddNewTemplate(tempFileName);
				AssertEquals("Success", true, result.Success);

				var newTemplates = menuCustomisation
					.AvailableTemplates
					.Cast<VisualizerTemplate>()
					.Except(systemTemplates)
					.ToArray();

				AssertMultilineASCIIEquals("expected added templates",
					"TestTemplate",
					string.Join("\n\r", newTemplates.Select(t => t.SO_Name)));
			}
		}

		#region Implementation

		DummyNonPersistentBusinessObject Dummy
		{
			get { return dummy ?? (dummy = new DummyNonPersistentBusinessObject()); }
		}

		DummyNonPersistentBusinessObject dummy;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VisualizerMenuCustomisation(Dummy, Factory, "Consol");
		}

		#endregion
	}
}
