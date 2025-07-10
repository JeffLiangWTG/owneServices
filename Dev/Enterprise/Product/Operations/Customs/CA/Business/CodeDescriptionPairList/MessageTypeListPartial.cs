using System;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	partial class MessageTypeList
	{
		public static MessageType GetMessagesTypesRightFor(ZString messageType)
		{
			switch (messageType)
			{
				case Codes.G7Export:
					return MessageType.G7ExportDeclaration;
				case Codes.DataLoadingModule:
					return MessageType.DataLoadingModule;
				case Codes.EDIRelease:
					return MessageType.EDIRelease;
				case Codes.B3CUSDEC:
				case Codes.CommercialAccountingDeclaration:
					return MessageType.B3Cusdec;
				case "":
					return MessageType.Undefined;
				default:
					throw new InvalidOperationException("messageType is not known");
			}
		}

		public static ZString GetCADOrB3CMessageType(ZBool b3Accepted)
		{
			return b3Accepted || !UniversalReferenceConstants.IsCarmR2 ? Codes.B3CUSDEC : Codes.CommercialAccountingDeclaration;
		}
	}
}
