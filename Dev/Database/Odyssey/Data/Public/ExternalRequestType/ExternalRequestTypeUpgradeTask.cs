namespace Enterprise.DbUpgrader.Data
{
	public class ExternalRequestTypeUpgradeTask : EmbeddedUpgradeTask
	{
		public ExternalRequestTypeUpgradeTask()
			: base(new ExternalRequestTypeDataFile())
		{
		}
	}
}
