using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentGroupStatusConfigurationRegistryItem))]
	public class IncidentGroupStatusConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<IncidentGroupTypeCollection>
	{
		protected override StronglyTypedRegistryItem<IncidentGroupTypeCollection, IncidentGroupTypeCollection> GetNewRegistryItem()
		{
			return new IncidentGroupStatusConfigurationRegistryItem(
				"IncidentGroupStatusConfigurationForTest",
				(NoResString)"IncidentGroupStatusConfigurationForTest",
				(NoResString)"Incident Group Stage Configuration(ForTest)",
				(NoResString)"Setup stage sequence, naming and behaviour for Incident Management Groups.(ForTest)",
				new IncidentGroupTypeCollection());
		}

		public void TestHintText()
		{
			var incidentGroupStatusConfigurationRegistryItemTest = new IncidentGroupStatusConfigurationRegistryItemTest();
			var hintText = incidentGroupStatusConfigurationRegistryItemTest.GetNewRegistryItem().Hint;
			AssertEquals("Hint Text", "Setup stage sequence, naming and behaviour for Incident Management Groups.(ForTest)", hintText);
		}
	}
}
