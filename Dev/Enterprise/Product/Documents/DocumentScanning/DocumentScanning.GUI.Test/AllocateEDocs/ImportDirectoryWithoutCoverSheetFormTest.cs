using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI
{
	[TestedType(typeof(ImportDirectoryWithoutCoverSheetForm))]
	sealed class ImportDirectoryWithoutCoverSheetFormTest : ImportDirectoryFormTest
	{
		protected override Form GetFormToBashCore()
		{
			AssemblyDataLookup.ClearDataForTesting();
			return new ImportDirectoryWithoutCoverSheetForm(new FileImporter(new DocumentFactoryProvider().GetFactory(Factory), true));
		}

		public void TestDocTypeDropDownEditValueTooLong()
		{
			using (var importDirectoryWithoutCoverSheetForm = GetFormToBashCore())
			{
				importDirectoryWithoutCoverSheetForm.Show();
				Application.DoEvents();
				var docTypeDropDownEdit = (ZDropEdit)importDirectoryWithoutCoverSheetForm.Controls.Find("DocTypeDropDownEdit", false)[0];
				AssertEquals(4, docTypeDropDownEdit.CodeBox.MaxLength);
			}
		}
	}
}
