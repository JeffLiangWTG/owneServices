using System.Windows.Forms;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(CNDataInterfaceGUI))]
	public class CNDataInterfaceGUIFormGBT24589_1Test : ZFormBasherTest
	{
		public void TestFormVerbAndCaption()
		{
			using (CNDataInterfaceGUI form = new CNDataInterfaceGUI(new ChinaStandard2010DataInterfaceWrapper(Factory)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Form verb", "", form.FormVerb);
				AssertEquals("Form caption", "China Standard GB-T 24589.1", form.FormCaption);
			}
		}

		public void TestDirectoryPopUpButton_Click()
		{
			ChinaStandard2010DataInterfaceWrapper bizObj = new ChinaStandard2010DataInterfaceWrapper(Factory);
			using (CNDataInterfaceGUI form = new CNDataInterfaceGUI(bizObj))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.PathToSelectInShowCommonDialog = bizObj.ExportDirectory;
				form.DirectoryPopUpButton_Click(null, null);

				CommonDialog dialog = ZFormModaliser.LastCommonDialogShownDialogForTest;
				AssertEquals("Dialog Type", typeof(FolderBrowserDialog), dialog.GetType());
				AssertEquals("Selected Path", bizObj.ExportDirectory, ((FolderBrowserDialog)dialog).SelectedPath);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new CNDataInterfaceGUI(new ChinaStandard2010DataInterfaceWrapper(Factory));
		}

		#endregion
	}
}
