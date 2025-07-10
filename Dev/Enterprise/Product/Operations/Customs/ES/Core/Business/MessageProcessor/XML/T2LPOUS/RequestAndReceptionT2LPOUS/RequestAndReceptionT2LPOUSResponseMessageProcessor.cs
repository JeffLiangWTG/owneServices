using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class RequestAndReceptionT2LPOUSResponseMessageProcessor : T2LPOUSCommonResponseMessageProcessor<Iep01SalType, IMessagePrettyFormatter>
	{
		public RequestAndReceptionT2LPOUSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"T2L POUS Request and Reception Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCIEP01V1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lRequestPous, Messaging.DeclarationMessageTypeList.Codes.T2lReceptionPous };

		protected override ZBool IsAnnexMessage(EDIMessage message) => isMessageTypeReception(message);

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Iep01SalType response, EDIMessage message, CusEntryHeader entryHeader) => new RequestAndReceptionT2LPOUSMessagePrettyFormatter(response);

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message)
			=> isMessageTypeReception(message) ? new T2LReceptionDocumentRequest(businessObject, certName) : new T2LExpeditionDocumentRequest(businessObject, certName);

		protected override ZString ProcessAcceptedDeclaration(Iep01SalType response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (isMessageTypeReception(message))
			{
				SetT2CMovementReferenceNumber(entryHeader, response.Mrnt2L);
				SetSentMessageStatusAsReceived(message, entryHeader.Factory);
				SetMessageStatusAsReceived(message);
				SetCHStatusAsReceived(entryHeader);
			}

			ProcessCommonAcceptedDeclaration(entryHeader, response.RiskAnalysisResultCode, response.Csvt2L, message, preparationDateAndTime: response.Message.PreparationDateAndTime, shouldUseTRMForDocTrigger: isMessageTypeReception(message));

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(Iep01SalType response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (isMessageTypeReception(message))
			{
				SetSentMessageStatusAsRejected(message, entryHeader.Factory);
				SetMessageStatusAsRejected(message);
				SetCHStatusAsReceived(entryHeader);
			}
		}

		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message)
			=> isMessageTypeReception(message) ? EDocHelper.FileNamesDictT2LReception(mrn, oldCSVClearance) : EDocHelper.FileNamesDictT2LExpeditionAmendment(mrn, oldCSVClearance);

		protected override bool ShouldSetMovementReferenceNumber(EDIMessage message) => !isMessageTypeReception(message);

		bool isMessageTypeReception(EDIMessage message) => message.EM_MessageType == Messaging.DeclarationMessageTypeList.Codes.T2lReceptionPous;

		const string XsdSchemaNameCCIEP01V1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.Incoming.CCIEP01V1Sal.xsd";
	}
}
