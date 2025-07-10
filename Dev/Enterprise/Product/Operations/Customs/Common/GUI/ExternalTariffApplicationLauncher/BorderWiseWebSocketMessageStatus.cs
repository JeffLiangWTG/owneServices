using System;

namespace Enterprise.Customs.Common.GUI
{
	[Flags]
	public enum BorderWiseWebSocketMessageStatus
	{
		STA = 1,
		MSG = 2,
		FIN = 3,
		ACK = 4,
		ERR = 5,
		CAN = 6
	}
}
