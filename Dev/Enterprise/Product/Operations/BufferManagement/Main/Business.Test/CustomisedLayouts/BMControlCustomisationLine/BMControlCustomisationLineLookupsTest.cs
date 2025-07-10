using System.Reflection;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMControlCustomisationLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNoFieldsBecauseWeShouldBeCachingEverythingInTheFactory()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory);
			var line = customisation.CustomisationLines.AddNew();

			var fields = line.Lookups.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<FieldInfo>(), fields);
		}

		public void TestPropertyNames_ShouldBeCachedSeparatelyForPropertySource()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory);
			var line1 = customisation.CustomisationLines.AddNew();
			var line2 = customisation.CustomisationLines.AddNew();
			var line3 = customisation.CustomisationLines.AddNew();
			var line4 = customisation.CustomisationLines.AddNew();
			var line5 = customisation.CustomisationLines.AddNew();
			var line6 = customisation.CustomisationLines.AddNew();

			line1.PropertySource = PropertySourceList.Codes.Job;
			line2.PropertySource = PropertySourceList.Codes.Job;
			line3.PropertySource = PropertySourceList.Codes.ProcessTask;
			line4.PropertySource = PropertySourceList.Codes.ProcessTask;
			line5.PropertySource = PropertySourceList.Codes.Workflow;
			line6.PropertySource = PropertySourceList.Codes.Workflow;

			CombineAssertions("PropertyNames collection should be cached between lines which have the same PropertySource", () =>
			{
				Assert("PropertyNames: Job", object.ReferenceEquals(line1.Lookups.PropertyNames, line2.Lookups.PropertyNames));
				Assert("PropertyNames: Task", object.ReferenceEquals(line3.Lookups.PropertyNames, line4.Lookups.PropertyNames));
				Assert("PropertyNames: Workflow", object.ReferenceEquals(line5.Lookups.PropertyNames, line6.Lookups.PropertyNames));

				Assert("PropertyNameDescriptions: Job", object.ReferenceEquals(line1.Lookups.PropertyNameDescriptions, line2.Lookups.PropertyNameDescriptions));
				Assert("PropertyNameDescriptions: Task", object.ReferenceEquals(line3.Lookups.PropertyNameDescriptions, line4.Lookups.PropertyNameDescriptions));
				Assert("PropertyNameDescriptions: Workflow", object.ReferenceEquals(line5.Lookups.PropertyNameDescriptions, line6.Lookups.PropertyNameDescriptions));
			});

			CombineAssertions("PropertyNames collection should be different between lines which have different PropertySources", () =>
			{
				Assert("PropertyNames: Job and task", !object.ReferenceEquals(line1.Lookups.PropertyNames, line3.Lookups.PropertyNames));
				Assert("PropertyNames: Job and workflow", !object.ReferenceEquals(line1.Lookups.PropertyNames, line5.Lookups.PropertyNames));
				Assert("PropertyNames: Task and workflow", !object.ReferenceEquals(line3.Lookups.PropertyNames, line5.Lookups.PropertyNames));

				Assert("PropertyNameDescriptions: Job and task", !object.ReferenceEquals(line1.Lookups.PropertyNameDescriptions, line3.Lookups.PropertyNameDescriptions));
				Assert("PropertyNameDescriptions: Job and workflow", !object.ReferenceEquals(line1.Lookups.PropertyNameDescriptions, line5.Lookups.PropertyNameDescriptions));
				Assert("PropertyNameDescriptions: Task and workflow", !object.ReferenceEquals(line3.Lookups.PropertyNameDescriptions, line5.Lookups.PropertyNameDescriptions));
			});
		}
	}
}
