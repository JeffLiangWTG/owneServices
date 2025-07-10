using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{	
	[TestedType(typeof(PopulateProcessHeaderEffectiveAgreedDeliveryDate))]
	class PopulateProcessHeaderEffectiveAgreedDeliveryDateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateProcessHeaderEffectiveAgreedDeliveryDate();

		protected override void PrepareTestData()
		{
			var systemPK = Guid.NewGuid();
			var componentPK = Guid.NewGuid();

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_INS_ProcessHeaderUpdateEffectiveAgreedDeliveryDate", ProcessHeaderSchema.Constants.TableName))
			{
				TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.BMSystem
(FS_PK, FS_Name, FS_Description, FS_SystemCreateTimeUtc, FS_SystemCreateUser, FS_SystemLastEditTimeUtc, FS_SystemLastEditUser)
VALUES ('{systemPK}', 'ASystem', 'A BMS System', getutcdate(), 'E', getutcdate(), 'E')

INSERT INTO dbo.BMComponent
(FC_PK, FC_FS_System, FC_Name, FC_Type, FC_SystemCreateTimeUtc, FC_SystemCreateUser, FC_SystemLastEditTimeUtc, FC_SystemLastEditUser)
VALUES ('{componentPK}', '{systemPK}', 'Component1', 'BUC', getutcdate(), 'E', getutcdate(), 'E')

INSERT INTO dbo.ProcessHeader
	(FH_PK, FH_AgreedDeliveryDate, FH_FH_ParentHeader, FH_WorkflowType, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser, FH_Status, FH_IsActive, FH_FC_DedicatedBuffer)
VALUES
	-- job header with no AgreedDeliveryDate
	('10000000-64F7-485F-8D8F-BD732BAB1710', NULL, NULL, 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{componentPK}'),
	('11000000-64F7-485F-8D8F-BD732BAB1710', NULL, '10000000-64F7-485F-8D8F-BD732BAB1710', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{componentPK}'),
	('12000000-64F7-485F-8D8F-BD732BAB1710', '2024-01-01', '10000000-64F7-485F-8D8F-BD732BAB1710', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{componentPK}'),
	('13000000-64F7-485F-8D8F-BD732BAB1710', '2024-01-01', '10000000-64F7-485F-8D8F-BD732BAB1710', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, NULL),
	-- job header with an AgreedDeliveryDate
	('20000000-64F7-485F-8D8F-BD732BAB1710', '2023-01-01', NULL, 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{componentPK}'),
	('21000000-64F7-485F-8D8F-BD732BAB1710', NULL, '20000000-64F7-485F-8D8F-BD732BAB1710', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{componentPK}'),
	('22000000-64F7-485F-8D8F-BD732BAB1710', '2024-01-01', '20000000-64F7-485F-8D8F-BD732BAB1710', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 1, '{componentPK}'),
	('23000000-64F7-485F-8D8F-BD732BAB1710', '2024-01-01', '20000000-64F7-485F-8D8F-BD732BAB1710', 'WKI', GETUTCDATE(), GETUTCDATE(), 'E', 'E', 'OPN', 0, '{componentPK}');
");
			}
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				Assert("JLW1 not set", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '10000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc IS NULL"));
				Assert("WF1.1 no ADD", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '11000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc IS NULL"));
				Assert("WF1.2 uses WF ADD", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '12000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc = '2024-01-01'"));
				Assert("WF1.3 not set (no dedicated buffer)", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '13000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc IS NULL"));
				Assert("JLW2 not set", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '20000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc IS NULL"));
				Assert("WF2.1 uses JLW ADD", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '21000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc = '2023-01-01'"));
				Assert("WF2.2 uses WF ADD", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '22000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc = '2024-01-01'"));
				Assert("WF2.3 not set (inactive)", Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '23000000-64F7-485F-8D8F-BD732BAB1710' AND FH_EffectiveAgreedDeliveryDateUtc IS NULL"));
			});
		}
	}
}
