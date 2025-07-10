namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.CSharp.CodeStyle
{
	class IDE0051
	{
		//IDE0051:Remove unused private members
		void UnusedMethod()
		{ }

		//IDE0051:Remove unused private members
		readonly int UnusedField = 5;
	}
}
