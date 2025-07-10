using System.Collections.Generic;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETMessageSendingObject : ISadMessageSendingObject
{
	IETHeader MessageHeader { get; }
	IEnumerable<IETLine> MessageLines { get; }
	IEnumerable<INBMessageSendingObject> NBMessages { get; }
}
