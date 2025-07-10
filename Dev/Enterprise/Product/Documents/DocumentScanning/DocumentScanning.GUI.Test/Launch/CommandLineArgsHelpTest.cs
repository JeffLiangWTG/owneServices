using System.Collections;
using System.Collections.Specialized;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Launch.Testing
{
	sealed class CommandLineArgsHelpTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			fHelper = new CommandLineArgsHelp();

			Hashtable options = new Hashtable();
			options.Add("-OptionA", false);
			options.Add("-optionB:", false);
			options.Add("-oPtK", false);
			options.Add("-OptionC", false);

			string[] args = new string[] { "MyServer", "MyDB", "-OptionA", "-optionB:123", "-oPtK" };
			CommandLineArguments cmdLineArguments = new CommandLineArguments(args, options);
			fHelper.ReadInArguments(cmdLineArguments);
		}

		CommandLineArgsHelp fHelper;

		protected override void TearDown()
		{
			fHelper = null;
			base.TearDown();
		}

		public void TestReadInArguments()
		{
			AssertEquals("Server", "MyServer", fHelper.CmdLineArguments.ServerName);
			AssertEquals("Server", "MyDB", fHelper.CmdLineArguments.DatabaseName);
		}

		public void TestIsOption()
		{
			AssertEquals("IsOption", true, fHelper.IsOption("OptionA"));
		}

		public void TestGetOptionValue()
		{
			AssertEquals("Option value", "123", fHelper.GetOptionValue("optionB"));
		}

		public void TestMandatoryArgs()
		{
			StringCollection found = fHelper.MandatoryArgs();
			AssertEquals("Count", found.Count, 2);
			AssertEquals("Server", "MyServer", found[0]);
			AssertEquals("Server", "MyDB", found[1]);
		}

		bool MatchFound(string[] strings, string toMatch, int startIndex)
		{
			for (int ii = startIndex; ii < strings.Length; ii++)
			{
				if (strings[ii] == toMatch)
				{
					return true;
				}
			}
			return false;
		}

		public void TestCmdLineArgsAsLine()
		{
			string[] found = fHelper.CmdLineArgsAsLine();
			AssertEquals("Count", found.Length, 5);
			AssertEquals("Server", "MyServer", found[0]);
			AssertEquals("DB", "MyDB", found[1]);

			Assert("Opt1", MatchFound(found, "-OptionA", 2));
			Assert("Opt2", MatchFound(found, "-optionB:123", 2));
			Assert("Opt3", MatchFound(found, "-oPtK", 2));

			Assert("Opt4", !MatchFound(found, "-OptionC", 2));
		}
	}
}
