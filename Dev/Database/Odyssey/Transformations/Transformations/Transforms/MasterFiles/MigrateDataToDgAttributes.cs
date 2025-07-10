using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	class MigrateDataToDgAttributes : DataTransformation
	{
		public override string UserDescription => "Copy UNDG USDOT ShippingName to UNDG Attribute.";
		const string TriggerName = "TG_ZZUNDGSubstance_CopyOnUpdate";
		const string LastProcessedChunkPKName = "MigrateDataToDgAttributes.DA_DG.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, ZZUNDGSubstanceSchema.Constants.TableName) && DbObjectCreator.TableExists(Db.Connection, UNDGAttributeSchema.Constants.TableName))
			{
				AddTrigger();
				DoTransform();
			}
		}
		protected override void OfflinePreUpgradeTransform()
		{
			DbObjectCreator.DropTriggerIfExists(Db.Connection, TriggerName);
		}

		void DoTransform()
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, ZZUNDGSubstanceSchema.Instance);
			var operation = new GuidChunkingOperation(manager, BatchSize, rowCount, UpdateChunk, LastProcessedChunkPKName);
			operation.DoChunking();
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteScalar(
					MoveUSDOTShippingNameToAttributeSQL(),
					cmd =>
					{
						cmd.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
						cmd.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
					});
				transactionManager.CommitTransaction();
			}
		}

		static string MoveUSDOTShippingNameToAttributeSQL()
		{
			return $@"
					INSERT INTO dbo.UNDGAttribute (
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
					SELECT
						NEWID(),
						DG_UsrUSDOTShippingName, 
						'USN', 
						DG_PK,
						'EN',
						0,
						GETUTCDATE(), 
						'~BP', 
						GETUTCDATE(), 
						'~BP' 
					FROM 
						dbo.ZZUNDGSubstance
					WHERE 
						DG_UsrUSDOTShippingName <> ''
						AND dbo.ZZUNDGSubstance.DG_PK >= @FromPK
						AND dbo.ZZUNDGSubstance.DG_PK <= @ToPK
						AND NOT EXISTS (
							SELECT 1
							FROM dbo.UNDGAttribute
							WHERE 
								dbo.UNDGAttribute.DA_Type = 'USN'
								AND dbo.UNDGAttribute.DA_DG = dbo.ZZUNDGSubstance.DG_PK
						);";
		}

		static void AddTrigger()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, ZZUNDGSubstanceSchema.Constants.TableName, TriggerName))
			{
				var createTriggerSQL = string.Format(CreateTriggerSQL, TriggerName);
				Db.Connection.ExecuteNonQuery(createTriggerSQL);
			}
		}

		const string CreateTriggerSQL = @"
CREATE TRIGGER dbo.{0}
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
	}
}
