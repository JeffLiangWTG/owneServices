using System.Globalization;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class GlowThemeItemToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string TargetTableName
		{
			get { return "BPMConfigurationItem"; }
		}

		protected override string GetDataToPreserveFilter(string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, @$"
WHERE EXISTS (
        SELECT null
        FROM [{targetDbName}].dbo.StmData
        WHERE SD_Name = 'PreserveGlowTheme'
            AND CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'True'
    )
    AND VCM_ConfigurationKey IN ('CoBranding', 'Theme')");
		}

		public override string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, @$"
-- DELETE DATA WHICH WILL BE RE-POPULATED FROM {AuxTableName} TABLE --
IF EXISTS (SELECT NULL FROM sys.databases WHERE name = '{auxDbName}')
BEGIN
    DELETE [{targetDbName}].dbo.{TargetTableName}
    WHERE EXISTS (
            SELECT null
            FROM [{auxDbName}].dbo.TempStmData
            WHERE SD_Name = 'PreserveGlowTheme'
                AND CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'True'
        )
        AND VCM_ConfigurationKey IN ('CoBranding', 'Theme')
END");
		}

		#endregion
	}
}
