namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class StmUpgradeToPreserve : PreserveTestValueScripts
	{
		protected override string TargetTableName
		{
			get { return "StmUpgrade"; }
		}

		public override string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return @"
					--
					";
		}

		public override string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			return
				string.Format(@"
				DECLARE @RestoredCurrentPackage uniqueidentifier
				SET @RestoredCurrentPackage = (SELECT TOP 1 SZ_PK FROM [{2}]..StmUpgrade WHERE SZ_Status = 'CUR')
				DELETE FROM [{2}]..StmUpgrade WHERE {3} IN (SELECT {3} FROM [{0}]..{1}) AND SZ_Status <> 'CUR' 
				" +
				base.GetCopyTempDbDataToTestDbScript(auxDbName, targetDbName)
				+ @"
				IF(@RestoredCurrentPackage is not null)
					UPDATE [{2}]..StmUpgrade SET
						SZ_Status = 'RDY',
						SZ_SystemLastEditTimeUtc = GetUtcDate(),
						SZ_SystemLastEditUser = '~BP'
				WHERE 1 = 1
					AND SZ_Status = 'CUR'
					AND SZ_PK <> @RestoredCurrentPackage
				"
				, auxDbName, AuxTableName, targetDbName, VersionFunction);
		}

		protected override string GetDataToPreserveFilter(string targetDbName)
		{
			return "WHERE ((SZ_Status = 'CUR') OR (SZ_Status = 'RDY' AND SZ_ExeVersionDate > DATEADD(month, -3, getdate())))";
		}

		protected override string GetPasteTempDataFilter(string targetDbName)
		{
			return string.Format("WHERE {1} NOT IN (SELECT {1} FROM [{0}]..StmUpgrade WHERE SZ_Status = 'CUR')", targetDbName, VersionFunction);
		}

		internal const string VersionFunction = " cast(SZ_MajorVersion as varchar) + '.' + cast(SZ_MinorVersion as varchar) + '.' + cast(SZ_Release as varchar) + '.' + cast(SZ_Patch as varchar) ";
	}
}
