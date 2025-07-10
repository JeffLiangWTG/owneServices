using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public static class MessageSendingTestHelper
	{
		public static JobDeclarationMessageSendingObject GetSendingObjectFromEntry(NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> collection, CusEntryHeader entry)
		{
			var sendingObject = collection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault(x => x.Header.PK.Equals(entry.PK));
			if (sendingObject.MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration || sendingObject.MessageType == CDSEDIMessageTypeList.Codes.ArrivalNotification)
			{
				AmendmentMessageHelper.Instance.CanBeAmended(sendingObject);
			}
			return sendingObject;
		}

		public static JobDeclarationMessageSendingObject GetSendingObjectFromEntry(NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> collection, CusEntryHeader entry, EU.Business.WorldCustomsOrganisation.MessageBuilders.AmendmentMessageHelper messageHelper)
		{
			var sendingObject = collection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault(x => x.Header.PK.Equals(entry.PK));
			if (sendingObject.MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration || sendingObject.MessageType == CDSEDIMessageTypeList.Codes.ArrivalNotification)
			{
				messageHelper.CanBeAmended(sendingObject);
			}
			return sendingObject;
		}

		public static void CreateOriginalNewMessage(Business.Declaration.CusEntryHeader entryHeader)
		{
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xml = xml.Replace(GB.Business.Declaration.CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entryHeader.CH_BGMReference); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			xml = xml.Replace(GB.Business.Declaration.CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entryHeader.DeclarationUCR); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			xml = xml.Replace(GB.Business.Declaration.CusEntryHeader.LRNReferencePlaceHolderXmlFriendly, entryHeader.LRN); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save

			var originalMessageToAdd = entryHeader.Messages.AddNew(typeof(CDSNewDeclarationEDIMessage));
			originalMessageToAdd.EM_MessageType = "NEW";
			originalMessageToAdd.EM_MessageText = xml;
			originalMessageToAdd.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		public static void CreateNewAmendmentMessage(Business.Declaration.CusEntryHeader entryHeader, string messageStatus = EDIMessageStatusList.Codes.Acknowledged)
		{
			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new CusdecMessageFunction.New());
			var xml = messageBuilder.Build();
			xml = xml.Replace(GB.Business.Declaration.CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entryHeader.CH_BGMReference); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			xml = xml.Replace(GB.Business.Declaration.CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entryHeader.DeclarationUCR); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			xml = xml.Replace(GB.Business.Declaration.CusEntryHeader.LRNReferencePlaceHolderXmlFriendly, entryHeader.LRN); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save

			var originalMessageToAdd = entryHeader.Messages.AddNew(typeof(CDSAmendmentComparisonEDIMessage));
			originalMessageToAdd.EM_MessageType = "NAM";
			originalMessageToAdd.EM_MessageText = xml;
			originalMessageToAdd.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessageToAdd.EM_Status = messageStatus;
		}

		public static void CreateMessageFromSampleXML(Business.Declaration.CusEntryHeader entryHeader, ZString sampleXml, Type typeOfMessage, ZString messageType)
		{
			sampleXml = sampleXml.Replace(GB.Business.Declaration.CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entryHeader.CH_BGMReference); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			sampleXml = sampleXml.Replace(GB.Business.Declaration.CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entryHeader.DeclarationUCR); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save
			sampleXml = sampleXml.Replace(GB.Business.Declaration.CusEntryHeader.LRNReferencePlaceHolderXmlFriendly, entryHeader.LRN); // Will still contain placeholders until flipped; in the real world this is handled by the DeclarationMessageBuilder on save

			var originalMessageToAdd = entryHeader.Messages.AddNew(typeOfMessage);
			originalMessageToAdd.EM_MessageType = messageType;
			originalMessageToAdd.EM_MessageText = sampleXml;
			originalMessageToAdd.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}
	}
}
