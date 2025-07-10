using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC016A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC016AProcessor : DTBaseProcessor<Cc016AType>
	{
		public DTCC016AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC016A processor";

		protected override ZString GetNewMessageStatus(Cc016AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;

		protected override ZString GetNewDepartureStatus(Cc016AType messageObject) => EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationRejected;

		protected override ZString GetMessageInterpretation(Cc016AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetMessageStatusInterpretation(messageObject));
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			sb.Append(GetFunctionalErrorInterpretation(messageObject));
			return sb.ToString();
		}

		internal override void UpdateGuaranteeTransactionsIfNeeded(Cc016AType messageObject, EDIMessage incomingMessage)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeader.GetOutgoingMessage(incomingMessage), EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.France, false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}
	}
}
