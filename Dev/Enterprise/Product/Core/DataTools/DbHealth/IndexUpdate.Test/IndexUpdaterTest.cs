using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	class IndexUpdaterTest : TransactionedTestCase
	{
		public void TestQueryIndexUpdateInfoDoesNotReturnDuplicatedIndexes()
		{
			// Arrange
			var testDbName = Db.Connection.CurrentDatabase;
			const string tableName = "TestTable1";
			const string indexName = "TestTable1_X";
			const string user1 = "AAA";
			const string user2 = "BBB";

			Db.Connection.ExecuteNonQuery($@"
CREATE TABLE {testDbName.QuoteName()}.dbo.{tableName}(id int NOT NULL PRIMARY KEY, x uniqueidentifier NULL);
CREATE NONCLUSTERED INDEX {indexName} ON {testDbName.QuoteName()}.dbo.{tableName}(x);
");

			Db.Connection.ExecuteNonQuery($@"
INSERT INTO [dbo].[StmExtendedProperty] ([SEP_Class], [SEP_DatabaseNameSuffix], [SEP_Name], [SEP_Value], [SEP_SchemaName], [SEP_MajorObjectName], [SEP_MinorObjectName], [SEP_SystemCreateUser], [SEP_SystemLastEditUser])
VALUES('Index', '', 'LastRebuild', '2016-03-22', 'dbo', '{tableName}', '{indexName}', '{user1}', '{user1}');

INSERT INTO [dbo].[StmExtendedProperty] ([SEP_Class], [SEP_DatabaseNameSuffix], [SEP_Name], [SEP_Value], [SEP_SchemaName], [SEP_MajorObjectName], [SEP_MinorObjectName], [SEP_SystemCreateUser], [SEP_SystemLastEditUser])
VALUES('Index', '', 'LastReorganise', '2016-03-22', 'dbo', '{tableName}', '{indexName}', '{user1}', '{user2}');
");

			// Act
			var allIndexesToUpdate = new IndexUpdaterForTest(Mock.Of<ILogger>()).QueryIndexUpdateInfo_Expose(Db.Connection, testDbName);

			// Assert
			var targetIndex = allIndexesToUpdate.Where(x =>
				x.DatabaseName == testDbName
				&& x.TableName == tableName
				&& x.IndexName == indexName);

			AssertEquals(1, targetIndex.Count());
		}

		public class IndexUpdaterForTest : IndexUpdater
		{
			public IndexUpdaterForTest(ILogger logger) : base(logger)
			{
			}

			public List<IndexUpdateInfo> QueryIndexUpdateInfo_Expose(DbConnection connection, string targetDb) => base.QueryIndexUpdateInfo(connection, targetDb);

			protected override string CustomExceptionMessage => throw new NotImplementedException();

			protected override void RunOnGivenDB(DbConnection connection, string dbName) => throw new NotImplementedException();
		}
	}
}
