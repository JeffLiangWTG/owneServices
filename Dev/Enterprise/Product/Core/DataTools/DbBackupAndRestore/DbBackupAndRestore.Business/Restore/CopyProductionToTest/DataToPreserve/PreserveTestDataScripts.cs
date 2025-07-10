using System.Globalization;
using System.Text;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public abstract class PreserveTestValueScripts : IPreserveTestValueScripts
	{
		#region IPreserveTestValueScripts Members

		public virtual string GetPopulateTemporaryDataScript(string auxDbName, string targetDbName)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(CultureInfo.InvariantCulture, DropTempTableIfExistsScript, auxDbName, AuxTableName);
			builder.AppendFormat(CultureInfo.InvariantCulture, PopulateTemporaryDataScript, auxDbName, AuxTableName, targetDbName, TargetTableName, GetDataToPreserveFilter(targetDbName));
			return builder.ToString();
		}

		public virtual string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, ClearDataToBeOverwrittenByTestDataScript, AuxTableName, targetDbName, TargetTableName, GetDataToClearFilter(targetDbName));
		}

		public virtual string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, CopyTempDbDataToTestDbScript, targetDbName, TargetTableName, auxDbName, AuxTableName, Db.DatabaseCollation, DataUtils.EscapeSingleQuotes(GetPasteTempDataFilter(targetDbName)));
		}

		#endregion

		/// <summary>
		/// Override this method if you want restrict the test data to be saved before the restore.
		/// </summary>
		/// <param name="targetDbName">To specify the database name.</param>
		/// <returns>SQL filter.</returns>
		protected virtual string GetDataToPreserveFilter(string targetDbName)
		{
			return string.Empty;
		}

		/// <summary>
		/// Override this method if you want restrict the data to be cleared before overwriting it with saved test data.
		/// </summary>
		/// <param name="targetDbName">To specify the database name.</param>
		/// <returns>SQL filter.</returns>
		protected virtual string GetDataToClearFilter(string targetDbName)
		{
			return GetDataToPreserveFilter(targetDbName);
		}

		/// <summary>
		/// Override this method if you need an extra criteria
		/// when inserting the saved data from the temporary
		/// to the Test database.
		/// e.g.: when a FK to a non-preserved table must be checked.
		/// </summary>
		/// <param name="targetDbName">To specify the database name.</param>
		/// <returns>SQL filter.</returns>
		protected virtual string GetPasteTempDataFilter(string targetDbName)
		{
			return string.Empty;
		}

		protected string AuxTableName
		{
			get { return string.Format(CultureInfo.InvariantCulture, "Temp{0}", TargetTableName); }
		}

		abstract protected string TargetTableName { get; }

		#region Scripts

		const string DropTempTableIfExistsScript = @"
-- DROP PREVIOUS {1} TABLE IF THERE IS ONE
IF EXISTS (SELECT null FROM [{0}].sys.objects WHERE name = '{1}')
BEGIN
	DROP TABLE [{0}]..{1}
END
";

		const string PopulateTemporaryDataScript = @"
-- POPULATE {1} TABLE --
SELECT * INTO [{0}]..{1} 
FROM [{2}]..{3}
{4}
";

		const string ClearDataToBeOverwrittenByTestDataScript = @"
-- DELETE DATA WHICH WILL BE RE-POPULATED FROM {0} TABLE --
DELETE [{1}]..{2}
{3}
";

		const string CopyTempDbDataToTestDbScript = @"
-- COPY PREVIOUSLY SAVED TEST DATA FROM {3} TABLE --
DECLARE @{1}SqlText nvarchar(max)
DECLARE @{1}ColumnNameList varchar(max)

SELECT
			@{1}ColumnNameList = ISNULL(@{1}ColumnNameList + ', ', '') + InsCol.name
			FROM
						[{0}].sys.tables InsTab
						INNER JOIN [{2}].sys.tables SelTab
									ON SelTab.name = ('Temp' + InsTab.name) COLLATE {4}
						INNER JOIN [{0}].sys.columns InsCol
									ON InsTab.object_id = InsCol.object_id
						INNER JOIN [{2}].sys.columns SelCol
									ON SelTab.object_id = SelCol.object_id
			WHERE
						SelCol.name = InsCol.name COLLATE {4} 
						AND InsTab.name = '{1}'
						AND InsCol.is_computed = 0

SET @{1}SqlText =
	'INSERT [{0}]..{1} (' + @{1}ColumnNameList + ') '+
	' SELECT ' + @{1}ColumnNameList +
	' FROM [{2}]..{3} {5}'
EXEC (@{1}SqlText)
";
		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	static class SchemaTestHelper
	{
		public static bool TableExists(string tableName)
		{
			string sqlText = string.Format(@"
				IF EXISTS (SELECT null FROM {0}.sys.objects WHERE name = '{1}')
				BEGIN
					SELECT 1
				RETURN
				END
				SELECT 0", Db.DatabaseName, tableName);
			return ((int)Db.Connection.ExecuteScalar(sqlText) == 1);
		}

		public static bool ColumnExists(string columnName)
		{
			string sqlText = string.Format(@"
				IF EXISTS (SELECT null FROM {0}.sys.columns WHERE name = '{1}')
				BEGIN
					SELECT 1
					RETURN
				END
				SELECT 0", Db.DatabaseName, columnName);
			return ((int)Db.Connection.ExecuteScalar(sqlText) == 1);
		}

		public static bool ColumnExistsAndIsNullable(string columnName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS (SELECT null FROM {0}.sys.columns WHERE name = '{1}' AND is_nullable = 1)
				BEGIN
					SELECT 1
					RETURN
				END
				SELECT 0", Db.DatabaseName, columnName);
			return ((int)Db.Connection.ExecuteScalar(sqlText) == 1);
		}

		public static bool ConstraintExists(string constraintName, string definition)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS (SELECT null FROM {0}.sys.check_constraints WHERE name = '{1}' AND definition = '{2}')
				BEGIN
					SELECT 1
					RETURN
				END
				SELECT 0", Db.DatabaseName, constraintName, definition.Replace("'", "''"));
			return ((int)Db.Connection.ExecuteScalar(sqlText) == 1);
		}
	}
}

#endif
#endregion



