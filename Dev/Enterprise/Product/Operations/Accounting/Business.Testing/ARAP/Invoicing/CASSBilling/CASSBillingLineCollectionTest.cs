using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(CASSBillingLineCollection))]
	public class CASSBillingLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CASSBillingLineCollection>
	{
		public void TestAddRemove()
		{
			Assert(!GetCollectionToTest().AllowNew);
			Assert(GetCollectionToTest().AllowRemove);
		}

		#region Implementation

		protected override CASSBillingLineCollection GetCollectionToTest()
		{
			return new CASSBillingLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CASSBillingLine(Factory);
		}

		#endregion
	}
}
