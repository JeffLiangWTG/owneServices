namespace Enterprise.DbUpgrader.Startup
{
	class ForceSchemaScriptDataUpgradeVersionInfo : ForceSchemaScriptUpgradeVersionInfo
	{
		public ForceSchemaScriptDataUpgradeVersionInfo()
			: base()
		{
		}

		#region IUpgradeStartupInfo Members

		public override bool IsRequired_Data
		{
			get { return true; }
		}

		#endregion
	}
}
