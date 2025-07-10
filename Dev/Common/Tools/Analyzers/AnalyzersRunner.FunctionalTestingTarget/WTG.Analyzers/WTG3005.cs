namespace AnalyzersRunner.FunctionalTestingTarget.WTG.Analyzers
{
	class WTG3005
	{
		public void Method()
		{
			//WTG3005:Don't call ToString() on a string
			_ = "".ToString();
		}
	}
}
