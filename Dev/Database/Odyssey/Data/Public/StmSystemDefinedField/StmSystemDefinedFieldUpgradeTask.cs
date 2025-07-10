namespace Enterprise.DbUpgrader.Data
{
	public class StmSystemDefinedFieldUpgradeTask : EmbeddedUpgradeTask
	{
		public StmSystemDefinedFieldUpgradeTask() : base(new StmSystemDefinedFieldDataFile())
		{
		}

		public StmSystemDefinedFieldUpgradeTask(EmbeddedDataFile resourceDataFile) : base(resourceDataFile)
		{
		}
	}
}
