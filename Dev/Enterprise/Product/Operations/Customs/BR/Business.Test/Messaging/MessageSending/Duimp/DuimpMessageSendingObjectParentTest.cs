using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DuimpMessageSendingObjectParent))]
	class DuimpMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
	{
		public override void TestSendMessagesAndSave()
		{
			var sendingObjParent = (BaseMessageSendingObjectParent)GetNewBusinessObject();
			var declaration = sendingObjParent.TopLevelBusinessObject as JobDeclaration;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			sendingObjParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().ForEach(x => x.ShouldSend = false);
			AssertEquals("No messages sent", 0, (sendingObjParent as IMessageSendingObjectParent).SendMessagesAndSave());

			sendingObjParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().ForEach(x => x.ShouldSend = true);
			AssertEquals("Four messages sent", 2, (sendingObjParent as IMessageSendingObjectParent).SendMessagesAndSave());

			foreach (var entryHeader in declaration.CustomsEntryHeaders)
			{
				foreach (EDIMessage message in entryHeader.Messages)
				{
					AssertEquals("EDIMessage created", ShowCreateInterchangeOnSendingMessage ? Constants.EDIMessageStatusCodes.Manual : EDIMessage.Status.Queued, message.EM_Status);
					AssertEquals("EDIMessage saved", true, message.IsInDatabase);
					if (ShowCreateInterchangeOnSendingMessage)
					{
						AssertEquals("Interchange saved", true, message.Interchange.IsInDatabase);
						AssertEquals("Interchange.EI_TransportType", EDIInterchange.TransportType.tXT, message.Interchange.EI_TransportType);
						AssertEquals("Interchange.EI_Status", Constants.EDIMessageStatusCodes.Manual, message.Interchange.EI_Status);
					}
					else
					{
						AssertNull("Interchange Null", message.Interchange);
					}
				}
			}
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = (DuimpMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;

			AssertEquals("Properties count", 5, properties.Count());
			CombineAssertions(() =>
			{
				AssertEquals("MessageType - PropertyName", "MessageType", properties.ElementAt(0).PropertyName);
				AssertEquals("MovementReferenceNumber - PropertyName", "MovementReferenceNumber", properties.ElementAt(1).PropertyName);
				AssertEquals("SubmittedDate - PropertyName", "SubmittedDate", properties.ElementAt(2).PropertyName);
				AssertEquals("CustomsStatus - PropertyName", "CustomsStatus", properties.ElementAt(3).PropertyName);
				AssertEquals("MessageStatusDescription - PropertyName", "MessageStatusDescription", properties.ElementAt(4).PropertyName);

				AssertEquals("Msg. Type - ColumnWidth", 100, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Entry Number - ColumnWidth", 140, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Sub. Date - ColumnWidth", 140, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Status - ColumnWidth", 100, properties.ElementAt(3).ColumnWidth);
				AssertEquals("MessageStatusDescription - ColumnWidth", 180, properties.ElementAt(4).ColumnWidth);

				AssertEquals("Msg. Type - IsMandatory", true, properties.ElementAt(0).IsMandatory);
				AssertEquals("Entry Number - IsMandatory", true, properties.ElementAt(1).IsMandatory);
				AssertEquals("Sub. Date - IsMandatory", true, properties.ElementAt(2).IsMandatory);
				AssertEquals("Status - IsMandatory", true, properties.ElementAt(3).IsMandatory);
				AssertEquals("MessageStatusDescription - IsMandatory", false, properties.ElementAt(4).IsMandatory);

				AssertEquals("MovementReferenceNumber - Caption", "Entry Number", properties.ElementAt(1).ResourceString.Caption);
			});
		}

		public void TestGetSendingObjectsCollection()
		{
			var sendingObjParent = (BaseMessageSendingObjectParent)GetNewBusinessObject();
			var declaration = sendingObjParent.TopLevelBusinessObject as JobDeclaration;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CDI;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CDI;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.SUF;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.SUF;

			AssertEquals("Only formal entries can be sent", 2, sendingObjParent.SendingObjectsCollection.Count);
			Assert(sendingObjParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>().All(x => x.Header.CH_MessageType == MessageTypeList.Codes.CDI));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			return new DuimpMessageSendingObjectParent(declaration);
		}

		protected override bool ShowCreateInterchangeOnSendingMessage => false;

		protected override Type DeclarationMessageSendingObjectType => typeof(DuimpMessageSendingObject);
	}
}
