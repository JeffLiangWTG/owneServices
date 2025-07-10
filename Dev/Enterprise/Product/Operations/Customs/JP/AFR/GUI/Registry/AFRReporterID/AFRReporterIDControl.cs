using System;
using Enterprise.Core.Forms;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class AFRReporterIDControl : RegistryZUserControl
	{
		public AFRReporterIDControl()
		{
			InitializeComponent();
			ViewButton.Visible = Env.CurrentUser.IsSupportUser;
		}

		public new AFRReporterID CurrentDataItem => (AFRReporterID)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			SetBusinessEntityReadOnly();
		}

		void SetBusinessEntityReadOnly()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ReadOnly = ReadOnly;
			}
		}

		void ViewButton_Click(object sender, EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			if (DeveloperLoginForm.TryAuthenticate())
			{
				Globals.Message.ShowInformation(PasswordTextBox.Text, ResString.GetMultilingualString("89806CCF-82FF-4912-821B-3F4A0E07A002", "Password"));
			}
		}
	}
}
