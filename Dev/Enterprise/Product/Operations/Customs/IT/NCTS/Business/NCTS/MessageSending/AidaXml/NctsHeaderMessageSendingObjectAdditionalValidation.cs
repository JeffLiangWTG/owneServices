using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsHeaderMessageSendingObjectAdditionalValidation
{
	internal NctsHeaderMessageSendingObjectAdditionalValidation(NctsHeaderMessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		InternalNotifications = new List<INotification>();
	}

	internal IEnumerable<INotification> GetAdditionalErrors()
	{
		InternalNotifications.Clear();

		CheckPresentationDateTimeMessage();

		return InternalNotifications;
	}

	void CheckPresentationDateTimeMessage()
	{
		var movementHeader = sendingObject.NctsHeader?.MovementHeader;
		var presentationDateTime = movementHeader?.BM_PresentationDateTime ?? ZDateTimeOffset.Empty;

		if (presentationDateTime.Date < ZDate.Today)
		{
			AddError($"{movementHeader.BM_PresentationDateTimeInfo.HumanReadableName}: {ValidationCaptions.NctsDepartureMovementHeader.DateOfPresentationCanNotBeInPast}");
		}
	}

	void AddError(string message)
	{
		InternalNotifications.Add(new Notification(CargoWise.EntityFramework.NotificationType.MessageError, message));
	}

	List<INotification> InternalNotifications { get; }
	readonly NctsHeaderMessageSendingObject sendingObject;
}
