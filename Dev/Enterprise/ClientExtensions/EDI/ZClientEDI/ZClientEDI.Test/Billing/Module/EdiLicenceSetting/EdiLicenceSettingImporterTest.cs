using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class EdiLicenceSettingImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var importer = new EdiLicenceSettingImporter();
			importer.Import();
			Application.DoEvents();
			var form = Application.OpenForms.OfType<MultistepDataImportWizardForm>().Single();
			AssertNotNull(form);
			form.Close();
		}
	}
}