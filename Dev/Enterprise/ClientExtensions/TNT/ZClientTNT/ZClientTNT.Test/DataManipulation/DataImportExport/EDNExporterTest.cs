using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.TNT.Testing
{
	public class EDNExporterTest : TestCaseWithFactory
	{
		public void TestGetDeclarationCusEntryNumbersSelectCommand_NotIsNotLock()
		{
			EDNExporterForTesting exporter = new EDNExporterForTesting();
			string sqlText = exporter.GetDeclarationCusEntryNumbersSelectCommand();
			AssertEquals("StmALog should not have a 'with (nolock)' clause otherwise it will miss records", false, sqlText.ToLower().IndexOf("StmALog with (nolock)".ToLower()) != -1);
		}

		protected const string TestEDN = "Num123";
	}
}
