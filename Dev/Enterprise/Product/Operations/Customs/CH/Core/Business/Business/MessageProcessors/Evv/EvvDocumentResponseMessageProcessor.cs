using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

sealed class EvvDocumentResponseMessageProcessor : BaseEvvResponseMessageProcessor<IEvvCommonProvider>
{
	public EvvDocumentResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("AA137A96-EE0E-4009-B598-0DA2F9F102A6", "Customs eVV Document Message Response Processor");

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => [MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties,
	MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, MessageSubTypeCodeList.Codes.TaxationDecisionVat ];

	protected override BusinessObject FindLinkedObject(EDIMessage message, IEvvCommonProvider xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IEvvCommonProvider evvResponse)
	{
		if (evvResponse != null && message.EM_LinkedObject is IStmALogParent logParent)
		{
			var sentEdiMessage = message.Factory.GetOutgoingMessageFromSessionId(message.Interchange.EI_SessionGUID);
			var dataContext = string.Empty;
			switch (message.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.TaxationDecisionVat:
				case MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties:
					dataContext = CHEDIMessageDocumentSupporter.EVVTaxationDocument;
					break;
				case MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat:
				case MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties:
					dataContext = CHEDIMessageDocumentSupporter.EVVRefundDocument;
					break;
			}
			message.EM_ApplicationReference = $"{evvResponse.DocumentNumber}.{evvResponse.DocumentVersion}";

			var reference = StmALog.GenerateEventReference(ZString.Empty, new Dictionary<string, string>()
				{
					{ EventReferenceParameters.Codes.CustomsStatus, "Received-OK" },
					{ EventReferenceParameters.Codes.Type, GetDocumentTypeFromMessageSubType(message.EM_MessageSubType) },
				});
			logParent.Logs.AddNew(Events.ElectronicAssessmentDecisionStatus, reference);

			CreateDocuments(message, evvResponse, dataContext);
			UpdateStatementLineStatus(message.Factory, sentEdiMessage.EM_ApplicationReference, sentEdiMessage?.EM_MessageSubType, BordereauReceivedStatusList.Codes.Received);
		}
	}
}
