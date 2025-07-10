using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(UsageLineCollection))]
	internal sealed class UsageLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UsageLineCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override UsageLineCollection GetCollectionToTest()
		{
			return new UsageLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UsageLine(Factory);
		}

		#endregion
	}
}
