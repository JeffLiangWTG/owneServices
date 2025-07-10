using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using NUnit.Framework;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceTaskCategoryDescriptorsTest : TestCase
	{
		public void TestMissingCategories()
		{
			// Arrange
			var categories = ServiceTaskCategoryDescriptors
				.Get(x => true)
				.Select(descriptor => (descriptor.Code, descriptor.Description))
				.ToDictionary(tuple => tuple.Code, tuple => tuple.Description);

			var hostedServiceTasks = ObjectFactory.Get<IClientHostedServiceAttributeProvider>()
				.GetClientHostedServiceAttributes();
				
			// Act
			var result = hostedServiceTasks
				.Where(config => !categories.ContainsKey(config.Category))
				.Select(config => $"'{config.Category}' category used by '{config.Code} - {config.Description}' service task in '{config.TypeAssemblyName}'");

			// Assert
			AssertContainsExactElementsInAnyOrder("Category list needs a new element(s)", Enumerable.Empty<string>(), result);
		}

		public void TestExcessiveCategories()
		{
			// Arrange
			var categories = ServiceTaskCategoryDescriptors
				.Get(x => true)
				.Select(descriptor => (descriptor.Code, descriptor.Description))
				.ToList();

			var unusedCategories = new HashSet<string>
			{
				"BP",
				"CSP",
			};

			var hostedServiceTasks = ObjectFactory.Get<IClientHostedServiceAttributeProvider>()
				.GetClientHostedServiceAttributes()
				.Select(config => config.Category)
				.Distinct()
				.ToHashSet();

			// Act
			var result = categories
				.Where(tuple => !hostedServiceTasks.Contains(tuple.Code))
				.Where(tuple => !unusedCategories.Contains(tuple.Code))
				.Select(tuple => tuple);

			// Assert
			AssertContainsExactElementsInAnyOrder("Categories which are not used by any task", Enumerable.Empty<string>(), result);
		}

		public void TestGet()
		{
			var result = ServiceTaskCategoryDescriptors.Get("ZZZ");
			AssertNull(result);

			var results = ServiceTaskCategoryDescriptors.Get(x => x.Code.StartsWith("A"));

			AssertContainsExactElementsInAnyOrder(new[] { "AUC", "ACC", "ARC" }, results.Select(x => x.Code));

			AssertDescriptions("DOM", "Port Transport");
			AssertDescriptions("ESC", "Spain Customs");
		}

		public void TestProductivityWiseModeDescriptors()
		{
			var results = ServiceTaskCategoryDescriptors.Get(x => x.IsShownInProductivityWiseMode);

			AssertContainsExactElementsInAnyOrder(new[] { "ACC", "BI", "BMS", "BP", "DBM", "DDC", "DOC", "ESV", "MAI", "SAL", "SYS", "WFL", "TST" }, results.Select(x => x.Code));
		}

		public void TestChinaCustomsCategory()
		{
			AssertDescriptions("CNC", "China Customs");
		}

		public void TestTelematicsCategory()
		{
			AssertDescriptions("TEL", "Telematics");
		}

		public void TestItalianCustomsCategory()
		{
			AssertDescriptions("ITC", "Italian Customs");
		}

		public void TestIrelandCustomsCategory()
		{
			AssertDescriptions("IEC", "Ireland Customs");
		}

		public void TestKoreaCustomsCategory()
		{
			AssertDescriptions("KRC", "KR Customs Messaging");
		}

		public void TestMexicanCustomsCategory()
		{
			AssertDescriptions("MXC", "MX Customs Messaging");
		}

		public void TestChileanCustomsCategory()
		{
			AssertDescriptions("CHL", "CL Customs Messaging");
		}

		public void TestBelgianCustomsCategory()
		{
			AssertDescriptions("BEC", "BE Customs Messaging");
		}

		public void TestDutchCustomsCategory()
		{
			AssertDescriptions("NLC", "NL Customs Messaging");
		}

		public void TestArgentinianCustomsCategory()
		{
			AssertDescriptions("ARC", "AR Customs Messaging");
		}

		public void TestSwissCustomsCategory()
		{
			AssertDescriptions("CHC", "CH Customs Messaging");
		}

		public void TestEUICS2Category()
		{
			AssertDescriptions("ICS", "EU ICS2 Messaging");
		}

		public void TestPolishCustomsCategory()
		{
			AssertDescriptions("PLC", "PL Customs Messaging");
		}

		public void TestRatingsCategory()
		{
			AssertDescriptions("RAT", "Rating");
		}

		static void AssertDescriptions(string category, string expectedDescription)
		{
			var resultDesc = ServiceTaskCategoryDescriptors.Get(category);
			AssertEquals(expectedDescription, resultDesc.Description);
		}
	}
}
