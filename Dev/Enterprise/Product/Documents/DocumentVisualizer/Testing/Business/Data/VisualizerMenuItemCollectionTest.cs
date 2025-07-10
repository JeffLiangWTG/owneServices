using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	[TestedType(typeof(VisualizerMenuItemCollection))]
	sealed class VisualizerMenuItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoad_Filtering()
		{
			var menuItem1 = Factory.New<VisualizerMenuItem>();
			menuItem1.SU_BusinessContext = "Consol";

			var menuItem2 = Factory.New<VisualizerMenuItem>();
			menuItem2.SU_BusinessContext = "Consol";

			var menuItem3 = Factory.New<VisualizerMenuItem>();
			menuItem3.SU_BusinessContext = "Shipment";

			var menuItem4 = Factory.New<VisualizerMenuItem>();
			menuItem4.SU_MenuType = Constants.StmMenuItemTypes.Documents;

			var menuItem5 = Factory.New<VisualizerMenuItem>();
			menuItem5.SU_MenuType = "XXX";

			var allNewMenuItems = new[]
				{
					menuItem1,
					menuItem2,
					menuItem3,
					menuItem4,
					menuItem5
				};

			var collection = new VisualizerMenuItemCollection(Factory, "");
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				new[] { menuItem1, menuItem2, menuItem3 },
				collection.Cast<VisualizerMenuItem>().Intersect(allNewMenuItems));

			collection = new VisualizerMenuItemCollection(Factory, "Consol");
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				new[] { menuItem1, menuItem2 },
				collection.Cast<VisualizerMenuItem>().Intersect(allNewMenuItems));
		}

		public void TestLoad_Filtering_ExcludeMenusLinkedToExcludedTemplate()
		{
			var template1 = Factory.NewWithValidTestData<StmTemplate>();
			var template2 = Factory.NewWithValidTestData<StmTemplate>();

			var menuItem1 = Factory.NewWithValidTestData<VisualizerMenuItem>();
			menuItem1.SU_BusinessContext = "Test";

			var pivot1 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot1.SI_SU = menuItem1.PK;
			pivot1.SI_SO = template1.PK;

			var menuItem2 = Factory.NewWithValidTestData<VisualizerMenuItem>();
			menuItem2.SU_BusinessContext = "Test";

			var pivot2 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot2.SI_SU = menuItem2.PK;
			pivot2.SI_SO = template2.PK;

			var menuItem3 = Factory.NewWithValidTestData<VisualizerMenuItem>();
			menuItem3.SU_BusinessContext = "Test";

			var pivot3 = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot3.SI_SU = menuItem3.PK;
			pivot3.SI_SO = template2.PK;

			Factory.Save();

			var collection = new VisualizerMenuItemCollection(Factory, "Test", new[] { template2.PK });
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				"menu items linked to excluded template have not been loaded",
				new[] { menuItem1 },
				collection.Cast<VisualizerMenuItem>());
		}

		public void TestLoad_Filtering_IncludeMenusWithoutLinkingWithAnyTemplates()
		{
			var menuItem = Factory.NewWithValidTestData<VisualizerMenuItem>();
			menuItem.SU_BusinessContext = "Test";

			Factory.Save();

			var collection = new VisualizerMenuItemCollection(Factory, "Test", new[] { ZGuid.NewZGuid() });
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				"menu items without linking with any templates should be loaded",
				new[] { menuItem },
				collection.Cast<VisualizerMenuItem>());
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = new VisualizerMenuItemCollection(Factory, "Consol");
			AssertEquals("Consol", collection.AddNew().SU_BusinessContext);

			collection = new VisualizerMenuItemCollection(Factory, "Shipment");
			AssertEquals("Shipment", collection.AddNew().SU_BusinessContext);
		}

		public void TestTypeForLoadNewTemplateFromFile()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				var collection = new VisualizerMenuItemCollection(Factory, "Consol");
				var template = collection.LoadNewTemplateFromFile(tempFileName);
				AssertType("Should be VisualizerTemplate", typeof(VisualizerTemplate), template);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VisualizerMenuItemCollection(Factory, "Consol");
		}

		#endregion
	}
}
