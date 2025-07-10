using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCAcknowledgeMessageSender
	{
		public ZString SendAcceptMessage(ZGuid notificationPK) => SendMessage(notificationPK, true);

		public ZString SendRejectMessage(ZGuid notificationPK) => SendMessage(notificationPK, false);

		string SendMessage(ZGuid notificationPK, bool isAccepted)
		{
			var errorMessage = ZString.Empty;
			var factory = new BusinessObjectFactory();
			var notification = factory.LoadTop1<QuarantineNexDocNotification>(new ZQuery(QuarantineNexDocNotificationSchema.PK, notificationPK));
			if (notification == null)
			{
				errorMessage = Res.GetString("06CA6215-861F-4EF3-A341-ACA24AFB256B", "Could not find the selected Notification.");
			}
			else if (!notification.IsAcknowledgedStatusForSendingMessage)
			{
				errorMessage = notification.WrongAcknowledgedStatusForSendingMessage;
			}
			else if (!(notification.CanSendAcknowledgeForward || notification.CanSendAcknowledgeTransfer))
			{
				errorMessage = Res.GetString("4D69C9F2-9940-490E-8051-B22F555AE753", "Acknowledge type must Forward or Transfer.");
			}
			else
			{
				new NEXDOCAcknowledgeMessageBuilder(notification).CreateNewMessage(isAccepted);
				notification.QN_AcknowledgeStatus = isAccepted ? NEXDOCAcknowledgeStatus.Codes.PendingAccept : NEXDOCAcknowledgeStatus.Codes.PendingReject;
				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}

			return errorMessage;
		}
	}
}
