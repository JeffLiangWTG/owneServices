using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShipmentInspectionTypeCollection))]
	sealed class ShipmentInspectionTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<ShipmentInspectionTypeCollection>
	{
		public void TestAllowNew()
		{
			var coll = new ShipmentInspectionTypeCollection();
			AssertEquals(true, coll.AllowNew);

			coll.CountryCode = "AU";
			AssertEquals(false, coll.AllowNew);

			coll.CountryCode = "EU";
			AssertEquals(true, coll.AllowNew);
		}

		public void TestEditExistingUserAddedItem()
		{
			var coll = new ShipmentInspectionTypeCollection();
			var item = coll.AddNew();

			AssertEquals("Existing user added item can be deleted", true, ((ICanDelete)item).CanDelete);
			AssertEquals("Code can be edited", false, item.CodeInfo.ReadOnly);
			AssertEquals("Description can be edited", false, item.DescriptionInfo.ReadOnly);
			AssertEquals("ShowInList can be edited", false, item.ShowInListInfo.ReadOnly);
			AssertEquals("AllowedOnPassengerFlights can be edited", false, item.AllowedOnPassengerFlightsInfo.ReadOnly);
		}

		#region Implementation

		protected override ShipmentInspectionTypeCollection GetCollectionToTest()
		{
			return new ShipmentInspectionTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipmentInspectionType();
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		#endregion
	}
}
