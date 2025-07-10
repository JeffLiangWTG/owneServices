using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataRefreshBusSubscription_Test : TestCaseWithFactory
	{
		public void TestDoActionUpdatesObject()
		{
			var businessObject = Factory.New<DummyBusinessObject_ForDataRefreshTest>();
			businessObject.TestingRefreshDeleted = false;
			Assert("Precondition", !businessObject.IveBeenRefreshed);
			DataRefreshBusSubscription subscription = new DataRefreshBusSubscription(Factory, businessObject);
			subscription.DoAction(new[] { businessObject });
			Assert(businessObject.IveBeenRefreshed);
		}

		public void TestDoActionIncludesDeleted()
		{
			var businessObject = Factory.New<DummyBusinessObject_ForDataRefreshTest>();
			businessObject.TestingRefreshDeleted = true;
			businessObject.DeletedObjectFoundInRefresh = false;

			var deletedObject = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			deletedObject.Delete();
			Assert("Deleted Object Precondition", deletedObject.IsDeleted);
			Assert("Precondition", !businessObject.DeletedObjectFoundInRefresh);

			DataRefreshBusSubscription subscription = new DataRefreshBusSubscription(Factory, businessObject);
			subscription.DoAction(new[] { businessObject, deletedObject });
			Assert("Deleted Objects were included", businessObject.DeletedObjectFoundInRefresh);
			Assert("Refresh happened", businessObject.IveBeenRefreshed);

			businessObject.TestingRefreshDeleted = false;
			businessObject.DeletedObjectFoundInRefresh = false;
			businessObject.IveBeenRefreshed = false;

			subscription = new DataRefreshBusSubscription(Factory, businessObject);
			subscription.DoAction(new[] { businessObject, deletedObject });
			Assert("Deleted Objects were not included", !businessObject.DeletedObjectFoundInRefresh);
			Assert("Refresh happened", businessObject.IveBeenRefreshed);
		}

		class DummyBusinessObject_ForDataRefreshTest : DummyBusinessObject, IDataRefreshBusSubscriber
		{
			public DummyBusinessObject_ForDataRefreshTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			BusinessObjectFactory IDataRefreshBusSubscriber.Factory
			{
				get { return Factory; }
			}

			bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => TestingRefreshDeleted;

			void IDataRefreshBusSubscriber.UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
			{
				AssertEquals(this, publishedObjects.First());
				if (TestingRefreshDeleted)
				{
					DeletedObjectFoundInRefresh = publishedObjects.OfType<BusinessObject>().Any(publishedObject => publishedObject.IsDeleted);
				}
				IveBeenRefreshed = true;
			}
			public bool TestingRefreshDeleted { get; set; }
			public bool DeletedObjectFoundInRefresh { get; set; }
			public bool IveBeenRefreshed { get; set; }
		}
	}
}
