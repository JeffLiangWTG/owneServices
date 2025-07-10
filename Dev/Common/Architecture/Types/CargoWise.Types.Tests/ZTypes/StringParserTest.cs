
namespace CargoWise.Types.Tests
{
	using NUnit.Framework;

	public class StringParserTest : TestCase
	{
		public void TestTrimBlankLinesAndNewLines()
		{
			string[] lines = new string[3];
			lines[0] = "OK";
			lines[1] = "trim me\r\n";
			lines[2] = "";

			lines = StringParser.TrimBlankLinesAndNewLines(lines);
			AssertEquals("only 2 lines ok", 2, lines.Length);
			AssertEquals("non-trimmed", "OK", lines[0]);
			AssertEquals("trimmed", "trim me", lines[1]);
		}
	}
}
