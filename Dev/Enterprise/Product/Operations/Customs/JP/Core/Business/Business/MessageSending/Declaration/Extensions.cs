using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	static class Extensions
	{
		public static ZString GetProcedureCode(this JobDeclarationMessageSendingObject sendingObject)
		{
			switch (sendingObject)
			{
				case MessageSendingObject messageSendingObject:
					return messageSendingObject.ProcedureCode;

				case MSXMessageSendingObject msxMessageSendingObject:
					return JPProcedureCodeList.Codes.MSX;

				default:
					return string.Empty;
			}
		}
	}
}
