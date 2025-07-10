namespace AnalyzersRunner.FunctionalTestingTarget.Microsoft.CodeAnalysis.NetAnalyzers
{
	class CA2013
	{
		public void Method()
		{
			//CA2013:Do not use ReferenceEquals with value types
			_ = ReferenceEquals(1, 2);
		}
	}
}
