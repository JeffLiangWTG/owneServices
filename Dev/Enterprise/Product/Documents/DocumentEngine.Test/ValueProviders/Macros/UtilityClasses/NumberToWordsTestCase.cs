using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	public abstract class NumberToWordsTestCase : TestCase
	{
		internal abstract INumberToWords GetInstance();

		public void Assert(long number, string expected)
		{
			AssertEquals(number.ToString(), expected, instance.GetNumberAsString(number));
		}

		protected override void SetUp()
		{
			instance = GetInstance();
			base.SetUp();
		}

		INumberToWords instance;
	}
}
