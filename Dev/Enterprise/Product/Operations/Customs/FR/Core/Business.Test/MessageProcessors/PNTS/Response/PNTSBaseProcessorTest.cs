using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	abstract class PNTSBaseProcessorTest<T, TProcessor> : TestCaseWithFactory
		where T : class
		where TProcessor : PNTSBaseProcessor<T>
	{
		public void TestCorrectMessage()
		{
			PrepareTestData();
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			tempHeader.AMA_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			tempHeader.AMA_JobReference = "JobRef001";
			tempHeader.CustomsStatusDate = ZDateTime.BrettsBirthday;
			AddPropertiesForTemporaryStorageHeader(tempHeader);

			var processor = GetPNTSBaseProcessor();

			var message = Factory.New<PNTSEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_MessageType = "STO";
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = GetMessageText();
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("MessageStatus should be assigned.", MessageStatusCodeList.Codes.OK, message.EM_Status);
			AssertEquals("MessageSubType should not be changed.", GetMessageSubType(), message.EM_MessageSubType);
			AssertEquals("MessageInterpretation should be assigned.", GetExpectedMessageInterpretation(), message.EM_MessageInterpretation);
			AssertEquals("TemporaryHeader should be linked to the message.", tempHeader.PK, message.EM_LinkedObject.PK);

			AssertEquals("EntryStatus should be assigned.", GetExpectedCustomsStatus(), tempHeader.CustomsStatus);
			AssertEquals("Temporary Header Message Status should have been updated.", GetExpectedNewMessageStatus(), tempHeader.AMA_MessageStatus);
			AssertEquals("CRN should have been updated.", GetExpectedCRN(), tempHeader.CRN);
			AssertEquals("MRN should have been updated.", GetExpectedMRN(), tempHeader.MRN);
			AssertEquals("FRN should have been updated.", GetExpectedFRN(), tempHeader.FRN);
			AssertEquals("Customs Status Date should have been updated.", GetExpectedCustomsStatusDate(), tempHeader.RegistrationEntryNumber.CE_IssueDate);
		}

		protected virtual void PrepareTestData()
		{
			PNTSMessageTestHelper.SetUpStatusAndDescriptionMaps(Factory);
		}

		public void TestBadlyFormattedMessage()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			tempHeader.AMA_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			tempHeader.AMA_JobReference = "JobRef001";
			tempHeader.CustomsStatusDate = ZDateTime.BrettsBirthday;
			AddPropertiesForTemporaryStorageHeader(tempHeader);

			var processor = GetPNTSBaseProcessor();

			var message = Factory.New<PNTSEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_MessageType = "STO";
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = $"<{typeof(T).Name.ToUpper()}></{typeof(T).Name.ToUpper()}>";
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			Factory.Save();
			processor.ProcessMessage(message);

			AssertEquals("MessageStatus should be discard", MessageStatusCodeList.Codes.DCD, message.EM_Status);
			AssertEquals("MessageSubType should not be changed.", GetMessageSubType(), message.EM_MessageSubType);
			AssertEquals("No TemporaryHeader should be linked.", null, message.EM_LinkedObject);
			AssertEquals("Error note should be added to the message.", GetExpectedErrorTextIfEntryNumberNotFound(), (message.Notes.GetAllNotes().Last() as StmNote).ST_NoteText);

			AssertEquals("Temporary Header Message Status should not have been updated.", "", tempHeader.AMA_MessageStatus);
		}

		protected ApplicationTypeMessageProcessor GetPNTSBaseProcessor() => (TProcessor)Activator.CreateInstance(typeof(TProcessor), new BatchProcessor.LoggingInformation());

		protected abstract ZString GetMessageText();

		protected abstract ZString GetExpectedMessageInterpretation();

		protected abstract void AddPropertiesForTemporaryStorageHeader(TemporaryStorageHeader header);

		protected abstract ZString GetExpectedCustomsStatus();

		protected abstract ZString GetExpectedNewMessageStatus();

		protected abstract ZDateTime GetExpectedCustomsStatusDate();

		protected abstract ZString GetExpectedCRN();

		protected abstract ZString GetExpectedMRN();

		protected abstract ZString GetExpectedFRN();

		protected abstract CusEntryNumber GetCusEntryNumber(TemporaryStorageHeader tempHeader);

		protected abstract ZString GetExpectedErrorTextIfEntryNumberNotFound();

		protected abstract ZString GetMessageSubType();
	}
}
