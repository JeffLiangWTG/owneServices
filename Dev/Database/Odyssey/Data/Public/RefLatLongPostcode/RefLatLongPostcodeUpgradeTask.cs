namespace Enterprise.DbUpgrader.Data
{
	public class RefLatLongPostcodeUpgradeTask : EmbeddedUpgradeTask
	{
		public RefLatLongPostcodeUpgradeTask() : base(new RefLatLongPostcodeDataFile())
		{
		}
	}
}
