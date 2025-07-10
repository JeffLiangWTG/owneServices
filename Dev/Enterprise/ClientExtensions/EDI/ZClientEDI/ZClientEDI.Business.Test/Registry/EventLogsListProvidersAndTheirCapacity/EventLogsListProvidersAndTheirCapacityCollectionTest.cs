using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(EventLogsListProvidersAndTheirCapacityCollection))]
	internal sealed class EventLogsListProvidersAndTheirCapacityCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EventLogsListProvidersAndTheirCapacityCollection>
	{
		#region GetSystems

		public void TestGetProviders()
		{
			var collection = new EventLogsListProvidersAndTheirCapacityCollection();
			collection.AddNewOrGetExisting("CargoWise One", 3);
			collection.AddNewOrGetExisting("CargoWise One Mobile", 3);
			collection.AddNewOrGetExisting("CargoWise One Mobile", 2);

			AssertEquals("CargoWise One", collection.GetProvider("CargoWise One", 3).ProviderCode);
			AssertNull(collection.GetProvider("CargoWise One Mobile", 1));
			AssertEquals("CargoWise One Mobile", collection.GetProvider("CargoWise One Mobile", 2).ProviderCode);
		}

		public void TestUpdateOrAddNew()
		{
			var collection = new EventLogsListProvidersAndTheirCapacityCollection();
			collection.AddNewOrGetExisting("CargoWise One", 3);

			AssertEquals(1, collection.Count);

			collection.AddNewOrGetExisting("CargoWise One", 3);
			AssertEquals(1, collection.Count);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override EventLogsListProvidersAndTheirCapacityCollection GetCollectionToTest()
		{
			return new EventLogsListProvidersAndTheirCapacityCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EventLogsListProvidersAndTheirCapacity();
		}

		#endregion
	}
}
