namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1161
	{
		public void BadCode()
		{
			// CW1161 Do not use constant string literals - use resource strings instead.
			_ = "This is a bad string";
		}
	}
}
