namespace Enterprise.DbUpgrader.Data
{
	public class InventoryHeldCodeUpgradeTask : EmbeddedUpgradeTask
	{
		public InventoryHeldCodeUpgradeTask()
			: base(new InventoryHeldCodeDataFile())
		{
		}
	}
}
