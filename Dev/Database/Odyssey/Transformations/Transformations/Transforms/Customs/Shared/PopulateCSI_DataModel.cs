using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class PopulateCSI_DataModel : DataTransformation
	{
		public override string UserDescription => "Populate new column CSI_DataModel";

		public PopulateCSI_DataModel() : this(1000)
		{
		}

		internal PopulateCSI_DataModel(int batchSize) : base()
		{
			this.batchSize = batchSize;
		}

		readonly int batchSize;

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

		bool SourceColumnsExist()
		{
			return DbObjectCreator.TableExists(Db.Connection, CusSupportingInfoSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, CusEntryInstructionSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, CusClassPartPivotSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, CusEntryLineSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, JobDeclarationSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, JobComInvoiceHeaderSchema.Constants.TableName)
					&& DbObjectCreator.ColumnExists(Db.Connection, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_DataModel)
					&& DbObjectCreator.ColumnExists(Db.Connection, CusClassPartPivotSchema.Constants.TableName, CusClassPartPivotSchema.Constants.CI_RN_NKCountry)
					&& DbObjectCreator.ColumnExists(Db.Connection, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.Constants.CL_DataModel)
					&& DbObjectCreator.ColumnExists(Db.Connection, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.Constants.JE_DataModel)
					&& DbObjectCreator.ColumnExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.JI_DataModel)
					&& DbObjectCreator.ColumnExists(Db.Connection, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.JZ_DataModel);
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

		IF EXISTS
		(
			SELECT NULL FROM @Updates WHERE DataModel = ''
		)
		BEGIN
			RAISERROR('Attempt to insert/update without [CSI_DataModel] for [CusSupportingInfo].', 16, 1)
			ROLLBACK
		END
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
				AND CSI_PK BETWEEN @LowerBound AND @UpperBound

				{UpdateDataModelSql}

				{DeleteOrphanedSql}
				""";

			using var cmd = Db.Connection.Command(sql);
			cmd.AddParameterBasedOnDbColumn("@LowerBound", lowerBound, CusSupportingInfoSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@UpperBound", upperBound, CusSupportingInfoSchema.PK);

			cmd.ExecuteNonQuery();
		}

		const string LastProcessedPropertyName = "LastProcessedGuidForPopulateCSI_DataModel";

		internal const string SyncTriggerName = "TG_CusSupportingInfo_PopulateCSI_DataModel";

		const string CreateTempTableSql = """
			DECLARE @Updates TABLE (
					PK UNIQUEIDENTIFIER,
					ParentID UNIQUEIDENTIFIER,
					DataModel VARCHAR(3)
			);

			INSERT INTO @Updates (PK, ParentID, DataModel)
			SELECT CSI_PK PK, ParentID, CASE
					WHEN LEN(DataModel) < 2 THEN ''
					WHEN DataModel = 'PR' THEN 'US'
					WHEN DataModel = 'LI' THEN 'CH'
					ELSE ISNULL(DataModel, '') END 
			FROM dbo.CusSupportingInfo
			LEFT JOIN (
				SELECT ParentID = CEI_PK, DataModel = CEI_DataModel FROM dbo.CusEntryInstruction
				UNION ALL
				SELECT ParentID = CI_PK, DataModel = CI_RN_NKCountry FROM dbo.CusClassPartPivot
				UNION ALL
				SELECT ParentID = CL_PK, DataModel = CL_DataModel FROM dbo.CusEntryLine
				UNION ALL
				SELECT ParentID = JE_PK, DataModel = JE_DataModel FROM dbo.JobDeclaration
				UNION ALL
				SELECT ParentID = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
				UNION ALL
				SELECT ParentID = JZ_PK, DataModel = JZ_DataModel FROM dbo.JobComInvoiceHeader
			) A ON ParentID = CSI_ParentID
			WHERE CSI_DataModel = '' AND CSI_ParentTableCode IN ('CEI', 'CI', 'CL', 'JE', 'JI', 'JZ')
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
	}
}
