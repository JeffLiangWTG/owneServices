using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class ClientLicenceFeeImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var importer = new ClientLicenceFeeImporter();
			importer.Import();
			Application.DoEvents();
			var form = Application.OpenForms.OfType<MultistepDataImportWizardForm>().Single();
			AssertNotNull(form);
			form.Close();
		}
	}
}