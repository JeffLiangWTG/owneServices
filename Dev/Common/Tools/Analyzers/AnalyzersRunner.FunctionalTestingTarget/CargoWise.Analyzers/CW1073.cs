namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1073
	{
		internal string field = "password12345";

		public void Method()
		{
			//CW1073:Do Not Hardcode SQL Password
			_ = "kwo123456789f";
		}
	}
}
