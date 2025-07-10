using NUnit.Framework;

namespace Enterprise.DocumentScanning.Launch.Testing
{
	sealed class DiagnosticsManagerTest : TestCase
	{
		public void TestAddingLines()
		{
			DiagnosticsManager dM = new DiagnosticsManager();

			dM.AppendMessage("hello ");
			dM.AppendMessageLine("world");
			dM.AppendMessageLine("line 2");

			string expectedResult = "hello world\r\nline 2\r\n";
			AssertEquals("append", expectedResult, dM.TheMessage);
		}
	}
}
