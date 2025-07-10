using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Preview;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlBillForBindingOnly))]
	public class StlBillForBindingOnlyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StlBillForBindingOnly(null);
		}
	}

	[TestedType(typeof(UsageLineForBindingOnly))]
	public class UsageLineForBindingOnlyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UsageLineForBindingOnly(null);
		}
	}

	[TestedType(typeof(StlMonthlyUsageForBindingOnly))]
	public class StlMonthlyUsageForBindingOnlyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StlMonthlyUsageForBindingOnly(null);
		}
	}

	[TestedType(typeof(UsageLineForBindingOnlyCollection))]
	public class UsageLineForBindingOnlyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UsageLineForBindingOnlyCollection>
	{
		protected override UsageLineForBindingOnlyCollection GetCollectionToTest()
		{
			return new UsageLineForBindingOnlyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UsageLineForBindingOnly(null);
		}
	}

	[TestedType(typeof(StlMonthlyUsageForBindingOnlyCollection))]
	public class StlMonthlyUsageForBindingOnlyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlMonthlyUsageForBindingOnlyCollection>
	{
		protected override StlMonthlyUsageForBindingOnlyCollection GetCollectionToTest()
		{
			return new StlMonthlyUsageForBindingOnlyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlMonthlyUsageForBindingOnly(null);
		}
	}
}
