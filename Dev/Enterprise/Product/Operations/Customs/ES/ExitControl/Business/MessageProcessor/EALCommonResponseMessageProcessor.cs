using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public abstract class EALCommonResponseMessageProcessor<TResponseProvider, TPrettyMessage> : ESCommonResponseMessageProcessor<CusExitReport, TResponseProvider>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode, IMRNField
		where TPrettyMessage : IMessagePrettyFormatter
	{
		public EALCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		const string AcceptedResponseCode = AESAndNCTS5ResponseTypeCodeList.Codes.AcceptedMessage;

		protected abstract ZString XsdSchemaEmbeddedResourceName { get; }
		protected virtual ZBool IsOnlyAcceptedDeclaration => false;

		protected abstract TPrettyMessage GetNewMessagePrettyFormatter(TResponseProvider response);

		protected sealed override void ProcessMessageCore(EDIMessage message, CusExitReport linkedBusinessObject, TResponseProvider provider)
		{
			message.EM_MessageNum = ((ZString)provider.ServiceSegmentId).Left(AutoEDIMessage.Schema.EM_MessageNumMaxLength);
			var messagePrettyFormatter = GetNewMessagePrettyFormatter(provider);

			RemoveCusPollingTransactionsIfNeeded(linkedBusinessObject.Factory, MessageTypesToInclude, linkedBusinessObject.Consignment?.CXC_MovementReference ?? ZString.Empty);

			if (IsOnlyAcceptedDeclaration || provider.ResponseCode == AcceptedResponseCode)
			{
				ProcessAcceptedDeclaration(provider, message, linkedBusinessObject);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted(ZString.Empty);
				SetCHStatusAsReceived(linkedBusinessObject, messageStatus: LogicalStatusList.Codes.Accepted);
				SetMessageSubTypeAsAccepted(message);
			}
			else
			{
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
				SetCHStatusAsRejected(linkedBusinessObject, messageStatus: LogicalStatusList.Codes.Error);
				SetMessageSubTypeAsRejected(message);
			}

			SetMessageStatusAsReceived(message);
		}

		protected abstract void ProcessAcceptedDeclaration(TResponseProvider response, EDIMessage message, CusExitReport report);
		protected virtual void ProcessRejectedDeclaration(TResponseProvider response, EDIMessage message, CusExitReport report) { }

		protected void CreateOrUpdateCusEntryNumber(CusExitReport report, EDIMessage message, ZString newCSVClearance, ZString entryStatus, ZDateTime issueDate)
		{
			var oldCSVClearance = report.ClearanceReferenceNumber;

			if ((!oldCSVClearance.IsEmpty && !newCSVClearance.IsEmpty && oldCSVClearance != newCSVClearance)
				|| ForceChangeExistingEdocsFileNames(message))
			{
				var fileNamesDict = FileNamesDict(report.Consignment.CXC_MovementReference, oldCSVClearance, message);
				EDocHelper.ChangeExistingEDocsFileNames(report, fileNamesDict, EDocsSaver);
			}

			if (!newCSVClearance.IsEmpty || !entryStatus.IsEmpty || !issueDate.IsEmpty)
			{
				var newEntryNumber = CusEntryNumber.LoadOrCreate(report, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
				newEntryNumber.CE_EntryNum = newCSVClearance;
				newEntryNumber.CE_EntryStatus = entryStatus;
				newEntryNumber.CE_IssueDate = issueDate;
				newEntryNumber.CE_EntryIsSystemGenerated = true;

				TriggerMisingDocumentRequest(report, message);
			}
		}

		protected override TResponseProvider GetMessageProviderCore(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithoutValidation<TResponseProvider>(XsdSchemaEmbeddedResourceName, bodyTextReader, isAES: true, isNCTS: false);
			}
		}

		protected override CommonDocumentRequest<CusExitReport> GetNewDocumentRequest(CusExitReport businessObject, ZString certName, EDIMessage message) => new ExitControlDocumentRequest(businessObject, certName);
		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictArrivalAtExit(mrn, oldCSVClearance);
	}
}
