using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LocationsChargesCollection))]
	sealed class LocationsChargesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<LocationsChargesCollection>
	{
		public void TestIndexerAndAddNew()
		{
			LocationsChargesGroup locationsChargesGroup1 = Collection.AddNew();
			LocationsChargesGroup locationsChargesGroup2 = Collection.AddNew();

			AssertEquals("Collection[0]", locationsChargesGroup1, Collection[0]);
			AssertEquals("Collection[1]", locationsChargesGroup2, Collection[1]);
		}

		public void TestGroupFromLocation()
		{
			AssertNull("No records", Collection["US"]);
			AssertNull("No records", Collection["USNYC"]);
			LocationsChargesGroup group = Collection.AddNew();
			group.Location = "USNYC";
			AssertNotNull("There is such record", Collection["USNYC"]);
			AssertNull("No such records", Collection["USLAX"]);
			group = Collection.AddNew();
			group.Location = "US";
			AssertNotNull("There is such record", Collection["USNYC"]);
			AssertNotNull("Country record must be returned for this", Collection["USLAX"]);
		}

		#region Implementation

		protected override LocationsChargesCollection GetCollectionToTest()
		{
			return new LocationsChargesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LocationsChargesGroup(null, Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
