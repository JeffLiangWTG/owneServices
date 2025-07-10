using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

class UpdatePopulateCSI_DataModelWithCusEntryHeader : DataTransformation
{
	public override string UserDescription => "Update populate new column CSI_DataModel with CusEntryHeader";

	internal UpdatePopulateCSI_DataModelWithCusEntryHeader(int batchSize) : base()
	{
		this.batchSize = batchSize;
	}

	public UpdatePopulateCSI_DataModelWithCusEntryHeader() : this(1000)
	{
	}

	protected override void OnlinePreUpgradeTransform()
	{
		if (SourceColumnsExist())
		{
			CreateNewColumnsAndSyncTriggerIfNeeded();

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, CusSupportingInfoSchema.Instance);
			var operation = new GuidChunkingOperation(manager, batchSize, rowCount, ProcessChunk, LastProcessedPropertyName);
			operation.DoChunking();
		}
	}

	static void CreateNewColumnsAndSyncTriggerIfNeeded()
	{
		DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_DataModel, "VARCHAR(3)", "''");

		if (!DbObjectCreator.TriggerExists(Db.Connection, CusSupportingInfoSchema.Constants.TableName, SyncTriggerName))
		{
			var createTriggerSql = $"""
				CREATE TRIGGER dbo.{SyncTriggerName} ON dbo.CusSupportingInfo 
					AFTER INSERT, UPDATE
				AS
				BEGIN
					IF (@@rowcount = 0) RETURN;
					SET NOCOUNT ON;

					IF UPDATE(CSI_ParentID) OR UPDATE(CSI_ParentTableCode)
					BEGIN
						{CreateTempTableSql}
						AND CSI_PK in (SELECT CSI_PK FROM inserted)

						{UpdateDataModelSql}

					END
				END
				""";

			Db.Connection.ExecuteNonQuery(createTriggerSql);
		}
	}

	static void ProcessChunk(Guid lowerBound, Guid upperBound)
	{
		var sql = $"""
				{CreateTempTableSql}
				AND CSI_ParentID BETWEEN @LowerBound AND @UpperBound

				{UpdateDataModelSql}

				{DeleteOrphanedSql}
				""";

		using var cmd = Db.Connection.Command(sql);
		cmd.AddParameterBasedOnDbColumn("@LowerBound", lowerBound, CusSupportingInfoSchema.CSI_ParentID);
		cmd.AddParameterBasedOnDbColumn("@UpperBound", upperBound, CusSupportingInfoSchema.CSI_ParentID);
		cmd.ExecuteNonQuery();
	}

	const string CreateTempTableSql = """
			DECLARE @Updates TABLE (
					PK UNIQUEIDENTIFIER,
					ParentID UNIQUEIDENTIFIER,
					DataModel VARCHAR(3)
			);

			INSERT INTO @Updates (PK, ParentID, DataModel)
			SELECT CSI_PK PK, ParentID, CASE
					WHEN DataModel = 'PR' THEN 'US'
					WHEN DataModel = 'LI' THEN 'CH'
					ELSE DataModel END 
			FROM dbo.CusSupportingInfo
			LEFT JOIN (
				SELECT ParentID = CH_PK, DataModel = CH_DataModel FROM dbo.CusEntryHeader
			) A ON ParentID = CSI_ParentID
			WHERE LEN(CSI_DataModel) < 2 AND CSI_ParentTableCode = 'CH'
		""";

	const string UpdateDataModelSql = """
			UPDATE dbo.CusSupportingInfo
			SET
				CSI_SystemLastEditTimeUtc = ISNULL(CSI_SystemLastEditTimeUtc, GETUTCDATE()),
				CSI_SystemLastEditUser = CASE WHEN CSI_SystemLastEditUser = '' THEN '~BP' ELSE CSI_SystemLastEditUser END,
				CSI_DataModel = DataModel
			FROM dbo.CusSupportingInfo JOIN @Updates ON CSI_PK = PK
			WHERE DataModel <> ''
		""";

	const string DeleteOrphanedSql = """
			DELETE FROM dbo.CusSupportingInfo WHERE CSI_PK IN (SELECT PK FROM @Updates WHERE ParentID IS NULL)
		""";

	bool SourceColumnsExist() => DbObjectCreator.TableExists(Db.Connection, CusSupportingInfoSchema.Constants.TableName)
		&& DbObjectCreator.TableExists(Db.Connection, CusEntryHeaderSchema.Constants.TableName)
		&& DbObjectCreator.ColumnExists(Db.Connection, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.Constants.CH_DataModel);

	readonly int batchSize;
	const string LastProcessedPropertyName = "LastProcessedGuidForUpdatePopulateCSI_DataModelWithCusEntryHeader";
	internal const string SyncTriggerName = "TG_CusSupportingInfo_UpdatePopulateCSI_DataModelWithCusEntryHeader";
}
