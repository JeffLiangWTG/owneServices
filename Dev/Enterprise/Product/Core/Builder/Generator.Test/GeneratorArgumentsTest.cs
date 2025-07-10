using NUnit.Framework;

namespace Enterprise.Builder.Generator
{
	sealed class GeneratorArgumentsTest : TestCase
	{
		public void TestShowGui()
		{
			var arguments = GeneratorArguments.Parse(new[] { ".", "Odyssey" });
			AssertEquals(".", arguments.ServerName);
			AssertEquals("Odyssey", arguments.DatabaseName);
			AssertNull(nameof(arguments.CWSharedPath), arguments.CWSharedPath);
			AssertEquals(nameof(arguments.Option), string.Empty, arguments.Option);
			AssertEquals(nameof(arguments.Argument), string.Empty, arguments.Argument);
			AssertEquals(nameof(arguments.Argument2), string.Empty, arguments.Argument2);
		}

		public void TestSilentRestoreDBWithAdminConnection()
		{
			var arguments = GeneratorArguments.Parse(new[] { ".", "Odyssey", "-SilentRestoreDB", "-NoDataRegen" });
			AssertEquals(".", arguments.ServerName);
			AssertEquals("Odyssey", arguments.DatabaseName);
			AssertEquals(null, arguments.CWSharedPath);
			AssertEquals("-SilentRestoreDB", arguments.Option);
			AssertEquals("-NoDataRegen", arguments.Argument);
		}

		public void TestRegen()
		{
			var arguments = GeneratorArguments.Parse(new[] { ".", "Odyssey", "-SilentAutoRegen", "-Checkout", @"-cwShared=d:\CWShared" });
			AssertEquals(".", arguments.ServerName);
			AssertEquals("Odyssey", arguments.DatabaseName);
			AssertEquals(@"d:\CWShared", arguments.CWSharedPath);
			AssertEquals("-SilentAutoRegen", arguments.Option);
			AssertEquals("-Checkout", arguments.Argument);
			AssertEquals("", arguments.Argument2);
		}
	}
}
