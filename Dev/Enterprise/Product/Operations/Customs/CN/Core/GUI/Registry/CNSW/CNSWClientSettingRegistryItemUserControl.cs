using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNSWClientSettingRegistryItemUserControl : RegistryZUserControl
	{
		public CNSWClientSettingRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MachineNameTextBox.ReadOnly = SendFolderTextBox.ReadOnly = ReceiveFolderTextBox.ReadOnly = ErrorResponseFolderTextBox.ReadOnly = ArchiveFolderTextBox.ReadOnly = AcdaSendFolderTextBox.ReadOnly = AcdaReceiveFolderTextBox.ReadOnly = AcdaErrorResponseFolderTextBox.ReadOnly = AcdaArchiveFolderTextBox.ReadOnly = RunningIntervalInSecondsCalcEdit.ReadOnly = readOnly;
		}
	}
}
