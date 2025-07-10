using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public abstract class CusEntryHeaderAmendmentMessageSender<TMessageDataProvider, TCurrentDataProvider, TCurrentDataHeaderInterface> : MessageSender
		where TMessageDataProvider : IMessageDataProvider
		where TCurrentDataProvider : IMessageDataProvider, TCurrentDataHeaderInterface
		where TCurrentDataHeaderInterface : IMessageDataProvider
	{
		protected CusEntryHeaderAmendmentMessageSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(factory)
		{
			if (messageSendingObjects == null)
			{
				throw new ArgumentException("messageSendingObjects must not be null.");
			}

			Parents = messageSendingObjects;
			var declarations = Parents.Select(x => x.Header.Declaration).Distinct();
			foreach (var declaration in declarations)
			{
				declaration.SetValidationModeOnElectronicMessaging(MessageType);
			}
		}
		protected readonly IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents;

		public override int Send()
		{
			var messages = new List<EDIMessage>();

			try
			{
				foreach (var parent in Parents)
				{
					var entry = parent.Header;

					var currentSnapshot = GetCurrentDataProvider(entry);
					currentSnapshot.RoundDecimalValueRoundedWithDecimalPlaces();
					var messageDataProvider = GetMessageDataProvider(entry, currentSnapshot, parent.AmendedItems);
					messageDataProvider.RoundDecimalValueRoundedWithDecimalPlaces();
					var stream = GetMessageBuilder(messageDataProvider, parent).MessageContent;

					var message = GenerateMessage(entry);
					messages.Add(message);

					if (ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(MessageType))
					{
						message.RecordVersionNumberFromCusEntryNum(entry, OriginalMessageType);
					}
					else
					{
						message.RecordVersionNumberFromCH_VersionID(entry);
					}
					message.EM_MessageSubType = parent.AmendmentType;
					message.SetEM_MessageTextOrDataSource(stream);

					ReserveStream(stream);

					OnSent(entry, currentSnapshot, parent.AmendmentVersion + 1);
				}
			}
			catch (Exception)
			{
				messages.ForEach(x => x.Delete());
				messages.Clear();
				throw;
			}
			return messages.Count;
		}

		public ZString OriginalMessageType => ElectronicDocumentTypeList.GetOriginalType(MessageType);

		protected virtual EDIMessage GenerateMessage(CusEntryHeader entry)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageType;
			entry.Messages.Add(message);
			return message;
		}
		protected abstract void UpdateMessageStatus(CusEntryHeader parent);
		protected abstract IMessageBuilder GetMessageBuilder(TMessageDataProvider messageDataProvider, IAmendmentDetails amendmentDetails);
		protected abstract TCurrentDataProvider GetCurrentDataProvider(CusEntryHeader parent);
		protected abstract TMessageDataProvider GetMessageDataProvider(CusEntryHeader parent, TCurrentDataProvider currentSnapshot, AmendedItemCollection amendedItems);

		void OnSent(CusEntryHeader entry, TCurrentDataProvider currentDataProvider, ZShort snapshotVersion)
		{
			UpdateMessageStatus(entry);
			CreateSnapshot(entry, currentDataProvider, snapshotVersion);
			entry.Declaration.RemoveValidationModeOnElectronicMessaging(MessageType);
		}

		void CreateSnapshot(CusEntryHeader parent, TCurrentDataProvider currentDataProvider, ZShort snapshotVersion)
		{
			var stream = KRXmlObjectSerializer.Serialize(currentDataProvider);
			ReserveStream(stream);
			AccumulativeAmendmentManager.CreateNewSnapshot(parent, OriginalMessageType, stream, snapshotVersion);
		}
	}
}
