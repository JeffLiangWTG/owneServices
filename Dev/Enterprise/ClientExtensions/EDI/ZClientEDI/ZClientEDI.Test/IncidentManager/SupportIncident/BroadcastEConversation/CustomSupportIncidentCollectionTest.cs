using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	[TestedType(typeof(CustomSupportIncidentCollection))]
	public class CustomSupportIncidentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CustomSupportIncidentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => new CustomSupportIncident(null);

		protected override CustomSupportIncidentCollection GetCollectionToTest() => new CustomSupportIncidentCollection(Factory);
	}
}
