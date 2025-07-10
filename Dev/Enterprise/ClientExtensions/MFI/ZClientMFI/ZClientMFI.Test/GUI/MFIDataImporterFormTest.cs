using System.Windows.Forms;
using Enterprise.Billing.Integration;
using Enterprise.Client.MFI.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Test.GUI
{
	[TestedType(typeof(MFIDataImporterForm))]
	public class MFIDataImporterFormTest : ZFormBasherTest
	{
		public void TestImportFileFilter()
		{
			using (MFIDataImporterForm form = MFIDataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				AssertEquals("Filter should be 'All (*.*)|*.*'", "All (*.*)|*.*", form.InternalImportFileFilterTest);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MFIDataImporterForm();
		}
	}
}
