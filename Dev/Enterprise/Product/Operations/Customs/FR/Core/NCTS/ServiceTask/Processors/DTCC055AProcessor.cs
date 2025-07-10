using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC055A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC055AProcessor : DTBaseProcessor<Cc055AType>
	{
		public DTCC055AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC055A processor";

		protected override ZString GetNewMessageStatus(Cc055AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Cc055AType messageObject) => NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretation(Cc055AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Guaref2 != null)
			{
				foreach (var guaref in messageObject.Guaref2)
				{
					sb.Append(GetKeyValuePairInterpretation("Submitted guarantee", guaref.GuaRefNumGrnref21));
					if (guaref.Invguarns != null)
					{
						sb.Append(GetKeyValuePairInterpretation("Reason", guaref.Invguarns.InvGuaReaRns12));
					}
				}
			}
			sb.Append(GetKeyValuePairInterpretation("Action required", "You can submit an amendment request, a cancellation request or do nothing. In this last case, the declaration status will automatically turn into \"Goods Not released for Transit\" after 30 days."));
			return sb.ToString();
		}

		internal override void UpdateGuaranteeTransactionsIfNeeded(Cc055AType messageObject, EDIMessage incomingMessage)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeader.GetOutgoingMessage(incomingMessage),
			EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.France, false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}
	}
}
