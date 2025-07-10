namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1053
	{
		public void Method()
		{
			//CW1053:Do not use default SingleRefDatabaseName constant
			_ = "CW-refdatabase-1";
		}
	}
}
