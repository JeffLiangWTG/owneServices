namespace Enterprise.DbUpgrader.Shared
{
	public interface IUpgradeActionProvider
	{
		IUpgradeAction UpgradeAction { get; }
	}
}
