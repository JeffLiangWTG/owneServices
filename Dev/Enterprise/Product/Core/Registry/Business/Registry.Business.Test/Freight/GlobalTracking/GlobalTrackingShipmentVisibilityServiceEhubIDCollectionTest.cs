using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityServiceEhubIDCollection))]
	sealed class GlobalTrackingShipmentVisibilityServiceEhubIDCollectionTest : RegistryBusinessObjectCollectionTestCase<GlobalTrackingShipmentVisibilityServiceEhubIDCollection>
	{
		public void TestCollectionAddItem()
		{
			var collection = new GlobalTrackingShipmentVisibilityServiceEhubIDCollection();
			AssertEquals(0, collection.Count);

			var serviceEhubID = collection.Add("CA", (NoResString)"CA", "CONTAINER_TRACKING");
			AssertEquals(1, collection.Count);
			AssertEquals("CA", serviceEhubID.Code);
			AssertEquals("", serviceEhubID.Description);
			AssertEquals("CA", serviceEhubID.Service);
			AssertEquals("CONTAINER_TRACKING", serviceEhubID.EhubID);
		}

		#region Implementation

		protected override GlobalTrackingShipmentVisibilityServiceEhubIDCollection GetCollectionToTest()
		{
			return new GlobalTrackingShipmentVisibilityServiceEhubIDCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlobalTrackingShipmentVisibilityServiceEhubID();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
