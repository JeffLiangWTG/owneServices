using System;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZSaveExceptionTest : TransactionedTestCase
	{
		public void TestConstruction()
		{
			DataRow row = MakeDummyRow();
			ZDataException innerEx = new ZDataException(new Exception(), row, Db.Connection);
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZSaveException ex = new ZSaveException(innerEx, factory);

			AssertEquals("row", innerEx.Row, ex.Row);
			AssertEquals("factory", factory, ex.Factory);
		}

		public void TestBusinessObjectsForRowWithNoneFound()
		{
			DataRow row = MakeDummyRow();
			ZDataException innerEx = new ZDataException(new Exception(), row, Db.Connection);
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZSaveException ex = new ZSaveException(innerEx, factory);
			AssertEquals("bizOs", 0, ex.BusinessObjects.Length);
		}

		public void TestBusinessObjectsForRow()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObject dummy = factory.New(typeof(DummyBusinessObject));

			ZDataException innerEx = new ZDataException(new Exception(), dummy.Row, Db.Connection);

			ZSaveException ex = new ZSaveException(innerEx, factory);
			AssertEquals("bizOs length", 1, ex.BusinessObjects.Length);
			AssertEquals("found dummy", dummy, ex.BusinessObjects[0]);
		}

		public void TestGetIndexNameIfUniqueIndexViolation()
		{
			DbCommand command = Db.Connection.Command("create unique index NR_UX__Z0_Code on DummyBizO (Z0_Code)");
			command.ExecuteNonQuery();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "Same";
			dummy2.Z0_Code = "Same";

			try
			{
				factory.Save();
				Fail("Expected an exception");
			}
			catch (ZSaveException ex)
			{
				AssertEquals("Should return the correct index that was violated", "NR_UX__Z0_Code", ex.IndexNameIfUniqueIndexViolation);
			}
		}

		public void TestFriendlyMessageWithWierdException()
		{
			DataRow row = MakeDummyRow();
			Exception innerException = new Exception();
			ZSaveException e = new ZSaveException(new ZDataException(innerException, row, Db.Connection), new BusinessObjectFactory());
			AssertEquals("No msg", "", e.FriendlyMessage);
		}

		public void TestFriendlyMessageWithDBException()
		{
			ZSaveException e = null;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyDependantBusinessObject dD = factory.New<DummyDependantBusinessObject>();
			dD.ZD1_Z0 = Guid.NewGuid();

			try
			{
				factory.Save();
			}
			catch (ZSaveException ex)
			{
				e = ex;
			}

			Assert("Has msg", !string.IsNullOrEmpty(e.FriendlyMessage)); // FK constraint violation
			AssertContains("Has GetAdditionalInfoForZSaveExceptionCore overriden msg", string.Format("Business object additional info: Related Business Object PK: {0}", dD.ZD1_Z0), e.Message);
		}

		public void TestMessageContainsServerNameAndDatabaseNameWithoutDataRow()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var saveException = new ZSaveException(new ZDataException(new Exception(), null, Db.Connection), factory);

			// Act
			var message = saveException.Message;

			// Assert
			AssertEquals(true, message.Contains(factory.RowFactory.DbConnection.ServerName));
			AssertEquals(true, message.Contains(factory.RowFactory.DbConnection.CurrentDatabase));
		}

		public void TestMessageContainsServerNameAndDatabaseNameHavingDataRow()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var saveException = new ZSaveException(new ZDataException(new Exception(), MakeDummyRow(), Db.Connection), factory);

			// Act
			var message = saveException.Message;

			// Assert
			AssertEquals(true, message.Contains(factory.RowFactory.DbConnection.ServerName));
			AssertEquals(true, message.Contains(factory.RowFactory.DbConnection.CurrentDatabase));
		}

		public void TestDuplicateKeySaveExceptionMessage()
		{
			// Arrange
			var command = Db.Connection.Command("create unique index NR_UX__Z0_Code on DummyBizO (Z0_Code)");
			command.ExecuteNonQuery();

			var factory = new BusinessObjectFactory();
			var bo1 = factory.New<DummyBusinessObject>();
			var bo2 = factory.New<DummyBusinessObject>();
			bo1.Z0_Code = "Same";
			bo2.Z0_Code = "Same";

			try
			{
				factory.Save();
				Fail("Expected an ZSaveException");
			}
			catch (ZSaveException saveException)
			{
				// Act
				var message = saveException.Message;

				// Assert
				var expectedMessage = $@"
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: DummyBizo
PK: {bo2.PK}
RowState: Added
Factory validation suspended: False
Factory name for debugging: 
Business object around row = CargoWise.EntityFramework.Testing.DummyBusinessObject
Business object validation suspended: 0
Business object is marking as needing validation suspended: False
Business object light validation is enabled: True
Business object additional info: 

Inner Message = Cannot insert duplicate key row in object 'dbo.DummyBizo' with unique index 'NR_UX__Z0_Code'. The duplicate key value is (Same ).

";

				AssertEquals(expectedMessage, message);
			}
		}

		DataRow MakeDummyRow()
		{
			DataTable table = new DataTable("BlahBlah");
			DataColumn col = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(col);
			table.PrimaryKey = new DataColumn[] { col };
			DataRow result = table.NewRow();
			return result;
		}
	}
}
