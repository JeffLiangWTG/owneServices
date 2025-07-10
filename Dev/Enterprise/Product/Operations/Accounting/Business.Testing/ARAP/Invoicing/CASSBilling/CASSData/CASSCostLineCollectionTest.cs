using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class CASSCostLineCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : INonPersistentBusinessObjectCollection
	{
		public void TestAddRemove()
		{
			Assert(!GetCollectionToTest().AllowNew);
			Assert(!GetCollectionToTest().AllowRemove);
		}

		public override void TestAddNew()
		{
			Assert("Not supported", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supported", true);
		}

		public override void TestAddAndDeleteOfElementAsThoughBinding()
		{
			Assert("Not supported", true);
		}

		public override void TestTypedAddNew()
		{
			Assert("Not supported", true);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CASSCostLineCollection<>);
		}
	}
}
