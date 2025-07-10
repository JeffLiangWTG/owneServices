using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Testing
{
	public class MatchedTransactionsImporterTest : TestCase
	{
		public void TestImporterForm()
		{
			MatchedTransactionsImporter importer = new MatchedTransactionsImporter();
			using (importer.ImporterForm)
			{
				AssertNotNull(importer.ImporterForm);
				AssertEquals("Matched transaction files (*.out)|*.out|Text files (*.txt)|*.txt|All files (*.*)|*.*", importer.ImporterForm.DialogFilter);
				AssertEquals("Import Matched Transactions", importer.ImporterForm.FormHeading);
			}
		}
	}
}
