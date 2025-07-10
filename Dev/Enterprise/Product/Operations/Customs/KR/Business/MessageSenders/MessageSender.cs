using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public abstract class MessageSender<TParent, TDataProvider> : MessageSender
		where TParent : IEDIMessageCollectionProvider
		where TDataProvider : IMessageDataProvider
	{
		protected MessageSender(IEnumerable<TParent> parents, BusinessObjectFactory factory)
			: base(factory)
		{
			Parents = parents;
		}
		protected readonly IEnumerable<TParent> Parents;

		public override int Send()
		{
			var result = new List<EDIMessage>();

			try
			{
				foreach (var parent in Parents)
				{
					var messages = CreateMessages(parent);
					result.AddRange(messages);

					foreach (EDIMessage message in messages)
					{
						var messageDataProvider = GetMessageDataProvider(parent, GetMessageID(message));
						messageDataProvider.RoundDecimalValueRoundedWithDecimalPlaces();

						var stream = GetMessageBuilder(messageDataProvider).MessageContent;
						message.SetEM_MessageTextOrDataSource(stream);
						parent.Messages.Add(message);

						CreateSnapshotIfNeeded(parent, messageDataProvider);
						ReserveStream(stream);
					}
					OnSent(parent);
				}
			}
			catch (Exception)
			{
				result.ForEach(x => x.Delete());
				result.Clear();
				throw;
			}
			return result.Count;
		}

		protected virtual IEnumerable<EDIMessage> CreateMessages(TParent parent)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageType;
			return new EDIMessage[1] { message };
		}

		protected virtual ZString GetMessageID(EDIMessage message) => ZString.Empty;

		void CreateSnapshotIfNeeded(TParent parent, TDataProvider messageDataProvider)
		{
			if (ShouldCreateSnapshot)
			{
				CreateSnapshotCore(parent, messageDataProvider);
			}
		}

		protected virtual void CreateSnapshotCore(TParent parent, TDataProvider messageDataProvider)
		{
		}

		bool ShouldCreateSnapshot => ElectronicDocumentTypeList.GenerateSnapshot(MessageType);
		protected abstract void UpdateMessageStatus(TParent parent);
		protected abstract IMessageBuilder GetMessageBuilder(TDataProvider messageDataProvider);
		protected abstract TDataProvider GetMessageDataProvider(TParent parent, ZString messageID);

		void OnSent(TParent parent)
		{
			UpdateMessageStatus(parent);
			OnSentCore(parent);
		}

		protected virtual void OnSentCore(TParent parent)
		{
		}
	}

	public abstract class MessageSender : IDisposable
	{
		public static MessageSender New(IEnumerable<JobDeclarationMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory, string messageType, MessageFunctionCode messageFunctionCode)
		{
			MessageSender result = null;
			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5BA:
					result = new GOVCBR5BASender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5UL:
					result = new GOVCBR5ULSender(messageSendingObjects.Cast<PenaltyRefundRequestMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5UA:
					result = new GOVCBR5UASender(messageSendingObjects.Cast<PenaltyExemptionRequestMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5BB:
					result = new GOVCBR5BBSender(messageSendingObjects.Cast<JobDeclarationAmendmentMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._830:
					result = new GOVCBR830Sender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5DP:
				case ElectronicDocumentTypeList.Codes._5DQ:
					result = new GOVCBR5DP5DQSender(messageSendingObjects.Select(x => x.Header), messageType, factory);
					break;
				case ElectronicDocumentTypeList.Codes._5DR:
				case ElectronicDocumentTypeList.Codes._5DS:
					if (messageFunctionCode == MessageFunctionCode.Amendment)
					{
						result = new GOVCBR5DS5DRAmendmentSender(messageSendingObjects.Cast<JobDeclarationAmendmentMessageSendingObject>(), messageType, factory);
					}
					else if (messageFunctionCode == MessageFunctionCode.Cancellation)
					{
						result = new GOVCBR5DR5DSCancellationSender(messageSendingObjects.Cast<JobDeclarationMiscMessageSendingObject>(), messageType, factory);
					}
					break;
				case ElectronicDocumentTypeList.Codes._5AS:
					if (messageFunctionCode == MessageFunctionCode.Amendment)
					{
						result = new GOVCBR5ASAmendmentSender(messageSendingObjects.Cast<JobDeclarationAmendmentMessageSendingObject>(), factory);
					}
					else if (messageFunctionCode == MessageFunctionCode.Extend)
					{
						result = new GOVCBR5ASExtendOfPeriodSender(messageSendingObjects.Cast<JobDeclarationMiscMessageSendingObject>(), factory);
					}
					break;
				case ElectronicDocumentTypeList.Codes._DKJ:
					result = new GOVCBRDKJSender(messageSendingObjects.Cast<JobDeclarationMiscMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._DF3:
					result = new GOVCBRDF3MessageSender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5SC:
					result = new GOVCBR5SCSender(messageSendingObjects.Select(x => x.Header), factory, messageType);
					break;
				case ElectronicDocumentTypeList.Codes._DHR:
					result = new GOVCBRDHRSender(messageSendingObjects.Select(x => x.Header), factory, messageType);
					break;
				case ElectronicDocumentTypeList.Codes._929:
					result = new GOVCBR929Sender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5BF:
					result = new GOVCBR5BFSender(messageSendingObjects.Cast<CancellationMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5BD:
					result = new GOVCBR5BDSender(messageSendingObjects.Cast<EarlyReleaseMiscMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5SI:
					result = new GOVCBR5SISender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._105:
					result = new GOVCBR105AmendmentSender(messageSendingObjects.Cast<JobDeclarationAmendmentMessageSendingObject>(), factory, messageType);
					break;
				case ElectronicDocumentTypeList.Codes._DHS:
					result = new GOVCBRDHSAmendmentSender(messageSendingObjects.Cast<JobDeclarationAmendmentMessageSendingObject>(), factory, messageType);
					break;
				case ElectronicDocumentTypeList.Codes._5TM:
					result = new GOVCBR5TMSender(messageSendingObjects.Cast<JobDeclarationMiscMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._934:
					result = new GOVCBR934Sender(messageSendingObjects.Cast<ValuationDeclarationMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._008:
					result = new GOVCBR008Sender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._D87:
					result = new GOVCBRD87Sender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5SM:
					result = new GOVCBR5SMSender(messageSendingObjects.Select(x => x.Header), factory);
					break;
				case ElectronicDocumentTypeList.Codes._D72:
					result = new GOVCBRD72Sender(messageSendingObjects.Cast<ExtendReExportDateMessageSendingObject>().Where(x => x.ShouldSend), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5FN:
					result = new GOVCBR5FNSender(messageSendingObjects.Cast<JobDeclarationMiscMessageSendingObject>(), factory);
					break;
				case ElectronicDocumentTypeList.Codes._5FE:
					result = new GOVCBR5FEAmendmentSender(messageSendingObjects.Cast<JobDeclarationAmendmentMessageSendingObject>(), factory);
					break;
				default:
					break;
			}
			return result;
		}

		protected MessageSender(BusinessObjectFactory factory)
		{
			Factory = factory;
			streams = new List<System.IO.Stream>();
		}
		public abstract ZString MessageType { get; }
		public abstract int Send();
		public static ZString MessageSendSuccessful => ResString.GetMultilingualString("F3E9D8C2-2716-4B78-8F52-7BA2E1403C55", "{0} Message(s) sent successfully");
		public static ZString MessageSendFailure => ResString.GetMultilingualString("B6EAE5DE-1414-42FC-BD56-39309F3B928D", "Failed to send message");

		protected readonly BusinessObjectFactory Factory;
		readonly List<System.IO.Stream> streams;

		protected void ReserveStream(System.IO.Stream stream)
		{
			streams.Add(stream);
		}

		void IDisposable.Dispose()
		{
			streams.ForEach(x => x.Dispose());
		}
	}
}
