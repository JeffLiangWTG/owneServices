namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	// CW1154 Public/Internal classes should be in their own code file.
	public class TestClass
	{
		public string Name { get; set; }
	}
	internal class AnotherTestClass
	{
		public string Name { get; set; }
	}
}
