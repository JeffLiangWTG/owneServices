using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer.Testing
{
	sealed class DIFDocumentEventMessageProcessorTest : TestCaseWithFactory
	{
		public void TestUpdateStatus()
		{
			var currentStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			var outMessage = (EDIMessage)Factory.New<Integration.Customs.CA.IEDIMessage>();
			outMessage.FillWithValidTestData();
			outMessage.EM_LinkUniqueID = addInfo.PK;
			outMessage.EM_LinkTable = addInfo.TablePrefix;
			outMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			outMessage.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			outMessage.EM_SystemCreateUser = currentStaffMember.GS_Code;
			outMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var inMessage = Factory.New<EDIMessage>();
			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + SuccessEmbeddedResourceName);
			UniversalEvent universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new DIFDocumentEventMessageProcessor(logger, universalEvent, inMessage, addInfo);
			processor.Process();
			AssertEquals(StatusList.Codes.AcknowledgedOriginal, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AwaitingAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.AcknowledgedAmendment, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AwaitingChange;
			processor.Process();
			AssertEquals(StatusList.Codes.AcknowledgedChange, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AwaitingWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcknowledgedWithdrawal, addInfo.EX_Status);

			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + FailEmbeddedResourceName);
			var universalEventFail = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new DIFDocumentEventMessageProcessor(logger, universalEventFail, inMessage, addInfo);

			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.ErrorAcknowledgedOriginal, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AwaitingAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.ErrorAcknowledgedAmendment, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AwaitingChange;
			processor.Process();
			AssertEquals(StatusList.Codes.ErrorAcknowledgedChange, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AwaitingWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.ErrorAcknowledgedWithdrawal, addInfo.EX_Status);
		}

		public void TestUpdateStatusFromCBSAResponse()
		{
			var currentStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			var outMessage = (EDIMessage)Factory.New<Integration.Customs.CA.IEDIMessage>();
			outMessage.FillWithValidTestData();
			outMessage.EM_LinkUniqueID = addInfo.PK;
			outMessage.EM_LinkTable = addInfo.TablePrefix;
			outMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			outMessage.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			outMessage.EM_SystemCreateUser = currentStaffMember.GS_Code;
			outMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var inMessage = Factory.New<EDIMessage>();
			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + AcceptedEmbeddedResourceName);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			addInfo.EX_Status = StatusList.Codes.AcknowledgedOriginal;

			var processor = new DIFDocumentEventMessageProcessor(logger, universalEvent, inMessage, addInfo);
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedOriginal, addInfo.EX_Status);

			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedOriginal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedOriginal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedOriginal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedOriginal, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedAmendment, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedAmendment, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedAmendment, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedAmendment, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedChange;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedChange, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingChange;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedChange, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedChange;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedChange, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedChange;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedChange, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedWithdrawal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedWithdrawal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedWithdrawal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.AcceptedWithdrawal, addInfo.EX_Status);

			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + RejectedEmbeddedResourceName);
			var universalEventFail = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new DIFDocumentEventMessageProcessor(logger, universalEventFail, inMessage, addInfo);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedAmendment, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedAmendment, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedAmendment, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedAmendment;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedAmendment, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedOriginal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedOriginal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedOriginal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedOriginal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedOriginal, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedChange;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedChange, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingChange;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedChange, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedChange;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedChange, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedChange;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedChange, addInfo.EX_Status);

			addInfo.EX_Status = StatusList.Codes.AcknowledgedWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedWithdrawal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AwaitingWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedWithdrawal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.RejectedWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedWithdrawal, addInfo.EX_Status);
			addInfo.EX_Status = StatusList.Codes.AcceptedWithdrawal;
			processor.Process();
			AssertEquals(StatusList.Codes.RejectedWithdrawal, addInfo.EX_Status);
		}

		public void TestEmailContent()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			var outMessage = (EDIMessage)Factory.New<Integration.Customs.CA.IEDIMessage>();
			outMessage.FillWithValidTestData();
			outMessage.EM_LinkUniqueID = addInfo.PK;
			outMessage.EM_LinkTable = addInfo.TablePrefix;
			outMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			outMessage.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			outMessage.EM_SystemCreateUser = currentStaff.GS_Code;
			outMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var inMessage = Factory.New<EDIMessage>();
			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + SuccessEmbeddedResourceName);
			inMessage.EM_MessageText = Regex.Replace(inMessage.EM_MessageText, @"\s", string.Empty);
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			universalEvent.EventParameters.RequestNumber = "REF001";
			var processor = new DIFDocumentEventMessageProcessor(logger, universalEvent, inMessage, addInfo);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIF Document"));
			AssertNotNull(email);
			AssertContains("REF001", email.Subject);
			AssertContains("Receipt Number", email.Body);
			AssertContains("<a href=\"edient", email.Body);
			AssertEquals(
				@"<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CADIFDocument</Type>
          <Key>REF001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2013-12-01T20:17:41</EventTime>
    <EventType>DDV</EventType>
    <EventParameters>
      <ReferenceNumber>10207000013799</ReferenceNumber>
      <RequestNumber>REF001</RequestNumber>
      <ReceiptNumber>80db27d9-0402-4955-9837-5403c80555df</ReceiptNumber>
    </EventParameters>
  </Event>
</UniversalEvent>", inMessage.EM_MessageText);

			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + FailEmbeddedResourceName);
			var universalEventFail = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new DIFDocumentEventMessageProcessor(logger, universalEventFail, inMessage, addInfo);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIF Document"));
			AssertNotNull(email);
			AssertContains("Reason", email.Body);
		}

		public void TestEmailContentForCBSA()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@cargowise.com";
			currentStaff.GS_Code = "~1";
			var outMessage = (EDIMessage)Factory.New<Integration.Customs.CA.IEDIMessage>();
			outMessage.FillWithValidTestData();
			outMessage.EM_LinkUniqueID = addInfo.PK;
			outMessage.EM_LinkTable = addInfo.TablePrefix;
			outMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			outMessage.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			outMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			outMessage.EM_SystemCreateUser = currentStaff.GS_Code;
			outMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var inMessage = Factory.New<EDIMessage>();
			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + AcceptedEmbeddedResourceName);
			inMessage.EM_MessageText = Regex.Replace(inMessage.EM_MessageText, @"\s", string.Empty);
			UniversalEvent universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			universalEvent.EventParameters.RequestNumber = "REF001";
			var processor = new DIFDocumentEventMessageProcessor(logger, universalEvent, inMessage, addInfo);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIF Document"));
			AssertNotNull(email);
			AssertContains("REF001", email.Subject);
			AssertContains("Department", email.Body);
			AssertContains("is accepted by CBSA", email.Subject);
			AssertContains("<a href=\"edient", email.Body);
			AssertEquals(
				@"<UniversalEvent>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CADIFDocument</Type>
          <Key>REF001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2013-12-01T20:17:41</EventTime>
    <EventType>DDV</EventType>
    <EventParameters>
      <RequestNumber>10207900000602</RequestNumber>
      <Department>CBSA</Department>
    </EventParameters>
  </Event>
</UniversalEvent>", inMessage.EM_MessageText);

			inMessage.EM_MessageText = resourceRetriever.GetString(EmbeddedResourcePrefix + RejectedEmbeddedResourceName);
			var universalEventFail = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			processor = new DIFDocumentEventMessageProcessor(logger, universalEventFail, inMessage, addInfo);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("DIF Document"));
			AssertNotNull(email);
			AssertContains("Department", email.Body);
			AssertContains("is rejected by CBSA", email.Subject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var requiredDocument = ((IDISHost)jobDeclaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_Status = StatusList.Codes.AwaitingOriginal;
			addInfo.EX_ReferenceNumber = "REF001";
			Factory.Save();
		}
		EmbeddedResourceRetriever resourceRetriever;
		IXmlSessionTracker logger;
		JobRequiredDocumentAddInfo addInfo;

		const string EmbeddedResourcePrefix = "Enterprise.Customs.CA.DIF.Business.Testing.UniversalDataTransfer.TestFiles.";

		const string SuccessEmbeddedResourceName = "UniversalEventSuccess.xml";

		const string FailEmbeddedResourceName = "UniversalEventFail.xml";

		const string AcceptedEmbeddedResourceName = "CBSA Accepted.xml";

		const string RejectedEmbeddedResourceName = "CBSA Rejected.xml";
	}
}
