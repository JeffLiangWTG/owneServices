using System;
using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CMRMessageResponseProcessorTestCase : TestCaseWithFactory
	{
		public void TestPreProcessMessageSetsStatusAndBranch()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MessageReference = "08156997662";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_MessageReference = "S00002158";
			hawb.CS_IsResponsePending = true;
			hawb.CS_IsPrealerted = false;

			var outgoingMessage = (CMRAIRCRMessage)hawb.Messages.AddNew(typeof(CMRAIRCRMessage));
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_MessageText = originalAIRCRMessage;
			Factory.Save();

			var incomingMessage = Factory.New<CMRAIRCRRMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = clearAIRCRRMessage;
			incomingMessage.EM_GB = ZGuid.Empty;

			AssertEquals("outgoingMessage.EM_GB", true, outgoingMessage.EM_GB.IsValid);

			var logger = new LoggingInformation();
			var processor = new CMRMessageResponseProcessor(logger, "ACR", "Air");
			processor.PreProcessMessage(incomingMessage);

			AssertEquals("EM_LinkedObject", hawb.PK, incomingMessage.EM_LinkedObject.PK);
			AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
			AssertEquals("EM_GB", outgoingMessage.EM_GB, incomingMessage.EM_GB);
		}

		public void TestPreProcessMessageSetsStatusAndLinkedObject()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MessageReference = "08156997662";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_MessageReference = "S00002158";
			hawb.CS_IsResponsePending = true;
			hawb.CS_IsPrealerted = false;

			var incomingMessage = Factory.New<CMRAIRCRRMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = clearAIRCRRMessage;
			incomingMessage.EM_GB = ZGuid.Empty;

			var logger = new LoggingInformation();
			var processor = new CMRMessageResponseProcessor(logger, "ACR", "Air");
			processor.PreProcessMessage(incomingMessage);

			AssertEquals("EM_LinkedObject", hawb.PK, incomingMessage.EM_LinkedObject.PK);
			AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
			AssertEquals("EM_GB", ZGuid.Empty, incomingMessage.EM_GB);
		}

		public void TestPreProcessMessageLogsError()
		{
			using (AUCustomsDataRegistry.Instance.IgnoreUnknownResponses.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var incomingMessage = Factory.New<CMRAIRCRRMessage>();
				incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
				incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMessage.EM_Status = EDIMessage.Status.Queued;
				incomingMessage.EM_MessageText = clearAIRCRRMessage;
				incomingMessage.EM_GB = ZGuid.Empty;

				var logger = new LoggingInformation();
				var processor = new CMRMessageResponseProcessor(logger, "ACR", "Air");
				processor.PreProcessMessage(incomingMessage);

				AssertEquals("EM_LinkedObject", null, incomingMessage.EM_LinkedObject);
				AssertEquals("EM_Status", EDIMessage.Status.Error, incomingMessage.EM_Status);
				AssertEquals("EM_GB", ZGuid.Empty, incomingMessage.EM_GB);
			}
		}

		public void TestPreProcessMessageThrows()
		{
			var incomingMessage = Factory.New<CMRAIRCRRMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = clearAIRCRRMessage;
			incomingMessage.EM_GB = ZGuid.Empty;

			var processor = new TestHelperCMRMessageResponseProcessor("TestMessage");
			processor.PreProcessMessage(incomingMessage);

			AssertEquals("EM_LinkedObject", null, incomingMessage.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Error, incomingMessage.EM_Status);
			AssertEquals("EM_GB", ZGuid.Empty, incomingMessage.EM_GB);

			AssertEquals("Report Email Sent", 1, processor.SentReportEmails.Count);
			var email = (EmailDef)processor.SentReportEmails[0];
			AssertEquals("Mail Subject", "TestMessage Message Processor Error Report", email.Subject);
			AssertContains(@"<br />
<b>There has been a problem processing the attached ACR (EDI Message) message.</b>
<br />
<br />
Error Details:<br />
<br />
Could not find an associated business object (Job) for document reference = 'AIRCRR/S00002158/1'
<br />
<br />
Status: ORIGINAL ACCEPTED<br>Status Description: THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS<br>", email.Body);
			AssertEquals("1 attachement", 3, email.Attachments.Count);
		}

		protected string originalAIRCRMessage = @"UNH+1+CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00002158/1:1+9'RFF+HWB:1'RFF+MWB:08156997662'NAD+CN+++MY TEST CONSIGNOR+CONSIGOR ADRESS+SYDNEY++2000+AU'NAD+CZ+++MY TEST CONSIGNEE+CONSIGEE ADRESS+AUKLAND+++NZ'NAD+VW+51001191402::95'TDT+20+569++6+QF::3'LOC+8+AUSYD::6'LOC+76+NZAKL::6'LOC+12+AUSYD::6'LOC+91+NZAKL::6'DTM+178:20041210:102'CNI+1'RFF+UCN:S00002158'MOA+96:NDV'GID+1'PAC+10'FTX+AAA+++STUFF'MEA+AAE+G+KG:100.00'UNT+21+1'";
		protected string clearAIRCRRMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+490F DAGH CDGE:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:S00002158/1::001'DTM+310:20041216010409:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

		public void TestNoReceiptEmailException()
		{
			var email = new EmailDef();
			AssertNoExceptionThrown(() => ((IErrorNotification)Processor).SendErrorToPostMaster(email));
		}

		public void TestReleasePendingOriginalOnWithdrawalAccepted()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			CMRMessageResponseProcessor.SendReportToWebUsers.Value = true;
			ResponseMessage.StatusToReturn = "WITHDRAWN";
			ResponseMessage.SetWrappedObject(DummyWithSentWithdrawalAndPendingOriginal);
			Processor.ProcessMessage(ResponseMessage);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, DummyWithSentWithdrawalAndPendingOriginal.Messages[1].EM_Status);
			AssertEquals("Notification Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Report Email Sent", 1, Processor.SentReportEmails.Count);
			var email = Processor.SentReportEmails[0] as EmailDef;
			AssertEquals("CC Recipient presented", 1, email.CCRecipients.Count);
			AssertEquals("CC Recipient address", "test@edi.com.au", email.CCRecipients[0].Email);
		}

		public void TestCancelPendingOriginalOnWithdrawalRejected()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			CMRMessageResponseProcessor.SendReportToWebUsers.Value = true;
			ResponseMessage.StatusToReturn = "REJECTED";
			ResponseMessage.SetWrappedObject(DummyWithSentWithdrawalAndPendingOriginal);
			Processor.ProcessMessage(ResponseMessage);
			AssertEquals("EM_Status", EDIMessage.Status.Cancelled, DummyWithSentWithdrawalAndPendingOriginal.Messages[1].EM_Status);
			AssertEquals("Notification Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Report Email Sent", 1, Processor.SentReportEmails.Count);
			var email = Processor.SentReportEmails[0] as EmailDef;
			AssertEquals("CC Recipient presented", 1, email.CCRecipients.Count);
			AssertEquals("CC Recipient address", "test@edi.com.au", email.CCRecipients[0].Email);
			AssertEquals("Subject", " Message - REJECTED", email.Subject);
		}

		public void TestSendReportToWebUsers()
		{
			CMRMessageResponseProcessor.SendReportToWebUsers.Value = false;
			AssertEquals("Should be False", false, CMRMessageResponseProcessor.SendReportToWebUsers.Value);
			CMRMessageResponseProcessor.SendReportToWebUsers.Value = true;
			AssertEquals("Should be True", true, CMRMessageResponseProcessor.SendReportToWebUsers.Value);
		}

		public void TestUnknownEntitySetsErrorStatus()
		{
			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Env.Registry.AUCustoms.ExportDeclarationSendErrorsToGroup = group.PK.ToGuid();
			Env.Registry.AUCustoms.ExportDeclarationSendErrors = Core.Constants.EmailTo.NoEmails;
			Factory.Save();

			var message = Factory.New<CMRSEACRRMessage>();
			message.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			var processor = new TestHelperCMRMessageResponseProcessor("TestMessage");
			processor.ProcessMessage(message);
			AssertEquals("Discarded", EDIMessage.Status.Error, message.EM_Status);
			AssertEquals("Report Email Sent", 1, processor.SentReportEmails.Count);
			var email = processor.SentReportEmails[0] as EmailDef;
			AssertEquals("Mail Subject", "TestMessage Message Processor Error Report", email.Subject);
			AssertContains(@"<br />
<b>There has been a problem processing the attached  (EDI Message) message.</b>
<br />
<br />
Error Details:<br />
<br />
Could not find an associated business object (Job) for document reference = 'SEACRR/42243-60087/SYD3'
<br />
<br />
Status: ORIGINAL ACCEPTED<br>Status Description: THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS<br>", email.Body);
			AssertEquals("1 attachement", 3, email.Attachments.Count);
		}

		public void TestUnknownEntitySetsErrorStatusWithRegistryOveride()
		{
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.IgnoreUnknownResponses.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var message = Factory.New<CMRSEACRRMessage>();
				message.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
				var processor = new TestHelperCMRMessageResponseProcessor("TestMessage");
				processor.ProcessMessage(message);
				AssertEquals("Discarded", EDIMessage.Status.Error, message.EM_Status);
				AssertEquals("Report Email Sent", 0, processor.SentReportEmails.Count);
			}
		}

		public void TestGetAssociatedWebUsersAddresses()
		{
			var bizO_1 = GetDummyBOWithAutoLogs("email@address.com (CMPCODE)", ZString.Empty);

			var addresses = Processor.GetAssociatedWebUsersAddresses(bizO_1);
			AssertEquals("One Addresses", 1, addresses.Length);
			AssertEquals("Correct Address", "email@address.com", addresses[0]);

			var bizO_2 = GetDummyBOWithAutoLogs("email@address.com (CMPCODE)", "Something Custom");
			AssertEquals("Reference", ZString.Format("{0} - Something Custom", "email@address.com (CMPCODE)"), bizO_2.Logs.AutoCreatedLog.SL_Reference);

			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_2);
			AssertEquals("One Addresses", 1, addresses.Length);
			AssertEquals("Correct Address", "email@address.com", addresses[0]);

			var bizO_3 = GetDummyBOWithAutoLogs("B ad@email.addr (GOOD)", "Something like a@ddre.ss");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_3);
			AssertEquals("No Addresses", 0, addresses.Length);

			var bizO_4 = GetDummyBOWithAutoLogs("Bad@em_ail.addr.es (GOOD)", "Something like a@ddre.ss");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_4);
			AssertEquals("No Addresses", 0, addresses.Length);

			var bizO_5 = GetDummyBOWithAutoLogs("Bad@email.addre.s (GOOD)", "Something like a@ddre.ss");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_5);
			AssertEquals("No Addresses", 0, addresses.Length);

			var bizO_6 = GetDummyBOWithAutoLogs("gOOd@email.addr.es (BaD)", "Something like (CLIENT)");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_6);
			AssertEquals("No Addresses", 0, addresses.Length);

			var bizO_7 = GetDummyBOWithAutoLogs("gOOd@email.addr.es (B)", "Something like (CLIENT)");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_7);
			AssertEquals("No Addresses", 0, addresses.Length);

			var bizO_8 = GetDummyBOWithAutoLogs("", "Something like a@ddre.ss (CLIENT)");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_8);
			AssertEquals("No Addresses", 0, addresses.Length);

			var bizO_9 = GetDummyBOWithAutoLogs("good@email.addr.es(COMPCOD)", "Custom Reference");
			addresses = Processor.GetAssociatedWebUsersAddresses(bizO_9);
			AssertEquals("No Addresses", 0, addresses.Length);
		}

		public void TestReportEmail()
		{
			var message = Factory.New<CMRCUSRESMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::XXXXX+2E05 688F 3H15:1+8'
DTM+9:20051007230022690835:ZZZ'
DTM+132:20051012:102'
FTX+AHN+++CONSOLIDATED STATUS:TRANSHIP'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA7XRYL'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+1002++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9163D::95'
NAD+MR+AAA394E::95'
NAD+UD+41000495269::95'
RFF+ABO:XXX::1'
RFF+MWB:08143636375'
RFF+HWB:805899300'
UNT+16+000001'".Replace("\r\n", "");
			var bizo = Factory.New<DummyBusinessObject>();
			message.EM_LinkedObject = bizo;
			var processor = new TestHelperCMRMessageResponseProcessor();
			var reportEmail = processor.GetReportfForTest(message);
			AssertContains("Email Report Text", @"<strong>
<br>Status: CONSOLIDATED STATUS<br>Status Description: TRANSHIPAAAA7XRYLN/A<br>
<br />
</strong>A  message has been received from the ACS.<br />
<br />
<!--DynamicHtml1-->
<br />
<!--DynamicHtml2-->
<br />
<!--DynamicHtml3-->
<hr />
<br />
<strong><large>
<!--EndSection Details--></strong></large>", reportEmail.Body);
		}

		public void TestOnlySelectDeclarationInCurrentCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "OB0987123";
			declaration.JE_VoyageFlightNo = "4365";
			declaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			declaration.Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.China);
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUXXX";
			branch.GB_GC = company.PK;
			var otherCountryDeclaration = factory.New<JobDeclaration>();
			otherCountryDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			otherCountryDeclaration.JE_MasterBill = "OB0987123";
			otherCountryDeclaration.JE_VoyageFlightNo = "4365";
			otherCountryDeclaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			otherCountryDeclaration.JE_GB = branch.PK;
			factory.Save();
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
TDT+20+4365++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB0987123'
DOC+1'
PAC+++LCL:67:95'
UNT+33+000001'".Replace("\r\n", "");
			AssertNoExceptionThrown("Unable to cast object of type ", delegate
			{ Processor.ProcessMessage(message); });
		}

		#region Implementation

		CusSCAOceanBill oceanBill;
		CusSCAOceanBill DummyWithSentWithdrawalAndPendingOriginal
		{
			get
			{
				if (oceanBill == null)
				{
					oceanBill = Factory.New<CusSCAOceanBill>();
					oceanBill.Messages.AddNew(typeof(CMRMessage));
					oceanBill.Messages.AddNew(typeof(CMRMessage));
					oceanBill.Messages[0].EM_MessageType = "XXX";
					oceanBill.Messages[0].EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
					oceanBill.Messages[0].EM_Status = EDIMessage.Status.Sent;
					oceanBill.Messages[1].EM_MessageType = "XXX";
					oceanBill.Messages[1].EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
					oceanBill.Messages[1].EM_Status = EDIMessage.Status.Pending;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					var webUserEvent = oceanBill.Logs.AddNew(Events.EditedARecord, "test@edi.com.au (TEST)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					webUserEvent.SL_GS_NKUser = CMRMessageResponseProcessor.WebUserCode;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					webUserEvent = oceanBill.Logs.AddNew(Events.EditedARecord, "Hosting.Notifications@cargowise.com (TESTB)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					webUserEvent.SL_GS_NKUser = CMRMessageResponseProcessor.WebUserCode;
				}
				return oceanBill;
			}
		}

		TestHelperCMRMessageResponseProcessor processor;
		TestHelperCMRMessageResponseProcessor Processor
		{
			get
			{
				if (processor == null)
				{
					processor = new TestHelperCMRMessageResponseProcessor();
				}
				return processor;
			}
		}

		TestHelperEDIMessage responseMessage;
		TestHelperEDIMessage ResponseMessage
		{
			get
			{
				if (responseMessage == null)
				{
					responseMessage = Factory.New<TestHelperEDIMessage>();
				}
				return responseMessage;
			}
		}

		protected DummyBizOWithAutoLogs GetDummyBOWithAutoLogs(ZString defaultReference, ZString customPrefix)
		{
			var bizO = Factory.New<DummyBizOWithAutoLogs>();
			bizO.Logs.AutoCreatedLogDefaultSL_Reference = defaultReference;
			bizO.SetLogReferenceSuffix(customPrefix);

			Factory.Save();
			AssertNotNull("AutoCreatedLog is not null", bizO.Logs.AutoCreatedLog);
			bizO.Logs.AutoCreatedLog.SL_GS_NKUser = CMRMessageResponseProcessor.WebUserCode;

			return bizO;
		}

		#endregion

		#region TestHelperEDIMessage

		class TestHelperEDIMessage : CMRCUSRESMessage
		{
			public TestHelperEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString StatusToReturn;
			protected override ZString GetStatusCore()
			{
				return StatusToReturn;
			}

			public void SetWrappedObject(BusinessObject wrappedObject)
			{
				this.wrappedObject = wrappedObject;
			}

			BusinessObject wrappedObject;
			protected override internal BusinessObject GetWrappedObject()
			{
				return wrappedObject;
			}
		}

		#endregion

		#region TestHelperCMRMessageResponseProcessor

		class TestHelperCMRMessageResponseProcessor : CMRMessageResponseProcessor
		{
			public TestHelperCMRMessageResponseProcessor()
				: base(new LoggingInformation(), ZString.Empty, ZString.Empty)
			{
			}

			public TestHelperCMRMessageResponseProcessor(ZString friendlyName)
				: base(new LoggingInformation(), ZString.Empty, friendlyName)
			{
			}

			protected override void SendReport(EmailDef email)
			{
				base.SendReport(email);
				SentReportEmails.Add(email);
			}

			public EmailDef GetReportfForTest(CMRCUSRESMessage message)
			{
				incomingMessage = message;
				return GetReport();
			}

			public ArrayList SentReportEmails = new ArrayList();
		}

		#endregion
	}
}
