namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountsExporterARAPTest : AccountsExporterTest
	{
		public void TestAllTransactionsExported()
		{
			AssertEquals("Outgoing same as incoming", true, ((AccountsExporterARAP)Exporter).AllTransactionsExported);
		}
	}
}
