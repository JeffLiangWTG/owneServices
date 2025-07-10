using System;
using System.Data;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FactoryChangeSetTest : TestCaseWithFactory
	{
		public void TestGetAddedBusinessObjects()
		{
			var unchangedObjected = Factory.NewWithValidTestData<DummyBusinessObject>();
			var modifiedBusinessObjects = Factory.NewWithValidTestData<DummyBusinessObject>();
			var deletedItem = Factory.NewWithValidTestData<DummyBusinessObject>();

			Factory.Save();
			deletedItem.Delete();
			modifiedBusinessObjects.Z0_Number = 69;

			var newItem = Factory.NewWithValidTestData<DummyBusinessObject>();

			var changeset = new FactoryChangeSet(new[] { unchangedObjected, modifiedBusinessObjects, deletedItem, newItem });
			AssertContainsExactElementsInAnyOrder("Should only include our new bizo", new[] { newItem }, changeset.GetAddedObjects());
		}

		public void TestLoadDatabaseEdition_UsingSameFactory()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			dummyBizObj.Z0_Date = new ZDateTime(2014, 7, 22);
			Factory.Save();

			using (RowFactory.SetCachedTables(DummyBusinessObject.Schema.TableName))
			{
				var factory = new BusinessObjectFactory();
				var newLoadedDBO = factory.Load<DummyBusinessObject>(dummyBizObj.PK);
				AssertNotNull("Precondition: Should have loaded DummyBusinessObject into UberFactory cache", RowFactory.UberFactoryRememberToLock.RowFactory.GetRow(DummyBusinessObject.Schema.TableName, dummyBizObj.PK));

				var dummyDatabaseInstance1 = FactoryChangeSet.LoadDatabaseEdition(dummyBizObj, factory);
				AssertEquals(dummyDatabaseInstance1.Factory, factory);

				var dummyDatabaseInstance2 = FactoryChangeSet.LoadDatabaseEdition(dummyBizObj);
				AssertNotEquals(dummyDatabaseInstance2.Factory, factory);
			}
		}

		public void TestLoadDatabaseEdition_WhenTableIsCached()
		{
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			dummyBizObj.Z0_Date = new ZDateTime(2014, 7, 22);
			Factory.Save();

			using (RowFactory.SetCachedTables(DummyBusinessObject.Schema.TableName))
			{
				new BusinessObjectFactory().Load<DummyBusinessObject>(dummyBizObj.PK);
				AssertNotNull("Precondition: Should have loaded DummyBusinessObject into UberFactory cache", RowFactory.UberFactoryRememberToLock.RowFactory.GetRow(DummyBusinessObject.Schema.TableName, dummyBizObj.PK));

				// Update the DummyBusinessObject using sql
				var updateSql = string.Format(@"
UPDATE {0}
SET {1} = @Z0_Date
where {2} = @Z0_PK",
	DummyBusinessObject.Schema.TableName,
	DummyBusinessObject.Schema.Z0_Date,
	DummyBusinessObject.Schema.PK);

				using (var command = TestConnection.Command(updateSql))
				{
					command.AddParameter("@Z0_Date", SqlDbType.DateTime, new DateTime(2014, 9, 9));
					command.AddParameter("@Z0_PK", SqlDbType.UniqueIdentifier, dummyBizObj.PK.ToGuid());
					command.ExecuteNonQuery();
				}

				var dummyDatabaseInstance = FactoryChangeSet.LoadDatabaseEdition(dummyBizObj);
				AssertType(typeof(DummyBusinessObject), dummyDatabaseInstance);
				AssertEquals("Should have loaded the value written to database using sql", new ZDateTime(2014, 9, 9), ((DummyBusinessObject)dummyDatabaseInstance).Z0_Date);
			}
		}

		class TestDummyBusinessObject : DummyBusinessObject, IBusinessObjectReload
		{
			public TestDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public SchemaColumn ReloadPerformanceIncreaseColumn => throw new NotImplementedException();

			public object ReloadPerformanceIncreaseColumnValue => throw new NotImplementedException();

			public BusinessObject Reload(BusinessObjectFactory loadingFactory)
			{
				var date = this.Z0_Date;
				return null;
			}
		}

		public void TestLoadDatabaseEdition_WhenObjectIsDeleted()
		{
			var dummyBizObj = Factory.New<TestDummyBusinessObject>();
			dummyBizObj.Z0_Date = new ZDateTime(2014, 7, 22);
			Factory.Save();

			using (RowFactory.SetCachedTables(DummyBusinessObject.Schema.TableName))
			{
				new BusinessObjectFactory().Load<DummyBusinessObject>(dummyBizObj.PK);

				dummyBizObj.Delete();
				AssertNoExceptionThrown("No exception should be thrown here", () => { var dummyDatabaseInstance = FactoryChangeSet.LoadDatabaseEdition(dummyBizObj); });
			}
		}
	}
}
