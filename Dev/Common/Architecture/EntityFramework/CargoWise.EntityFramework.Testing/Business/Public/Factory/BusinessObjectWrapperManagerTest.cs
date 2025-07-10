using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectWrapperManagerTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapperManager manager = new BusinessObjectWrapperManager();
			AssertEquals(0, manager.GetCount(bizO));
			manager.Add(DummyBusinessObjectWrapper.Load(bizO));
			AssertEquals(1, manager.GetCount(bizO));
		}

		public void TestAddWillNotTakeSameTypeTwice()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapperManager manager = new BusinessObjectWrapperManager();
			DummyBusinessObjectWrapper wrapper = DummyBusinessObjectWrapper.Load(bizO);
			manager.Add(wrapper);
			try
			{
				manager.Add(wrapper);
				Fail("Did not throw exception");
			}
			catch (ApplicationException)
			{
				Assert(true);
			}
		}

		class DummyDummyBusinessObjectWrapper : DummyBusinessObjectWrapper
		{
			public DummyDummyBusinessObjectWrapper(DummyBusinessObject bizO) : base(bizO)
			{
			}
		}

		public void TestDeleteFor()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapperManager manager = new BusinessObjectWrapperManager();
			manager.Add(DummyBusinessObjectWrapper.Load(bizO));
			manager.Add(new DummyDummyBusinessObjectWrapper(bizO));
			AssertEquals(2, manager.GetCount(bizO));
			manager.DeleteFor(bizO);
			AssertEquals(0, manager.GetCount(bizO));
		}

		public void TestGetWrappers()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapperManager manager = new BusinessObjectWrapperManager();
			manager.Add(DummyBusinessObjectWrapper.Load(bizO));
			manager.Add(new DummyDummyBusinessObjectWrapper(bizO));
			AssertEquals(2, manager.GetCount(bizO));
		}

		public void TestRemove()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapperManager manager = new BusinessObjectWrapperManager();
			DummyBusinessObjectWrapper wrapper = DummyBusinessObjectWrapper.Load(bizO);
			manager.Add(wrapper);
			AssertEquals(1, manager.GetCount(bizO));
			manager.Remove(wrapper);
			AssertEquals(0, manager.GetCount(bizO));
		}
	}
}
