using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public static class CDSMessageStatusCalculator
	{
		public static ZString GetMessageAwaitingStatus(CDSEDIMessage outgoingMessage)
		{
			switch (outgoingMessage)
			{
				case CDSNewDeclarationEDIMessage _:
					return MessageStatusList.Codes.AwaitingOriginal;
				case CDSAmendDeclarationEDIMessage _:
					return MessageStatusList.Codes.AwaitingChange;
				case CDSCancelDeclarationEDIMessage _:
					return MessageStatusList.Codes.AwaitingDelete;
				default:
					return ZString.Empty;
			}
		}

		public static ZString GetH7MessageAwaitingStatus(Type outgoingMessageType, CusdecMessageFunction newAmendDelete = null)
		{
			if (outgoingMessageType == typeof(CDSNewDeclarationEDIMessage))
			{
				return MessageStatusList.Codes.AwaitingOriginal;
			}

			if (outgoingMessageType == typeof(CDSArrivalAmendmentDeclarationEDIMessage) && newAmendDelete is CusdecMessageFunction.Amended)
			{
				return MessageStatusList.Codes.AwaitingChange;
			}

			if (outgoingMessageType == typeof(CDSArrivalAmendmentDeclarationEDIMessage) && newAmendDelete is CusdecMessageFunction.Deleted)
			{
				return MessageStatusList.Codes.AwaitingDelete;
			}

			return ZString.Empty;
		}

		public static ZString GetMessageSentStatus(CDSEDIMessage outgoingMessage)
		{
			switch (outgoingMessage)
			{
				case CDSNewDeclarationEDIMessage _:
				case CDSAmendDeclarationEDIMessage _:
				case CDSCancelDeclarationEDIMessage _:
					return MessageStatusList.Codes.Sent;
				default:
					return ZString.Empty;
			}
		}

		public static ZString GetMessageAcknowledgedStatus(CDSEDIMessage outgoingMessage)
		{
			switch (outgoingMessage)
			{
				case CDSNewDeclarationEDIMessage _:
					return MessageStatusList.Codes.AcknowledgedOriginal;
				case CDSAmendDeclarationEDIMessage _:
					return MessageStatusList.Codes.AcknowledgedChange;
				case CDSCancelDeclarationEDIMessage _:
					return MessageStatusList.Codes.AcknowledgedDelete;
				default:
					return ZString.Empty;
			}
		}

		public static ZString GetMessageRejectedStatus(CDSEDIMessage outgoingMessage)
		{
			switch (outgoingMessage)
			{
				case CDSNewDeclarationEDIMessage _:
					return MessageStatusList.Codes.ErrorOriginal;
				case CDSAmendDeclarationEDIMessage _:
					return MessageStatusList.Codes.ErrorChange;
				case CDSCancelDeclarationEDIMessage _:
					return MessageStatusList.Codes.ErrorDelete;
				default:
					return ZString.Empty;
			}
		}

		public static ZString GetMessageClearStatus(CDSEDIMessage outgoingMessage)
		{
			switch (outgoingMessage)
			{
				case CDSNewDeclarationEDIMessage _:
					return MessageStatusList.Codes.ClearOriginal;
				case CDSAmendDeclarationEDIMessage _:
					return MessageStatusList.Codes.ClearChange;
				case CDSCancelDeclarationEDIMessage _:
					return MessageStatusList.Codes.ClearDelete;
				default:
					return ZString.Empty;
			}
		}
	}
}
