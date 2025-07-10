using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CalculateDeliveryDueDateTransportModeCollection))]
	sealed class CalculateDeliveryDueDateTransportModeCollectionTest : RegistryBusinessObjectCollectionTestCase<CalculateDeliveryDueDateTransportModeCollection>
	{
		public void TestCollectionAddItem()
		{
			var collection = new CalculateDeliveryDueDateTransportModeCollection();
			AssertEquals(0, collection.Count);

			var hbl1 = collection.Add("AAA", (NoResString)"AAA DESCRIPTION");
			AssertEquals(1, collection.Count);
			AssertEquals("AAA", hbl1.Code);
			AssertEquals("AAA DESCRIPTION", hbl1.Description);
			AssertEquals("AAA DESCRIPTION", hbl1.EnglishDescription);
			AssertEquals(false, hbl1.Enabled);
		}

		#region Implementation

		protected override CalculateDeliveryDueDateTransportModeCollection GetCollectionToTest()
		{
			return new CalculateDeliveryDueDateTransportModeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CalculateDeliveryDueDateTransportMode();
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
