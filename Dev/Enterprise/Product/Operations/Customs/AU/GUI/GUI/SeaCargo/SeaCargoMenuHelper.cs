using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public static class SeaCargoMenuHelper
	{
		public static void ScheduleOriginalSending(CusSCAOceanBill oceanBill, ForwardingConsol consol = null)
		{
			if (oceanBill != null)
			{
				if (oceanBill.HasDeferredScheduledMessageLog && Globals.Message.Show(
							string.Format("Deferred sending of messages has already been scheduled ({0})\r\nDo you wish to reschedule them?", oceanBill.DeferredScheduledDateForDisplayInCanberraTime),
							"Already Scheduled", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
				{
					return;
				}

				var notifications = Customs.Business.MessageSendingValidation.New(consol == null ? oceanBill : consol, null).CheckBusinessObjectLevelValidation();
				if (GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.IsEmpty)
				{
					Globals.Message.Show("Please enter a Customs Registration Number before sending CMR messages", "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (notifications.ErrorCount > 0)
				{
					Globals.Message.Show("There are errors.\r\n\r\n" + notifications.NotificationsAsString(10) + "\r\nPlease fix these errors before scheduling original message sending", "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				if (GetConfirmation(oceanBill, notifications.NotificationsAsString(10)) == DialogResult.No)
				{
					return;
				}

				oceanBill.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
				try
				{
					oceanBill.Factory.Save();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(e);
				}

				Globals.Message.Show(string.Format("Message sending scheduled {0}.", oceanBill.DeferredScheduledMessagesDateTimeString), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			return;
		}

		public static DialogResult GetConfirmation(CusSCAOceanBill oceanBill, string warningMessage)
		{
			bool hasMessageErrors = !string.IsNullOrEmpty(warningMessage);
			string caption = hasMessageErrors ? "Message Errors" : "Message Sending";
			string displaytext = hasMessageErrors ? "There are message errors.\r\n\r\n" + warningMessage + "\r\n\r\n" : string.Empty;
			displaytext += oceanBill.ScheduledMessagesConfirmationText;
			if (displaytext.StartsWith("Original Sea Cargo Reports are within the late cargo reporting time frame"))
			{
				displaytext += "\r\n\r\nDo you wish to continue with sending these messages NOW?";
			}
			else
			{
				displaytext += "\r\n\r\nDo you wish to continue with the background scheduling of messages?";
			}

			return Globals.Message.Show(displaytext, caption, MessageBoxButtons.YesNo, DialogResult.No);
		}
	}
}
