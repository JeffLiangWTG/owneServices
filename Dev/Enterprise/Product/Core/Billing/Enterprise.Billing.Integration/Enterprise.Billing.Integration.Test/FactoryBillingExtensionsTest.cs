namespace Enterprise.Billing.Integration.Test
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;

	public class FactoryBillingExtensionsTestCase : TestCaseWithFactory
	{
		public void TestRecordSnapshot()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var snapshot = Factory.RecordSnapshot();
			AssertNotNull(snapshot);
			var diff = snapshot.Except(new BusinessObjectFactory().RecordSnapshot());
			AssertEquals(1, diff.Count());
			AssertEquals(new ObjectSnapshot(dummy), diff.First());
		}

		public void TestGetStatus()
		{
			var obj = Factory.New<DummyBusinessObject>();
			AssertEquals(ObjectStatusEnum.New, obj.GetStatus());
			Factory.Save();

			obj.Z0_Guid = Guid.NewGuid();
			AssertEquals(ObjectStatusEnum.Updated, obj.GetStatus());

			Factory.Save();
			AssertEquals(ObjectStatusEnum.Unchanged, obj.GetStatus());

			obj.Delete();
			AssertEquals(ObjectStatusEnum.Deleted, obj.GetStatus());
		}
	}
}
