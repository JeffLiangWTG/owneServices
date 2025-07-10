using System.Text;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class KeyAppendStrategyTest : TestCaseWithXmlDoc
	{
		public void TestAppendMatch()
		{
			KeyAppendStrategy appendStrategy = new KeyAppendStrategy();
			StringBuilder builder = new StringBuilder();

			appendStrategy.AppendMatch(line1, builder);
			AssertEquals(tidy1 + System.Environment.NewLine, builder.ToString());

			appendStrategy.AppendMatch(line2, builder);
			AssertEquals(string.Join(System.Environment.NewLine, new string[] { tidy1, tidy2 }) + System.Environment.NewLine, builder.ToString());

			appendStrategy.AppendMatch(line3, builder);
			AssertEquals(string.Join(System.Environment.NewLine, new string[] { tidy1, tidy2, tidy3 }) + System.Environment.NewLine, builder.ToString());

			appendStrategy.AppendMatch(line4, builder);
			AssertEquals(string.Join(System.Environment.NewLine, new string[] { tidy1, tidy2, tidy3, tidy4 }) + System.Environment.NewLine, builder.ToString());
		}

		const string line1 = @"at Enterprise.Registry.Business.AccountingDataRegistry.get_ProfitShareChargeCode() in Q:\Enterprise\Registry.Business\Accounting\AccountingDataRegistry.cs:line 705";
		const string line2 = @"at Enterprise.Registry.Business.AccountingDataRegistry.AddRegistryItems() in Q:\Enterprise\Registry.Business\Accounting\AccountingDataRegistry.cs:line 67";
		const string line3 = @"at Enterprise.Registry.Business.AccountingDataRegistry..ctor() in Q:\Enterprise\Registry.Business\Accounting\AccountingDataRegistry.cs:line 28";
		const string line4 = @"at Enterprise.Registry.Business.AccountingDataRegistry.get_Instance() in Q:\Enterprise\Registry.Business\Accounting\AccountingDataRegistry.cs:line 19";

		const string tidy1 = @"at Enterprise.Registry.Business.AccountingDataRegistry.get_ProfitShareChargeCode(";
		const string tidy2 = @"at Enterprise.Registry.Business.AccountingDataRegistry.AddRegistryItems(";
		const string tidy3 = @"at Enterprise.Registry.Business.AccountingDataRegistry..ctor(";
		const string tidy4 = @"at Enterprise.Registry.Business.AccountingDataRegistry.get_Instance(";

		public void TestAppendMatchRemovesLines()
		{
			KeyAppendStrategy appendStrategy = new KeyAppendStrategy();
			StringBuilder builder = new StringBuilder();

			appendStrategy.AppendMatch(@"----- Exception caught and reported here -----", builder);

			AssertEquals(string.Empty, builder.ToString());

			appendStrategy.AppendMatch(@"at WTG.ErrorReporting.NLog.ErrorReportingNLogTarget.Write(", builder);

			AssertEquals(string.Empty, builder.ToString());
		}
	}
}
