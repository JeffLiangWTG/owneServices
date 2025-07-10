using System.Globalization;
using System.Linq;
using System.Text;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class NumberFountainsToPreserve : IPreserveTestValueScripts
	{
		static string[] NumberFountainsNameList => new[]
		{
			"IncidentApprovalClientRef"
		};

		static string NumberFountainsSqlNameList
		{
			get
			{
				return "\r\n" + string.Join(",\r\n", NumberFountainsNameList.Select(x => '\'' + x + '\'')) + "\r\n";
			}
		}

		public string GetPopulateTemporaryDataScript(string auxDbName, string targetDbName)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(DropTempTableIfExistsScript, auxDbName, TempStmNumsTableName);
			builder.AppendFormat(PopulateTemporaryDataScript, auxDbName, targetDbName, CargoWise.Data.DataUtils.EscapeSingleQuotes(GetStmNumsFilter()));
			return builder.ToString();
		}

		public string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, ClearDataToBeOverwrittenByTestDataScript, targetDbName, GetStmNumsFilter());
		}

		public string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			return string.Format(CopyTempStmNumsScript, auxDbName, targetDbName);
		}

		string GetStmNumsFilter()
		{
			return string.Format(CultureInfo.InvariantCulture, @"where SN_Name in ({0})", NumberFountainsSqlNameList);
		}

		const string StmNumsTableName = "StmNums";
		const string TempStmNumsTableName = "Temp" + StmNumsTableName;

		#region Scripts

		const string DropTempTableIfExistsScript = @"
-- DROP PREVIOUS {1} TABLE IF THERE IS ONE
IF EXISTS (SELECT null FROM [{0}].sys.tables WHERE name = '{1}')
BEGIN
	DROP TABLE [{0}]..{1}
END
";

		const string PopulateTemporaryDataScript = @"
-- POPULATE TempStmNums TABLE --
DECLARE @StmNumsSQLText nvarchar(max)
DECLARE @StmNumsPK nvarchar(10)

IF (exists(select null from [{1}].sys.columns c join [{1}].sys.tables t on c.object_id = t.object_id where t.name = 'StmNums' and c.name = 'SN_Id'))
BEGIN
	SET @StmNumsPK = 'SN_Id'
END ELSE BEGIN
	SET @StmNumsPK = 'SN_PK'
END

-- preserve
SET @StmNumsSQLText = N'
	SELECT
		  SN_Name,
		  SN_Owner,
		  SN_Value = CASE
				WHEN COUNT(*) = MAX(SG_Value) - MIN(SG_Value) + 1 THEN MIN(SG_Value)
				ELSE MIN(SN_Value)
		  END
		  INTO [{0}]..TempStmNums
	FROM [{1}]..StmNums
	LEFT JOIN [{1}]..StmNumberCache on SG_SN = ' + @StmNumsPK + '
	{2}
	GROUP BY SN_Name, SN_Owner'

EXEC sp_executesql @StmNumsSQLText
";

		const string ClearDataToBeOverwrittenByTestDataScript = @"
-- DELETE DATA WHICH WILL BE RE-POPULATED FROM TempStmNums TABLE --
DELETE [{0}]..StmNums
{1}
";

		const string CopyTempStmNumsScript = @"
-- COPY PREVIOUSLY SAVED StmNums TEST DATA --
DECLARE @StmNumsSQLText nvarchar(max)

IF (exists(select null from [{1}].sys.columns c join [{1}].sys.tables t on c.object_id = t.object_id where t.name = 'StmNums' and c.name = 'SN_Id'))
BEGIN
	SET @StmNumsSQLText = N'
		insert [{1}]..StmNums (SN_Name, SN_Value, SN_Owner)
		select SN_Name, SN_Value, isnull(SN_Owner, 0x)
		from [{0}]..TempStmNums'
END ELSE BEGIN
	SET @StmNumsSQLText = N'
		insert [{1}]..StmNums (SN_Name, SN_Value, SN_Owner, SN_PK)
		select SN_Name, SN_Value, SN_Owner, NEWID()
		from [{0}]..TempStmNums'
END

EXEC sp_executesql @StmNumsSQLText
";

		#endregion
	}
}
