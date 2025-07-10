using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.StmData
{
	sealed class StmDataDataFileTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMustCleanDataBeforeSetup()
		{
			StmDataDataFile testDataFile = new StmDataDataFile();
			AssertEquals("MustCleanDataBeforeSetup", true, testDataFile.MustCleanDataBeforeSetup);

			DataSet data = testDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 1, data.Tables.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestNonGLAccountRowsAreNotLoaded()
		{
			Guid accountPk1 = Guid.NewGuid();
			Guid accountPk2 = Guid.NewGuid();
			Guid thingPk3 = Guid.NewGuid();
			Guid thingPk4 = Guid.NewGuid();

			InsertStmData(accountPk1, "GL_RANDOM_AND_NOT_REAL_ACCOUNT");
			InsertStmData(accountPk2, "another_random_and_not_real_account");
			InsertStmData(thingPk3, "A_RANDOM_AND_NOT_REAL_THING");
			InsertStmData(thingPk4, "another_random_and_not_real_thing");

			StmDataDataFile testDataFile = new StmDataDataFile();
			DataSet data = testDataFile.LoadDataFromDatabase();

			Assert("StmData related to GL Accounts should have been loaded", data.Tables[StmDataSchema.Constants.TableName].Rows.Contains(accountPk1));
			Assert("StmData NOT related to GL Accounts should NOT have been loaded", !data.Tables[StmDataSchema.Constants.TableName].Rows.Contains(accountPk2));
			Assert("StmData NOT related to GL Accounts should NOT have been loaded", !data.Tables[StmDataSchema.Constants.TableName].Rows.Contains(thingPk3));
			Assert("StmData NOT related to GL Accounts should NOT have been loaded", !data.Tables[StmDataSchema.Constants.TableName].Rows.Contains(thingPk4));
		}

		void InsertStmData(Guid pk, string name)
		{
			string query = string.Format("INSERT {0} ({1}, {2}, {3}) VALUES (@PK, @SD_Name, @SD_Type)",
				StmDataSchema.Constants.TableName,
				StmDataSchema.PK.Name,
				StmDataSchema.SD_Name.Name,
				StmDataSchema.SD_Type.Name);

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@SD_Name", SqlDbType.VarChar, name);
				command.AddParameter("@SD_Type", SqlDbType.Char, "GID");
				command.ExecuteNonQuery();
			}
		}
	}
}
