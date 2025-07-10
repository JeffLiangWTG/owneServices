using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public abstract class ESResponseMessageProcessor<TResponseProvider> : ESCommonResponseMessageProcessor<CusEntryHeader, TResponseProvider>
	{
		protected ESResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"ES Generic Response Message Processor";

		protected void SetCSVClearanceAndTriggerDocumentRequest(CusEntryHeader businessObject, EDIMessage message, ZString newCSVClearance) => SetCSVClearanceAndTriggerDocumentRequest(businessObject, message, newCSVClearance, businessObject.MovementReferenceNumber);
		protected override ZString GetOldCSVClearance(CusEntryHeader businessObject) => businessObject.CSVClearance;
		protected override void SetNewCSVClearance(CusEntryHeader businessObject, ZString newCSVClearance) => businessObject.SetCSVClearanceNum(newCSVClearance);
		protected void SetMovementReferenceNumber(CusEntryHeader businessObject, ZDateTime admissionDate, string circuit = null)
		{
			businessObject.MovementReferenceNumberIssueDate = SetEntryIssueDate(businessObject.MovementReferenceNumberIssueDate, admissionDate);
			if (circuit != null)
			{
				businessObject.SetMovementReferenceNumberEntryStatus(circuit);
			}
		}
	}
}
