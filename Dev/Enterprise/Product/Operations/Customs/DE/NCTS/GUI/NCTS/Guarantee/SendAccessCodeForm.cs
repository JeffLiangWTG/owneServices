using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public partial class SendAccessCodeForm : ZChildForm
	{
		public SendAccessCodeForm(SendAccessCodeViewModel viewModel)
		: base(viewModel)
		{
			InitializeComponent();

			sendAccessCodeUserControl.SendButton.Click += SendButtonOnClick;
			sendAccessCodeUserControl.CancelButton.Click += CancelButtonOnClick;
		}

		public new SendAccessCodeViewModel BusinessEntity => (SendAccessCodeViewModel)base.BusinessEntity;

		void SendButtonOnClick(object sender, EventArgs e)
		{
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				Close();
			}
		}

		protected override void SaveInternal()
		{
			if (BusinessEntity.Send())
			{
				Globals.Message.Show(Res.GetString("61dbd604-bf7a-4eba-98b4-6d0b3ce18760", "The message has been sent."));
			}
		}

		void CancelButtonOnClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
