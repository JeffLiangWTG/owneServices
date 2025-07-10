using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	interface IMinimumDotNetVersionDefinition
	{
		string MinimumDotNetVersionRequired { get; }
		bool IsDotNetSupportedVersion(DotNetVersion dotNetVersion);
		bool IsAnOldRecord(PcWithDotNetVersionRecord dotNetVersion);
	}
}
