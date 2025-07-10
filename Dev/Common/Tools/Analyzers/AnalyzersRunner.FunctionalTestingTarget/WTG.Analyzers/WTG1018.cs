namespace AnalyzersRunner.FunctionalTestingTarget.WTG.Analyzers
{
	class WTG1018
	{
		void Method(bool first, bool second, string third, bool last)
		{
			_ = first;
			_ = second;
			_ = third;
			_ = last;
		}

		//WTG1018: Boolean literals as method arguments should be passed as named arguments.
		public void BadCode()
		{
			Method(true, false, "test", true);
		}

		public void GoodCode()
		{
			Method(first: true, second: false, "test", last: true);
		}
	}
}
