using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentGroupStatusConfigurationCollection))]
	public class IncidentGroupStatusConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IncidentGroupStatusConfigurationCollection>
	{
		public void TestAddNewSetupFromCollection()
		{
			var collection = GetCollectionToTest();
			var setup1 = collection.AddNew();
			var setup2 = collection.AddNew();

			AssertEquals(10, setup1.Sequence);
			AssertEquals(20, setup2.Sequence);
		}

		public new void TestAdd()
		{
			base.TestAdd();
			var setupCollection = GetCollectionToTest();

			var bizo = new IncidentGroupStatusConfiguration();
			setupCollection.Add(bizo);

			AssertEquals("Their FallBakcLevel don't sync", setupCollection.CurrentFallbackLevel.GetHashCode(), bizo.CurrentFallbackLevel?.GetHashCode());
		}

		#region Implementation
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override IncidentGroupStatusConfigurationCollection GetCollectionToTest()
		{
			return new IncidentGroupStatusConfigurationCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IncidentGroupStatusConfiguration(NewFallbackLevel(), Factory);
		}
		#endregion
	}
}
