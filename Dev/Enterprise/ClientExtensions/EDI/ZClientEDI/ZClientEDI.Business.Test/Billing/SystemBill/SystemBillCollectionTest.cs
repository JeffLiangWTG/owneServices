using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SystemBillCollection))]
	internal sealed class SystemBillCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SystemBillCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override SystemBillCollection GetCollectionToTest()
		{
			return new SystemBillCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SystemBill(Factory);
		}

		#endregion
	}
}
