namespace Enterprise.DbUpgrader.Data
{
	public class BarcodeRuleUpgradeTask : EmbeddedUpgradeTask
	{
		public BarcodeRuleUpgradeTask()
			: base(new BarcodeRuleDataFile())
		{
		}
	}
}
