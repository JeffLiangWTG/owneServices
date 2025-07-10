using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(TariffFormatter))]
	sealed class TariffFormatterTest : TestCase
	{
		public void TestFormat()
		{
			var formatter = new TariffFormatter();
			AssertEquals("87149990905", formatter.Format("87149990905"));
			AssertEquals("87149990905", formatter.Format("A87149990905"));
			AssertEquals("87149990905", formatter.Format("8714.99.90.90-5"));
		}

		public void TestDisplayFormat()
		{
			var formatter = new TariffFormatter();
			AssertEquals("8714.99.909", formatter.DisplayFormat("871499909"));
			AssertEquals("8714.99", formatter.DisplayFormat("871499"));
			AssertEquals("8714", formatter.DisplayFormat("8714"));
		}
	}
}
