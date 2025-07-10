namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1097
	{
		public void Method()
		{
			//CW1097:Do Not Hardcode Tmp Or Temp Path
			_ = "\\temp";
		}
	}
}
