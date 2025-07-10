using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface INBMessageSendingObject
{
	ZString AnnualProgressiveNumber { get; }
	INBHeader Header { get; }
	IEnumerable<IPreviousOperationInfo> DataBlocks { get; }
}
