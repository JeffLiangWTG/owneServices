namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	public enum CA1700
	{
		High = 1,
		Low = 2,

		//CA1700:Do not name enum values 'Reserved'
		//Note: not enabled, so should not be reported
		Reserved = 3
	}
}
