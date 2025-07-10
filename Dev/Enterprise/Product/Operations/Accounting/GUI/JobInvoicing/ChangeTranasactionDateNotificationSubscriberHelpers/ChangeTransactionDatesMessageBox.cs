using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ChangeTransactionDatesMessageBox : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ChangeTransactionDatesMessageBox()
		{
		}

		public ChangeTransactionDatesMessageBox(ChangeTransactionDatesBusinessObject businessObject)
			: base(businessObject)
		{
		}

		public YesNoYesAllNoAllMessageBoxResult YesNoAllResult
		{
			get;
			set;
		}

		public bool ShowYesNoAllButtons
		{
			get;
			set;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("7D652977-7356-4bbe-9A4A-C4D09B9BE743", "Back Date and/or Back Post AR Transactions"); }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (YesNoAllResult == YesNoYesAllNoAllMessageBoxResult.None && ShowYesNoAllButtons)
			{
				e.Cancel = true;
				Globals.Message.ShowInformation(Res.GetString("60912CDD-0949-475f-9A54-A0E46DBC1F6C", "You must choose one of the answers by pressing the buttons at the bottom of the window."));
			}
			else if (DialogResult == DialogResult.Yes ||
						YesNoAllResult == YesNoYesAllNoAllMessageBoxResult.Yes ||
						YesNoAllResult == YesNoYesAllNoAllMessageBoxResult.YesToAll)
			{
				ValidateAll(ValidationType.Light);

				if (BusinessEntity.HasErrors())
				{
					ShowErrorsDialog();
					e.Cancel = true;
				}
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			ControlDpiScalingHelper.SetTop(ref RevenueRecognitionDatesTextBox, zLabel2.Bottom + RevenueRecognitionDatesTextBox.Height, false);
			MainPanel.ClientSize = ControlDpiScalingHelper.NewScaledSize(MainPanel.ClientSize.Width, RevenueRecognitionDatesTextBox.Bottom + RevenueRecognitionDatesTextBox.Height, false);
			int clientWidth = zLabel2.Right + zLabel2.Left;

			if (ShowYesNoAllButtons)
			{
				YesNoCancelPanel.Visible = false;
				AcceptButton = AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.Value ?
								YesButtonOnYesNoAllPanel : NoButtonOnYesNoAllPanel;
				ClientSize = ControlDpiScalingHelper.NewScaledSize(clientWidth, YesNoAllPanel.Bottom, false);
			}
			else
			{
				YesNoAllPanel.Visible = false;
				AcceptButton = AccountingConfigurationRegistry.Instance.DefaultAllowUsersToBackDateInvoicesSetting.Value ?
								YesButtonOnYesNoCancelPanel : NoButtonOnYesNoCancelPanel;
				CancelButton = CancelButtonOnYesNoCancelPanel;
				ClientSize = ControlDpiScalingHelper.NewScaledSize(clientWidth, YesNoCancelPanel.Bottom, false);
			}
		}

		void YesButton_Click(object sender, EventArgs e)
		{
			YesNoAllResult = YesNoYesAllNoAllMessageBoxResult.Yes;
			Close();
		}

		void NoButton_Click(object sender, EventArgs e)
		{
			YesNoAllResult = YesNoYesAllNoAllMessageBoxResult.No;
			Close();
		}

		void YesToAllButton_Click(object sender, EventArgs e)
		{
			YesNoAllResult = YesNoYesAllNoAllMessageBoxResult.YesToAll;
			Close();
		}

		void NoToAllButton_Click(object sender, EventArgs e)
		{
			YesNoAllResult = YesNoYesAllNoAllMessageBoxResult.NoToAll;
			Close();
		}
	}
}
