using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class EFTPaymentForm : ZChildForm
	{
		public bool IsOKToSend;

		public override string FormCaption
		{
			get { return "Send payment messages (Customs Charge & Quarantine Service Charge)"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public EFTPaymentForm(EFTPaymentInformationCollection eFTPaymentInfos) : base(eFTPaymentInfos)
		{
			this.eFTPaymentInfos = eFTPaymentInfos;
			SetupControlsVisibility();
		}

		readonly EFTPaymentInformationCollection eFTPaymentInfos;

		void SetupControlsVisibility()
		{
			var declaration = eFTPaymentInfos.Declaration;
			var queuedEntryPaymentsEnabled = declaration.IsQueuedEntryPaymentsEnabled;

			scheduledPaymentDate.Visible = queuedEntryPaymentsEnabled;
			if (queuedEntryPaymentsEnabled)
			{
				var isQueuedPayment = declaration.IsQueuedEntryPayment;
				scheduledPaymentDate.ReadOnly = isQueuedPayment;
				oKBoundButton.Enabled = !isQueuedPayment;
			}
		}

		internal void OKBoundButton_Click(object sender, System.EventArgs e)
		{
			if (!eFTPaymentInfos.HasAmountsToPay)
			{
				IsOKToSend = false;
				Globals.Message.ShowInformation("There are no amounts to pay and no payment message will be sent.");
			}
			else
			{
				IsOKToSend = true;

				if (!scheduledPaymentDate.DateTimeValue.IsEmpty && scheduledPaymentDate.DateTimeValue.IsValid)
				{
					if (scheduledPaymentDate.DateTimeValue < ZDateTime.Now)
					{
						IsOKToSend = Globals.Message.Show("Scheduled date is earlier than the current date so the payment message will be sent immediately, do you want to continue?", "Valid Payment Scheduled Date", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
					}
					else
					{
						IsOKToSend = Globals.Message.Show("Payment will be scheduled for " + scheduledPaymentDate.DateTimeValue + ", do you want to continue?", "Message Payment Scheduled", MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
					}
				}

				if (IsOKToSend)
				{
					var messageErrorsInPayment = new List<INotification>();

					foreach (var paymentInfo in eFTPaymentInfos)
					{
						paymentInfo.RunPreSaveValidation();

						if (paymentInfo.HasMessageErrors)
						{
							var messageErrors = new ZNotificationCollector(paymentInfo, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors();
							messageErrorsInPayment.AddRange(messageErrors);
						}
					}

					if (messageErrorsInPayment.Count > 0)
					{
						IsOKToSend = GetConfirmationWithMessageErrorNotification(messageErrorsInPayment.ToUniqueMessageListString()) == DialogResult.Yes;
					}
				}

				if (IsOKToSend)
				{
					Close();
				}
			}
		}

		DialogResult GetConfirmationWithMessageErrorNotification(string messageError)
		{
			return Globals.Message.Show("There are message errors.\r\n\r\n" + messageError + "\r\n\r\nAre you sure you wish to continue?", "Message Errors", MessageBoxButtons.YesNo, DialogResult.No);
		}

		void CancelBoundButton_Click(object sender, System.EventArgs e)
		{
			IsOKToSend = false;
			Close();
		}
	}
}
