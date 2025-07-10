using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed partial class ZDataExceptionTest : TransactionedTestCase
	{
		public void TestConstructor()
		{
			Exception innerException = new Exception();
			DataRow row = MakeDummyRow();
			ZDataException e = new ZDataException(innerException, row, Db.Connection);

			AssertEquals("Set", row, e.Row);
			AssertEquals("Set", innerException, e.InnerException);
		}

		public void TestConstructorForSingleRows()
		{
			var table = new DataTable("BlahBlah");
			var col = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(col);

			var row = table.NewRow();
			row["PK"] = Guid.NewGuid();

			var innerException = new Exception("innerException");
			var ex = new ZDataException(innerException, row, Db.Connection);

			var expectedMessage = @$"Error from Data layer: TableName=BlahBlah, PK={row["PK"].ToString()}, RowState=Detached
InnerException Message = innerException";

			AssertEquals("Use the single row as the row property", row, ex.Row);
			AssertEquals("Row information is printed", expectedMessage, ex.Message);
		}

		public void TestConstructorForMultipleRows()
		{
			var dataRows = new List<DataRow>();
			var table = new DataTable("BlahBlah");
			var col = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(col);

			var row1 = table.NewRow();
			row1["PK"] = Guid.NewGuid();
			dataRows.Add(row1);

			var row2 = table.NewRow();
			row2["PK"] = Guid.NewGuid();
			dataRows.Add(row2);

			var row3 = table.NewRow();
			row3["PK"] = Guid.NewGuid();
			dataRows.Add(row3);

			var innerException = new Exception("innerException");
			var ex = new ZDataException(innerException, dataRows, true, Db.Connection);

			var expectedMessage = @$"Error from Data layer: TableName=BlahBlah, PK={row1["PK"].ToString()}, RowState=Detached
Error from Data layer: TableName=BlahBlah, PK={row2["PK"].ToString()}, RowState=Detached
Error from Data layer: TableName=BlahBlah, PK={row3["PK"].ToString()}, RowState=Detached
InnerException Message = innerException";

			AssertEquals("Use the first row as the row property", row1, ex.Row);
			AssertEquals("All rows are printed when needPrintAll is true", expectedMessage, ex.Message);

			ex = new ZDataException(innerException, dataRows, false, Db.Connection);

			expectedMessage = @$"Error from Data layer: TableName=BlahBlah, PK={row1["PK"].ToString()}, RowState=Detached
InnerException Message = innerException";

			AssertEquals("Use the first row as the row property", row1, ex.Row);
			AssertEquals("The first row is printed when needPrintAll is false", expectedMessage, ex.Message);
		}

		public void TestError50000PassesDescriptionToFriendlyMessage()
		{
			try
			{
				Db.Connection.ExecuteNonQuery("RAISERROR ('Hello', 16, 1)");
				Fail("Expected SqlException not thrown");
			}
			catch (SqlException ex)
			{
				ZDataException exception = new ZDataException(ex, null, Db.Connection);
				AssertEquals("Hello", exception.FriendlyMessage);
			}
		}

		public void TestFriendlyMessageWithWeirdException()
		{
			Exception innerException = new Exception();
			ZDataException e = new ZDataException(innerException, MakeDummyRow(), Db.Connection);
			AssertEquals("No msg", "", e.FriendlyMessage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestFriendlyMessageWithDBException()
		{
			ZDataException e = null;

			ZSqlDataAccessor accessor = new ZSqlDataAccessor(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""));
			DataTable dummyTable = accessor.GetTable(DummyDependentBizoSchema.Constants.TableName);
			DataRow row = dummyTable.NewRow();

			new DummyDependantBusinessObject(new BusinessObjectFactory(), row).ZD1_Z0 = Guid.NewGuid(); // put in defaults and invalid FK
			dummyTable.Rows.Add(row);

			try
			{
				accessor.Save();
			}
			catch (ZDataException ex)
			{
				e = ex;
			}

			Assert("Has msg", !string.IsNullOrEmpty(e.FriendlyMessage)); // FK constraint violation
		}
		public void TestShouldBeReportedToEDI_DetectsConstraintAsException()
		{
			SqlError sqlError = SqlExceptionBuilder.CreateSqlError(547, byte.MaxValue, byte.MinValue, "dbserver", "The INSERT CHECK \"", "@@lols", 111);
			SqlErrorCollection errors = SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
			SqlException exception = SqlExceptionBuilder.CreateSqlException(errors);
			ZDataException zde = new ZDataException(exception, null, Db.Connection);
			AssertEquals(547, exception.Number);
			AssertEquals(DbErrorType.InsertConflictedWithCheckConstraint, zde.CoreErrorHandler.ExceptionType);
			Assert(zde.ShouldBeReportedToEDI);
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
