using System.Globalization;
using System.Text;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class RegistryItemsGetMinTransformationVersion : IPreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		public string GetPopulateTemporaryDataScript(string auxDbName, string targetDbName)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(DropTempTableIfExistsScript, auxDbName, TempTableName);
			builder.AppendFormat(PopulateTemporaryDataScript, targetDbName, MainTableName, auxDbName, TempTableName, GetDataToPreserveFilter());
			return builder.ToString();
		}

		public string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Empty;
		}

		public string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(updateQuery , MainTableName, targetDbName, TempTableName, auxDbName);
			return builder.ToString();
		}

		string GetDataToPreserveFilter()
		{
			return string.Format(CultureInfo.InvariantCulture, @"WHERE (SD_Name in ('{0}','{1}'))", CopyProductionToTestHelper.TransformationVersion, CopyProductionToTestHelper.MinorTransformationVersion);
		}

		const string DropTempTableIfExistsScript = @"
-- DROP PREVIOUS {1} TABLE IF THERE IS ONE
IF exists (SELECT * FROM [{0}].sys.tables WHERE name = '{1}')
BEGIN
	DROP TABLE [{0}]..{1}
END
";

		const string PopulateTemporaryDataScript = @"
-- POPULATE {1} TABLE --
SELECT * INTO [{2}]..{3}
FROM [{0}]..{1} 
{4}";

		readonly string updateQuery = string.Format(@"

DECLARE @MajorNewB varbinary(max) = (SELECT SD_BinaryValue FROM {2} WHERE SD_Name in ({0}))
DECLARE @MinorNewB varbinary(max) = (SELECT SD_BinaryValue FROM {2} WHERE SD_Name in ({1}))
DECLARE @MajorOldB varbinary(max) = (SELECT SD_BinaryValue FROM {3} WHERE SD_Name in ({0}))
DECLARE @MinorOldB varbinary(max) = (SELECT SD_BinaryValue FROM {3} WHERE SD_Name in ({1}))

DECLARE @MajorNew int = ISNULL(CONVERT(int,convert(nvarchar(max), @MajorNewB)),0)
DECLARE @MinorNew int = ISNULL(CONVERT(int,convert(nvarchar(max), @MinorNewB)),0)
DECLARE @MajorOld int = ISNULL(CONVERT(int,convert(nvarchar(max), @MajorOldB)),0)
DECLARE @MinorOld int = ISNULL(CONVERT(int,convert(nvarchar(max), @MinorOldB)),0)

SELECT @MajorNew , @MinorNew , @MajorOld , @MinorOld 

if (NOT @MajorNewB IS NULL)
BEGIN
	if (@MajorOldB IS NULL)
		BEGIN
				DELETE FROM {2}
					WHERE SD_Name in ( {0} , {1} )
		END
	ELSE
		BEGIN
			IF (    @MajorOld < @MajorNew 
					OR (@MajorOld = @MajorNew AND @MinorOld < @MinorNew) 
				)
			BEGIN
					UPDATE {2}
						SET SD_BinaryValue = @MajorOldB
						WHERE SD_Name = {0} 
					UPDATE {2}
						SET SD_BinaryValue = @MinorOldB
						WHERE SD_Name = {1}
			END
		END
END

", "'" + CopyProductionToTestHelper.TransformationVersion + "'", "'" + CopyProductionToTestHelper.MinorTransformationVersion + "'", "[{1}]..{0}", "[{3}]..{2}");

		string TempTableName
		{
			get { return string.Format("Temp{0}TransformationVersion", MainTableName); }
		}

		string MainTableName
		{
			get { return "StmData"; }
		}

		#endregion
	}
}
