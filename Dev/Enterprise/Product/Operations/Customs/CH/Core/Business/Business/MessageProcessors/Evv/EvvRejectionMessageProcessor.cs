using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

sealed class EvvRejectionMessageProcessor : BaseEvvResponseMessageProcessor<IEvvRejectionProvider>
{
	public EvvRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("060B606E-800F-4867-825F-A04EB0DF27F3", "Customs eVV Rejection Message Response Processor");

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.RuleError, MessageSubTypeCodeList.Codes.XmlSchemaError };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IEvvRejectionProvider xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IEvvRejectionProvider evvResponse)
	{
		if (evvResponse != null && message.EM_LinkedObject is IStmALogParent logParent)
		{
			var customsStatus = "Received-ERROR";
			var outgoingEdiMessage = message.Factory.GetOutgoingMessageFromSessionId(message.Interchange?.EI_SessionGUID ?? ZGuid.Empty);

			var reference = StmALog.GenerateEventReference(ZString.Empty, new Dictionary<string, string>()
			{
				{ EventReferenceParameters.Codes.CustomsStatus, customsStatus },
				{ EventReferenceParameters.Codes.Type, GetDocumentTypeFromMessageSubType(outgoingEdiMessage.EM_MessageSubType) },
			});
			logParent.Logs.AddNew(Events.ElectronicAssessmentDecisionStatus, reference);
			UpdateStatementLineStatus(message.Factory, outgoingEdiMessage?.EM_ApplicationReference, outgoingEdiMessage?.EM_MessageSubType, BordereauReceivedStatusList.Codes.RejectedByCustoms);
		}
	}
}
