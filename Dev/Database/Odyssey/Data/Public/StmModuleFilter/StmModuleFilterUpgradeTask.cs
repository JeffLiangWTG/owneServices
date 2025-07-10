namespace Enterprise.DbUpgrader.Data
{
	public class StmModuleFilterUpgradeTask : EmbeddedUpgradeTask
	{
		public StmModuleFilterUpgradeTask() : base(new StmModuleFilterDataFile())
		{
		}

		public StmModuleFilterUpgradeTask(EmbeddedDataFile resourceDataFile) : base(resourceDataFile)
		{
		}
	}
}
