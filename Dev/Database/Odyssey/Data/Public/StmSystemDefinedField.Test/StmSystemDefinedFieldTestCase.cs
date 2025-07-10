using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	abstract class StmSystemDefinedFieldTestCase : TransactionedTestCase
	{
		protected DataSet InsertData()
		{
			DataHelpers.ClearTable(TableName);

			InsertRow(PK1, "Name1", "Context1");
			InsertRow(PK2, "Name2", "Context2");

			var dataFile = new StmSystemDefinedFieldDataFile();
			return dataFile.LoadDataFromDatabase();
		}

		protected void AssertData(DataSet dataSet)
		{
			AssertEquals("dataSet.Tables.Count", 1, dataSet.Tables.Count);
			AssertEquals("dataSet.Tables[\"StmSystemDefinedField\"].Rows.Count", 2, dataSet.Tables[TableName].Rows.Count);

			DataRow row1 = dataSet.Tables[TableName].Rows[0];
			DataRow row2 = dataSet.Tables[TableName].Rows[1];

			Assert(dataSet.Tables[TableName].Rows.Contains(PK1));
			Assert(dataSet.Tables[TableName].Rows.Contains(PK2));
		}

		protected void InsertRow(Guid pK, string name, string businessContext)
		{
			string sqlText = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES (@S1_PK, @S1_Name, @S1_BusinessContext)",
				TableName, S1_PK, S1_Name, S1_BusinessContext);

			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameter("@S1_PK", SqlDbType.UniqueIdentifier, pK);
				command.AddParameter("@S1_Name", SqlDbType.VarChar, name);
				command.AddParameter("@S1_BusinessContext", SqlDbType.VarChar, businessContext);
				command.ExecuteNonQuery();
			}
		}

		protected void DeleteRow(Guid pK)
		{
			string sqlText = string.Format("DELETE FROM {0} WHERE {1} = @S1_PK",
				TableName, S1_PK);

			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameter("@S1_PK", SqlDbType.UniqueIdentifier, pK);
				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			PK1 = Guid.NewGuid();
			PK2 = Guid.NewGuid();
		}

		protected Guid PK1;
		protected Guid PK2;
		const string TableName = StmSystemDefinedFieldSchema.Constants.TableName;
		const string S1_PK = StmSystemDefinedFieldSchema.Constants.PK;
		const string S1_Name = StmSystemDefinedFieldSchema.Constants.S1_Name;
		const string S1_BusinessContext = StmSystemDefinedFieldSchema.Constants.S1_BusinessContext;
	}
}
