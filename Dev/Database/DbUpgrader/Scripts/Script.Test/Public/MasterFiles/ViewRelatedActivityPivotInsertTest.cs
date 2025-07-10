using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(ViewRelatedActivityPivot_Insert))]
	class ViewRelatedActivityPivotInsertTest : DbCreateScriptTest
	{
		public void TestInsert()
		{
			const string insertSql = @"
INSERT INTO dbo.ViewRelatedActivityPivot
	(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
	('1F910665-6AA7-492C-9BB7-75D4615ECAAF', @RAP_ParentActivityTableCode, @RAP_ParentActivityID, @RAP_ChildActivityTableCode, @RAP_ChildActivityID, @RAP_SalesRelationTreeID, @RAP_SystemCreateTimeUtc, @RAP_SystemCreateUser, @RAP_SystemLastEditTimeUtc, @RAP_SystemLastEditUser)
";

			var childActivityId = Guid.NewGuid();
			var parentActivityId = Guid.NewGuid();
			var salesRelationTreeId = Guid.NewGuid();
			var now = new DateTime(2012, 2, 17, 1, 0, 0, DateTimeKind.Utc);
			var support = "E";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@RAP_ParentActivityTableCode", SqlDbType.VarChar, "G0");
				command.AddParameter("@RAP_ParentActivityID", SqlDbType.UniqueIdentifier, parentActivityId);
				command.AddParameter("@RAP_ChildActivityTableCode", SqlDbType.VarChar, "G8");
				command.AddParameter("@RAP_ChildActivityID", SqlDbType.UniqueIdentifier, childActivityId);
				command.AddParameter("@RAP_SalesRelationTreeID", SqlDbType.UniqueIdentifier, salesRelationTreeId);
				command.AddParameter("@RAP_SystemCreateTimeUtc", SqlDbType.DateTime,now);
				command.AddParameter("@RAP_SystemCreateUser", SqlDbType.VarChar, support);
				command.AddParameter("@RAP_SystemLastEditTimeUtc", SqlDbType.DateTime, now);
				command.AddParameter("@RAP_SystemLastEditUser", SqlDbType.VarChar, support);

				command.ExecuteNonQuery();

				command.CommandText = "SELECT * FROM dbo.RelatedActivityPivot WHERE RAP_PK = '1F910665-6AA7-492C-9BB7-75D4615ECAAF'";
				using (var result = command.ExecuteReader())
				{
					result.Read();

					AssertEquals(childActivityId, (Guid)result["RAP_ChildActivityID"]);
					AssertEquals(parentActivityId, (Guid)result["RAP_ParentActivityID"]);
					AssertEquals(salesRelationTreeId, (Guid)result["RAP_SalesRelationTreeID"]);
					AssertEquals("G0", result["RAP_ParentActivityTableCode"]);
					AssertEquals("G8", result["RAP_ChildActivityTableCode"]);
					AssertEquals(now, (DateTime)result["RAP_SystemCreateTimeUtc"]);
					AssertEquals(support, result["RAP_SystemCreateUser"]);
					AssertEquals(now, (DateTime)result["RAP_SystemLastEditTimeUtc"]);
					AssertEquals(support, result["RAP_SystemLastEditUser"]);
				}
			}
		}
	}
}
