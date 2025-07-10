using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultNumberOfDecimalsCollection))]
	sealed class DefaultNumberOfDecimalsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultNumberOfDecimalsCollection>
	{
		public void TestDuplicateRegistryEntry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var registryEntry = new DefaultNumberOfDecimals();
			registryEntry.UnitOfMeasure = "KG";
			registryEntry.TransportMode = "AIR";
			registryEntry.NumberOfDecimals = 2;
			registryEntry.RoundingMode = RoundingModes.Up;

			collection.Add(registryEntry);

			var duplicateRegistryEntry = new DefaultNumberOfDecimals();
			duplicateRegistryEntry.UnitOfMeasure = "KG";
			duplicateRegistryEntry.TransportMode = "AIR";
			duplicateRegistryEntry.NumberOfDecimals = 2;
			duplicateRegistryEntry.RoundingMode = RoundingModes.Up;

			AssertEquals(true, collection.IsDuplicateItem(duplicateRegistryEntry));
			AssertEquals(false, collection.IsDuplicateItem(null));
		}

		public void TestDefaultValueForModeAndUnit()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var registryEntry = new DefaultNumberOfDecimals();
			registryEntry.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			registryEntry.TransportMode = Core.Constants.TransportModes.Air;
			registryEntry.NumberOfDecimals = 2;
			registryEntry.RoundingMode = RoundingModes.Up;

			collection.Add(registryEntry);

			AssertEquals(registryEntry, collection.GetDefaultValueForModeAndUnit("AIR", "KG"));
			AssertEquals(null, collection.GetDefaultValueForModeAndUnit("BEAR", "CAGE"));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultNumberOfDecimalsCollection GetCollectionToTest()
		{
			return new DefaultNumberOfDecimalsCollection(Module.Freight);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultNumberOfDecimals();
		}

		#endregion
	}
}
