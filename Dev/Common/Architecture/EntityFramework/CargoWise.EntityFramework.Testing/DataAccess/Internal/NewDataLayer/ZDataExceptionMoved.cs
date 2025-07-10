using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	//Moved from Test Extraction task
	partial class ZDataExceptionTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestDetachedRowDump()
		{
			ZSqlDataAccessor accessor = new ZSqlDataAccessor(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""));
			DataTable dummyTable = accessor.GetTable(DummyDependentBizoSchema.Constants.TableName);
			DataRow row = dummyTable.NewRow();
			string message = ZDataException.CreateMessage(row, null);
			AssertEquals("Error from Data layer: TableName=DummyDependentBizo, PK=00000000-0000-0000-0000-000000000000, RowState=Detached", message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowDump()
		{
			ZSqlDataAccessor accessor = new ZSqlDataAccessor(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""));
			DataTable dummyTable = accessor.GetTable(DummyDependentBizoSchema.Constants.TableName);
			DataRow row = dummyTable.NewRow();

			new DummyDependantBusinessObject(null, row);

			dummyTable.Rows.Add(row);
			string actual = ZDataException.CreateMessage(row, null);
			string expected = @"ZD1_Code = 
ZD1_Number = 0
ZD1_NumberUnit = 
ZD1_NumberUnitCode = 
ZD1_Z0 = ";

			Assert("Message should contain " + expected + System.Environment.NewLine + " but was " + actual, actual.IndexOf(expected) > -1);
		}
	}
}
