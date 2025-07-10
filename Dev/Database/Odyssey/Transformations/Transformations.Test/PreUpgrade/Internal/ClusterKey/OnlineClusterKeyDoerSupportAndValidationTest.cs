namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class OnlineClusterKeyDoerSupportAndValidationTest : BaseClusterKeyDoerSupportAndValidationTest
	{
		protected override bool IsOnline => true;
	}
}
