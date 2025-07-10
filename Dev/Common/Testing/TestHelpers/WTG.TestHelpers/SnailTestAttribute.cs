namespace WTG.TestHelpers
{
	public static class SnailTestAttribute
	{
		public static bool IncludeSnailTests
		{
			get
			{
				return NUnit.Framework.SnailTestAttribute.IncludeSnailTests;
			}
			set
			{
				NUnit.Framework.SnailTestAttribute.IncludeSnailTests = value;
			}
		}
	}
}
