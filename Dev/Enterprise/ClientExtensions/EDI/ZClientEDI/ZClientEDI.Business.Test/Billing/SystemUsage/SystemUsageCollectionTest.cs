using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SystemUsageCollection))]
	internal sealed class SystemUsageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SystemUsageCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override SystemUsageCollection GetCollectionToTest()
		{
			return new SystemUsageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}

		#endregion
	}
}
