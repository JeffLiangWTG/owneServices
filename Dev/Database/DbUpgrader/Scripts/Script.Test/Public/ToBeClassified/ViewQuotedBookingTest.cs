using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(ViewQuotedBooking))]
	class ViewQuotedBookingTest : DbCreateScriptTest
	{
		public void TestViewQuotedBookingCastsToBit()
		{
			var testHelper = new TestDbHelper(TestConnection);

			var reader = (IDataReader)testHelper.RunSQL(null, "SELECT * FROM dbo.ViewQuotedBooking", CommandType.Text, TestDbHelperBase.SQLExecutionTypes.ExecuteReader);
			var dataTable = new DataTable();
			dataTable.Load(reader);
			Assert("Should be boolean", dataTable.Columns["VB_IsCanceled"].DataType == typeof(bool));
			Assert("Should be boolean", dataTable.Columns["VB_IsConsolidated"].DataType == typeof(bool));
		}
	}
}

