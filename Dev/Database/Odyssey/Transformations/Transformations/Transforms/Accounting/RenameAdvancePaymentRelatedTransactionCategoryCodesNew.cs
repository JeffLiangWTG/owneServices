using System;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class RenameAdvancePaymentRelatedTransactionCategoryCodesNew : DataTransformation
	{
		public override string UserDescription => "Rename transaction category codes that are related to Advance Payment";

		protected override void OfflinePreUpgradeTransform()
		{
			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletionNew);

			if (statusValue != bool.TrueString && DbObjectCreator.TableExists(Db.Connection, AccTransactionHeaderSchema.Constants.TableName))
			{
				DropOldConstraints();

				var connection = Db.Connection;
				if (DbObjectCreator.TableExists(connection, AccTransactionHeaderSchema.Constants.TableName)
					&& DbObjectCreator.ColumnExists(connection, AccTransactionHeaderSchema.Constants.TableName, TemporaryColumn)
					&& DbObjectCreator.IndexExists(connection, AccTransactionHeaderSchema.Constants.TableName, TemporaryIndex))
				{
					DoTransform();

					DropDefaultConstraint();
					DropTemporaryIndex();
					DropTemporaryTrigger();
					DropTemporaryColumn();
				}

				ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletionNew, bool.TrueString);
			}
		}

		protected override void OnlinePreUpgradeTransform()
		{
			//Cleaning up temp DB Objects created by the previous version of this transformation..
			DropTemporaryConstraint();
			DropTemporaryTrigger();

			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletionNew);

			if (statusValue != bool.TrueString && DbObjectCreator.TableExists(Db.Connection, AccTransactionHeaderSchema.Constants.TableName))
			{
				DropOldConstraints();

				CreateTemporaryColumns();

				CreateTemporaryIndex();

				CreateTemporaryTrigger();

				PopulateFlagColumn();

				ShowInfo(FormattableString.Invariant($"Identification of advance payment transactions requiring category renaming completed."));
			}
			else
			{
				ShowInfo(FormattableString.Invariant($"Advance payment transaction category renaming transformation was run before."));
			}
		}

		void DoTransform()
		{
			var transformSql = FormattableString.Invariant($@"
UPDATE dbo.AccTransactionHeader
	SET AH_TransactionCategory = 
	CASE 
		WHEN AH_TransactionCategory='CAI' THEN 'API'
		WHEN AH_TransactionCategory='CAP' THEN 'APP'
		WHEN AH_TransactionCategory='CAR' THEN 'APR'
	END,
	AH_SystemLastEditTimeUtc = GETUTCDATE(), 
	AH_SystemLastEditUser = '~BP'
FROM dbo.AccTransactionHeader
WHERE [{TemporaryColumn}] = 'Y';");

			using (var command = Db.Connection.Command(transformSql))
			{
				command.ExecuteNonQuery();
			}
		}

		void PopulateFlagColumn()
		{
			var lastProcessedDTPropertyString = ExtProperty.Database.Select(Db.Connection, LastProcessedDTPropertyStringNew);
			var lastProcessedDT = long.TryParse(lastProcessedDTPropertyString, out var parsedDT) ? new DateTime(parsedDT) : new DateTime(2024, 01, 31, 0, 0, 0);

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in DateTimeChunker.GenerateChunks(1000, lastProcessedDT, AccTransactionHeaderSchema.AH_SystemCreateTimeUtc))
			{
				UpdateChunk(chunk);

				if (stopWatch.Elapsed.TotalMinutes > 1)
				{
					ShowInfo(FormattableString.Invariant($"Identification of advance payment transactions requiring category renaming completed upto {chunk.UpperBound.ToString("dd MMM yyyy hh:mm:ss")}"));
					lastProcessedDTPropertyString = chunk.UpperBound.Ticks.ToString();
					ExtProperty.Database.Update(Db.Connection, LastProcessedDTPropertyStringNew, lastProcessedDTPropertyString);

					stopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedDTPropertyStringNew);
		}

		void UpdateChunk(DateTimeChunk chunk)
		{
			var transformSql = FormattableString.Invariant($@"
UPDATE dbo.AccTransactionHeader
	SET
	[{TemporaryColumn}] = 'Y',
	AH_SystemLastEditTimeUtc = GETUTCDATE(),
	AH_SystemLastEditUser = '~BP'
FROM dbo.AccTransactionHeader WITH (FORCESEEK, INDEX (NR_RX__AH_SystemCreateTimeUtc))
WHERE AH_SystemCreateTimeUtc BETWEEN @LowerBound AND @UpperBound 
	AND AH_Ledger IN ('AR', 'AP') 
	AND AH_TransactionType = 'JNL'
	AND AH_TransactionCategory IN ('CAI', 'CAR', 'CAP')
OPTION (MAXDOP 1);");

			using (var command = Db.Connection.Command(transformSql))
			{
				command.AddParameter("@LowerBound", AccTransactionHeaderSchema.AH_SystemCreateTimeUtc.SqlDbType, chunk.LowerBound);
				command.AddParameter("@UpperBound", AccTransactionHeaderSchema.AH_SystemCreateTimeUtc.SqlDbType, chunk.UpperBound);
				command.ExecuteNonQuery();
			}
		}

		#region Old Constraint

		void DropOldConstraints()
		{
			var sql = FormattableString.Invariant(@$"ALTER TABLE dbo.AccTransactionHeader DROP CONSTRAINT [{OldConstraintName}]");
			if (DbObjectCreator.ObjectExists(Db.Connection, OldConstraintName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}

			DropTemporaryConstraint();
		}

		void DropTemporaryConstraint()
		{
			var sql = @"ALTER TABLE dbo.AccTransactionHeader DROP CONSTRAINT [Constraint_AH_CAH_CashAdvanceRequestHeader_New]";
			if (DbObjectCreator.ObjectExists(Db.Connection, "Constraint_AH_CAH_CashAdvanceRequestHeader_New"))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		#endregion

		#region TemporaryTrigger

		void CreateTemporaryTrigger()
		{
			var sql = FormattableString.Invariant($@"
CREATE TRIGGER dbo.{TriggerName}
	ON dbo.AccTransactionHeader 
	AFTER INSERT
AS
SET NOCOUNT ON;
UPDATE AH
	SET
	AH.[{TemporaryColumn}] = 'Y',
	AH_SystemLastEditTimeUtc = GETUTCDATE(), 
	AH_SystemLastEditUser = COALESCE(i.AH_SystemLastEditUser, '~BP')
FROM inserted i
	INNER JOIN AccTransactionHeader AH ON AH.AH_PK = i.AH_PK
WHERE i.AH_TransactionType = 'JNL' 
	AND i.AH_Ledger IN ('AR', 'AP')
	AND i.AH_TransactionCategory IN ('CAI', 'CAR', 'CAP');");

			if (!DbObjectCreator.TriggerExists(Db.Connection, AccTransactionHeaderSchema.Constants.TableName, TriggerName))
			{
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		void DropTemporaryTrigger()
		{
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID('{TriggerName}'))
DROP TRIGGER {TriggerName};"));
		}

		#endregion

		#region Temporary Index

		void CreateTemporaryIndex()
		{
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = '{TemporaryIndex}' AND object_id = OBJECT_ID('AccTransactionHeader'))
BEGIN
    CREATE INDEX {TemporaryIndex} ON AccTransactionHeader ([{TemporaryColumn}])
	INCLUDE([AH_TransactionCategory],[AH_SystemLastEditTimeUtc],[AH_SystemLastEditUser]) 
	WHERE [{TemporaryColumn}] = 'Y'
	WITH (ONLINE = ON);
END"));
		}

		void DropTemporaryIndex()
		{
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = '{TemporaryIndex}' AND object_id = OBJECT_ID('AccTransactionHeader'))
BEGIN
    DROP INDEX [{TemporaryIndex}] ON dbo.AccTransactionHeader;
END"));
		}

		#endregion

		#region Temporary Column

		void DropDefaultConstraint()
		{
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
DECLARE @sql NVARCHAR(MAX);

SELECT @sql = 'ALTER TABLE AccTransactionHeader DROP CONSTRAINT ' + QUOTENAME(name)
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('AccTransactionHeader') AND col_name(parent_object_id, parent_column_id) = '{TemporaryColumn}';
EXEC sp_executesql @sql;"));
		}

		void CreateTemporaryColumns()
		{
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
IF COL_LENGTH('AccTransactionHeader', '{TemporaryColumn}') IS NULL
    ALTER TABLE AccTransactionHeader ADD [{TemporaryColumn}] CHAR(1) NOT NULL DEFAULT '';"));
		}

		void DropTemporaryColumn()
		{
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
ALTER TABLE dbo.AccTransactionHeader DROP COLUMN [{TemporaryColumn}];"));
		}

		#endregion

		const string TriggerName = "TG_AccTransactionHeader_UpdateAdvancePaymentTransactionCategory";
		const string OldConstraintName = "Constraint_AH_CAH_CashAdvanceRequestHeader";
		const string TemporaryColumn = "CW!!CAH_Transaction_Flag";
		const string TemporaryIndex = "_WTG_CAH_Transaction_Flag";

		public const string LastProcessedDTPropertyStringNew = "RenameAdvancePaymentRelatedTransactionCategoryCodes.LastProcessedDTNew";
		public const string TransformationHasRunToCompletionNew = "RenameAdvancePaymentRelatedTransactionCategoryCodes.TransformationHasRunToCompletionNew";
	}
}
