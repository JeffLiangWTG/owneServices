using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public partial class CLSMSMessageSendingRegistryItemUserControl : RegistryZUserControl
	{
		public CLSMSMessageSendingRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MachineNameTextBox.ReadOnly
				= ApplicationNodeNameTextBox.ReadOnly
				= ApplicationNodePasswordTextBox.ReadOnly
				= EnableSMSMessageSendingCheckBox.ReadOnly
				= XtCredentialStatusDropEdit.ReadOnly
				= RunningIntervalInSecondsCalcEdit.ReadOnly
				= SendFolderTextBox.ReadOnly
				= UnknownFolderTextBox.ReadOnly
				= InvalidFolderTextBox.ReadOnly
				= RejectedFolderTextBox.ReadOnly
				= ReceiveFolderTextBox.ReadOnly
				= AcceptedFolderTextBox.ReadOnly
				= readOnly;
		}
	}
}
