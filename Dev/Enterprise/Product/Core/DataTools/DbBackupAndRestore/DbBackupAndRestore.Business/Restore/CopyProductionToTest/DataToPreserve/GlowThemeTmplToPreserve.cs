using System.Globalization;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class GlowThemeTmplToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string TargetTableName
		{
			get { return "BPMConfigurationTmpl"; }
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
    AND VCT_PK IN (
        SELECT VCM_VCT_Template
        from [{targetDbName}].dbo.BPMConfigurationItem
        WHERE VCM_ConfigurationKey IN ('CoBranding', 'Theme')
    )");
		}

		public override string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, @$"
-- DELETE DATA WHICH WILL BE RE-POPULATED FROM {AuxTableName} TABLE --
IF EXISTS (SELECT NULL FROM sys.databases WHERE name = '{auxDbName}')
BEGIN
    -- Disable Foreign Key
    ALTER TABLE [{targetDbName}].dbo.BPMConfigurationItem NOCHECK CONSTRAINT BPMConfigurationItem_VCM_VCT_Template_FK2_BPMConfigurationTmpl_RRR_120N;

    DELETE [{targetDbName}].dbo.{TargetTableName}
    WHERE EXISTS (
            SELECT null
            FROM [{auxDbName}].dbo.TempStmData
            WHERE SD_Name = 'PreserveGlowTheme'
                AND CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'True'
        )
        AND VCT_PK IN (
            SELECT VCM_VCT_Template
            from [{targetDbName}].dbo.BPMConfigurationItem
            WHERE VCM_ConfigurationKey IN ('CoBranding', 'Theme')
                AND VCM_VCT_Template NOT IN (
                    SELECT VCM_VCT_Template
                    FROM [{targetDbName}].dbo.BPMConfigurationItem
                    WHERE VCM_ConfigurationKey NOT IN ('CoBranding', 'Theme')
                )
    
            UNION
    
            SELECT VCT_PK FROM [{auxDbName}].dbo.TempBPMConfigurationTmpl
        )
END");
		}

		public override string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			var defaultCopyScript = base.GetCopyTempDbDataToTestDbScript(auxDbName, targetDbName);

			return string.Format(CultureInfo.InvariantCulture, @$"
{defaultCopyScript}

-- Enable Foreign Key
ALTER TABLE [{targetDbName}].dbo.BPMConfigurationItem CHECK CONSTRAINT BPMConfigurationItem_VCM_VCT_Template_FK2_BPMConfigurationTmpl_RRR_120N;
");
		}

		#endregion
	}
}
