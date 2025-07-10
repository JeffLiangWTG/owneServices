namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1099
	{
		public void Method()
		{
			//CW1099:Res.GetString Default Text Must Be String Literal
			_ = Res.GetString("resource-key", GetType().FullName);
		}
	}
}
