namespace Enterprise.DbUpgrader.Transformations.Testing
{
	sealed class OffineClusterKeyDoerSupportAndValidationTest : BaseClusterKeyDoerSupportAndValidationTest
	{
		protected override bool IsOnline => false;
	}
}
