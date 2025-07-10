using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business;

public interface IMessageSendingObjectParent
{
	BusinessObjectFactory Factory { get; }

	IEnumerable<BaseMessageSendingObject> SelectedSendingObjects { get; }

	EDIMessage[] SendAndSaveMessages(MessageSendingContext context);

	ZString ValidateBeforeSend();
}
