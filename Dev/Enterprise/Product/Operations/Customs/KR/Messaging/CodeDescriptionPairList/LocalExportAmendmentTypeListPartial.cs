using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class LocalExportAmendmentTypeList
	{
		public static string GetAmendmentType(MessageFunctions.MessageFunctionCode messageType)
		{
			var result = ZString.Empty;
			switch (messageType)
			{
				case MessageFunctions.MessageFunctionCode.Amendment:
					result = MessageSubTypeLocalExport.Amendment;
					break;
				case MessageFunctions.MessageFunctionCode.Cancellation:
					result = MessageSubTypeLocalExport.Cancellation;
					break;
			}
			return result;
		}
	}
}
