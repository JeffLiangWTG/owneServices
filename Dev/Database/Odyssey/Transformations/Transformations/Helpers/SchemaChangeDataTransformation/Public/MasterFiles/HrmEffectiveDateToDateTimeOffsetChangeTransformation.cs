using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations
{
	public abstract class HrmEffectiveDateToDateTimeOffsetChangeTransformation : SchemaChangeDataTransformation
	{
		protected abstract SchemaColumn EffectiveDateColumn { get; }
		protected abstract SqlDbType EffectiveDateColumnOriginalType { get; }
		protected abstract string StaffPKColumnName { get; }

		protected string SchemaName => EffectiveDateColumn.TableSchema.SqlSchemaName;
		protected string TableName => EffectiveDateColumn.TableSchema.TableName;
		string SourceTableName => SourceTables[TableName].FullyQualifiedName;
		string PrimaryKeyColumnName => EffectiveDateColumn.TableSchema.PK.Name;
		string EffectiveDateColumnName => EffectiveDateColumn.Name;
		SqlDbType EffectiveDateColumnNewType => EffectiveDateColumn.SqlDbType;

		protected virtual SchemaColumn BeneficiaryEffectiveDateColumn => GlbBeneficiaryBranchDepartmentSchema.GBB_EffectiveDate;
		protected virtual string BeneficiaryStaffPKColumnName => GlbBeneficiaryBranchDepartmentSchema.GBB_GS_Staff.Name;
		protected virtual SchemaColumn HomeEffectiveDateColumn => GlbEmployingBranchDepartmentSchema.GHB_EffectiveDate;
		protected virtual string HomeStaffPKColumnName => GlbEmployingBranchDepartmentSchema.GHB_GS_Staff.Name;

		protected abstract string FKConstraintName { get; }
		protected abstract string TriggerName { get; }
		string EffectiveEndDateColumnName => EffectiveDateColumn.Name;
		protected abstract string AutoEffectiveEndDateColumnName { get; }

		public override sealed string UserDescription => $"Converting {SchemaName}.{TableName}.{EffectiveDateColumnName} from {EffectiveDateColumnOriginalType} to {EffectiveDateColumnNewType}";

		public override sealed bool IsRequired => base.IsRequired && SourceColumnExistsAndTypeIsNotChanged;

		public bool SourceColumnExistsAndTypeIsNotChanged
		{
			get
			{
				if (!DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, SchemaName, TableName, EffectiveDateColumnName))
				{
					return false;
				}

				var dataType = DbObjectCreator.GetColumnType(Db.Connection, TableName, EffectiveDateColumnName);
				return string.Compare(dataType, EffectiveDateColumnOriginalType.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

		protected override sealed SourceTableCollection GetSourceTables()
		{
			var tables = new SourceTableCollection();
			tables.Add(GetTableOriginalData());
			return tables;
		}

		protected sealed override void PasteToTarget()
		{
			using (DataTransformationHelper.SuspendFkIfExists(SchemaName, TableName, FKConstraintName))
			using (DataTransformationHelper.SuspendTriggerIfExists(TriggerName, $"[{SchemaName}].[{TableName}]"))
			{
				DoDataTransform();
				PopulateAutoEffectiveEndDate();
			}
		}

		void DoDataTransform()
		{
			var sql = $@"
-- temp table
DROP TABLE IF EXISTS #StaffWithEffectiveDates
CREATE TABLE #StaffWithEffectiveDates ({PrimaryKeyColumnName} UNIQUEIDENTIFIER NOT NULL, {StaffPKColumnName} UNIQUEIDENTIFIER NOT NULL, {EffectiveDateColumnName} DateTimeOffset NOT NULL, OffsetSampleTime DateTimeOffset)

-- Populate the temporary table EffectiveDate value from the source table
INSERT INTO
	#StaffWithEffectiveDates ({PrimaryKeyColumnName}, {StaffPKColumnName}, {EffectiveDateColumnName})
SELECT
	{PrimaryKeyColumnName}, {StaffPKColumnName}, {EffectiveDateColumnName}
FROM
	{SourceTableName}

--  Convert the temporary table EffectiveDate from beneficiary effective datetimeoffset
UPDATE
	#StaffWithEffectiveDates
SET
	OffsetSampleTime = {BeneficiaryEffectiveDateColumn.Name},
	{EffectiveDateColumnName} = TODATETIMEOFFSET({EffectiveDateColumnName}, datepart(tz, {BeneficiaryEffectiveDateColumn.Name}))
FROM
	#StaffWithEffectiveDates SED
INNER JOIN
	{BeneficiaryEffectiveDateColumn.TableSchema.SqlSchemaName}.{BeneficiaryEffectiveDateColumn.TableName} B
ON
	B.{BeneficiaryEffectiveDateColumn.TableSchema.PK.Name} =
    (
		SELECT
			TOP 1 {BeneficiaryEffectiveDateColumn.TableSchema.PK.Name}
		FROM
			{BeneficiaryEffectiveDateColumn.TableSchema.SqlSchemaName}.{BeneficiaryEffectiveDateColumn.TableName}
		WHERE
			{BeneficiaryStaffPKColumnName} = SED.{StaffPKColumnName}
		AND
			CAST({BeneficiaryEffectiveDateColumn.Name} AS DATE) <= SED.{EffectiveDateColumnName}
		ORDER BY
			{BeneficiaryEffectiveDateColumn.Name} DESC
    )
WHERE
	OffsetSampleTime IS NULL

-- Convert the temporary table EffectiveDate from home branch effective datetimeoffset for those who do not have a beneficiary effective datetimeoffset
UPDATE
	#StaffWithEffectiveDates
SET
	OffsetSampleTime = {HomeEffectiveDateColumn.Name},
	{EffectiveDateColumnName} = TODATETIMEOFFSET({EffectiveDateColumnName}, datepart(tz, {HomeEffectiveDateColumn.Name}))
FROM
	#StaffWithEffectiveDates SED
INNER JOIN
	{HomeEffectiveDateColumn.TableSchema.SqlSchemaName}.{HomeEffectiveDateColumn.TableName} H
ON
	H.{HomeEffectiveDateColumn.TableSchema.PK.Name} =
    (
		SELECT
			TOP 1 {HomeEffectiveDateColumn.TableSchema.PK.Name}
		FROM
			{HomeEffectiveDateColumn.TableSchema.SqlSchemaName}.{HomeEffectiveDateColumn.TableName}
		WHERE
			{HomeStaffPKColumnName} = SED.{StaffPKColumnName}
		AND
			CAST({HomeEffectiveDateColumn.Name} AS DATE) <= SED.{EffectiveDateColumnName}
		ORDER BY
			{HomeEffectiveDateColumn.Name} DESC
    )
WHERE
	OffsetSampleTime IS NULL

-- Convert the temporary table EffectiveDate from the server time offset for those who do not have a beneficiary or a home branch effective datetimeoffset
UPDATE
	#StaffWithEffectiveDates
SET
	OffsetSampleTime = SYSDATETIMEOFFSET(),
	{EffectiveDateColumnName} = TODATETIMEOFFSET({EffectiveDateColumnName}, datepart(tz, SYSDATETIMEOFFSET()))
WHERE
	OffsetSampleTime IS NULL

--  Copy the temporary table EffectiveDate values into the real table
UPDATE
	{SchemaName}.{TableName}
SET
	{SchemaName}.{TableName}.{EffectiveDateColumnName} = SED.{EffectiveDateColumnName}
FROM
	{SchemaName}.{TableName}
INNER JOIN
	#StaffWithEffectiveDates SED
ON
	{SchemaName}.{TableName}.{PrimaryKeyColumnName} = SED.{PrimaryKeyColumnName}

-- drop temp table
DROP TABLE #StaffWithEffectiveDates
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		protected void PopulateAutoEffectiveEndDate()
		{
			Db.Connection.ExecuteNonQuery($@"
				UPDATE [{SchemaName}].[{TableName}]
				SET
					[{AutoEffectiveEndDateColumnName}] = nextRecord.{EffectiveEndDateColumnName}
				FROM [{SchemaName}].[{TableName}] updating
				CROSS APPLY (
					SELECT TOP 1 {EffectiveEndDateColumnName}
					FROM [{SchemaName}].[{TableName}] prospectiveNext
					WHERE
						prospectiveNext.{StaffPKColumnName} = updating.{StaffPKColumnName} AND
						prospectiveNext.{EffectiveEndDateColumnName} > updating.{EffectiveEndDateColumnName}
					ORDER BY {EffectiveEndDateColumnName} ASC
				) as nextRecord;
			");
		}

		SourceTable GetTableOriginalData()
		{
			return new SourceTable(Db.DatabaseName, SchemaName, TableName, new[]
			{
				new SourceColumn(PrimaryKeyColumnName, nameof(SqlDbType.UniqueIdentifier)),
				new SourceColumn(StaffPKColumnName, nameof(SqlDbType.UniqueIdentifier)),
				new SourceColumn(EffectiveDateColumnName, EffectiveDateColumnOriginalType.ToString()),
			}, null);
		}
	}
}
