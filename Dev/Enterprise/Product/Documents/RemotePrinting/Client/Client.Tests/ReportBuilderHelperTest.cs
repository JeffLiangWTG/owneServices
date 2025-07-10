using System.Text;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class ReportBuilderHelperTest : TestCase
	{
		public void TestSetCustomValue()
		{
			var reportBuilder = ErrorReporter.CreateReportBuilderExposedForTest();
			reportBuilder.SetCustomValue("key1", "value 1");
			reportBuilder.SetCustomValue("key2", "value 2");

			var reportText = reportBuilder.ToStringAsync(Encoding.UTF8).Result;

			AssertContains("<key1>value 1</key1>", reportText);
			AssertContains("<key2>value 2</key2>", reportText);
		}

		public void TestSetCustomValueWithSection()
		{
			var reportBuilder = ErrorReporter.CreateReportBuilderExposedForTest();
			reportBuilder.SetCustomValueWithSection("section1", "key1", "value 1");
			reportBuilder.SetCustomValueWithSection("section1", "key2", "value 2");
			reportBuilder.SetCustomValueWithSection("section2", "key3", "value 3");

			var reportText = reportBuilder.ToStringAsync(Encoding.UTF8).Result;

			const string expectedSection1Data =
@"<section1>
    <key1>value 1</key1>
    <key2>value 2</key2>
  </section1>";
			AssertContains(expectedSection1Data, reportText);

			const string expectedSection2Data =
@"<section2>
    <key3>value 3</key3>
  </section2>";
			AssertContains(expectedSection2Data, reportText);
		}
	}
}
