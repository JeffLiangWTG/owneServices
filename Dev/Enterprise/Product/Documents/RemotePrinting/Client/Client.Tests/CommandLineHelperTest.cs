using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CommandLineHelperTest : TestCase
	{
		public void TestCommandLineToArgs()
		{
			AssertCommandLineToArgs("\"c:\\Program Files\\WiseTech Global\\Program.exe\" \"arg1:argument value\" arg2 arg3:value",
				"c:\\Program Files\\WiseTech Global\\Program.exe",
				"arg1:argument value",
				"arg2",
				"arg3:value");
		}

		void AssertCommandLineToArgs(string commandLine, params string[] expectedArgs)
		{
			var arguments = CommandLineHelper.CommandLineToArgs(commandLine);

			AssertContainsExactElementsInExactOrder(expectedArgs, arguments);
		}

		public void TestGetArgumentFromCommandLine()
		{
			AssertEquals("arg1:argument value", CommandLineHelper.GetArgument("\"c:\\Program Files\\WiseTech Global\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "arg1:"));
			AssertEquals("arg2", CommandLineHelper.GetArgument("\"c:\\Program Files\\WiseTech Global\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "arg2"));
			AssertEquals(string.Empty, CommandLineHelper.GetArgument("\"c:\\Program Files\\WiseTech Global\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "arg2:"));
			AssertEquals("arg3:value", CommandLineHelper.GetArgument("\"c:\\Program Files\\WiseTech Global\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "arg3:"));
		}

		public void TestGetArgumentFromArguments()
		{
			AssertEquals("arg1:argument value", CommandLineHelper.GetArgument(new string[] { "arg1:argument value", "arg2", "arg3:value" }, "arg1:"));
			AssertEquals("arg2", CommandLineHelper.GetArgument(new string[] { "arg1:argument value", "arg2", "arg3:value" }, "arg2"));
			AssertEquals(string.Empty, CommandLineHelper.GetArgument(new string[] { "arg1:argument value", "arg2", "arg3:value" }, "arg2:"));
			AssertEquals("arg3:value", CommandLineHelper.GetArgument(new string[] { "arg1:argument value", "arg2", "arg3:value" }, "arg3:"));
		}

		public void TestGetExeFileName()
		{
			AssertEquals(string.Empty, CommandLineHelper.GetExeFileName(null));
			AssertEquals(string.Empty, CommandLineHelper.GetExeFileName(""));
			AssertEquals(string.Empty, CommandLineHelper.GetExeFileName(" "));
			AssertEquals("c:\\Program", CommandLineHelper.GetExeFileName("c:\\Program Files\\WiseTech Global\\Program.exe"));
			AssertEquals("c:\\Program Files\\WiseTech Global\\Program.exe", CommandLineHelper.GetExeFileName("\"c:\\Program Files\\WiseTech Global\\Program.exe\""));
			AssertEquals("c:\\Program Files\\WiseTech Global\\Program.exe", CommandLineHelper.GetExeFileName("\"c:\\Program Files\\WiseTech Global\\Program.exe\" \"arg1:argument value\" arg2 arg3:value"));

			// Missing closing quote in exe file name
			AssertEquals("c:\\Program Files\\WiseTech Global\\Program.exe", CommandLineHelper.GetExeFileName("\"c:\\Program Files\\WiseTech Global\\Program.exe"));
			AssertEquals("c:\\Program Files\\WiseTech Global\\Program.exe arg1:value arg2 ", CommandLineHelper.GetExeFileName("\"c:\\Program Files\\WiseTech Global\\Program.exe arg1:value arg2 \"arg3:argument value\""));
		}

		public void TestMatchesExeFileName()
		{
			Assert(CommandLineHelper.MatchesExeFileName("\"c:\\Program Files\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "c:\\Program Files\\Program.exe"));
			Assert(CommandLineHelper.MatchesExeFileName("\"c:\\Program Files\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "c:\\program files\\program.exe"));
			Assert(CommandLineHelper.MatchesExeFileName("\"c:\\Program Files\\Program.exe\" \"arg1:argument value\" arg2 arg3:value", "C:\\Program Files\\PROGRAM.exe"));

			Assert(CommandLineHelper.MatchesExeFileName("\"c:\\Program Files\\Program.exe\"", "c:\\Program Files\\Program.exe"));

			Assert("Different file name", !CommandLineHelper.MatchesExeFileName("\"c:\\Program Files\\Program.exe\"", "c:\\Program Files\\Program1.exe"));
			Assert("No quotes results in 'c:\\Program' as file name", !CommandLineHelper.MatchesExeFileName("c:\\Program Files\\Program.exe", "c:\\Program Files\\Program.exe"));
		}
	}
}
