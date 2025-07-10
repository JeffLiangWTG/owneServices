using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR934MessageSenderTest : TestCaseWithFactory
	{
		IEnumerable<JobDeclarationMiscMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR934SenderForTest(MessageSendingObjects, Factory) : new GOVCBR934Sender(MessageSendingObjects, Factory);

		IEnumerable<JobDeclarationMiscMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationMiscMessageSendingObject> parents;

		IEnumerable<JobDeclarationMiscMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var entry = declaration.CustomsEntryHeaders.AddNew();
					var entryLine = entry.MergedLines.AddNew();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._934;
					var sendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._934, MessageFunctions.MessageFunctionCode.Original);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		IEnumerable<JobDeclarationMiscMessageSendingObject> messageSendingObjects;

		public void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationMiscMessageSendingObject parent in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, GetStatusField(parent.Header));
			}
		}

		public void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = Parents.Single().Header.Messages;
			AssertEquals("One message exits before sending a message.", 1, messages.Count);
			AssertExceptionThrown<Exception>(() => GetMessageSender().Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 1, messages.Count);
		}
		public ZBool IsExceptionTest;

		ZString GetStatusField(CusEntryHeader entry) => entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934).CE_EntryStatus;
	}

	class GOVCBR934SenderForTest : GOVCBR934Sender
	{
		public GOVCBR934SenderForTest(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects, factory)
		{
		}

		protected override Import934Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
	}
}
