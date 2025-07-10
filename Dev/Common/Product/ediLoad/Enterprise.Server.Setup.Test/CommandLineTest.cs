using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	class CommandLineTest : TestCase
	{
		public void TestParsing()
		{
			AssertEquals(@"C:\Program Files\App Name\app.exe", new CommandLine(@"""C:\Program Files\App Name\app.exe""").ExecutableName);
			AssertEquals(@"C:\Program Files\App Name\app.exe", new CommandLine(@"""C:\Program Files\App Name\app.exe"" -arg -argggh ""arg arg""").ExecutableName);
			AssertEquals(@"C:\Path\app.exe", new CommandLine(@"C:\Path\app.exe -arg -arg").ExecutableName);
			AssertEquals(@"C:\Path\app.exe", new CommandLine(@"C:\Path\app.exe").ExecutableName);
		}
	}
}
