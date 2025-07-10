namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1050
	{
		//CW1050:Use System.TimeSpan Type For A Duration
		public int ValueInSeconds;

		//CW1050:Use System.TimeSpan Type For A Duration
		public int ValueInMinutes;

		//CW1050:Use System.TimeSpan Type For A Duration
		public decimal ValueInHours;

		//will not trigger analyzer
		public int Value;
	}
}
