using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(ImportConfigurationForm))]
	sealed class ImportConfigurationFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ImportConfigurationForm(new FileImporter(new DocumentFactoryProvider().GetFactory(Factory), false));
		}

		[RequiresSTA]
		public void TestLabelsIsUpdatedProperly()
		{
			var importer = new FileImporter(new DocumentFactoryProvider().GetFactory(Factory), false);
			importer.IsUsingCoverSheet = true;
			using (var form = new ImportConfigurationForm(importer))
			{
				var useCoverSheetCheckBox = (ZCheckBox)form.Controls.Find("UseCoverSheetCheckBox", true)[0];
				var autoSingleRadioButton = (ZRadioButton)form.Controls.Find("AutoSingleRadioButton", true)[0];
				var autoRadioButton = (ZRadioButton)form.Controls.Find("AutoRadioButton", true)[0];

				form.Show();

				Assert(useCoverSheetCheckBox.Checked);
				AssertEquals(NoResourceStringData.GetData("Process as specified by Barcodes and create a new eDoc for each page").Caption, autoSingleRadioButton.Text);
				AssertEquals(NoResourceStringData.GetData("Process as specified by Barcodes").Caption, autoRadioButton.Text);

				useCoverSheetCheckBox.Checked = false;
				LabelCaptionTestHelper.FireApplicationIdle(); //trigger the caption refresh 
				AssertEquals(NoResourceStringData.GetData("Process as specified by User or Barcodes, and create a new eDoc for each page").Caption, autoSingleRadioButton.Text);
				AssertEquals(NoResourceStringData.GetData("Process as specified by User or Barcodes").Caption, autoRadioButton.Text);
			}
		}
	}
}
