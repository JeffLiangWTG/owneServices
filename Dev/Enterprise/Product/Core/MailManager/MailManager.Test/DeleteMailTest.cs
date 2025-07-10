using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MailManager.Testing
{
	sealed class DeleteMailTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestDeleteWhileAddingRow()
		{
			const int numOfRows = 4;
			const int numOfDays = 3;

			var date = ZDateTime.Now.AddDays(-(numOfDays + 1));
			CreateTestData(numOfRows, date);

			using (var insertConnection = Db.NewExtraConnectionToMainDb())
			{
				insertConnection.BeginTransaction();
				var sqlInsert = CreateInsertMailDbItemsStatement(ZGuid.NewZGuid(), date);
				insertConnection.ExecuteNonQuery(sqlInsert);

				var deleteEmailManagement = DatabaseEmailManagement.Create();
				var count = deleteEmailManagement.PurgeIncomingEmailOlderThan(numOfDays);

				AssertEquals(numOfRows, count);

				insertConnection.CommitTransaction();

				count = deleteEmailManagement.PurgeIncomingEmailOlderThan(numOfDays);
				AssertEquals(1, count);
			}
		}

		[UseSnapshotProtection]
		public void TestDeleteWhileUpdatingRow()
		{
			const int numOfRows = 4;
			const int numOfDays = 3;

			var date = ZDateTime.Now.AddDays(-(numOfDays + 1));
			var guids = CreateTestData(numOfRows, date);

			using (var insertConnection = Db.NewExtraConnectionToMainDb())
			{
				insertConnection.BeginTransaction();
				var newData = ZDateTime.Now.AddDays(-(numOfDays - 1));
				var sqlInsert = CreateUpdateMailDbItemsStatement(guids[0], newData);
				insertConnection.ExecuteNonQuery(sqlInsert);

				var deleteEmailManagement = DatabaseEmailManagement.Create();
				var count = deleteEmailManagement.PurgeIncomingEmailOlderThan(numOfDays);

				AssertEquals(numOfRows - 1, count);

				insertConnection.CommitTransaction();

				count = deleteEmailManagement.PurgeIncomingEmailOlderThan(numOfDays - 2);
				AssertEquals(1, count);
			}
		}

		static List<ZGuid> CreateTestData(int numOfRows, ZDateTime date)
		{
			Db.Connection.BeginTransaction();

			var guids = new List<ZGuid>();
			var sqlInsert = "";
			for (var i = 0; i < numOfRows; ++i)
			{
				var guid = ZGuid.NewZGuid();
				guids.Add(guid);
				sqlInsert += CreateInsertMailDbItemsStatement(guid, date);
				sqlInsert += System.Environment.NewLine;
			}

			Db.Connection.ExecuteNonQuery(sqlInsert);
			Db.Connection.CommitTransaction();

			return guids;
		}

		static string CreateInsertMailDbItemsStatement(ZGuid guid, ZDateTime date)
		{
			return string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES ('{8}', '{9}', '{10}-{11}-{12}', '{10}-{11}-{12}', '{10}-{11}-{12}', '{13}', '{13}');",
					MailDBItemsSchema.Constants.TableName,
					MailDBItemsSchema.Constants.PK,
					MailDBItemsSchema.Constants.MI_Direction,
					MailDBItemsSchema.Constants.MI_ReceivedDateTime,
					MailDBItemsSchema.Constants.MI_SystemCreateTimeUtc,
					MailDBItemsSchema.Constants.MI_SystemLastEditTimeUtc,
					MailDBItemsSchema.Constants.MI_SystemCreateUser,
					MailDBItemsSchema.Constants.MI_SystemLastEditUser,
					guid,
					MailDirection.Receive,
					date.Year,
					date.Month,
					date.Day,
					'E');
		}

		static string CreateUpdateMailDbItemsStatement(ZGuid guid, ZDateTime date)
		{
			return string.Format("UPDATE {0} SET {1} = '{2}-{3}-{4}' WHERE {5} = '{6}';",
					MailDBItemsSchema.Constants.TableName,
					MailDBItemsSchema.Constants.MI_ReceivedDateTime,
					date.Year,
					date.Month,
					date.Day,
					MailDBItemsSchema.Constants.PK,
					guid);
		}
	}
}
