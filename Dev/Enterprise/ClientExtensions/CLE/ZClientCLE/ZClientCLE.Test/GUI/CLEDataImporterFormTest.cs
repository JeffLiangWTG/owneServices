using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.CLE
{
	[TestedType(typeof(CLEDataImporterForm))]
	public class CLEDataImporterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CLEDataImporterForm();
		}

		public void TestImportFileFilter()
		{
			using (CLEDataImporterForm form = new CLEDataImporterForm())
			{
				AssertEquals("Import file filter", "Csv files (*.csv)|*.csv", form.InternalImportFileFilter);
			}
		}
	}
}
