using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(ViewClientProcessHeader))]
	sealed class ViewClientProcessHeaderTest : DbCreateScriptTest
	{
		public void TestIsEmptyWithCorrectColumns()
		{
			TestConnection.ExecuteNonQuery(BMDbTestHelper.GetProcessHeaderInsertSql(Guid.NewGuid(), null, Guid.NewGuid(), parentTableCode: "IM", workflowType: "INC"));

			var viewClientProcessHeaderResult = DataUtils.GetDataTableFromQuery(TestConnection, "select * from dbo.ViewClientProcessHeader");
			var viewProcessHeaderResult = DataUtils.GetDataTableFromQuery(TestConnection, "select * from dbo.ViewProcessHeader");

			AssertEquals("Result should have no rows", 0, viewClientProcessHeaderResult.Rows.Count);

			AssertNoExceptionThrown("ViewClientProcessHeader should have the same columns as ViewProcessHeader", () =>
			{
				AssertEqualColumns(viewClientProcessHeaderResult.Columns, viewProcessHeaderResult.Columns);
			});
		}

		public void AssertEqualColumns(DataColumnCollection expectedColumns, DataColumnCollection actualColumns)
		{
			AssertEquals("The number of columns should be equal.", expectedColumns.Count, actualColumns.Count);

			for (int i = 0; i < expectedColumns.Count; i++)
			{
				AssertEquals("Columns should have the same names.", expectedColumns[i].ColumnName, actualColumns[i].ColumnName);
				AssertEquals("Columns should have the same types.", expectedColumns[i].DataType, actualColumns[i].DataType);
			}
		}
	}
}

