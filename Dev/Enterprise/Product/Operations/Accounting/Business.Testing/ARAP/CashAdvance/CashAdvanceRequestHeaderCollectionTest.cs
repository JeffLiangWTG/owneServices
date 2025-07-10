using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	[TestedType(typeof(CashAdvanceRequestHeaderCollection))]
	class CashAdvanceRequestHeaderCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			AssertEquals(false, collection.AllowNew);
		}

		public void TestRemove()
		{
			AssertEquals(false, collection.AllowRemove);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => collection;

		protected override void SetUp()
		{
			base.SetUp();
			collection = new CashAdvanceRequestHeaderCollection(Factory);
		}
		CashAdvanceRequestHeaderCollection collection;
	}
}
