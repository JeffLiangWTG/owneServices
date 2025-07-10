namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class StmServiceHostToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string TargetTableName
		{
			get { return "StmServiceHost"; }
		}

		#endregion
	}
}
