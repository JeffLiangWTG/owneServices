using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT19581_2004.Testing
{
	[TestedType(typeof(CN2004DataInterfaceGUI))]
	public class CNDataInterfaceGUIFormGBT19581_2004Test : ZFormBasherTest
	{
		public void TestFormVerbAndCaption()
		{
			using (CN2004DataInterfaceGUI form = new CN2004DataInterfaceGUI(new ChinaStandard2004DataInterfaceWrapper(Factory)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Form verb", "", form.FormVerb);
				AssertEquals("Form caption", "China Standard GB-T 19851-2004", form.FormCaption);
			}
		}

		public void TestDirectoryPopUpButton_Click()
		{
			ChinaStandard2004DataInterfaceWrapper bizObj = new ChinaStandard2004DataInterfaceWrapper(Factory);
			using (CN2004DataInterfaceGUI form = new CN2004DataInterfaceGUI(bizObj))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.PathToSelectInShowCommonDialog = bizObj.ExportDirectory;
				form.DirectoryPopUpButton_Click(null, null);

				CommonDialog dialog = ZFormModaliser.LastCommonDialogShownDialogForTest;
				AssertEquals("Dialog Type", typeof(FolderBrowserDialog), dialog.GetType());
				AssertEquals("Selected Path", bizObj.ExportDirectory, ((FolderBrowserDialog)dialog).SelectedPath);
			}
		}

		public void TestDirectoryOrDeliveryComponentsHidden_WhenSwitchExportTXTOrXML()
		{
			ChinaStandard2004DataInterfaceWrapper bizObj = new ChinaStandard2004DataInterfaceWrapper(Factory);
			bizObj.ExportTXTOrXML = BizObjThatDoesntSaveForCN2004.FilesType.TXT;
			using (CN2004DataInterfaceGUI form = new CN2004DataInterfaceGUI(bizObj))
			{
				form.Show();
				Application.DoEvents();

				var deliveryTextBox = (ZTextBox)form.Controls.Find("DeliveryToTextBox", true)[0];
				var directoryTextBox = (ZTextBox)form.Controls.Find("DirectoryTextBox", true)[0];

				AssertEquals(true, deliveryTextBox.Visible);
				AssertEquals(false, form.DirectoryPopUpButton.Visible);
				AssertEquals(false, directoryTextBox.Visible);

				var exportXMLRadioButton = (ZRadioButton)form.Controls.Find("ExportXMLRadioButton", true)[0];
				exportXMLRadioButton.Checked = true;
				Application.DoEvents();

				AssertEquals(false, deliveryTextBox.Visible);
				AssertEquals(true, form.DirectoryPopUpButton.Visible);
				AssertEquals(true, directoryTextBox.Visible);
			}
		}

		public override void TestHasChangesOnPreviouslySavedObject()
		{
			base.TestHasChangesOnPreviouslySavedObject();
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CN2004DataInterfaceGUI(new ChinaStandard2004DataInterfaceWrapper(Factory));
		}

		#endregion
	}
}
