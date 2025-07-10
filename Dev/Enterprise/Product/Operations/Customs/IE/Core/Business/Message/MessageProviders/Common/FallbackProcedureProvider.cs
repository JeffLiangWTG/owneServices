using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class FallbackProcedureProvider : MessageProvider, IFallbackProcedure
	{
		public static FallbackProcedureProvider New(IFallbackProcedureSendingObject sendingAction) => new FallbackProcedureProvider(sendingAction);

		FallbackProcedureProvider(IFallbackProcedureSendingObject sendingAction)
		{
			SendingAction = sendingAction;
		}
		IFallbackProcedureSendingObject SendingAction { get; }

		public DateTime AlternativeDateOfAcceptance => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(SendingAction.AlternativeDateOfAcceptance);

		public string CustomsReferenceNumber => SendingAction.CustomsReferenceNumber;

		public string CustomsJustification => SendingAction.CustomsJustification;
	}
}
