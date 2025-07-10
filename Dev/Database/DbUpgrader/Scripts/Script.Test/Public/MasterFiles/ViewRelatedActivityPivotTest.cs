using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(ViewRelatedActivityPivot))]
	class ViewRelatedActivityPivotTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			var now = DateTime.UtcNow;
			var insertPivotsSql = @"
DECLARE @Company UNIQUEIDENTIFIER = (SELECT TOP 1 GC_PK FROM dbo.GlbCompany);

INSERT INTO dbo.RelatedActivityPivot
	(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
	(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'G8', '4e33c461-ed10-465c-a064-f8e5a52054cc', '4e33c461-ed10-465c-a064-f8e5a52054cc', @Now, 'E', @Now, 'E')

INSERT INTO dbo.GlbCompanyCampaign
	(G0_PK, G0_GC, G0_G0_Master, G0_HorizontalId, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES
	('63B63667-3202-475C-8929-A9095297BE2E', @Company, NULL, 0, 'Master Campaign', 'TST00001000', @Now, 'E', @Now, 'E'),
	('288CD212-7EFC-47E4-A410-FFBA9719D241', @Company, '63B63667-3202-475C-8929-A9095297BE2E', 1, 'Campaign Touch', 'TST00001001', @Now, 'E', @Now, 'E')
";

			using (var command = TestConnection.Command(insertPivotsSql))
			{
				command.AddParameter("@Now", SqlDbType.DateTime, now);

				command.ExecuteNonQuery();
			}

			AssertEquals(2, TestConnection.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.ViewRelatedActivityPivot WHERE RAP_SystemCreateTimeUtc = @Now", cmd => cmd.AddParameter("@Now", SqlDbType.SmallDateTime, now)));
			AssertEquals(1, TestConnection.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.ViewRelatedActivityPivot WHERE RAP_IsEditable = 1 AND RAP_SystemCreateTimeUtc = @Now", cmd => cmd.AddParameter("@Now", SqlDbType.SmallDateTime, now)));
			AssertEquals(1, TestConnection.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.ViewRelatedActivityPivot WHERE RAP_IsEditable = 0 AND RAP_SystemCreateTimeUtc = @Now", cmd => cmd.AddParameter("@Now", SqlDbType.SmallDateTime, now)));
		}
	}
}
