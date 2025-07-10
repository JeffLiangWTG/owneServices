using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class OrderRejectReasonForm : ZChildForm
	{
		public OrderRejectReasonForm()
		{ }

		public OrderRejectReasonForm(RejectOrderReasonHolder rejectHolder, string reasonTextBoxCaption, string formCaption)
			: base(rejectHolder)
		{
			InitializeComponent();

			this.rejectHolder = rejectHolder;

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(ReasonTextBox);
			TypeDescriptor.AddAttributes(ReasonTextBox, new SuppressFormsLocalizedTestAttribute());
			MissingResourceStringChecker.ExcludeFromTest(ReasonLabel);
			TypeDescriptor.AddAttributes(ReasonLabel, new SuppressFormsLocalizedTestAttribute());
#endif

			this.Text = formCaption;
			this.ReasonLabel.GetExtension<ILabelCaptionRenderer>().Caption = reasonTextBoxCaption + ":";
		}

		readonly RejectOrderReasonHolder rejectHolder;

		void OKReasonButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(RejectReasonCodeDropEdit.CodeBox.Text) || !rejectHolder.OrderRejectReasonCodes_List.ContainsCode(RejectReasonCodeDropEdit.CodeBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("1ee556d4-da9f-4643-8ce0-29196e69eeee", "Enter a valid reason code"));
			}
			else if (string.IsNullOrEmpty(ReasonTextBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("f516f85f-e99e-4edc-8a82-d08ed8f118c5", "Please enter a non-blank reason text!"));
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelReasonButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
			{
				rejectHolder.Reason = string.Empty;
			}
			base.OnFormClosing(e);
		}
	}
}
