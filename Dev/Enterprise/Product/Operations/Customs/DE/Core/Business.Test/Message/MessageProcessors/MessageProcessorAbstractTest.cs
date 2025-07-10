using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(DEBranchCustomsApplicationTypeMessageProcessor<EDIMessage>))]
	public abstract class MessageProcessorAbstractTest<T, TEDIMessage> : TestCaseWithFactory
		where T : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TEDIMessage : EDIMessage
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", MessageFriendlyName, Processor.MessageFriendlyName);
		}

		public void TestMustHaveLinkedObject()
		{
			var processor = Processor;
			var type = processor.GetType();
			PropertyInfo propertyInfo = null;
			while (type != null)
			{
				propertyInfo = type.GetProperty("MustHaveLinkedObject", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);

				if (propertyInfo != null)
				{
					break;
				}

				type = type.BaseType;
			}
			var mustHaveLinkedObject = (bool)propertyInfo.GetValue(processor);
			AssertEquals(ExpectedMustHaveLinkedObject, mustHaveLinkedObject);
		}

		public void TestNeedAttachDocumentsToMessage()
		{
			var processor = Processor;
			var t = processor.GetType();
			PropertyInfo propertyInfo = null;
			while (t != null)
			{
				propertyInfo = t.GetProperty("NeedAttachDocumentsToMessage", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);

				if (propertyInfo != null)
				{
					break;
				}

				t = t.BaseType;
			}
			var needAttachDocumentsToMessage = (bool)propertyInfo.GetValue(processor);
			AssertEquals(ExpectedNeedAttachDocumentsToMessage, needAttachDocumentsToMessage);
		}

		public void TestNeedAttachDocumentsToLinkedObject()
		{
			var processor = Processor;
			var type = processor.GetType();
			PropertyInfo propertyInfo = null;
			while (type != null)
			{
				propertyInfo = type.GetProperty("NeedAttachDocumentsToLinkedObject", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);

				if (propertyInfo != null)
				{
					break;
				}

				type = type.BaseType;
			}
			var needAttachDocumentsToLinkedObject = (bool)propertyInfo.GetValue(processor);
			AssertEquals(ExpectedNeedAttachDocumentsToLinkedObject, needAttachDocumentsToLinkedObject);
		}

		public void TestDelayStatusError()
		{
			var processor = Processor;
			var type = processor.GetType();
			PropertyInfo propertyInfo = null;
			while (type != null)
			{
				propertyInfo = type.GetProperty("DelayStatusError", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);

				if (propertyInfo != null)
				{
					break;
				}

				type = type.BaseType;
			}
			var delayStatusError = (bool)propertyInfo.GetValue(processor);
			AssertEquals(ExpectedDelayStatusError, delayStatusError);
		}

		protected abstract ZString MessageFriendlyName { get; }

		protected abstract DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage> Processor { get; }

		protected virtual bool ExpectedMustHaveLinkedObject => true;

		protected virtual bool ExpectedNeedAttachDocumentsToMessage => false;

		protected virtual bool ExpectedNeedAttachDocumentsToLinkedObject => true;

		protected virtual bool ExpectedDelayStatusError => false;

		public void TestEntryLinesLockedAndCH_HighestLineNumberNot0AfterProcessing()
		{
			if (this is ITestEntryLinesLockedAfterProcessing testEntryLinesLockedAfterProcessing)
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = testEntryLinesLockedAfterProcessing.DeclarationMessageType;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				var entryHeader = (Declaration.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				var entryLine3 = entryHeader.MergedLines.AddNew();
				entryLine3.CL_LineNumber = 3;
				AssertEquals("PreReq: EntryLines unlocked", expected: false, entryHeader.LockNumberOfEntryLines);
				AssertEquals("PreReq: HighestLineNumber", (ZShort)0, entryHeader.CH_HighestLineNumber);

				CombineAssertions(() =>
				{
					var messageToProcess = (TEDIMessage)testEntryLinesLockedAfterProcessing.PrepareMessagesAndGetMessageToProcessForEntryLinesLockedTest(entryHeader);
					ProcessMessage(messageToProcess);
					AssertEquals("Message successfully processed", "PRS", messageToProcess.EM_Status);
					AssertEquals("HasBeenlodgedAtCustoms is true as EntryNum populated", expected: true, entryHeader.HasBeenLodgedAtCustoms);
					AssertEquals("EntryLines remain locked as LockNumberOfEntryLines is true", expected: true, entryHeader.LockNumberOfEntryLines);
					AssertEquals("HighestLineNumber remains as UpdateHighestLineNumber() triggered due to change of CH_Status during message processing and not set to 0 as 'LockNumberOfEntryLines' true", (ZShort)3, entryHeader.CH_HighestLineNumber);
				});
			}
			else
			{
				Assert(true);
			}
		}

		protected void ProcessMessage(TEDIMessage message, bool doSave = false)
		{
			using (message.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(message);
				if (message.EM_Status != EDIMessage.Status.Error)
				{
					currentProcessor = Processor;
					currentProcessor.ProcessMessage(message);

					if (doSave)
					{
						Factory.Save();
					}
				}
			}
		}

		protected List<AttachedDocument> SampleAttachedDocument => new List<AttachedDocument>()
		{
			new AttachedDocument()
			{
				FileName = "file1.pdf",
				Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
				ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
			}
		};

		protected StmNote GetStmNote(TEDIMessage message) => GetStmNote(message.PK, EDIMessageSchema.Constants.TableName, "Processing Log");

		protected StmNote GetStmNote(ZGuid parentID, ZString tableName, ZString description)
		{
			var stmNoteQuery = new ZQuery(StmNoteSchema.ST_Table, tableName);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_ParentID, parentID);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_Description, description);
			return Factory.LoadTop1<StmNote>(stmNoteQuery);
		}

		protected EDIMessage CreateOriginalMessageLinkedToParent<K>(BusinessObject objectToLink, ZString messageIdentifier)
			where K : EDIMessage
		{
			var originalMessage = Factory.New<K>();
			Factory.Save();
			originalMessage.EM_MessageNum = messageIdentifier;
			originalMessage.EM_LinkedObject = objectToLink;
			originalMessage.EM_Status = EDIMessage.Status.Sent;
			return originalMessage;
		}

		protected EDIMessage CreateOriginalMessageLinkedToParent<K>(BusinessObject objectToLink, ZString messageIdentifier, string senderEmailAddress)
			where K : EDIMessage
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = senderEmailAddress;
			var originalMessage = CreateOriginalMessageLinkedToParent<K>(objectToLink, messageIdentifier);
			originalMessage.EM_SystemCreateUser = user.GS_Code;
			return originalMessage;
		}

		protected void AssertEmailForSingleRecipientWithTable(ZString testCase, EmailDef email, ZString recipient, ZString subject, ZString bodyTitle, ZString bodyMessageHeader, ZString bodyMessageSummary, ZString bodyMessageTable)
		{
			AssertEmailForSingleRecipient(testCase, email, recipient, subject, bodyTitle, bodyMessageHeader, bodyMessageSummary);
			AssertContains(testCase + "Body-Message-Table", bodyMessageTable, email.Body);
		}

		protected void AssertEmailForSingleRecipient(ZString testCase, EmailDef email, ZString recipient, ZString subject, ZString bodyTitle, ZString bodyMessageHeader, ZString bodyMessageSummary)
			=> AssertEmail(testCase, email, new[] { recipient }, subject, bodyTitle, bodyMessageHeader, bodyMessageSummary);

		protected void AssertEmail(ZString testCase, EmailDef email, ZString[] recipients, ZString subject, ZString bodyTitle, ZString bodyMessageHeader, ZString bodyMessageSummary)
		{
			var body = email.Body;
			AssertContainsExactElementsInAnyOrder(testCase + "Recipients", recipients, email.Recipients.Cast<RecipientDef>().Select(x => x.Email));
			AssertContains(testCase + "Subject", subject, email.Subject);
			AssertContains(testCase + "Body-Title", bodyTitle, body);
			AssertContains(testCase + "Body-Message-Header", bodyMessageHeader, body);
			AssertContains(testCase + "Body-Message-Summary", bodyMessageSummary, body);
		}

		protected void AssertEmailWithTable(ZString testCase, EmailDef email, ZString[] recipients, ZString subject, ZString bodyTitle, ZString bodyMessageHeader, ZString bodyMessageSummary, ZString bodyMessageTable)
		{
			AssertEmail(testCase, email, recipients, subject, bodyTitle, bodyMessageHeader, bodyMessageSummary);
			AssertContains(testCase + "Body-Message-Table", bodyMessageTable, email.Body);
		}

		protected void AssertDocumentLinkingSubscribers(BusinessObject[] subscribers)
		{
			AssertContainsExactElementsInAnyOrder("Document Linking Subscribers", subscribers, GetDocumentLinking());
		}

		protected DocumentLinking GetDocumentLinking()
		{
			var t = currentProcessor.GetType();
			FieldInfo fieldInfo = null;
			while (t != null)
			{
				fieldInfo = t.GetField("documentLinking", BindingFlags.Instance | BindingFlags.NonPublic);

				if (fieldInfo != null)
				{
					break;
				}

				t = t.BaseType;
			}

			return (DocumentLinking)fieldInfo.GetValue(currentProcessor);
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}
		protected LoggingInformation logger;

		DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage> currentProcessor;
	}

	interface ITestEntryLinesLockedAfterProcessing
	{
		string DeclarationMessageType { get; }
		EDIMessage PrepareMessagesAndGetMessageToProcessForEntryLinesLockedTest(Declaration.CusEntryHeader entryHeader);
	}
}
