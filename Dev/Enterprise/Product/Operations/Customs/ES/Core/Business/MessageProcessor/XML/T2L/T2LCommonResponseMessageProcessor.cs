using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public abstract class T2LCommonResponseMessageProcessor<TResponse> : XMLResponseMessageProcessor<TResponse, IMessagePrettyFormatter>
		where TResponse : class, ICommonServiceSegment, IT2LCommon, IResponseCode
	{
		public T2LCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"ES T2L Declaration Message Processor";

		protected sealed override ZString AcceptedResponseCode => ResponseMessageCodeList.AcceptedCode;

		protected override ZString ProcessAcceptedDeclaration(TResponse response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (ShouldChangeEntryStatus)
			{
				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			}

			return ZString.Empty;
		}

		protected void SetCSVClearance(IT2LCSVClearanceField response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var csvClearance = response.CSVClearance;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
		}

		protected void SetAcceptanceDate(TResponse response, CusEntryHeader entryHeader)
		{
			var serviceSegmentId = response.ServiceSegmentId;
			var acceptanceDateCorrect = ZDateTime.TryParseExact(serviceSegmentId.LeftOrNull(14), out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
			if (acceptanceDateCorrect)
			{
				SetMovementReferenceNumber(entryHeader, acceptanceDate);
			}
		}

		protected void SetCircuit(IT2LCircuitField response, CusEntryHeader entryHeader)
		{
			if (!response.CircuitSpecified)
			{
				return;
			}

			var circuitCode = GetCircuitCodeFromText(response.Circuit.ToString());
			if (!circuitCode.IsEmpty)
			{
				entryHeader.SetMovementReferenceNumberEntryStatus(circuitCode);

				ZString entryStatus = (string)circuitCode switch
				{
					CircuitCodeList.Codes.GREEN => EntryStatusCodes.Cleared,
					CircuitCodeList.Codes.RED or CircuitCodeList.Codes.ORANGE => EntryStatusCodes.CustomsDeclarationAccepted,
					_ => ZString.Empty
				};

				if (!entryStatus.IsEmpty)
				{
					entryHeader.CH_EntryStatus = entryStatus;
				}
			}
		}

		protected ZBool ShouldTriggerAnnexes(CusEntryHeader entryHeader) => entryHeader.CH_EntryStatus == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;

		protected override List<MessageBuilderData> GetMessageBuildersData(CusEntryHeader entryHeader, CertificateObject certificateObject)
		{
			var builderManager = new ESMessageBuilderManager(DeclarationMessageTypeList.Codes.T2lAnnex, DeclarationMessageSubTypeList.Codes.OriginalDeclaration, entryHeader, certificateObject);

			return ESMessageSender.GetT2LAnnexesMessageBuilders(entryHeader, builderManager);
		}

		protected virtual ZBool ShouldChangeEntryStatus => true;

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new T2LExpeditionDocumentRequest(businessObject, certName);
	}
}
