using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateFCM_GG_ReleaseGroupTransform))]
	public class UUpdateFCM_GG_ReleaseGroupTransformTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.FeatureControlHeader WHERE FCM_FeatureControlCode is NULL"));
			AssertEquals(new Guid("94755E71-A87A-4034-8DFA-785773A49607"), TestConnection.ExecuteScalar<Guid>("SELECT FCM_GG_ReleaseGroup FROM dbo.FeatureControlHeader WHERE FCM_FeatureControlCode = 'ACCEINVCF'"));
			AssertEquals(new Guid("94755E71-A87A-4034-8DFA-785773A49607"), TestConnection.ExecuteScalar<Guid>("SELECT FCM_GG_ReleaseGroup FROM dbo.FeatureControlHeader WHERE FCM_FeatureControlCode = 'ACCRBKFTR'"));
			AssertEquals(new Guid("6e04de89-5c4f-4f2c-9ba0-b4ffc4100883"), TestConnection.ExecuteScalar<Guid>("SELECT FCM_GG_ReleaseGroup FROM dbo.FeatureControlHeader WHERE FCM_FeatureControlCode = 'CR5RESWIZ'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateFCM_GG_ReleaseGroupTransform();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "FeatureControlHeader", $@"
CREATE TABLE dbo.FeatureControlHeader
(
   [FCM_PK] UNIQUEIDENTIFIER NOT NULL,
   [FCM_FeatureControlCode] VARCHAR(9) NOT NULL DEFAULT '',
   [FCM_Description] NVARCHAR(80) NOT NULL DEFAULT '',
   [FCM_GG_ReleaseGroup] UNIQUEIDENTIFIER NULL,
   [FCM_WKI_ActiveWorkItem] UNIQUEIDENTIFIER NULL,
   [FCM_WKI_DeactivateWorkItem] UNIQUEIDENTIFIER NULL,
   [FCM_SystemCreateTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [FCM_SystemLastEditTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
   [FCM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);
 
ALTER TABLE [FeatureControlHeader] SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [FeatureControlHeader]
ADD CONSTRAINT [PK_UX__FCM_PK] PRIMARY KEY NONCLUSTERED ([FCM_PK] ASC)
WITH (IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE CLUSTERED INDEX [NR_UC__FCM_FeatureControlCode] ON [FeatureControlHeader] ([FCM_FeatureControlCode] ASC)
WITH (IGNORE_DUP_KEY = OFF)
;

ALTER TABLE [FeatureControlHeader] WITH NOCHECK
	  ADD CONSTRAINT [FeatureControlHeader_FCM_GG_ReleaseGroup_FK2_GlbGroup_RRR_120N] FOREIGN KEY
		  ( [FCM_GG_ReleaseGroup] )
		  REFERENCES [GlbGroup]
		  ( [GG_PK] );

			");

			var glbGroupTestData = @"INSERT INTO dbo.GlbGroup(GG_PK, GG_Code, GG_Type, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser)
VALUES('6e04de89-5c4f-4f2c-9ba0-b4ffc4100883', 'TTT', 'STF', 'Test code', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var cmd = Db.Connection.Command(glbGroupTestData))
			{
				cmd.ExecuteNonQuery();
			}

			var featureControlHeaderTestData = @"INSERT INTO dbo.FeatureControlHeader
(FCM_PK, FCM_FeatureControlCode, FCM_Description, FCM_GG_ReleaseGroup, FCM_SystemCreateTimeUtc, FCM_SystemCreateUser, FCM_SystemLastEditTimeUtc, FCM_SystemLastEditUser)
VALUES
(NEWID(), 'ACCEINVCF', 'Accounting Electronic Invoicing Configuration', NULL, '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
(NEWID(), 'ACCRBKFTR', 'Accounting Reporting Book Feature', NULL, '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
(NEWID(), 'CR5RESWIZ', 'CR5 Resolution Wizard', '6e04de89-5c4f-4f2c-9ba0-b4ffc4100883', '2020-06-03 12:00:00', 'E', '2020-06-01 12:00:00', 'E');";

			using (var cmd = Db.Connection.Command(featureControlHeaderTestData))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
