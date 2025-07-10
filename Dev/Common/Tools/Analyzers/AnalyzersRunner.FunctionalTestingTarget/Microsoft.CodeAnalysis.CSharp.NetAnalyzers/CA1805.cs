using System.ComponentModel;

namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.NetAnalyzers
{
	/// <summary>
	/// Rule: 	Do not initialize unnecessarily
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used for testing CA1805")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Used for testing CA1805")]
	class CA1805
	{
		//CA1805:Do not initialize unnecessarily
		int _value1 = 0;

		//CA1805:Do not initialize unnecessarily
		static readonly int _value2 = 0;

		//https://github.com/dotnet/roslyn-analyzers/issues/7341
		//CA1805 does not trigger due to having an attribute
		[ReadOnly(true)]
		int _value3 = 0;
	}
}
