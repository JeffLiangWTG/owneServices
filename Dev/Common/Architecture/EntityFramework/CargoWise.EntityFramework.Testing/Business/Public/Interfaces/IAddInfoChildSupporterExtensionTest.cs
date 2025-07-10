using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class IAddInfoChildSupporterExtensionTest : TestCaseWithFactory
	{
		public void TestLoadOrCreateAddInfoChildDoesNotCauseHasChanges()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			AssertEquals("bizObj.HasChanges", false, bizObj.HasChanges);
			var bizObj1 = bizObj.AddInfoChild;
			AssertEquals("bizObj.HasChanges", false, bizObj.HasChanges);
			AssertEquals("bizObj1.HasChanges", false, bizObj1.HasChanges);
			DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter bizObj2 = null;
			AssertSame(bizObj1, bizObj.LoadOrCreateAddInfoChild(ref bizObj2));
			AssertEquals("bizObj.HasChanges", false, bizObj.HasChanges);
			AssertEquals("bizObj1.HasChanges", false, bizObj1.HasChanges);
			bizObj.UnRegisterEditableChildObject(bizObj1);
			bizObj1.Delete();
			bizObj.HasChanges = false;
			bizObj2 = null;
			bizObj.LoadOrCreateAddInfoChild(ref bizObj2);
			AssertNotNull(bizObj2);
			AssertEquals("bizObj.HasChanges", false, bizObj.HasChanges);
			AssertEquals("bizObj2.HasChanges", false, bizObj2.HasChanges);
		}

		public void TestLoadOCreateAddInfoChild()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			bizObj.AddInfoChild.Delete();
			var bizObj1 = Factory.New<DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter>();
			bizObj1.Z0_Guid = bizObj.PK;
			AssertEquals(false, bizObj.IsRegisteredEditableChildObject(bizObj1));
			DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter bizObj2 = null;
			AssertSame(bizObj1, bizObj.LoadOrCreateAddInfoChild(ref bizObj2));
			AssertSame(bizObj1, bizObj2);
			AssertEquals(bizObj.PK, bizObj2.Z0_Guid);
			AssertEquals(true, bizObj.IsRegisteredEditableChildObject(bizObj1));

			bizObj1.Delete();
			AssertEquals(false, Object.ReferenceEquals(bizObj.LoadOrCreateAddInfoChild(ref bizObj2), bizObj1));
			AssertEquals(bizObj.PK, bizObj2.Z0_Guid);
			AssertEquals(false, bizObj.IsRegisteredEditableChildObject(bizObj1));
			AssertEquals(true, bizObj.IsRegisteredEditableChildObject(bizObj2));
		}

		public void TestLoadOCreateAddInfoChild_SupporterIsDeleted()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter bizObjWithAddInfoChild1 = null;
			DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter bizObjWithAddInfoChild2 = null;
			bizObj.LoadOrCreateAddInfoChild(ref bizObjWithAddInfoChild1);
			AssertNotNull(bizObjWithAddInfoChild1);

			bizObj.Delete();
			bizObj.LoadOrCreateAddInfoChild(ref bizObjWithAddInfoChild1);
			AssertEquals(true, bizObjWithAddInfoChild1.IsDeleted);

			bizObj.LoadOrCreateAddInfoChild(ref bizObjWithAddInfoChild2);
			AssertNull(bizObjWithAddInfoChild2);
		}

		public void TestLoadOCreateAddInfoChild_UseOrderByPK()
		{
			var supporter = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			supporter.AddInfoChild.Delete();
			var pk = supporter.PK;
			DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter matchedBizObj = null;
			for (var i = 1; i < 10; i++)
			{
				var childBizObj = Factory.New<DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter>();
				childBizObj.Z0_Guid = pk;
				if (matchedBizObj == null || matchedBizObj.PK > childBizObj.PK)
				{
					matchedBizObj = childBizObj;
				}
			}

			DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter bizObj = null;
			AssertSame(matchedBizObj, supporter.LoadOrCreateAddInfoChild(ref bizObj));
		}

		public void TestLoadOCreateAddInfoChild_NullForeignKey()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			bizObj.ChildForeignKeyColumnForTesting = null;
			AssertNoExceptionThrown(() =>
			{
				_ = bizObj.AddInfoChild;
			});
		}
	}
}
