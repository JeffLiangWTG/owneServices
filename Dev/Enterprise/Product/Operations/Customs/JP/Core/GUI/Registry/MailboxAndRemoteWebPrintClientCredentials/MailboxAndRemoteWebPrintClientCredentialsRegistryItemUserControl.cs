using Enterprise.Registry.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl : RegistryZUserControl
	{
		public MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LocalComputerAliasTextBox.ReadOnly = readOnly;
			DomainNameTextBox.ReadOnly = readOnly;
			ReceivingIntervalIntEdit.ReadOnly = readOnly;
			SendingIntervalIntEdit.ReadOnly = readOnly;
			StatusDropEdit.ReadOnly = readOnly;
			VerboseCheckBox.ReadOnly = readOnly;
			FailureNotificationGroupFindbox.ReadOnly = readOnly;
			DownTimeStartDateEdit.ReadOnly = readOnly;
			DownTimeEndDateEdit.ReadOnly = readOnly;
		}
	}
}
