using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	[TestedType(typeof(VisualizerTemplateCollection))]
	sealed class VisualizerTemplateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var template1 = Factory.New<VisualizerTemplate>();
			var template2 = Factory.New<VisualizerTemplate>();

			var template3 = Factory.New<VisualizerTemplate>();
			template3.SO_TemplateType = StmTemplateTypes.Codes.Document;

			var template4 = Factory.New<VisualizerTemplate>();
			template4.SO_TemplateType = "XXX";

			var allNewTemplates = new[]
				{
					template1,
					template2,
					template3,
					template4
				};

			var collection = new VisualizerTemplateCollection(Factory, Array.Empty<ZGuid>());
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				new[] { template1, template2 },
				collection.Cast<VisualizerTemplate>().Intersect(allNewTemplates));

			collection.Load(new ZQuery());

			AssertContainsExactElementsInAnyOrder(
				new[] { template1, template2 },
				collection.Cast<VisualizerTemplate>().Intersect(allNewTemplates));
		}

		public void TestAdditionalFilter()
		{
			var template1 = Factory.New<VisualizerTemplate>();
			var template2 = Factory.New<VisualizerTemplate>();

			var template3 = Factory.New<VisualizerTemplate>();
			template3.SO_DataContext = "XXX";

			var allNewTemplates = new[]
				{
					template1,
					template2,
					template3
				};

			var collection = new VisualizerTemplateCollection(Factory, Array.Empty<ZGuid>());
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				new[] { template1, template2, template3 },
				collection.Cast<VisualizerTemplate>().Intersect(allNewTemplates));
		}

		public void TestDoNotLoadExcludedTemplates()
		{
			var template1 = Factory.New<VisualizerTemplate>();
			var template2 = Factory.New<VisualizerTemplate>();
			var template3 = Factory.New<VisualizerTemplate>();

			var collection = new VisualizerTemplateCollection(Factory, new[] { template2.PK, template3.PK });
			collection.Load();

			AssertContainsExactElementsInAnyOrder(
				"Excluded templates have not been loaded",
				new[] { template1.PK },
				collection.Select(t => t.PK).Intersect(new[] { template1.PK, template2.PK, template3.PK }));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new VisualizerTemplateCollection(Factory, Array.Empty<ZGuid>());
		}

		#endregion

	}
}
