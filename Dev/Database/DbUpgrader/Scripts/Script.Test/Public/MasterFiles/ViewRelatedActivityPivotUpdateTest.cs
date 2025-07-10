using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(ViewRelatedActivityPivot_Update))]
	class ViewRelatedActivityPivotUpdateTest : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			const string insertPivotsSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);

INSERT INTO dbo.RelatedActivityPivot
	(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
	('A3F1A156-879D-4603-BF9A-A1DC0CBF6479', 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'G8', '4e33c461-ed10-465c-a064-f8e5a52054cc', '4e33c461-ed10-465c-a064-f8e5a52054cc', getutcdate(), 'ZZ', getutcdate(), 'ZZ')

INSERT INTO dbo.GlbCompanyCampaign
	(G0_PK, G0_GC, G0_G0_Master, G0_HorizontalId, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES
	('63B63667-3202-475C-8929-A9095297BE2E', @Company, NULL, 0, 'Master Campaign', 'TST00001000', getutcdate(), 'ZZ', getutcdate(), 'ZZ'),
	('288CD212-7EFC-47E4-A410-FFBA9719D241', @Company, '63B63667-3202-475C-8929-A9095297BE2E', 1, 'Campaign Touch', 'TST00001001', getutcdate(), 'ZZ', getutcdate(), 'ZZ')
";

			using (var command = TestConnection.Command(insertPivotsSql))
			{
				command.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command("UPDATE dbo.ViewRelatedActivityPivot SET RAP_PK = @PK WHERE RAP_IsEditable = 1"))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				AssertExceptionThrown<SqlException>("RAP_PK SqlException", "RAP_PK cannot be changed", () => command.ExecuteNonQuery());
			}

			using (var command = TestConnection.Command("UPDATE dbo.ViewRelatedActivityPivot SET RAP_IsEditable = 1 WHERE RAP_IsEditable = 0"))
			{
				AssertExceptionThrown<SqlException>("RAP_IsEditable SqlException", "RAP_IsEditable cannot be changed", () => command.ExecuteNonQuery());
			}

			const string updateSql = @"
UPDATE dbo.ViewRelatedActivityPivot SET
	RAP_ChildActivityID = @RAP_ChildActivityID,
	RAP_ChildActivityTableCode = @RAP_ChildActivityTableCode,
	RAP_ParentActivityID = @RAP_ParentActivityID,
	RAP_ParentActivityTableCode = @RAP_ParentActivityTableCode,
	RAP_SalesRelationTreeID = @RAP_SalesRelationTreeID,
	RAP_SystemCreateTimeUtc = @RAP_SystemCreateTimeUtc,
	RAP_SystemCreateUser = @RAP_SystemCreateUser,
	RAP_SystemLastEditTimeUtc = @RAP_SystemLastEditTimeUtc,
	RAP_SystemLastEditUser = @RAP_SystemLastEditUser
WHERE
	RAP_PK = 'A3F1A156-879D-4603-BF9A-A1DC0CBF6479'
";

			var childActivityId = Guid.NewGuid();
			var parentActivityId = Guid.NewGuid();
			var salesRelationTreeId = Guid.NewGuid();
			var now = new DateTime(2012, 2, 17, 1, 0, 0, DateTimeKind.Utc);
			var support = "E";

			using (var command = TestConnection.Command(updateSql))
			{
				command.AddParameter("@RAP_ChildActivityID", SqlDbType.UniqueIdentifier, childActivityId);
				command.AddParameter("@RAP_ChildActivityTableCode", SqlDbType.VarChar, "G0");
				command.AddParameter("@RAP_ParentActivityID", SqlDbType.UniqueIdentifier, parentActivityId);
				command.AddParameter("@RAP_ParentActivityTableCode", SqlDbType.VarChar, "G8");
				command.AddParameter("@RAP_SalesRelationTreeID", SqlDbType.UniqueIdentifier, salesRelationTreeId);
				command.AddParameter("@RAP_SystemCreateTimeUtc", SqlDbType.DateTime, now);
				command.AddParameter("@RAP_SystemCreateUser", SqlDbType.VarChar, support);
				command.AddParameter("@RAP_SystemLastEditTimeUtc", SqlDbType.DateTime, now);
				command.AddParameter("@RAP_SystemLastEditUser", SqlDbType.VarChar, support);

				command.ExecuteNonQuery();

				command.CommandText = "SELECT * FROM dbo.RelatedActivityPivot WHERE RAP_PK = 'A3F1A156-879D-4603-BF9A-A1DC0CBF6479'";
				using (var result = command.ExecuteReader())
				{
					result.Read();

					AssertEquals(childActivityId, (Guid)result["RAP_ChildActivityID"]);
					AssertEquals(parentActivityId, (Guid)result["RAP_ParentActivityID"]);
					AssertEquals(salesRelationTreeId, (Guid)result["RAP_SalesRelationTreeID"]);
					AssertEquals("G8", result["RAP_ParentActivityTableCode"]);
					AssertEquals("G0", result["RAP_ChildActivityTableCode"]);
					AssertEquals(now, (DateTime)result["RAP_SystemCreateTimeUtc"]);
					AssertEquals(support, result["RAP_SystemCreateUser"]);
					AssertEquals(now, (DateTime)result["RAP_SystemLastEditTimeUtc"]);
					AssertEquals(support, result["RAP_SystemLastEditUser"]);
				}
			}
		}
	}
}
