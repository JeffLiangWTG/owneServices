using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.DependencyInjection;
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade
{
	public abstract class PopulateAuditTimeAndUser : DataTransformation, IPopulateAuditTimeAndUserInfo
	{
		public override sealed string UserDescription
		{
			get { return String.Format("Populate audit Time and User columns for table: {0}", TableName); }
		}

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, TableName) && DbObjectCreator.TableExists(Db.Connection, StmALogSchema.Constants.TableName))
			{
				var updateColumnList = new List<string>();
				var newValueExpressionList = new List<string>();
				var whereClauseList = new List<string>();
				var finalWhereClauseList = new List<string>();

				var highWatermarkProperty = ExtProperty.Table.Select(
					Db.Connection,
					TableSchema.SqlSchemaName,
					TableName,
					HighWatermarks.PopulateAuditTimeAndUserHighWatermark);
				var highWatermarkValue = default(DateTime);
				var hasHighWatermarkRecorded = !string.IsNullOrWhiteSpace(highWatermarkProperty)
					&& DateTime.TryParse(highWatermarkProperty, out highWatermarkValue);
				ShowInfo(hasHighWatermarkRecorded ? $"\tUsing High Watermark: [{highWatermarkProperty}]" : "\tUsing No High Watermark");

				const string HighWatermarkParameter = "@HighWatermark";
				var finalShouldOverwriteOldValuesForEdit = ShouldOverwriteOldValues || hasHighWatermarkRecorded;
				var stmALogWhereClause = hasHighWatermarkRecorded ? $"AND SL_PostedTimeUtc > DATEADD(hour, -1, {HighWatermarkParameter})" :　string.Empty;

				if (CreateTime != null)
				{
					CreateColumnIfNotExists(Db.Connection, TableName, CreateTime.Name, string.Format("{0}", CreateTime.SqlDbType.ToString()));

					var fallbackToGetdate = string.Empty;
					if (ShouldProvideDefaultTimeIfLogMissing && !CreateTime.IsNullable)
					{
						fallbackToGetdate = ", GETUTCDATE()";
					}

					updateColumnList.Add(string.Format("[{0}]   = new_SystemCreateTimeUtc", CreateTime.Name));

					if (!ShouldOverwriteOldValues)
					{
						newValueExpressionList.Add(string.Format(", new_SystemCreateTimeUtc   = COALESCE(target.[{0}], source.CreateTimeUTCExpression{1})", CreateTime.Name, fallbackToGetdate));
						whereClauseList.Add(string.Format("OR target.[{0}] is NULL", CreateTime.Name));
					}
					else
					{
						newValueExpressionList.Add(string.Format(", new_SystemCreateTimeUtc   = COALESCE(source.CreateTimeUTCExpression, target.[{0}]{1})", CreateTime.Name, fallbackToGetdate));
					}

					finalWhereClauseList.Add(string.Format("OR ISNULL(NULLIF([{0}]  , new_SystemCreateTimeUtc  ), NULLIF(new_SystemCreateTimeUtc  , [{0}]  )) is NOT NULL", CreateTime.Name));
				}

				if (CreateUser != null)
				{
					CreateColumnIfNotExists(Db.Connection, TableName, CreateUser.Name, string.Format("{0}({1})", CreateUser.SqlDbType.ToString(), CreateUser.MaxLength.ToString()), string.Format("'{0}'", (string)CreateUser.SqlDbDefault));

					updateColumnList.Add(string.Format("[{0}]      = new_SystemCreateUser", CreateUser.Name));

					if (!ShouldOverwriteOldValues)
					{
						newValueExpressionList.Add(string.Format(", new_SystemCreateUser      = COALESCE(NULLIF(target.[{0}], ''), source.CreateUserExpression, '{1}')", CreateUser.Name, DefaultUserValue));
						whereClauseList.Add(string.Format("OR target.[{0}] is NULL OR target.[{0}] = ''", CreateUser.Name));
					}
					else
					{
						newValueExpressionList.Add(string.Format(", new_SystemCreateUser      = COALESCE(source.CreateUserExpression, target.[{0}], '{1}')", CreateUser.Name, DefaultUserValue));
					}

					finalWhereClauseList.Add(string.Format("OR ISNULL(NULLIF([{0}]     , new_SystemCreateUser     ), NULLIF(new_SystemCreateUser     , [{0}]     )) is NOT NULL", CreateUser.Name));
				}

				if (LastEditTime != null)
				{
					CreateColumnIfNotExists(Db.Connection, TableName, LastEditTime.Name, string.Format("{0}", LastEditTime.SqlDbType.ToString()));

					var fallbackToGetdate = string.Empty;
					if (ShouldProvideDefaultTimeIfLogMissing && !LastEditTime.IsNullable)
					{
						fallbackToGetdate = ", GETUTCDATE()";
					}

					updateColumnList.Add(string.Format("[{0}] = new_SystemLastEditTimeUtc", LastEditTime.Name));

					if (!finalShouldOverwriteOldValuesForEdit)
					{
						newValueExpressionList.Add(string.Format(", new_SystemLastEditTimeUtc = COALESCE(target.[{0}], source.EditTimeUTCExpression{1})", LastEditTime.Name, fallbackToGetdate));
						whereClauseList.Add(string.Format("OR target.[{0}] is NULL", LastEditTime.Name));
					}
					else
					{
						newValueExpressionList.Add(string.Format(", new_SystemLastEditTimeUtc = COALESCE(source.EditTimeUTCExpression, target.[{0}]{1})", LastEditTime.Name, fallbackToGetdate));
					}

					finalWhereClauseList.Add(string.Format("OR ISNULL(NULLIF([{0}], new_SystemLastEditTimeUtc), NULLIF(new_SystemLastEditTimeUtc, [{0}])) is NOT NULL", LastEditTime.Name));
				}

				if (LastEditUser != null)
				{
					CreateColumnIfNotExists(Db.Connection, TableName, LastEditUser.Name, string.Format("{0}({1})", LastEditUser.SqlDbType.ToString(), LastEditUser.MaxLength.ToString()), string.Format("'{0}'", (string)LastEditUser.SqlDbDefault));

					updateColumnList.Add(string.Format("[{0}]    = new_SystemLastEditUser", LastEditUser.Name));

					if (!finalShouldOverwriteOldValuesForEdit)
					{
						newValueExpressionList.Add(string.Format(", new_SystemLastEditUser    = COALESCE(NULLIF(target.[{0}], ''), source.EditUserExpression, '{1}')", LastEditUser.Name, DefaultUserValue));
						whereClauseList.Add(string.Format("OR target.[{0}] is NULL OR target.[{0}] = ''", LastEditUser.Name));
					}
					else
					{
						newValueExpressionList.Add(string.Format(", new_SystemLastEditUser    = COALESCE(source.EditUserExpression, target.[{0}], '{1}')", LastEditUser.Name, DefaultUserValue));
					}

					finalWhereClauseList.Add(string.Format("OR ISNULL(NULLIF([{0}]   , new_SystemLastEditUser   ), NULLIF(new_SystemLastEditUser   , [{0}]   )) is NOT NULL", LastEditUser.Name));
				}

				var updateColumns = string.Join(",\r\n\t", updateColumnList);
				var newValueExpressions = string.Join("\r\n\t\t\t\t", newValueExpressionList);
				var whereClause = ShouldOverwriteOldValues || finalShouldOverwriteOldValuesForEdit ? "OR 1=1" : string.Join("\r\n\t\t\t\t\t", whereClauseList);
				var finalWhereClause = string.Join("\r\n\t", finalWhereClauseList);

				var sql = $@"
;WITH
	ALogValue AS (
			SELECT
				SL_Parent

				, add_user     = MAX(CASE SL_SE_NKEvent WHEN 'ADD' THEN SL_GS_NKUser END)
				, add_time_utc = MAX(CASE SL_SE_NKEvent WHEN 'ADD' THEN SL_PostedTimeUtc END)

				, edt_first_user     = SUBSTRING(MIN(CASE SL_SE_NKEvent WHEN 'EDT' THEN CONVERT(char(23), SL_PostedTimeUtc, 121) + SL_GS_NKUser END), 24, 3)
				, edt_first_time_utc = MIN(CASE SL_SE_NKEvent WHEN 'EDT' THEN SL_PostedTimeUtc END)

				, edt_last_user      = SUBSTRING(MAX(CASE SL_SE_NKEvent WHEN 'EDT' THEN CONVERT(char(23), SL_PostedTimeUtc, 121) + SL_GS_NKUser END), 24, 3)
				, edt_last_time_utc  = MAX(CASE SL_SE_NKEvent WHEN 'EDT' THEN SL_PostedTimeUtc END)
			FROM
				dbo.StmALog
			WHERE 1=1
				AND SL_Table = '{TableName}'
				AND SL_SE_NKEvent in ('ADD', 'EDT')
				{stmALogWhereClause}
			GROUP BY
				SL_Parent
		)
	, ALogExpression AS (
			SELECT
				Parent = t.SL_Parent

				, CreateTimeUTCExpression = COALESCE(t.add_time_utc, t.edt_first_time_utc)
				, CreateUserExpression    = COALESCE(t.add_user, t.edt_first_user)

				, EditTimeUTCExpression   = COALESCE(t.edt_last_time_utc, t.add_time_utc)
				, EditUserExpression      = COALESCE(t.edt_last_user, t.add_user)
			FROM
				ALogValue AS t
		)
	, TargetValues AS (
			SELECT
				target.*
				{newValueExpressions}
			FROM
				{TableName} AS target
				LEFT JOIN ALogExpression AS source ON source.Parent = target.[{PK.Name}]
			WHERE
				(1=2
					{whereClause}
				)
				AND {RowFilter}
		)
UPDATE TargetValues WITH(TABLOCKX) SET
	{updateColumns}
WHERE 1=2
	{finalWhereClause}

OPTION(RECOMPILE)

SELECT @@ROWCOUNT

";

				Db.Connection.ExecuteNonQuery($@"-- We need to disable the UPDATE Trigger in case there is a null value (no log data)
IF EXISTS(SELECT null FROM sys.objects WHERE name = 'TG_{TableName}_AuditDetailsAreNotMissing_Update' AND type = 'TR')
BEGIN
	DISABLE TRIGGER TG_{TableName}_AuditDetailsAreNotMissing_Update ON {TableName}
END
");

				var rowsUpdated = Db.Connection.ExecuteScalar<int>(
					sql,
					cmd =>
					{
						if (hasHighWatermarkRecorded)
						{
							cmd.AddParameter(HighWatermarkParameter, SqlDbType.DateTime, highWatermarkValue);
						}
					});

				ShowInfo($"    {rowsUpdated:N0} row(s) updated");

				Db.Connection.ExecuteNonQuery($@"
IF EXISTS(SELECT null FROM sys.objects WHERE name = 'TG_{TableName}_AuditDetailsAreNotMissing_Update' AND type = 'TR')
BEGIN
	ENABLE TRIGGER TG_{TableName}_AuditDetailsAreNotMissing_Update ON {TableName}
END
");
			}
		}

		string TableName
		{
			get { return TableSchema.TableName; }
		}

		SchemaPKColumn PK
		{
			get { return GetTableSchema(TableName).PK; }
		}

		protected virtual ITableSchema GetTableSchema(string tableName)
		{
			return GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetTableSchema(tableName);
		}

		public virtual bool ShouldOverwriteOldValues
		{
			get { return false; }
		}

		SchemaDateTimeColumn CreateTime
		{
			get { return TableSchema.All.OfType<SchemaDateTimeColumn>().FirstOrDefault(c => c.Name.Equals(GetColumnNamePrefix(TableName) + "_SystemCreateTimeUtc", StringComparison.OrdinalIgnoreCase)); }
		}

		SchemaStringColumn CreateUser
		{
			get { return TableSchema.All.OfType<SchemaStringColumn>().FirstOrDefault(c => c.Name.Equals(GetColumnNamePrefix(TableName) + "_SystemCreateUser", StringComparison.OrdinalIgnoreCase)); }
		}

		SchemaDateTimeColumn LastEditTime
		{
			get { return TableSchema.All.OfType<SchemaDateTimeColumn>().FirstOrDefault(c => c.Name.Equals(GetColumnNamePrefix(TableName) + "_SystemLastEditTimeUtc", StringComparison.OrdinalIgnoreCase)); }
		}

		SchemaStringColumn LastEditUser
		{
			get { return TableSchema.All.OfType<SchemaStringColumn>().FirstOrDefault(c => c.Name.Equals(GetColumnNamePrefix(TableName) + "_SystemLastEditUser", StringComparison.OrdinalIgnoreCase)); }
		}

		protected virtual string GetColumnNamePrefix(string tableName)
		{
			return GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetColumnNamePrefix(TableName);
		}

		protected virtual string RowFilter => "1=1";

		#region IPopulateAuditTimeAndUserInfo Members

		public abstract ITableSchema TableSchema { get; }

		SchemaDateTimeColumn IPopulateAuditTimeAndUserInfo.CreateTime
		{
			get { return CreateTime; }
		}

		SchemaStringColumn IPopulateAuditTimeAndUserInfo.CreateUser
		{
			get { return CreateUser; }
		}

		SchemaDateTimeColumn IPopulateAuditTimeAndUserInfo.LastEditTime
		{
			get { return LastEditTime; }
		}

		SchemaStringColumn IPopulateAuditTimeAndUserInfo.LastEditUser
		{
			get { return LastEditUser; }
		}

		#endregion //IPopulateAuditTimeAndUserInfo Members

		public string DefaultUserValue { get; set; }

		protected virtual bool ShouldProvideDefaultTimeIfLogMissing => true;
	}
}
