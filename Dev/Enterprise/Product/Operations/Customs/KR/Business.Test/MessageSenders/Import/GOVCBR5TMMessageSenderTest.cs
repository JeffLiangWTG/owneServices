using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5TMMessageSenderTest : TestCaseWithFactory
	{
		IEnumerable<JobDeclarationMiscMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5TMSenderForTest(MessageSendingObjects, Factory) : new GOVCBR5TMSender(MessageSendingObjects, Factory);

		IEnumerable<JobDeclarationMiscMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationMiscMessageSendingObject> parents;

		IEnumerable<JobDeclarationMiscMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(ZString.Empty, ZBool.True);
					entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._929;
					var sendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5TM, MessageFunctions.MessageFunctionCode.Original);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationMiscMessageSendingObject>();
					messageSendingObjects.FirstOrDefault().MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
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

		ZString GetStatusField(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TM);
			return cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}

		class GOVCBR5TMSenderForTest : GOVCBR5TMSender
		{
			public GOVCBR5TMSenderForTest(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory) : base(sendingObjects, factory)
			{
			}

			protected override Import5TMHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
