namespace Enterprise.Integration
{
	public interface IWebConfig
	{
		bool ActiveBranchOnly { get; }
		bool ActiveDepartmentOnly { get; }
	}
}
