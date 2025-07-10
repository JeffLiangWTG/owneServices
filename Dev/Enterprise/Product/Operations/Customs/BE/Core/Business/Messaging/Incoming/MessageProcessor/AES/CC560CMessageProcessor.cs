using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC560C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class CC560CMessageProcessor : DeclarationMessageProcessor<ICC560CDataProvider>
{
	public CC560CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC560C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC560C };

	protected override Type MessageInterpreterType => typeof(CC560CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC560CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByMRN(message.Factory, messageDataProvider);

	protected internal override ICC560CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc560CType, CC560CDataProvider>();

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC560CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		ZString jobStatus;

		switch (messageDataProvider.NotificationType)
		{
			case NotificationTypesList.Codes.DecisionToControl:
			case NotificationTypesList.Codes.AdditionalDocumentsRequested:
				jobStatus = StatusCodes.DecisionToControl;
				break;
			case NotificationTypesList.Codes.IntentionToControl:
				jobStatus = StatusCodes.IntentionToControl;
				break;
			default:
				jobStatus = ZString.Empty;
				break;
		}

		entryHeader.Declaration.Logs.CreateRecreateOrUpdateEventLog(
			new EventValue(Events.CustomsImpedimentReceived,
			eventTime: ((ZDateTime?)messageDataProvider.AnticipatedControlDate)?.ToOffset(),
			reference: jobStatus == StatusCodes.IntentionToControl ? (NoResString)"Intention by Customs to control" : (NoResString)"Control of Customs"));

		CreateService(messageDataProvider, entryHeader.Declaration);

		return (StatusCodes.Control, EDIMessageStatusList.Codes.ProcessedOK, ZString.Empty);
	}

	void CreateService(ICC560CDataProvider provider, JobDeclaration declaration)
	{
		var service = declaration.Services.AddNew();
		service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ControlByCustoms;
		service.ES_Booked = provider.AnticipatedControlDate ?? ZDateTime.Empty;

		var notificationType = ZString.Empty;
		var controlType = ZString.Empty;

		switch (provider.NotificationType)
		{
			case NotificationTypesList.Codes.DecisionToControl:
				notificationType = NotificationTypesList.Descriptions.IntentionToControl;
				controlType = (NoResString)"controls";
				break;
			case NotificationTypesList.Codes.AdditionalDocumentsRequested:
				notificationType = NotificationTypesList.Descriptions.DecisionToControl;
				controlType = (NoResString)"controls";
				break;
			case NotificationTypesList.Codes.IntentionToControl:
				notificationType = NotificationTypesList.Descriptions.AdditionalDocumentsRequested;
				controlType = (NoResString)"documents";
				break;
		}
		service.ES_ServiceNote = $"{notificationType}. Look in the message log of declaration with MRN {provider.MRN} for the type of {controlType}.";
	}

	protected override bool CheckMessageSequenceIsValidCore(BEMessage message)
	{
		var entryStatus = ((CusEntryHeader)message.EM_LinkedObject).CH_EntryStatus;
		return entryStatus == StatusCodes.MRNAllocated;
	}

	protected override string MessageSequenceInvalidMessage => Res.GetString("0C75DB0D-1B56-4C0A-A3A0-DB4E5156C118", "Entry Status was not {0}", StatusCodes.MRNAllocated);
}
