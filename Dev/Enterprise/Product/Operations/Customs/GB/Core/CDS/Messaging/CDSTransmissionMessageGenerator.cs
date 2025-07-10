using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.Chief;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public class CDSTransmissionMessageGenerator : GbChiefEdifactTransmissionMessageGenerator
	{
		public CDSTransmissionMessageGenerator(IEnumerable<JobDeclarationMessageSendingObject> objectsToSend, Action afterSaveAction = null) : base(null)
		{
			ObjectsToSend = objectsToSend ?? Enumerable.Empty<JobDeclarationMessageSendingObject>();
			this.afterSaveAction = afterSaveAction;
		}

		protected override bool IsNewMessage(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			var sendingObject = GetMessageSendingObjectForEntry(entryHeader);
			return (sendingObject?.MessageType ?? ZString.Empty) == CDSEDIMessageTypeList.Codes.NewDeclaration;
		}

		protected override bool IsAmendMessage(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			var sendingObject = GetMessageSendingObjectForEntry(entryHeader);
			return (sendingObject?.MessageType ?? ZString.Empty) == CDSEDIMessageTypeList.Codes.AmendDeclaration;
		}

		public override IBuilderResult Generate(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			IBuilderResult result = null;
			var messageSendingObject = GetMessageSendingObjectForEntry(entryHeader);
			if (messageSendingObject != null)
			{
				result = base.Generate(entryHeader);
				var warningFactor = EnhancedValidationParticipationHelper.GetWarningFactor();
				var messageText = result?.Message?.EM_MessageText ?? ZString.Empty;
				if (warningFactor.HasValue && !messageText.IsEmpty && messageSendingObject.CanParticipateEnhancedValidation)
				{
					var entry = (CusEntryHeader)entryHeader;
					var isTreated = EnhancedValidationParticipationHelper.IsSubjectToTreatment(warningFactor.Value, entry);
					result.Message.EM_MessageText = messageText + $@"
<!--HMRC Digital Prompts: {(isTreated ? "treatment" : "control")}-->
<!--Profile: {entry.Declaration.JE_CustomsProfile}-->
<!--Gateway: {entry.Declaration.ZG_Gateway}-->
<!--Declaration Type: {entry.EntryInstruction.CEI_Style}-->
<!--Message Number: {EDIMessage.MessageNumberPlaceHolder}-->";
				}
			}

			return result;
		}

		protected override ZString GetStatus(EDIMessage outgoingMessage)
		{
			return CDSMessageStatusCalculator.GetMessageAwaitingStatus(outgoingMessage as CDSEDIMessage);
		}

		protected override void CheckMaximumPayloadSizeForChief()
		{
		}

		protected override void GenerateMessageTextButDoNotSave(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			generatedMessageText = GetEDIMessageText((CusEntryHeader)entryHeader, errorCollector);
		}

		protected override EDIMessage GetNewEdiMessage(EU.Business.Declaration.CusEntryHeader entry)
		{
			var ediMessageType = GetEDIMessageType((CusEntryHeader)entry).EdiMessageBizOType;
			var result = entry.Messages.AddNew(ediMessageType);

			if (result is CDSAmendDeclarationEDIMessage)
			{
				LinkComparisonMessageToAmendment(result, entry);
			}

			return result;
		}

		void LinkComparisonMessageToAmendment(EDIMessage message, EU.Business.Declaration.CusEntryHeader entry)
		{
			var comparisonMessage = entry.Messages.Cast<EDIMessage>().LastOrDefault(x => x.EM_ApplicationReference == EDIMessageApplicationReferencesForAmendment.Current);

			if (comparisonMessage != null)
			{
				comparisonMessage.EM_ApplicationReference = GbChiefEdifactTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(message);
			}
		}

		protected override void SetMessageTypesAndStatus(BuilderResult result)
		{
			if (result.Message.EM_MessageSubType.IsEmpty)
			{
				result.Message.EM_MessageSubType = messageSubType;
			}

			result.Message.EM_MessageText = generatedMessageText;
			result.Message.Saved += OnMessageSaved;
		}

		void OnMessageSaved(EDIMessage message, bool saveSucceeded)
		{
			if (message.IsInDatabase)
			{
				var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
				if (reloadedMessage != null && reloadedMessage.EM_MessageText.IsEmpty)
				{
					var errorMessage = $"Failed to save EM_MessageText (PK: {message.PK}, LinkUniqueID: {message.EM_LinkUniqueID})";
					ErrorReporter.ReportOnce(errorMessage);
				}
				afterSaveAction?.Invoke();
			}
			message.Saved -= OnMessageSaved;
		}

		protected override void PutDeclarationNumberIntoMessageFromPlaceholderCore(EDIMessage message, ZString messageText, EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			base.PutDeclarationNumberIntoMessageFromPlaceholderCore(message, messageText, entryHeader);
			message.EM_MessageText = message.EM_MessageText.Replace(CusEntryHeader.LRNReferencePlaceHolderXmlFriendly, DeclarationMessageBuilder.MakeXmlSafe(((CusEntryHeader)entryHeader).LRN));
		}

		public override ZString MakePrettyForInterpretation(EDIMessage message) => GetMessagePrettier(message)?.MakeHumanReadable() ?? "Interpretation will be supported soon.";

		CDSEDIMessagePrettier GetMessagePrettier(EDIMessage message)
		{
			switch (message)
			{
				case CDSNewDeclarationEDIMessage newDeclarationMessage:
					return new CDSNewDeclarationEDIMessagePrettier(newDeclarationMessage);
				case CDSCancelDeclarationEDIMessage cancelDeclarationMessage:
					return new CDSCancelDeclarationEDIMessagePrettier(cancelDeclarationMessage);
				case CDSAmendDeclarationEDIMessage amendDeclarationMessage:
					return GetAmendmentMessagePrettier(amendDeclarationMessage);
				case CDSInventoryLinkingMasterQueryRequestEDIMessage masterQueryRequestEDIMessage:
					return new CDSInventoryLinkingMasterQueryRequestEDIMessagePrettier(masterQueryRequestEDIMessage);
				default:
					return GetOthersMessagePrettier(message as CDSEDIMessage);
			}
		}

		CDSEDIMessagePrettier GetOthersMessagePrettier(CDSEDIMessage message)
		{
			var entry = message.EM_LinkedObject as CusEntryHeader;
			var messageSendingObject = GetMessageSendingObjectForEntry(entry);
			switch (messageSendingObject.MessageType)
			{
				case GbCusDecMessageFunctionsList.Codes.Associate:
					return new AssociateRequestEDIMessagePrettier(message);
				case GbCusDecMessageFunctionsList.Codes.Disassociate:
					return new DisassociateRequestEDIMessagePrettier(message);
				case GbCusDecMessageFunctionsList.Codes.Close:
					return new CloseRequestEDIMessagePrettier(message);
				case GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation:
				case GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation:
				case GbCusDecMessageFunctionsList.Codes.DepartureFromLocation:
					return new CDSInventoryLinkingMovementRequestEDIMessagePrettier((CDSInventoryLinkingMovementRequestEDIMessage)message);
				default:
					return null;
			}
		}

		CDSEDIMessagePrettier GetAmendmentMessagePrettier(EDIMessage message)
		{
			var entry = message.EM_LinkedObject as CusEntryHeader;
			var messageSendingObject = GetMessageSendingObjectForEntry(entry);

			switch (message)
			{
				case CDSNilAmendmentDeclarationEDIMessage amendMessage:
					return new CDSNilAmendmentDeclarationEDIMessagePrettier(amendMessage, messageSendingObject);
				case CDSFECAmendmentDeclarationEDIMessage amendMessage:
					return new CDSFECAmendmentDeclarationEDIMessagePrettier(amendMessage, messageSendingObject);
				case CDSArrivalAmendmentDeclarationEDIMessage amendMessage:
					return new CDSArrivalAmendmentDeclarationEDIMessagePrettier(amendMessage, messageSendingObject);
				default:
					return new CDSAmendDeclarationEDIMessagePrettier(message as CDSAmendDeclarationEDIMessage, messageSendingObject);
			}
		}

		protected override ZString GetApplicationCode(Customs.Business.CusEntryHeader entry) => ((CusEntryHeader)entry)?.GetApplicationCodeForMessage() ?? Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices;

		protected override ZString GetApplicationReference(EU.Business.Declaration.CusEntryHeader entryHeader) => "";

		IMessageBuilderManager BuilderManager { get; } = new MessageBuilderManager();

		ZString GetEDIMessageText(CusEntryHeader entry, EU.Business.ErrorCollector ec)
		{
			var how = GetEDIMessageType(entry).NewAmendDelete;
			var objectToSend = GetMessageSendingObjectForEntry(entry);
			var messageBuilder = BuilderManager.NewMessageBuilder(objectToSend, ec, how);
			// Return empty string if messageBuilder is null without adding an error to ErrorCollector and report to ErrorReporter
			// BuilderManager.NewMessageBuilder already add error to ErrorCollector for invalid declaration/entry type
			if (messageBuilder == null)
			{
				return ZString.Empty;
			}
			var result = messageBuilder.Build();
			if (result.IsEmpty)
			{
				var errorMessage = $"Failed to generate EDI message (EntryType: {objectToSend.EntryType}, MessageType: {objectToSend.MessageType})";
				ec.AddError(errorMessage);
				ErrorReporter.ReportOnce(errorMessage);
			}
			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public (Type EdiMessageBizOType, Customs.Business.CusdecMessageFunction NewAmendDelete) GetEDIMessageType(CusEntryHeader entry)
		{
			(Type EdiMessageBizOType, Customs.Business.CusdecMessageFunction NewAmendDelete) result = (typeof(CDSNewDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.New());
			var messageSendingObject = GetMessageSendingObjectForEntry(entry);

			if (messageSendingObject != null)
			{
				switch (messageSendingObject.MessageType)
				{
					case CDSEDIMessageTypeList.Codes.NewDeclaration:
						result = (typeof(CDSNewDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.New());
						break;
					case CDSEDIMessageTypeList.Codes.AmendDeclaration:
						result = (typeof(CDSAmendDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.Amended());
						break;
					case CDSEDIMessageTypeList.Codes.CancelDeclaration:
						result = (typeof(CDSCancelDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.Deleted());
						break;
					case CDSEDIMessageTypeList.Codes.NilAmendment:
						result = (typeof(CDSNilAmendmentDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.Amended());
						break;
					case CDSEDIMessageTypeList.Codes.FecChallenge:
						result = (typeof(CDSFECAmendmentDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.Amended());
						break;
					case CDSEDIMessageTypeList.Codes.ArrivalNotification:
						result = (typeof(CDSArrivalAmendmentDeclarationEDIMessage), new Customs.Business.CusdecMessageFunction.Amended());
						break;
					case CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest:
						messageSubType = messageSendingObject.MessageType;
						result = (typeof(CDSInventoryLinkingQueryRequestEDIMessage), new Customs.Business.CusdecMessageFunction.New());
						break;
					case CDSEDIMessageTypeList.Codes.MasterQueryDeclaration:
						messageSubType = messageSendingObject.MessageType;
						result = (typeof(CDSInventoryLinkingMasterQueryRequestEDIMessage), new Customs.Business.CusdecMessageFunction.New());
						break;
					case GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation:
					case GbCusDecMessageFunctionsList.Codes.DepartureFromLocation:
					case GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation:
						messageSubType = messageSendingObject.MessageType;
						result = (typeof(CDSInventoryLinkingMovementRequestEDIMessage), new Customs.Business.CusdecMessageFunction.New());
						break;
					case GbCusDecMessageFunctionsList.Codes.Associate:
					case GbCusDecMessageFunctionsList.Codes.Disassociate:
					case GbCusDecMessageFunctionsList.Codes.Close:
						messageSubType = messageSendingObject.MessageType;
						result = (typeof(CDSInventoryLinkingConsolidationRequestEDIMessage), new Customs.Business.CusdecMessageFunction.New());
						break;
				}
			}

			return result;
		}

		JobDeclarationMessageSendingObject GetMessageSendingObjectForEntry(EU.Business.Declaration.CusEntryHeader entry)
		{
			var entryPK = entry.PK;

			return ObjectsToSend.FirstOrDefault(x => x.Header.PK.Equals(entryPK));
		}

		public IEnumerable<JobDeclarationMessageSendingObject> ObjectsToSend { get; }

		ZString messageSubType = ZString.Empty;
		readonly Action afterSaveAction;
	}
}
