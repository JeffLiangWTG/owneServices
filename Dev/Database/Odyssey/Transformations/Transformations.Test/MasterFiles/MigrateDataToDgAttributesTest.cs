using System;
using System.Threading;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles
{
	[TestedType(typeof(MigrateDataToDgAttributes))]
	class MigrateDataToDgAttributesTest : DataTransformationTestCase
	{
		const string TriggerName = "TG_ZZUNDGSubstance_CopyOnUpdate";

		protected override DataTransformation GetNewTestTransformationInstance() => new MigrateDataToDgAttributes();

		Guid Guid1 = Guid.NewGuid();
		Guid Guid2 = Guid.NewGuid();
		Guid Guid3 = Guid.NewGuid();
		Guid Guid4 = Guid.NewGuid();
		Guid Guid5 = Guid.NewGuid();
		Guid Guid6 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);
			CreateUNDGSubstanceData(helper, Guid1, "1234", "b", "ship1");
			CreateUNDGSubstanceData(helper, Guid2, "5678", "b", "ship2");
			CreateUNDGSubstanceData(helper, Guid3, "1234", "c", "ship3");
			CreateUNDGSubstanceData(helper, Guid4, "5678", "c", "ship4");
			CreateUNDGSubstanceData(helper, Guid5, "9101", "d", "");
		}

		void CreateUNDGSubstanceData(TestDbHelper testDbHelper, Guid recordPK, string unno, string variant, string shippingName)
		{
			testDbHelper.Insert(ZZUNDGSubstanceSchema.Constants.TableName, new
			{
				DG_PK = recordPK,
				DG_IsActive = 1,
				DG_IsSystem = 0,
				DG_UNNO = unno,
				DG_Variation = "Variation description",
				DG_Class = "3",
				DG_SubLabel1 = "Label1",
				DG_PSN = "Proper Shipping Name",
				DG_PG = "PGI",
				DG_EMS = "F-E",
				DG_LQMaxAmt = 50.123,
				DG_LQMaxAmtUQ = "kg",
				DG_DglPhrase = "Dangerous goods limited phrase",
				DG_PackIns = "P001",
				DG_IMOTankIns = "IMO01",
				DG_UNTankIns = "UNTK01",
				DG_TankProv = "Tank provision details",
				DG_Pointers = "PNTR01",
				DG_StowCat = "A",
				DG_CodedStow = "Stowage information",
				DG_State = "S",
				DG_UsrUSDOTShippingName = shippingName,
				DG_ExceptedQuantityCode = "E2",
				DG_Code = unno + variant,
				DG_Variant = variant,
				DG_CargoMaxAmt = 100.456,
				DG_CargoMaxAmtUQ = "L",
				DG_CargoPackAmtType = "NLM",
				DG_IsNotOtherwiseSpecified = 0,
				DG_LQ2OrPaxMaxAmt = 2.567,
				DG_LQ2OrPaxMaxAmtType = "NAP",
				DG_LQ2OrPaxMaxAmtUQ = "mL",
				DG_LQMaxAmtType = "NAP",
				DG_LQSpecProvIndex = "SPX001",
				DG_PaxPackIns = "PAX01",
				DG_Standard = "IMO",
				DG_AutoVersion = 1,
				DG_SystemCreateTimeUtc = DateTime.UtcNow,
				DG_SystemCreateUser = "~BP",
				DG_SystemLastEditTimeUtc = DateTime.UtcNow,
				DG_SystemLastEditUser = "~BP"
			});
		}

		protected override void AssertTransformationResults()
		{
			DBTransformationTestHelper.DropIndexIfExists(UNDGAttributeSchema.Constants.TableName, "FK_UC__DA_DG_DA_Language_DA_Type_DA_Index");
			
			AssertEquals(true, TestConnection.Exists($"FROM UNDGAttribute WHERE DA_DG = '{Guid1}' AND DA_Descriptor = 'ship1'"));
			AssertEquals(true, TestConnection.Exists($"FROM UNDGAttribute WHERE DA_DG= '{Guid2}' AND DA_Descriptor = 'ship2'"));
			AssertEquals(true, TestConnection.Exists($"FROM UNDGAttribute WHERE DA_DG= '{Guid3}' AND DA_Descriptor = 'ship3'"));
			AssertEquals(true, TestConnection.Exists($"FROM UNDGAttribute WHERE DA_DG= '{Guid4}' AND DA_Descriptor = 'ship4'"));
			AssertEquals(false, TestConnection.Exists($"FROM UNDGAttribute WHERE DA_DG= '{Guid1}' AND DA_Descriptor = 'does not exist'"));
			AssertEquals(false, TestConnection.Exists($"FROM UNDGAttribute WHERE DA_DG= '{Guid5}'"));
		}

		public void TestTransformation_TempTriggerWorkedCorrectly()
		{
			DBTransformationTestHelper.DropIndexIfExists(UNDGAttributeSchema.Constants.TableName, "FK_UC__DA_DG_DA_Language_DA_Type_DA_Index");

			var date = new DateTime(2025, 1, 1);
			var oldShippingName = "old shipping name";
			var newShippingName = "new shipping name";
			var helper = new TestDbHelper(TestConnection);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			CreateUNDGSubstanceData(helper, Guid6, "9101", "a", oldShippingName);
			AssertEquals("Precondition: initial substance should be created", 1,
					(int)helper.RunSQL(new { DG_PK = Guid6 }, "SELECT COUNT(*) FROM dbo.ZZUNDGSubstance WHERE DG_PK = @DG_PK", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));

			AssertEquals("Precondition: trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, ZZUNDGSubstanceSchema.Constants.TableName, TriggerName));

			AssertEquals("Precondition: inital attribute should be created", 1,
					(int)helper.RunSQL(new { DA_Descriptor = oldShippingName }, "SELECT COUNT(*) FROM dbo.UNDGAttribute WHERE DA_Descriptor = @DA_Descriptor", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));

			AssertEquals("Precondition: new shipname in attribute should not exist", 0,
					(int)helper.RunSQL(new { DA_Descriptor = newShippingName }, "SELECT COUNT(*) FROM dbo.UNDGAttribute WHERE DA_Descriptor = @DA_Descriptor", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));

			var sql = @"UPDATE dbo.ZZUNDGSubstance SET DG_UsrUSDOTShippingName = @DG_UsrUSDOTShippingName, DG_SystemLastEditTimeUtc = @DG_SystemLastEditTimeUtc, DG_SystemLastEditUser = @DG_SystemLastEditUser WHERE DG_PK = @DG_PK";
			AssertNoExceptionThrown(() => helper.RunSQL(new { DG_UsrUSDOTShippingName = newShippingName, DG_SystemLastEditTimeUtc = date, DG_SystemLastEditUser = "~BP", DG_PK = Guid6 }, sql));

			AssertEquals("Attribute's shipping name should be updated", 1,
					(int)helper.RunSQL(new { DA_Descriptor = newShippingName }, "SELECT COUNT(*) FROM dbo.UNDGAttribute WHERE DA_Descriptor = @DA_Descriptor", executionType: TestDbHelperBase.SQLExecutionTypes.ExecuteScalar));
		}

		public void TestTransformation_TempTriggerCreatedAndDroppedCorrectly()
		{
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_ZZUNDGSubstance_CopyOnUpdate
ON dbo.ZZUNDGSubstance
AFTER INSERT, UPDATE
AS
BEGIN
	IF UPDATE (DG_UsrUSDOTShippingName)
	BEGIN
		MERGE dbo.UNDGAttribute AS target
		USING (SELECT DG_PK, DG_UsrUSDOTShippingName FROM Inserted) AS source
		ON target.DA_DG = source.DG_PK AND target.DA_Type = 'USN'
		WHEN MATCHED AND ISNULL(source.DG_UsrUSDOTShippingName, '') <> '' THEN
			UPDATE SET 
				target.DA_Descriptor = source.DG_UsrUSDOTShippingName,
				target.DA_SystemLastEditTimeUtc = GETUTCDATE(),
				target.DA_SystemLastEditUser = '~BP'
		WHEN MATCHED AND ISNULL(source.DG_UsrUSDOTShippingName, '') = '' THEN
			DELETE
		WHEN NOT MATCHED BY TARGET AND ISNULL(source.DG_UsrUSDOTShippingName, '') <> '' THEN
			INSERT (
				DA_PK,
				DA_Descriptor, 
				DA_Type, 
				DA_DG,
				DA_Language, 
				DA_IsSystem,
				DA_SystemCreateTimeUtc, 
				DA_SystemCreateUser,
				DA_SystemLastEditTimeUtc, 
				DA_SystemLastEditUser
			)
			VALUES (
				NEWID(),
				source.DG_UsrUSDOTShippingName, 
				'USN', 
				source.DG_PK,
				'EN',
				0,
				GETUTCDATE(), 
				'~BP', 
				GETUTCDATE(), 
				'~BP'
			);
	END
END;";

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, ZZUNDGSubstanceSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, GetTriggerDefinition(TriggerName));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be dropped.", false, DbObjectCreator.TriggerExists(TestConnection, ZZUNDGSubstanceSchema.Constants.TableName, TriggerName));
		}

		public void TestUserDescription()
		{
			AssertEquals("Copy UNDG USDOT ShippingName to UNDG Attribute.", GetNewTestTransformationInstance().UserDescription);
		}

		string GetTriggerDefinition(string triggerName)
		{
			return TestConnection.ExecuteScalar<string>($@"
SELECT
	TrgDefinition	= def.definition
FROM
	sys.triggers trg
	JOIN sys.sql_modules AS def ON def.object_id = trg.object_id
WHERE trg.name = '{triggerName}'
");
		}
	}
}
