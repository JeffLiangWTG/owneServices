namespace Enterprise.DbUpgrader.Shared
{
	public interface IUpgradeContext
	{
		bool IsHosted { get; }

		bool? IsInternalSystem { get; }

		bool? IsUATSystem { get; }
	}
}
