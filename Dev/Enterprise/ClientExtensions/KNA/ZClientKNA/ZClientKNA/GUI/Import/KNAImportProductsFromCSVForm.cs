using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.KNA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.KNA.GUI
{
	internal class KNAImportProductsFromCSVForm : ImportProductsFromCSVForm
	{
		protected override DataLoad GetNewDataLoader()
		{
			return new KNAOrgSupplierPartDataLoader();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			try
			{
				((KNAOrgSupplierPartDataLoader)dataLoader).ImportProductData(dataToLoad, UpdateYesRadioButton.Checked, LegacyCodesYesRadioButton.Checked);
			}
			catch (System.NotSupportedException)
			{
				Globals.Message.Show(Res.GetString("beb92821-d4f2-418f-b892-449a9dfd93f1", "Product data import from KNA CSV file has not been implemented for your country."));
			}
		}
		#region TestInternals
		internal ZTextBox InternalFileNameTextBoxTest => FileNameTextBox;
		internal ZButton InternalStartButtonTest => StartButton;
		internal ZStatusBar InternalMainStatusBarTest => MainStatusBar;
		internal ZButton InternalCopyLogToClipboardButtonTest => CopyLogToClipboardButton;
		internal ZButton InternalCloseButtonTest => CloseButton;
		internal KListBox InternalOutputListBoxTest => OutputListBox;
		internal KProgressBar InternalProgressBarTest => ProgressBar;
		internal string InternalGetLogTest() => GetLog();
		internal string InternalCreateLogInDataDirectoryTest(string logData) => CreateLogInDataDirectory(logData);
		#endregion
	}
}
