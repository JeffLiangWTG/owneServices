namespace WTG.TestHelpers
{
	public static class TestCase
	{
		public static string TestMethodPrefix => NUnit.Framework.TestCase.TestMethodPrefix;

		public static string BaseSourcePath
		{
			get
			{
				return NUnit.Framework.TestCase.BaseSourcePath;
			}
			set
			{
				NUnit.Framework.TestCase.BaseSourcePath = value;
			}
		}
	}
}
