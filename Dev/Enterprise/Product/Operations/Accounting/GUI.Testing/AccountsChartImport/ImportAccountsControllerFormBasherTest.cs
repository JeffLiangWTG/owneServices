using System.Windows.Forms;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ImportAccountsControllerForm.Testing
{
	[TestedType(typeof(ImportAccountsControllerForm))]
	public class ImportAccountsControllerFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ImportAccountsControllerForm(new AccountsImportBusinessObject(Factory));
		}
	}
}
