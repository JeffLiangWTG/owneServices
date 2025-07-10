using System.Collections;
using System.IO;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CommandLineArgumentsTest : TestCase
	{
		public void TestCommandLineFromFile()
		{
			string fileName = EnvProxy.Instance.GetTempFileName();

			try
			{
				using (StreamWriter writer = File.CreateText(fileName))
				{
					writer.WriteLine("-AutoLogin:henry");
					writer.WriteLine("-Branch:BNE");
				}

				Hashtable possibleOptions = new Hashtable();
				possibleOptions.Add("-AutoLogin:", false);
				possibleOptions.Add("-Branch:", false);

				CommandLineArguments testArguments = new CommandLineArguments(fileName, possibleOptions);
				AssertEquals("Option", "henry", possibleOptions["-AutoLogin:"].ToString());
				AssertEquals("Option", "BNE", possibleOptions["-Branch:"].ToString());
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		public void TestEmptyArguments()
		{
			string[] args = System.Array.Empty<string>();
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ServerName", null, testArguments.ServerName);
			AssertEquals("DatabaseName", null, testArguments.DatabaseName);
			AssertEquals("ModuleName", "", testArguments.ModuleName);
			AssertEquals("ShowLogin", false, TestOptions["-ShowLogin"]);
		}

		public void TestAllArguments()
		{
			string[] args = new string[] { "hy213", "TestOdyssey", "Ratings" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ServerName", "hy213", testArguments.ServerName);
			AssertEquals("DatabaseName", "TestOdyssey", testArguments.DatabaseName);
			AssertEquals("ModuleName", "Ratings", testArguments.ModuleName);
			AssertEquals("ShowLogin", false, TestOptions["-ShowLogin"]);
		}

		public void TestShowLogin()
		{
			string[] args = new string[] { "-ShowLogin", "hy213", "TestOdyssey", "Ratings" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ShowLogin", true, TestOptions["-ShowLogin"]);
			AssertEquals("ServerName", "hy213", testArguments.ServerName);
			AssertEquals("DatabaseName", "TestOdyssey", testArguments.DatabaseName);
			AssertEquals("ModuleName", "Ratings", testArguments.ModuleName);
		}

		public void TestShowLoginDefaults()
		{
			string[] args = new string[] { "-ShowLogin", "hy213" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ShowLogin", true, TestOptions["-ShowLogin"]);
			AssertEquals("ServerName", "hy213", testArguments.ServerName);
			AssertEquals("DatabaseName", null, testArguments.DatabaseName);
			AssertEquals("ModuleName", "", testArguments.ModuleName);
		}

		public void TestAutoTest()
		{
			string[] args = new string[] { "-AutoTest" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ShowLogin", false, TestOptions["-ShowLogin"]);
			AssertEquals("AutoTest", true, TestOptions["-AutoTest"]);
		}

		public void TestOpenDocumentScanningForm()
		{
			string[] args = new string[] { "-OpenDocScan" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ShowLogin", false, TestOptions["-ShowLogin"]);
			AssertEquals("OpenDocumentScanningForm", true, TestOptions["-OpenDocScan"]);
		}

		public void TestComposite()
		{
			string[] args = new string[] { "-AutoTest", "-ShowLogin", "hy213", "TestOdyssey", "Ratings" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ShowLogin", true, TestOptions["-ShowLogin"]);
			AssertEquals("ServerName", "hy213", testArguments.ServerName);
			AssertEquals("DatabaseName", "TestOdyssey", testArguments.DatabaseName);
			AssertEquals("ModuleName", "Ratings", testArguments.ModuleName);
			AssertEquals("AutoTest", true, TestOptions["-AutoTest"]);
			AssertEquals("OpenDocScan", false, TestOptions["-OpenDocScan"]);
		}

		public void TestOptionWithValuePart()
		{
			string[] args = new string[] { "-OpenDocMaintain:1234" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);
			AssertEquals("ShowLogin", false, TestOptions["-ShowLogin"]);

			object foundValuePartObject = TestOptions["-OpenDocMaintain:"];
			Assert("Type of found value", foundValuePartObject is string);

			string foundValuePartString = (string)foundValuePartObject;
			AssertEquals("Found value", foundValuePartString, "1234");
		}

		public void TestOptionWithValuePartNotUsed()
		{
			string[] args = new string[] { "-ShowLogin" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);

			object foundValuePartObject = TestOptions["-OpenDocMaintain:"];
			Assert("Type of found value", foundValuePartObject is bool);

			bool foundValuePartBool = (bool)foundValuePartObject;
			AssertEquals("Found value", false, foundValuePartBool);
		}

		public void TestOptionWithValuePartEmptyString()
		{
			string[] args = new string[] { "-OpenDocMaintain:" };
			CommandLineArguments testArguments = new CommandLineArguments(args, TestOptions);

			object foundValuePartObject = TestOptions["-OpenDocMaintain:"];
			Assert("Type of found value", foundValuePartObject is string);

			string foundValuePartString = (string)foundValuePartObject;
			AssertEquals("Found value", "", foundValuePartString);
		}

		Hashtable TestOptions;

		protected override void SetUp()
		{
			base.SetUp();
			TestOptions = new Hashtable();
			TestOptions.Add("-ShowLogin", false);
			TestOptions.Add("-AutoTest", false);
			TestOptions.Add("-OpenDocScan", false);
			TestOptions.Add("-OpenDocMaintain:", false);
		}
	}
}
