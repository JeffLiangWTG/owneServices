using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class SendNewSystemShutdownDateForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SendNewSystemShutdownDateForm()
		{
			InitializeComponent();
		}

		public SendNewSystemShutdownDateForm(SystemShutdownDate bo)
			: base(bo)
		{
			InitializeComponent();

			oldVersionLabel.Visible = bo.ParentDatabase != null && bo.ParentDatabase.VersionCanSupportCustomExpiryMessages != Customs.Business.TriState.True;
		}

		internal SystemShutdownDate SystemShutdown
		{
			get { return (SystemShutdownDate)BusinessEntity; }
		}

		void expiredButton_Click(object sender, EventArgs e)
		{
			var list = EDIDataRegistry.Instance.SystemExpiredMessages.Value;
			using (var form = new MessageChooserForm(list))
			{
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					SystemShutdown.ExpiredMessage = form.SelectedText;
				}
			}
		}

		void expiryWeekButton_Click(object sender, EventArgs e)
		{
			var list = EDIDataRegistry.Instance.SystemExpiryWithinWeekMessages.Value;
			using (var form = new MessageChooserForm(list))
			{
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					SystemShutdown.ExpiryWeekMessage = form.SelectedText;
				}
			}
		}

		void expiryMonthButton_Click(object sender, EventArgs e)
		{
			var list = EDIDataRegistry.Instance.SystemExpiryWithinMonthMessages.Value;
			using (var form = new MessageChooserForm(list))
			{
				if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					SystemShutdown.ExpiryMonthMessage = form.SelectedText;
				}
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			ValidateAll(ValidationType.Light);

			if (BusinessEntityForValidation.HasErrors())
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}
	}
}

