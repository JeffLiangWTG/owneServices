using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class TransactionNumberSettingForm : ZChildForm
	{
		public TransactionNumberSettingForm(TransactionNumberSettingBO transactionNumberSettingsBO)
			: base(transactionNumberSettingsBO)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void AddAdornments()
		{
			base.AddAdornments();
			ZFormMenuStrategy.AddAdornments(this);
		}

		void MoveToExistingButton_Click(object sender, EventArgs e)
		{
			TransactionNumberSettingBO settingBO = (TransactionNumberSettingBO)DataSource;
			var transactionNumberSetting = (TransactionNumberSetting)BindingContext[DataSource, nameof(TransactionNumberSettingBO.AvailableTransactionNumberSettingCollection)].GetCurrent();
			if (transactionNumberSetting == null)
			{
				return;
			}
			settingBO.AvailableTransactionNumberSettingCollection.Remove(transactionNumberSetting);
		}

		void MoveToAvailableButton_Click(object sender, EventArgs e)
		{
			TransactionNumberSettingBO settingBO = (TransactionNumberSettingBO)DataSource;
			var transactionNumberSetting = (TransactionNumberSetting)BindingContext[DataSource, nameof(TransactionNumberSettingBO.ExistingTransactionNumberSettingCollection)].GetCurrent();
			if (transactionNumberSetting == null)
			{
				return;
			}
			if (!transactionNumberSetting.CanDelete)
			{
				Globals.Message.ShowError(transactionNumberSetting.ReasonForNotAbleToDelete);
				return;
			}
			settingBO.ExistingTransactionNumberSettingCollection.Remove(transactionNumberSetting);
		}
	}
}
