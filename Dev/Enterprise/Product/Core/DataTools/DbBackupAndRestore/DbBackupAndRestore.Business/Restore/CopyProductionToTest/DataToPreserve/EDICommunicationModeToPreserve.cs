using System.Globalization;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class EDICommunicationModeToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string TargetTableName
		{
			get { return "EDICommunicationsMode"; }
		}

		public override string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Empty;
		}

		public override string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, CopyTempStmNumsScript, auxDbName, targetDbName);
		}

		const string CopyTempStmNumsScript = @"
-- OVERWRITE PROD DATA WITH TEST DATA IF DUPLICATE PKS --
		USE [{1}]
		DECLARE @OverwriteProdEdiCommsWithTestIfDuplicatePK NVARCHAR(MAX) = N'
			UPDATE [{1}].[dbo].[EDICommunicationsMode] SET '
 
			SELECT @OverwriteProdEdiCommsWithTestIfDuplicatePK = @OverwriteProdEdiCommsWithTestIfDuplicatePK + c.COLUMN_NAME + ' = b.' + c.COLUMN_NAME + ','
			FROM INFORMATION_SCHEMA.COLUMNS as c
			WHERE TABLE_NAME = 'EDICommunicationsMode'
 
			SET @OverwriteProdEdiCommsWithTestIfDuplicatePK = SUBSTRING(@OverwriteProdEdiCommsWithTestIfDuplicatePK, 1, LEN(@OverwriteProdEdiCommsWithTestIfDuplicatePK) - 1)
 
			SET @OverwriteProdEdiCommsWithTestIfDuplicatePK = @OverwriteProdEdiCommsWithTestIfDuplicatePK + ' FROM [{0}]..[TempEDICommunicationsMode] b '
			SET @OverwriteProdEdiCommsWithTestIfDuplicatePK = @OverwriteProdEdiCommsWithTestIfDuplicatePK + 'JOIN [{1}]..[EDICommunicationsMode] a '
			SET @OverwriteProdEdiCommsWithTestIfDuplicatePK = @OverwriteProdEdiCommsWithTestIfDuplicatePK + 'ON a.EK_PK = b.EK_PK'
 
		EXEC sp_executesql @OverwriteProdEdiCommsWithTestIfDuplicatePK

-- COPY TEST SPECIFIC DATA INTO TEST DB --
		DECLARE @CopyOldTestEdiCommsIntoNewTestDatabase NVARCHAR(MAX) = N'
			INSERT [{1}]..EDICommunicationsMode
			SELECT *
			FROM [{0}]..TempEDICommunicationsMode
			WHERE EK_PK NOT IN (SELECT EK_PK FROM [{1}]..EDICommunicationsMode)'

		EXEC sp_executesql @CopyOldTestEdiCommsIntoNewTestDatabase
		";

		#endregion
	}
}
