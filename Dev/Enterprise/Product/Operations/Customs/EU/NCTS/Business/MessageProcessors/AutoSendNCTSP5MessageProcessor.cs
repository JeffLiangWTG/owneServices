using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business;

public class AutoSendNCTSP5MessageProcessor : AutoSendNCTSMessageProcessor
{
	public AutoSendNCTSP5MessageProcessor(NctsHeader nctsHeader) : base(nctsHeader)
	{
		if (!nctsHeader.IsPhase5)
		{
			throw new ArgumentException("Only Supported for Phase 5");
		}
	}

	protected override ZString MessageDescription => Header.IsArrivalMovement ? (NoResString)"Arrival Movement" : (NoResString)"Departure Movement";

	protected override ZBool SendNctsMessageCore(INotifications notifications)
	{
		var messagingConfiguration = Header.Configuration.MessageSendingConfiguration;
		var sendingObjectParent = messagingConfiguration.GetNewNctsHeaderMessageSendingObjectParent(Header);

		var sendingObjectHasErrors = ValidateMessageSendingObjectParent(sendingObjectParent, notifications);
		if (!sendingObjectHasErrors)
		{
			return sendingObjectParent.SendAndSaveMessages();
		}

		return false;
	}

	bool ValidateMessageSendingObjectParent(NctsHeaderMessageSendingObjectParent sendingObjectParent, INotifications notifications)
	{
		sendingObjectParent.SendingObjectsCollection.MarkAsNeedingValidation();
		sendingObjectParent.RunPreSaveValidation();
		if (sendingObjectParent.HasErrors)
		{
			var errorLog = LogErrorsOnMessageSendingObjectParent(sendingObjectParent);
			notifications.AddError(errorLog);
			return true;
		}

		return false;
	}

	ZString LogErrorsOnMessageSendingObjectParent(NctsHeaderMessageSendingObjectParent sendingObjectParent) =>
		ZString.Format(
			(NoResString)"System cannot send {0} because of following errors on Job:{1}\r\n{2}",
			MessageDescription, Header.BH_JobReference, sendingObjectParent.GetErrors().ToUniqueMessageListString());
}
