using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TestHelpers.Xml;
using CusHAWB = Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB;
using CusMAWB = Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks.Testing
{
	[TestedType(typeof(CcsukInterchangePackagerServiceTask))]
	class CcsukInterchangePackagerServiceTaskTest : ServiceTaskTestCase<CcsukInterchangePackagerServiceTask>
	{
		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_G2G()
		{
			RunMessageTypeInterchangeTest(CcsukTransmissionMessageFunction.CUKG2G.Code, string.Empty);
			AssertEquals("Good2Go Interchange always sent to CUKSYS98CCSNES, regardless of EM_ApplicationReference", "CUKSYS98CCSNES", interchangeCreated.EI_To);
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_GEN()
		{
			RunMessageTypeInterchangeTest("GEN", string.Empty);
			AssertEquals("Non-Chief Interchange generated for message should have the recipient completed property", "APPREF_GEN", interchangeCreated.EI_To);
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_FSR()
		{
			RunMessageTypeInterchangeTest("FSR", string.Empty);
			AssertEquals("Non-Chief Interchange generated for message should have the recipient completed property", "APPREF_FSR", interchangeCreated.EI_To);
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_CAR()
		{
			RunMessageTypeInterchangeTest("CAR", string.Empty, true);
			AssertEquals("Non-Chief Interchange generated for message should have the recipient completed property", "APPREF_CAR", interchangeCreated.EI_To);
			hawb.Reload();
			AssertEquals("Present updated to PND when sending a message", PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask, hawb.PresenceOnNetworkStatus);

			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			RunMessageTypeInterchangeTest("CAR", string.Empty, true);
			AssertEquals("Non-Chief Interchange generated for message should have the recipient completed property", "APPREF_CAR", interchangeCreated.EI_To);
			hawb.Reload();
			AssertEquals("Presence unchanged from YES when sending a message", PresenceOnNetworkList.Codes.OnCommDb, hawb.PresenceOnNetworkStatus);
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_ChiefIMP()
		{
			RunMessageTypeInterchangeTest("", "IMP");
			AssertEquals("Chief Interchange generated for message should have the recipient completed property", "CUKCTM98CHFIMP", interchangeCreated.EI_To);
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_ChiefEXP()
		{
			RunMessageTypeInterchangeTest("", "EXP");
			AssertEquals("Chief Interchange generated for message should have the recipient completed property", "CUKCTM98CHFEXP", interchangeCreated.EI_To);
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_ChiefUnknown()
		{
			RunMessageTypeInterchangeTest("", "YYY");
			AssertContains("CHIEF for imports or exports", ErrorReporter.LastMessageReported);
			AssertContains("CHIEF for imports or exports", log[1]);
			ErrorReporter.Clear();
		}

		public void TestOutboundMessagesAllGoIntoInterchangesBasedOnMessageType_Unknown()
		{
			RunMessageTypeInterchangeTest("XXX", "");
			AssertContains("doesn't know how to send to the recipient defined", ErrorReporter.LastMessageReported);
			AssertContains("doesn't know how to send to the recipient defined", log[1]);
			ErrorReporter.Clear();
		}

		[TestDate(1987, 12, 11, 1, 2, 0)]
		public void TestPackageMessageIntoInterchangesAndMakePima()
		{
			EDIInterchange interchangeCreated = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.GbCcsuk));
			AssertNull("Pre req - no waiting CUK interchanges", interchangeCreated);
			string ediMessageTextUnderTest = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109730+8F1398F431C4406EBA6307F226B555B5'BGM+EFD::109++9'CST++EXD+2'LOC+36+GB'GEI+5+ACC:PI:109'GEI+5+PRG:PI:109'RFF+ABO:0GB168809029000-B00010133:'RFF+UCN'RFF+ABI:8888888:A'NAD+CN+GB543938615000++THE BODY SHOP+THE REGIONAL DISTRIBUTION CENTRE+LITTLEHAMPTON+EN+BN17 6LS+GB'NAD+DT+GB168809029000++CHARLES F STEAD+SHEEPSCAR TANNERY+LEEDS+EN+LS7 2BY+GB'UNS+D'DMS+B00010133'MOA+39'CST++3      ++Q   'PAC+1++SF'PCI++SDFSDF+EN'RFF+ZZZ'IMD+++:::SFDSDFSF::EN'DOC+998:::380+ASDADSA:::EN::Z'TAX+1+A40+G++F::::::109'TAX+1+A00+S++S::::::109'UNS+S'CNT+11:4'UNT+25+5'";
			EDIMessage message = SaveMessageAndEntryAndDeclaration(ediMessageTextUnderTest, "", ApplicationCodeList.Codes.GbCcsuk);

			TestPackageMessageIntoInterchangesAndMakePima_RunAndMakeAssertions(interchangeCreated, ediMessageTextUnderTest, message);
		}

		[TestDate(1987, 12, 11, 1, 2, 0)]
		public void TestPackageMessageIntoInterchangesAndMakePima_DifferentBranch()
		{
			EDIInterchange interchangeCreated = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.GbCcsuk));
			AssertNull("Pre req - no waiting CUK interchanges", interchangeCreated);
			string ediMessageTextUnderTest = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:04A:UN:109730+8F1398F431C4406EBA6307F226B555B5'BGM+EFD::109++9'CST++EXD+2'LOC+36+GB'GEI+5+ACC:PI:109'GEI+5+PRG:PI:109'RFF+ABO:0GB168809029000-B00010133:'RFF+UCN'RFF+ABI:8888888:A'NAD+CN+GB543938615000++THE BODY SHOP+THE REGIONAL DISTRIBUTION CENTRE+LITTLEHAMPTON+EN+BN17 6LS+GB'NAD+DT+GB168809029000++CHARLES F STEAD+SHEEPSCAR TANNERY+LEEDS+EN+LS7 2BY+GB'UNS+D'DMS+B00010133'MOA+39'CST++3      ++Q   'PAC+1++SF'PCI++SDFSDF+EN'RFF+ZZZ'IMD+++:::SFDSDFSF::EN'DOC+998:::380+ASDADSA:::EN::Z'TAX+1+A40+G++F::::::109'TAX+1+A00+S++S::::::109'UNS+S'CNT+11:4'UNT+25+5'";

			var comp = Factory.New<GlbCompany>();
			comp.GC_Code = "DAN";
			var branch = comp.Branches.AddNew();
			branch.GB_Code = "CLA";
			branch.GB_OH_OrgProxy = Environment.Env.CurrentBranch.OrganisationPK;
			Factory.Save();
			EDIMessage message = null;
			using (var env = Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				// Creds and declaration belong to different branch compared to service task... yet should still find them.
				message = SaveMessageAndEntryAndDeclaration(ediMessageTextUnderTest, "", ApplicationCodeList.Codes.GbCcsuk);
			}

			TestPackageMessageIntoInterchangesAndMakePima_RunAndMakeAssertions(interchangeCreated, ediMessageTextUnderTest, message);
		}

		void TestPackageMessageIntoInterchangesAndMakePima_RunAndMakeAssertions(EDIInterchange interchangeCreated, string ediMessageTextUnderTest, EDIMessage message)
		{
			CcsukInterchangePackagerServiceTask task = new CcsukInterchangePackagerServiceTask();
			InitialiseAndRunTaskSchedule(task);
			interchangeCreated = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.NotEqual, ""));
			message.Reload();
			AssertEquals("PND", message.EM_Status);
			AssertEquals("QUE", interchangeCreated.EI_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCcsuk, interchangeCreated.EI_ApplicationCode);
			AssertEquals("TRX", interchangeCreated.EI_ReceiveTransmit);
			AssertEquals("SENDER PIMA", interchangeCreated.EI_From);
			AssertEquals("CUKCTM98CHFEXP", interchangeCreated.EI_To);
			AssertEquals("UNB+UNOA:2+SENDER PIMA:IATA+CUKCTM98CHFEXP:IATA+871211:0102+1'", interchangeCreated.EI_HeaderText);
			AssertEquals(ediMessageTextUnderTest.Replace("<<MSGNO PLACEHOLDER>>", "123"), interchangeCreated.EI_BodyText);
			AssertEquals("UNZ+1+1'", interchangeCreated.EI_FooterText);
		}

		public void TestPackageCDSMessageIntoInterchanges_CDS_CCSUK_AS_CVC()
		{
			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty,
				Guid.Empty, Guid.Empty,
				new CDSDUCRAutomationSettings
				{
					CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				});

			var entry = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var declaration = entry.Declaration;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			entry.CH_CEI_Instruction = declaration.CustomsEntryInstructions[0].PK;
			entry.EntryInstruction.CEI_SubStyle = "Y";

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			messageSendingObject.ShouldSend = true;

			var messageSendingObjects = new[] { messageSendingObject };

			var generator = new CDSTransmissionMessageGenerator(messageSendingObjects.AsEnumerable());
			generator.Generate(entry);

			Factory.Save();

			InitialiseAndRunTaskSchedule(new CcsukInterchangePackagerServiceTask());
			var interchangeCreated = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.NotEqual, ZString.Empty));

			AssertInterchange(interchangeCreated
				, "Location"
				, RequestMessageTests.GetExpectedH1().Trim()
				, $@"<?ccsuk senderid=""Location"" recipientid=""CUKCTM98CDSUSR"" ext-correlation-id=""{interchangeCreated.PK.ToAlphanumericOnlyString()}""?>");
		}

		public void TestPackageCDSMessageIntoInterchanges()
		{
			var interchangeCreated = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.GbCcsuk));
			AssertNull("Pre req - no waiting CUK interchanges", interchangeCreated);
			string ediMessageTextUnderTest = @"<MetaData></MetaData>";
			var message = SaveMessageAndEntryAndDeclaration(ediMessageTextUnderTest, "CDS.CCSUK", ApplicationCodeList.Codes.GbCDSViaCCSUK);

			var task = new CcsukInterchangePackagerServiceTask();
			InitialiseAndRunTaskSchedule(task);
			interchangeCreated = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.NotEqual, ZString.Empty));
			message.Reload();
			AssertEquals(EDIMessageStatusList.Codes.Pending, message.EM_Status);
			AssertInterchange(interchangeCreated, "SENDER PIMA", @"<MetaData></MetaData>", $@"<?ccsuk senderid=""SENDER PIMA"" recipientid=""CUKCTM98CDSUSR"" ext-correlation-id=""{interchangeCreated.PK.ToAlphanumericOnlyString()}""?>");
		}

		public void TestPackageCDSMessageIntoInterchanges_InvalidEM_MessageOwner()
		{
			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertNull("Pre req - no existing interchanges", interchange);

			string ediMessageTextUnderTest = @"<MetaData></MetaData>";
			var message = SaveMessageAndEntryAndDeclaration(ediMessageTextUnderTest, "CUKSYS98COMMDB", ApplicationCodeList.Codes.GbCcsuk);
			message.EM_MessageType = "CAR";
			message.EM_MessageSubType = "FRI";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageOwner = ZString.Empty; // blank sender
			Factory.Save();

			var task = new CcsukInterchangePackagerServiceTask();
			log = InitialiseAndRunTaskSchedule(task);

			message.Reload();
			AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);

			interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertNull("no interchange was created (i.e. it was deleted before it was saved)", interchange);

			AssertContains("The interchange will be removed and this message marked as failed.", log[1]);
			ErrorReporter.Clear();
		}

		void AssertInterchange(EDIInterchange interchange, ZString from, ZString bodyText, ZString footerText)
		{
			AssertEquals("QUE", interchange.EI_Status);
			AssertEquals(ApplicationCodeList.Codes.GbCcsuk, interchange.EI_ApplicationCode);
			AssertEquals("TRX", interchange.EI_ReceiveTransmit);
			AssertEquals(from, interchange.EI_From);
			AssertEquals("CUKCTM98CDSUSR", interchange.EI_To);
			AssertEquals(string.Empty, interchange.EI_HeaderText);
			XmlComparison.CompareAndAssertXml(bodyText, interchange.EI_BodyText);
			AssertEquals(footerText, interchange.EI_FooterText);
		}

		public void TestInboundGenralMessagesArePickedUpAndParsed()
		{
			string wholeInterchange = "UNB+UNOA:2+CUKFFW98000CAR:IATA+CUKAIR98LHRCAX:IATA+101110:1636+1'" +
										"UNH+229+GENRAL:0:912:UN'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++THIS IS TO DANIEL?'S SHED'UNT+5+229'" +
										"UNZ+1+1'";
			var interchangeCreated = EDIInterchange.CreateNewInterchangeFromString(Factory, wholeInterchange, ApplicationCodeList.Codes.GbCcsuk);
			Factory.Save();

			CcsukInterchangePackagerServiceTask task = new CcsukInterchangePackagerServiceTask();
			InitialiseAndRunTaskSchedule(task);
			interchangeCreated.Reload();
			var queuedMessage = interchangeCreated.ContainedMessages[0];
			queuedMessage.Reload(); // WTF?
			AssertEquals(EDIMessageStatusList.Codes.Received, queuedMessage.EM_Status);
			AssertEquals("Sender pima is sent from interchange header", "CUKFFW98000CAR", queuedMessage.EM_ApplicationReference);
			AssertEquals("GEN", queuedMessage.EM_MessageType);
			// Other tests assert the emails, interpretation, etc; we just want to see that the task picks up the message
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CCS UK messages outbound (CUK)",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCcsuk),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CCS UK messages processing (CUK)",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCcsuk),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CCS UK messages outbound (CVC)",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCDSViaCCSUK),
				};
			}
		}

		EDIMessage SaveMessageAndEntryAndDeclaration(string ediMessageTextUnderTest, string applicationReference, string applicationCode)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entry = dec.CustomsEntryHeaders.AddNew();

			var message = MakeQueuedMessageOfType(ZString.Empty, ediMessageTextUnderTest, applicationReference, applicationCode);
			entry.Messages.Add(message);

			MakeBadgesAndCredentials();

			Factory.Save();
			return message;
		}

		void MakeBadgesAndCredentials()
		{
			var badgeDan = new BadgeCodeSetting();
			badgeDan.BadgeCode = "DAN";
			badgeDan.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			var existingBadges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			existingBadges.Add(badgeDan);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, existingBadges);
			Factory.Save();
			var cred = new CredentialsSetting();
			cred.BadgeCode = "DAN";
			cred.NesLocation = "CUKAIR98LHRABC";
			cred.NesRole = "XYZ";
			var existingCreds = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			existingCreds.Add(cred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, existingCreds);
			Factory.Save();
		}

		void RunMessageTypeInterchangeTest(string messageType, ZString chiefDeclarationType, bool linkMessageToHawbToo = false)
		{
			MakeBadgesAndCredentials();
			var task = new CcsukInterchangePackagerServiceTask();
			var outboundMessage = MakeQueuedMessageOfType(messageType, "Anything <<MSGNO PLACEHOLDER>>", "APPREF_" + messageType, ApplicationCodeList.Codes.GbCcsuk, linkMessageToHawbToo);
			if (!chiefDeclarationType.IsEmpty)
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = chiefDeclarationType;
				var entry = dec.CustomsEntryHeaders.AddNew();
				outboundMessage.EM_LinkedObject = entry;
			}
			Factory.Save();
			log = InitialiseAndRunTaskSchedule(task);
			outboundMessage.Reload();
			AssertNotNull("Message should have been packed into interchange", outboundMessage.Interchange);
			interchangeCreated = outboundMessage.Interchange;
			AssertEquals("SENDER PIMA", interchangeCreated.EI_From);
		}

		EDIMessage MakeQueuedMessageOfType(string messageType, string messageText, string applicationReference, string applicationCode, bool linkToHawb = false)
		{
			var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			var message = mockMessage.Object;
			message.EM_MessageOwner = "SENDER PIMA"; // sender
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "123";
			message.EM_MessageText = messageText;
			message.EM_MessageType = messageType;
			message.EM_ApplicationReference = applicationReference;
			if (linkToHawb)
			{
				if (hawb == null)
				{
					var mawb = Factory.New<CusMAWB>();
					hawb = mawb.ChildBills.AddNew();
				}
				hawb.Messages.Add(message);
			}
			return message;
		}

		CusHAWB hawb;
		TestServiceLogger log;
		EDIInterchange interchangeCreated;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			branchEnvironment = Enterprise.Environment.DisposableEnvironment.ForBranch(Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDownCore()
		{
			base.TearDownCore();
			branchEnvironment.Dispose();
		}
	}
	public class EDIMessageDummyForTest_123 : EDIMessage
	{
		public EDIMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}
}
