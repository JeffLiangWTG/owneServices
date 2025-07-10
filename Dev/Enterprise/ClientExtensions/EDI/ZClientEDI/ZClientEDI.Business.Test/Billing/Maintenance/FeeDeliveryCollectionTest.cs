using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(FeeDeliveryCollection))]
	internal sealed class FeeDeliveryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FeeDeliveryCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override FeeDeliveryCollection GetCollectionToTest()
		{
			return new FeeDeliveryCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FeeDelivery(Factory, null, null, ZDateTime.Empty, null, null);
		}

		#endregion
	}
}
