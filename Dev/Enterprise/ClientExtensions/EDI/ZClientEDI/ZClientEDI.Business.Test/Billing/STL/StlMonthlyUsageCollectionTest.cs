using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlMonthlyUsageCollection))]
	internal sealed class StlMonthlyUsageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlMonthlyUsageCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override StlMonthlyUsageCollection GetCollectionToTest()
		{
			return new StlMonthlyUsageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlMonthlyUsage(Factory);
		}

		#endregion
	}
}
