using System;
using NUnit.Framework;

namespace Enterprise.Builder.GenerateDbUpgraderResources.Testing
{
	sealed class CommandLineArgsTest : TestCase
	{
		public void TestParse()
		{
			var args = CommandLineArgs.ParseCommandLineArguments(new[] { "-Merge", "-Checkout", @"-CWShared=D:\CWShared", "-NoGui" });
			AssertNotNull(args);
			AssertEquals("-MERGE", args.SetupAction);
			AssertEquals("-CHECKOUT", args.SourceSafeAction);
			AssertEquals(@"D:\CWShared", args.CWShared);
			AssertEquals(false, args.ShowGui);
		}

		public void TestEmpty()
		{
			var args = CommandLineArgs.ParseCommandLineArguments(Array.Empty<string>());
			AssertNotNull(args);
			AssertEquals(string.Empty, args.SetupAction);
			AssertEquals(string.Empty, args.SourceSafeAction);
			AssertEquals(string.Empty, args.CWShared);
			AssertEquals(true, args.ShowGui);
		}

		public void TestParseUnknown()
		{
			var args = CommandLineArgs.ParseCommandLineArguments(new[] { "-Merge", "-Checkout", @"-CWShared=D:\CWShared", "-NoGui", "-Mistory" });
			AssertNull(args);
		}
	}
}
