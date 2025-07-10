using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public abstract class MonthlyClosingDeclarationSender
	{
		protected MonthlyClosingDeclarationSender(CusReconDeclaration declaration, ZString messageName, IImportMessageHeader messageHeaderProvider, ZString messageRole)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
			this.messageHeaderProvider = messageHeaderProvider;
			this.messageRole = messageRole;
			MessageBuilder = MonthlyClosingMessageBuilderLoader.Instance.GetMessageBuilder(messageName, messageHeaderProvider);
		}
		protected readonly CusReconDeclaration Declaration;
		protected readonly IProduceMessageXml MessageBuilder;
		protected readonly IImportMessageHeader messageHeaderProvider;
		readonly ZString messageRole;

		protected IImportHeader DataProvider => messageHeaderProvider.Header;

		protected IEnumerable<int> LineNumbersInMessage10_2 => throw new NotImplementedException("ATLAS version 10.2 is not implemented yet");

		protected abstract IEnumerable<int> LineNumbersInMessage10_1 { get; }

		public abstract bool MessageHasInformationToSend { get; }

		protected void UpdateFinalizationFlagNote()
		{
			switch (messageRole)
			{
				case MonthlyClosingMessageRoleList.Codes.FinalMessage:
				case MonthlyClosingMessageRoleList.Codes.FinalizationMessage:
					Declaration.CreateFinalizationFlagNote(MonthlyClosingHelper.DeclarationIsFinalizedFlag);
					break;
				case MonthlyClosingMessageRoleList.Codes.AmendmentMessage:
				case MonthlyClosingMessageRoleList.Codes.FirstPartialMessage:
					Declaration.CreateFinalizationFlagNote(MonthlyClosingHelper.DeclarationNotFinalizedFlag);
					break;
			}
		}

		public void Send()
		{
			var message = Declaration.Factory.New<AtlasEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = EDIMessageTypeList.Codes.Import;
			message.EM_MessageSubType = messageHeaderProvider.MessageGroup;
			message.SetEM_MessageTextOrDataSource(MessageBuilder.GetXMLMessage());
			message.EM_LinkedObject = Declaration;
			message.EM_ApplicationReference = MessageBuilder.MessageTechnicalName;
			message.SetLogbookEORIBranchSuffix(messageHeaderProvider.InterchangeSender.EoriBranchSuffix);
			message.SetLogbookLocalReferenceNumber(DataProvider.LocalReferenceNumber);
			message.CreateMonthlyClosingLinesNote(MessageVersionRegistry.CurrentAtlasVersion == ATLASVersionNumberList.Codes._102 ? LineNumbersInMessage10_2 : LineNumbersInMessage10_1);

			Declaration.CRD_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
			Declaration.Messages.Add(message);

			UpdateFinalizationFlagNote();
		}
	}
}
