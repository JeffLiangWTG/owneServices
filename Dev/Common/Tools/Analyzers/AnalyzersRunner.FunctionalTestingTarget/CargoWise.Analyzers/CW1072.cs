namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1072
	{
		public void Method()
		{
			//CW1072:Do Not Use DB Name In SQL Commands
			_ = "Odyssey.dbo.tableName";
		}
	}
}
