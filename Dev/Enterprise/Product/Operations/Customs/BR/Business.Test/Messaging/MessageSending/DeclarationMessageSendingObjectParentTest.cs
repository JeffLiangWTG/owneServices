using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	abstract class DeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetSendingObjectsCollectioCore()
		{
			var sendingObjParent = (BaseMessageSendingObjectParent)GetNewBusinessObject();
			var declaration = sendingObjParent.TopLevelBusinessObject as JobDeclaration;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(2, sendingObjParent.SendingObjectsCollection.Count);
			foreach (var sendingObject in sendingObjParent.SendingObjectsCollection)
			{
				AssertType("Sending Object Type", DeclarationMessageSendingObjectType, sendingObject);
			}
		}

		public virtual void TestSendMessagesAndSave()
		{
			var sendingObjParent = (BaseMessageSendingObjectParent)GetNewBusinessObject();
			var declaration = sendingObjParent.TopLevelBusinessObject as JobDeclaration;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			sendingObjParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().ForEach(x => x.ShouldSend = false);
			AssertEquals("No messages sent", 0, (sendingObjParent as IMessageSendingObjectParent).SendMessagesAndSave());

			sendingObjParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().ForEach(x => x.ShouldSend = true);
			AssertEquals("Two messages sent", 2, (sendingObjParent as IMessageSendingObjectParent).SendMessagesAndSave());

			foreach (var entryHeader in declaration.CustomsEntryHeaders)
			{
				var message = entryHeader.Messages[0];

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

		public void TestSendMessagesAndSave_RollbackOnSavingFailed()
		{
			var sendingObjParent = (BaseMessageSendingObjectParent)GetNewBusinessObject();
			var declaration = sendingObjParent.TopLevelBusinessObject as JobDeclaration;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			sendingObjParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().ForEach(x => x.ShouldSend = true);

			Factory.Saving += SetInvalidImporter;
			void SetInvalidImporter(BusinessObjectFactory factory)
			{
				declaration.JE_OH_Importer = ZGuid.NewZGuid();
			}

			AssertEquals("No messages sent", 0, (sendingObjParent as IMessageSendingObjectParent).SendMessagesAndSave());
			foreach (var entryHeader in declaration.CustomsEntryHeaders)
			{
				AssertEquals("No message sent for Entry", 0, entryHeader.Messages.Count);
				AssertEquals("CH_Status rollback", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("CH_EntrySubmittedDate rollback", ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
				AssertEquals("No log created", 0, entryHeader.Logs.LogsNotInDB.Length);
			}
			Factory.Saving -= SetInvalidImporter;
		}

		protected virtual bool ShowCreateInterchangeOnSendingMessage => false;

		protected abstract Type DeclarationMessageSendingObjectType { get; }
	}
}
