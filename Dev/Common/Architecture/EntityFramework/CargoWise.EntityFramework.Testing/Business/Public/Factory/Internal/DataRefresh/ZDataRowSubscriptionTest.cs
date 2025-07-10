using System.Collections.Generic;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Testing
{
	[TestedType(typeof(DataRefreshBus.ZDataRowSubscription))]
	sealed class ZDataRowSubscriptionTest : DataRefreshBusSubscriptionTestCase<DataRefreshBus.ZDataRowSubscription>
	{
		protected override void SetUp()
		{
			base.SetUp();
			dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
		}

		DummyBusinessObject dummy;

		protected override IEnumerable<BusinessObject> GetObjectsToPublish(BusinessObjectFactory factory)
		{
			return new[] { factory.ImportFromAnotherFactory(dummy) };
		}

		protected override DataRefreshBus.ZDataRowSubscription GetSubscription(BusinessObjectFactory factory)
		{
			var rowFactory = factory.RowFactory;
			factory.ImportFromAnotherFactory(dummy);
			var row = (ZDataRow)rowFactory.GetRow(dummy.TableName, dummy.PK);
			return new DataRefreshBus.ZDataRowSubscription(row, factory, dummy.Factory);
		}

		public void TestUpdateForDataRefreshWhenCurrentThreadIsNotOwner()
		{
			var factory1 = new BusinessObjectFactory();
			var dummy = factory1.New<DummyBusinessObject>();
			const string theTruth = "Teacups can be found in strange places if you know how to look.";
			dummy.Z0_VarCharMax = theTruth;
			factory1.Save();

			var threadSentry = new Mock<IThreadSentry>();
			threadSentry.Setup(ts => ts.IsOwner).Returns(false);
			var factory2 = new BusinessObjectFactory();
			factory1.ThreadSentry = threadSentry.Object;
			var loadedDummy = factory2.Load<DummyBusinessObject>(dummy.PK);
			loadedDummy.Z0_VarCharMax = "If a single thread did think itself more important wouldn't that change things?";
			factory2.Save();
			AssertEquals(theTruth, dummy.Z0_VarCharMax);
		}

		public void TestUpdateForDataRefreshWhenCurrentThreadIsNotOwner_ABOCCollection_Update()
		{
			var factory1 = Factory.CreateNewFactory();
			var dummy = factory1.New<DummyBusinessObject>();
			var threadSentry = new Mock<IThreadSentry>();
			threadSentry.Setup(ts => ts.IsOwner).Returns(false);
			var factory2 = Factory.CreateNewFactory();
			factory2.ThreadSentry = threadSentry.Object;
			var query = new ZQuery(DummyBizoSchema.PK, dummy.PK);
			using (var index = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory2, query).IndexExposed)
			{
				index.HasChangesChanged += (sender, args) =>
				{
					// Just add the event handler to ensure SharedDataTableIndex is attached to the DataTable 
				};

				AssertEquals(0, index.Count);
				factory1.Save();
				AssertEquals(0, index.Count);
			}
		}

		public void TestUpdateForDataRefreshWhenCurrentThreadIsNotOwner_ABOCCollection_Delete()
		{
			var factory1 = Factory.CreateNewFactory();
			var dummy = factory1.New<DummyBusinessObject>();
			factory1.Save();
			var threadSentry = new Mock<IThreadSentry>();
			threadSentry.Setup(ts => ts.IsOwner).Returns(false);
			var factory2 = Factory.CreateNewFactory();
			factory2.ThreadSentry = threadSentry.Object;
			var query = new ZQuery(DummyBizoSchema.PK, dummy.PK);
			using (var index = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory2, query).IndexExposed)
			{
				index.HasChangesChanged += (sender, args) =>
				{
					// Just add the event handler to ensure SharedDataTableIndex is attached to the DataTable 
				};

				AssertEquals(1, index.Count);
				dummy.Delete();
				factory1.Save();
				AssertEquals(1, index.Count);
			}
		}

		public void TestUpdateForDataRefreshWhenCurrentThreadIsNotOwner_Deleted()
		{
			var factory1 = Factory.CreateNewFactory();
			var dummy = factory1.New<DummyBusinessObject>();
			factory1.Save();

			var threadSentry = new Mock<IThreadSentry>();
			threadSentry.Setup(ts => ts.IsOwner).Returns(false);
			var factory2 = Factory.CreateNewFactory();
			factory2.ThreadSentry = threadSentry.Object;
			var reloadedDummy = factory2.Load<DummyBusinessObject>(dummy.PK);

			dummy.Delete();
			factory1.Save();
			AssertEquals(false, reloadedDummy.IsDeleted);
		}

		public void TestUpdateForDataRefreshWhenCurrentThreadIsNotOwner_BizoCollection_Update()
		{
			var factory1 = Factory.CreateNewFactory();
			var dummy = factory1.New<DummyBusinessObject>();
			var threadSentry = new Mock<IThreadSentry>();
			threadSentry.Setup(ts => ts.IsOwner).Returns(false);
			var factory2 = Factory.CreateNewFactory();
			factory2.ThreadSentry = threadSentry.Object;
			var query = new ZQuery(DummyBizoSchema.PK, dummy.PK);
			var collection = new DummyBusinessObjectCollection(factory2, query);
			collection.IsManagedForDataRefresh = true;
			factory1.Save();
			AssertEquals(0, collection.Count);
		}

		public void TestUpdateForDataRefreshWhenCurrentThreadIsNotOwner_BizoCollection_Delete()
		{
			var factory1 = Factory.CreateNewFactory();
			var dummy = factory1.New<DummyBusinessObject>();
			factory1.Save();
			var threadSentry = new Mock<IThreadSentry>();
			threadSentry.Setup(ts => ts.IsOwner).Returns(false);
			var factory2 = Factory.CreateNewFactory();
			factory2.ThreadSentry = threadSentry.Object;
			var query = new ZQuery(DummyBizoSchema.PK, dummy.PK);
			var collection = new DummyBusinessObjectCollection(factory2, query);
			collection.IsManagedForDataRefresh = true;
			collection.Load();

			AssertEquals(1, collection.Count);
			dummy.Delete();
			factory1.Save();
			AssertEquals(1, collection.Count);
		}
	}
}
