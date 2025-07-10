namespace AnalyzersRunner.FunctionalTestingTarget.WTG.Analyzers
{
	static class WTG3006
	{
		public enum Items
		{
			Value0 = 0
		}

		//WTG3006:Prefer nameof over calling ToString on an enum literal
		public static string Method0() => Items.Value0.ToString();

		//WTG3006:Prefer nameof over calling ToString on an enum literal
		public static string Parenthesised => (Items.Value0).ToString();
	}
}
