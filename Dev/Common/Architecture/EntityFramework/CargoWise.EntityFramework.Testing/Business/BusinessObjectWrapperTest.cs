namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectWrapperTest : TestCaseWithFactory
	{
		public void TestConstructorStoresParent()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapper wrappedBizO = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals(bizO, wrappedBizO.WrappedBusinessObject);
		}

		public void TestDeleteRemovesFromWrapperHashtable()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapper wrappedBizO = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals("Precondition", 1, Factory.WrapperManager.GetCount(bizO));
			wrappedBizO.Delete();
			AssertEquals("Precondition", 0, Factory.WrapperManager.GetCount(bizO));
		}

		public void TestLoadReturnsObject()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapper wrappedObject1 = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals(bizO, wrappedObject1.WrappedBusinessObject);
		}

		public void TestLoadReturnsSameObjectTwice()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			BusinessObjectWrapper wrappedObject1 = DummyBusinessObjectWrapper.Load(bizO);
			BusinessObjectWrapper wrappedObject2 = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals(wrappedObject1, wrappedObject2);
		}

		public void TestLinkTableName()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			ILinkable wrapper = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals(bizO.TableName, wrapper.LinkTableName);
		}

		public void TestLinkTablePrefix()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			ILinkable wrapper = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals(bizO.TablePrefix, wrapper.LinkTablePrefix);
		}

		public void TestLinkPK()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			ILinkable wrapper = DummyBusinessObjectWrapper.Load(bizO);
			AssertEquals(bizO.PK, wrapper.LinkPK);
		}

		public void TestILinkableIsInDatabase()
		{
			DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
			bizO.SetIsInDatabase(true);
			BusinessObjectWrapper wrapper = DummyBusinessObjectWrapper.Load(bizO);
			ILinkable linkableWrapper = wrapper;
			AssertEquals(false, wrapper.IsInDatabase);
			AssertEquals(true, linkableWrapper.LinkIsInDatabase);
		}
	}
}
