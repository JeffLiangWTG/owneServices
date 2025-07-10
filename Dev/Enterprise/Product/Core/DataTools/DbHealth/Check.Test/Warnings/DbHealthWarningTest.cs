using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	public abstract class DbHealthWarningTest : TestCase
	{
		public void TestInstatiation()
		{
			DbHealthWarning testWarning = GetObject("S0", "W0", "D0", "A0");
			AssertEquals("Source Type", DbSouceType, testWarning.SourceType);
			AssertEquals("Source", "S0", testWarning.Source);
			AssertEquals("Warning Type", "W0", testWarning.WarningType);
			AssertEquals("Description", "D0", testWarning.Description);
			AssertEquals("Action", "A0", testWarning.Action);
		}

		protected abstract DbHealthWarning GetObject(string source, string warningType, string description, string action);

		protected abstract string DbSouceType { get; }
	}
}
