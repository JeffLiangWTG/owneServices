using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.ARAP
{
	public sealed partial class PaymentRejectionReasonForm : ZChildForm
	{
		public PaymentRejectionReasonForm(PaymentRejectionReasonHolder reversingHolder, ZString[] payments)
			: base(reversingHolder)
		{
			this.reversingHolder = reversingHolder;
			InitializeComponent();
			Text = Res.GetString("855B2147-6B8A-47DE-8ED6-4D2FEA86ACA5", "Payment Rejection");

			var paymentsText = string.Join(System.Environment.NewLine, payments);

			var bottomText = payments.Length > 1
				? Res.GetString("33062C87-272F-4908-AFB1-E1B19128E4A4", "Please enter the reason for rejecting these payments:")
				: Res.GetString("0B1606BF-B3FB-4F1B-9F6F-238CA1E953FE", "Please enter the reason for rejecting this payment:");

			paymentsLabel.GetExtension<ILabelCaptionRenderer>().Caption = paymentsText;
			bottomLabel.GetExtension<ILabelCaptionRenderer>().Caption = bottomText;

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(paymentsLabel);
			TypeDescriptor.AddAttributes(paymentsLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		readonly PaymentRejectionReasonHolder reversingHolder;

		void OKReasonButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ReversingReasonCodeDropEdit.CodeBox.Text) || !reversingHolder.ReasonCodesList.ContainsCode(ReversingReasonCodeDropEdit.CodeBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("697A9D7B-64EF-4289-BF1E-6BD164E6B9DF", "Please choose a valid reason code"));
			}
			else if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("E11B64DE-0A3C-4B26-B3A1-FD494798B89A", "Please enter a non-blank reason text"));
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		public ZLabel PaymentsLabel => paymentsLabel;

		public void SetReason(string code, string reason)
		{
			reversingHolder.Code = code;
			reversingHolder.Reason = reason;
		}

		void CancelReasonButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
