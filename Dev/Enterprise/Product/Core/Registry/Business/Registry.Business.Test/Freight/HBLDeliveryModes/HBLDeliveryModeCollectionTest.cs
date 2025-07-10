using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryModeCollection))]
	sealed class HBLDeliveryModeCollectionTest : RegistryBusinessObjectCollectionTestCase<HBLDeliveryModeCollection>
	{
		public void TestCollectionAddItem()
		{
			var collection = new HBLDeliveryModeCollection();
			AssertEquals(0, collection.Count);

			var hbl1 = collection.Add("AAA", (NoResString)"AAA DESCRIPTION");
			AssertEquals(1, collection.Count);
			AssertEquals("AAA", hbl1.Code);
			AssertEquals("AAA DESCRIPTION", hbl1.Description);
			AssertEquals("AAA DESCRIPTION", hbl1.EnglishDescription);
			AssertEquals(true, hbl1.ShowInList);
		}

		#region Implementation

		protected override HBLDeliveryModeCollection GetCollectionToTest()
		{
			return new HBLDeliveryModeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HBLDeliveryMode();
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
