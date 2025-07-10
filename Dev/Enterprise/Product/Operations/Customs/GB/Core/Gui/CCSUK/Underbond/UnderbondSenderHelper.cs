using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	static class UnderbondSenderHelper
	{
		public static void SendMessage(BusinessObject bizo, CcsukTransmissionMessageFunction.CUSDEC how, Customs.Business.ISendsMessagesToCustoms sendsMessagesToCustoms)
		{
			if (!bizo.IsInDatabase)
			{
				//NB. ZForm.FireSaveButton does NOT work the same as the user pressing 'save'. Enterprise.Customs.Business.CusUnderbond.OnSaving() is NOT called when we go via FireSaveButton(), so C4_SendersMEssageReference is not set. 
				sendsMessagesToCustoms.NotifyUserOfAnInvalidOperation("Please save your changes first.  This ensures system-generated reference numbers exist.");
				return;
			}

			how.CusUnderbond = (CusUnderbond)bizo;
			var currentStatus = how.CusUnderbond.C4_Status;
			if (!currentStatus.IsEmpty && currentStatus != EDIMessage.Status.Rejected)
			{
				if (currentStatus == EDIMessage.Status.Cancelled)
				{
					sendsMessagesToCustoms.WarnUserAboutSomething("This underbond's request has been cancelled.\r\nIt remains only for audit purposes.\r\nCreate a new underbond for a new request.", "Warning");
					return;
				}
				else if (!sendsMessagesToCustoms.YesNoQuery("A message for this record has already been created, are you sure you wish to generate another?", "Warning"))
				{
					return;
				}
			}

			var confirmations = new ZStringBuilder();
			var messageSender = new CcsukInventoryRemovalMessageSender(confirmations);
			if (!(how is CcsukTransmissionMessageFunction.CUSDEC.FBK) ||
				sendsMessagesToCustoms.ShowUserConfirmation(ConfirmationMessageCreateCusUnderbondsOnly(how.CusUnderbond), "Fallback", "To confirm, please type: ", "yes"))
			{
				messageSender.Send(how.CusUnderbond, how.CusUnderbond.WholeAwb, sendsMessagesToCustoms, how);
				if (confirmations.Length > 0)
				{
					sendsMessagesToCustoms.NotifyUserOfASuccessfulSend(confirmations.ToString());
				}
			}
		}

		public static string ConfirmationMessageCreateCusUnderbondsOnly(CusUnderbond underbond)
		{
			return @"A Fallback entry should only be sent once fallback procedures have been authorised by CCSUK or HMRC.
Are you sure you want to send a fallback for " + System.Environment.NewLine + System.Environment.NewLine + underbond.Awb.ReferenceNumber;
		}

		public static void GetBusinessObjectFromGrid(ZGrid grid, out BusinessObject result)
		{
			if (grid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError("Please select a single row first");
				result = null;
			}
			else
			{
				result = grid.SelectedElements[0];
			}
		}
	}
}
