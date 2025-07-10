using WTG.StaticAnalysis.Annotation;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	[Immutable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Used for testing CW1149")]
	public class CW1149
	{
		static readonly CW1149 instance = new CW1149();

		public static CW1149 Instance => instance;

		// CW1149 Singleton classes should not have public constructors.
		public CW1149()
		{
		}
	}
}
