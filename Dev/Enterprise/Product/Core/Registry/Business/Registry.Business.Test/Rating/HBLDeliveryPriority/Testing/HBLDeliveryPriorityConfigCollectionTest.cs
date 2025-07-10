using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryPriorityConfigCollection))]
	sealed class HBLDeliveryPriorityConfigCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HBLDeliveryPriorityConfigCollection>
	{
		public void TestRunPreSaveValidation()
		{
			var configCollection = new HBLDeliveryPriorityConfigCollection();
			var configuration = new HBLDeliveryPriorityConfig();
			configuration.ContainerMode = "FCL";
			configuration.HBLDeliveryMode = "DOOR/DOOR";
			configCollection.Add(configuration);

			configCollection.RunPreSaveValidation();
			// CompositeKey Validation
			AssertNoRowErrors("Precondition: Config should not have row errors", configuration);

			var configuration1 = new HBLDeliveryPriorityConfig();
			configuration1.ContainerMode = "FCL";
			configuration1.HBLDeliveryMode = "DOOR/DOOR";

			configCollection.Add(configuration1);

			configCollection.RunPreSaveValidation();
			AssertHasRowError("Config should have row error", configuration1, HBLDeliveryPriorityConfig.IdenticalConfigurationExists);

			configuration1.ContainerMode = "BCN";

			configCollection.RunPreSaveValidation();
			AssertNoRowErrors("Config should not have row errors", configuration);
		}

		#region Implementation

		protected override HBLDeliveryPriorityConfigCollection GetCollectionToTest()
			=> new HBLDeliveryPriorityConfigCollection(null, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new HBLDeliveryPriorityConfig(null, Factory);

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
