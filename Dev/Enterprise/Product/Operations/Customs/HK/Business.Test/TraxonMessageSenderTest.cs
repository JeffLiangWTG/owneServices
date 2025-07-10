using System;
using System.Linq;
using System.Threading;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.HK.Traxon.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.HK.Business.Testing
{
	[UseSnapshotProtection]
	class TraxonMessageSenderTest_Snapshot : TestCase
	{
		public void TestProcessWithConcurrencyException()
		{
			var factory = new BusinessObjectFactory();
			var company = GlbCompany.CurrentCompany;

			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81"))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027"))
			{
				var consol = TraxonMessageSenderTest.CreateConsol(factory);
				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test trigger";
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
				trigger.TriggerConditions.TriggerFieldName = "JW_ATD";
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = "ISC";
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				company = factory.Load<GlbCompany>(company.PK);
				var branch = company.Branches.AddNew();
				branch.GB_OH_OrgProxy = consol.ReceivingForwarder.PK;
				factory.Save();

				var transport = consol.Transports[0];
				transport.JW_ATD = new ZDateTime(2020, 11, 10);
				factory.Save();
				var saveCount = 0;
				BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
				{
					if (f.NameForDebugging == "Subscriber: WorkflowEventTriggerProcessor" && saveCount < 2)
					{
						saveCount++;
						var consolInProcessing = f.Load<ForwardingConsol>(consol.PK);
						if (consolInProcessing.Messages.Count > 0)
						{
							var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
							var consolInAnotherFactory = anotherFactory.Load<ForwardingConsol>(consol.PK);
							if (consolInAnotherFactory.JK_Phase != "TST" && consolInAnotherFactory.JK_Phase != "XXX")
							{
								consolInAnotherFactory.JK_Phase = "XXX";
								consolInAnotherFactory.JK_MasterBillNum = "567";
								anotherFactory.Save();
							}

							consolInProcessing.JK_Phase = "TST";
						}
					}
				});

				UnitTestUserNotification.Instance.ClearMessages();

				var logger = new LoggerForTest();
				var lwm = LogWalkerRunner.Master();
				var lwk = LogWalkerRunner.Default();
				lwm.Process(logger, CancellationToken.None);
				lwk.Process(logger, CancellationToken.None);

				AssertNull("Should have no save concurrency error", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("TraxonMessageSender is invoked", logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] Firing action: ISC"));
				Assert("LogWalker NewsTransmitter saving detected the concurrency and handled by merge", logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] A conflict during save will cause logs to be processed one at a time."));

				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				AssertEquals("ISAC message sent", 1, consol.Messages.Count);
				AssertEquals("The concurrency error cannot be resolved automatically. The only reason this worked was because we were in a transactioned test case.", "ALL", consol.JK_Phase);
			}
		}
	}

	class TraxonMessageSenderTest : TestCaseWithFactory
	{
		public void TestNoNullParameterIssueForNewConsol()
		{
			var company = GlbCompany.CurrentCompany;

			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81"))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027"))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				consol.JK_MasterBillNum = "435";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "HKHKG";
				consol.JK_OA_ReceivingForwarderAddress = address.PK;
				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "HKHKG";
				transport.JW_VoyageFlight = "23";
				transport.JW_ETA = new ZDateTime(2012, 12, 21);

				var task = consol.WorkflowItems.Triggers.AddNew();
				task.P9_Description = "Test Task";
				var notification = task.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = "ISC";
				AssertNoExceptionThrown(() =>
				{
					notification.Validation.ValidatePQ_Calc_TriggerParty();
				});
			}
		}

		public void TestNoGetEmptyRegistryValueException()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "HK";
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = "HKHKG";
			Factory.Save();

			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81"))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027"))
			{
				var consol = CreateConsol();
				var trigger = consol.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test trigger";
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
				trigger.TriggerConditions.TriggerFieldName = "JW_ATD";
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = "ISC";
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				company = Factory.Load<GlbCompany>(company.PK);
				branch.GB_OH_OrgProxy = consol.ReceivingForwarder.PK;
				Factory.Save();

				var transport = consol.Transports[0];
				transport.JW_ATD = new ZDateTime(2020, 11, 10);
				Factory.Save();

				var logger = new LoggerForTest();
				var lwm = LogWalkerRunner.Master();
				var lwk = LogWalkerRunner.Default();
				lwm.Process(logger, CancellationToken.None);
				lwk.Process(logger, CancellationToken.None);

				AssertNull("Should have no error", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("TraxonMessageSender is invoked", logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] Firing action: ISC"));
				Assert("LogWalker NewsTransmitter saving: no unhandled errors", !logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] The following logs have failed with unhandled errors:"));
				Assert("Get the correct sender by branch", !logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] failed to process logs. Affected records will be processed again one-by-one.\r\nsender cannot be null or empty.\r\nParameter name: sender"));

				consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				AssertEquals("ISAC message sent", 1, consol.Messages.Count);
			}
		}

		public void TestProcessorSearch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "HKHKG";
			var task = consol.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "ISC";
			Factory.Save();
			var workflowDescriptor = new JobConsolWorkflowDescriptor();
			AssertNotNull("Message processor not found", workflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)));
		}

		public void TestProcessWithReceivingAgent()
		{
			var consol = CreateConsol();
			var sender = new TraxonMessageSender(consol);

			var company = GlbCompany.CurrentCompany;
			company.SetCountry(Core.Constants.CountryCodes.HongKong);

			Factory.Save();

			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath);
			HKDataRegistry.Instance.CosacAgentCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342");
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");

			var logQuery = new ZQuery(StmActivityLogSchema.S7_FormCaption, Enterprise.Environment.Env.Licence.Manifest.Name);
			var count1 = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var countEmail1 = Factory.GetDatabaseCount(typeof(MailItem));
			sender.Process(null, new CancellationToken());
			var count2 = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			var countEmail2 = Factory.GetDatabaseCount(typeof(MailItem));
			AssertEquals("ISAC message not sent", 0, consol.Messages.Count);
			AssertEquals("Error notification sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Manifest licence should not be logged", 0, count2 - count1);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert("Has recipient", email.Recipients.Contains("hktraxontest@edi.com.au"));
			AssertEquals("Error title", "ISAC Send Message Error", email.Subject);
			AssertContains("Error message", "Can't find any company or branch whose Organization matches the Receiving Agent on the Consol", email.Body);
			AssertEquals("should not call Factory.Save() in SendErrorNotification", 0, countEmail2 - countEmail1);

			company = Factory.Load<GlbCompany>(company.PK);

			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = consol.ReceivingForwarder.PK;
			Factory.Save();
			countEmail2 = Factory.GetDatabaseCount(typeof(MailItem));
			AssertEquals("email saved after Factory.Save()", 1, countEmail2 - countEmail1);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			sender.Process(null, new CancellationToken());

			var count3 = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			AssertEquals("should not save log info before Factory.Save()", 0, count3 - count2);
			Factory.Save();

			count3 = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			AssertEquals("ISAC message sent", 1, consol.Messages.Count);
			AssertEquals("Error notification not sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Manifest licence is logged", 1, count3 - count2);
		}

		public void TestWorkflowEventAfterProcess()
		{
			var consol = CreateConsol();
			var sender = new TraxonMessageSender(consol);

			var company = Factory.New<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.HongKong);
			company.Branches.AddNew().GB_OH_OrgProxy = consol.ReceivingForwarder.PK;

			Factory.Save();

			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath);
			HKDataRegistry.Instance.CosacAgentCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342");
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");

			Func<StmALog, bool> logFilter = log => log.SL_Table == "JobConsol" && log.SL_Parent == consol.PK && log.SL_SE_NKEvent == Events.MessageStatusChange.Code;

			var eventsCountBeforeProcess = consol.GetLogs().Find(logFilter).Count();
			AssertEquals("no event before Process was created", 0, eventsCountBeforeProcess);

			consol.Transports[0].JW_VoyageFlight = "";

			sender.Process(null, new CancellationToken());

			var eventsAfterFailProcess = consol.GetLogs().Find(logFilter);
			var eventsCountAfterFailProcess = eventsAfterFailProcess.Count();
			AssertEquals("1 event after Process was created(sending failed)", 1, eventsCountAfterFailProcess - eventsCountBeforeProcess);
			AssertContains("failed event record", "FAL:", eventsAfterFailProcess.OrderByDescending(l => l.SL_EventTime).First().SL_Reference);

			consol.Transports[0].JW_VoyageFlight = "23";
			sender.Process(null, new CancellationToken());

			var eventsAfterSuccessProcess = consol.GetLogs().Find(logFilter);
			AssertEquals("1 event after Process was created(sending failed)", 1, eventsAfterSuccessProcess.Count() - eventsCountAfterFailProcess);
			AssertContains("failed event record", "SNT:", eventsAfterSuccessProcess.OrderByDescending(l => l.SL_EventTime).First().SL_Reference);
		}

		public void TestProcessWithSendingAgent()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "435";
			consol.JK_OA_SendingForwarderAddress = address.PK;

			var sender = new TraxonMessageSender(consol);

			var company = Factory.New<GlbCompany>();
			company.SetCountry(Core.Constants.CountryCodes.HongKong);
			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath);
			HKDataRegistry.Instance.CosacAgentCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342");
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_VoyageFlight = "23";
			transport.JW_ETA = ZDateTime.Today.AddDays(1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "123";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OuterPacks = 100;
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_GoodsDescription = "GOODS DESCRIPTION";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_FullName = "CONSIGNEE NAME";
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			consignee.MainAddress.OA_City = "TEST CONSIGNEE CITY";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			shipment.ConsigneePK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_FullName = "CONSIGNOR NAME";
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			consignor.MainAddress.OA_City = "TEST CONSIGNOR CITY";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "HKHKG";
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			sender.Process(null, new CancellationToken());
			AssertEquals("ISAC message not sent", 0, consol.Messages.Count);
			AssertEquals("Error notification sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert("Has recipient", email.Recipients.Contains("hktraxontest@edi.com.au"));
			AssertEquals("Error title", "ISAC Send Message Error", email.Subject);
			AssertContains("Error message", "Can't find any company or branch whose Organization matches the Sending Agent on the Consol", email.Body);

			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = address.Header.PK;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			sender.Process(null, new CancellationToken());
			AssertEquals("ISAC message sent", 1, consol.Messages.Count);
			AssertEquals("Error notification not sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestProcessWithUnmatchedOrgs()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "435";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.JK_OA_ReceivingForwarderAddress = address.PK;
			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test trigger";
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
			trigger.TriggerConditions.TriggerFieldName = "JW_ATD";
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "ISC";
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			var company = GlbCompany.CurrentCompany;
			company = Factory.Load<GlbCompany>(company.PK);

			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = address.Header.PK;
			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath);
			HKDataRegistry.Instance.CosacAgentCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342");
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "23";
			transport.JW_ETA = new ZDateTime(2012, 12, 21);

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = OrgHeader.UnmatchedOrganisationPK;
			shipment.ConsignorPK = OrgHeader.UnmatchedOrganisationPK;
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "123";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_OuterPacks = 100;
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";

			shipment.Notes.AddNew(false, "Unmatched Org Details", @"
<UnmatchOrgRecords>
	<UnmatchOrgRecord>
		<OrganisationType>Consignee</OrganisationType>
		<OrganisationSubType>Consignee</OrganisationSubType>
		<OwnerCode>CONSIGNEE</OwnerCode>
		<EDICode>CONSIGNEE</EDICode>
		<OrganisationName>CONSIGNEE NAME</OrganisationName>
		<AddressLine1>TEST CONSIGNEE ADDRESS 1</AddressLine1>
		<AddressLine2>TEST CONSIGNEE ADDRESS 2</AddressLine2>
		<City>TEST CONSIGNEE CITY</City>
		<PostCode>9999</PostCode>
		<Country>HK</Country>
	</UnmatchOrgRecord>
	<UnmatchOrgRecord>
		<OrganisationType>Consignor</OrganisationType>
		<OrganisationSubType>Consignor</OrganisationSubType>
		<OwnerCode>CONSIGNOR</OwnerCode>
		<EDICode>CONSIGNOR</EDICode>
		<OrganisationName>CONSIGNOR NAME</OrganisationName>
		<AddressLine1>TEST CONSIGNOR ADDRESS 1</AddressLine1>
		<AddressLine2/>
		<City>TEST CONSIGNOR CITY</City>
		<PostCode>2222</PostCode>
		<StateOrProvince>NSW</StateOrProvince>
		<Country>AU</Country>
	</UnmatchOrgRecord>
</UnmatchOrgRecords>");

			Factory.Save();

			transport.JW_ATD = new ZDateTime(2020, 11, 10);
			Factory.Save();

			var logger = new LoggerForTest();
			var lwm = LogWalkerRunner.Master();
			var lwk = LogWalkerRunner.Default();
			lwm.Process(logger, CancellationToken.None);
			lwk.Process(logger, CancellationToken.None);

			Assert(logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] Firing action: ISC"));
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("ISAC message sent", 1, consol.Messages.Count);
			AssertEquals("Message Text", @"
UNH+HMFX435+CUSEXP:D:95A:UN+100
BGM+85:::EXPRESS CONSIGNMENT MANIFEST+435/C00001000+5
LOC+5+SYD
LOC+8+HKG
CNT+7:1000.0:KGM
CNT+8:100
NAD+PK+12342
TDT+13+23
DTM+132:121221:101
RFF+MWB:435
CNT+10:1
CNI+1+123
CNT+8:100
CNT+1:100
MEA+WT++KGM:1000.0
LOC+5+SYD
LOC+8+HKG
NAD+CN+++CONSIGNEE NAME+TEST CONSIGNEE ADDRESS 1:TEST CONSIGNEE ADDRESS 2+TEST CONSIGNEE CITY++9999+HK
NAD+CZ+++CONSIGNOR NAME+TEST CONSIGNOR ADDRESS 1+TEST CONSIGNOR CITY+NSW+2222+AU
GDS+12
FTX+AAA
MOA+96:0.00:HKD
MOA+95:0.00:HKD
MOA+94:0.00:HKD
UNT+25+HMFX435".Trim(), consol.Messages[0].EM_FormattedMessageText.ToString().Trim());
		}

		public void TestProcessWithOverriddenOrgs()
		{
			var consol = CreateConsol();
			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test trigger";
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
			trigger.TriggerConditions.TriggerFieldName = "JW_ATD";
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "ISC";
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			var company = GlbCompany.CurrentCompany;
			company = Factory.Load<GlbCompany>(company.PK);
			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = consol.ReceivingForwarder.PK;
			Factory.Save();
			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath);
			HKDataRegistry.Instance.CosacAgentCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342");
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");

			var transport = consol.Transports[0];
			transport.JW_ATD = new ZDateTime(2020, 11, 10);
			Factory.Save();

			var logger = new LoggerForTest();
			var lwm = LogWalkerRunner.Master();
			var lwk = LogWalkerRunner.Default();
			lwm.Process(logger, CancellationToken.None);
			lwk.Process(logger, CancellationToken.None);

			Assert(logger.LogEntries.Contains("[WorkflowEventTrigger] [Default] Firing action: ISC"));
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("ISAC message sent", 1, consol.Messages.Count);
			AssertEquals("Message Text", @"
UNH+HMFX435+CUSEXP:D:95A:UN+100
BGM+85:::EXPRESS CONSIGNMENT MANIFEST+435/C00001000+5
LOC+5+SYD
LOC+8+HKG
CNT+7:1000.0:KGM
CNT+8:100
NAD+PK+12342
TDT+13+23
DTM+132:121221:101
RFF+MWB:435
CNT+10:1
CNI+1+123
CNT+8:100
CNT+1:100
MEA+WT++KGM:1000.0
LOC+5+SYD
LOC+8+HKG
NAD+CN++TE 8522345678+CONSIGNEE NAME+TEST CONSIGNEE ADDRESS 1:TEST CONSIGNEE ADDRESS 2+TEST CONSIGNEE CITY++9999+HK
NAD+CZ++TE 610222340008+CONSIGNOR NAME+TEST CONSIGNOR ADDRESS 1+TEST CONSIGNOR CITY+NSW+2222+AU
GDS+12
FTX+AAA
MOA+96:0.00:HKD
MOA+95:0.00:HKD
MOA+94:0.00:HKD
UNT+25+HMFX435".Trim(), consol.Messages[0].EM_FormattedMessageText.ToString().Trim());
		}

		public void TestProcessWithNoPhone()
		{
			var consol = CreateConsol();

			consol.Shipments[0].ConsigneeDocumentaryAddress.E2_Phone = "";
			consol.Shipments[0].ConsignorDocumentaryAddress.E2_Phone = "";
			Factory.Save();

			var generator = new TraxonMessageGenerator(new TraxonConsolStatus(consol));

			generator.GenerateSendMessages();
			var message = consol.Messages[0].EM_FormattedMessageText.ToString().Trim();
			AssertContains("Consignee has Fax Number", "NAD+CN++FX 8621310489+CONSIGNEE NAME+TEST CONSIGNEE ADDRESS 1:TEST CONSIGNEE ADDRESS 2+TEST CONSIGNEE CITY++9999+HK", message);
			AssertContains("Consignor has Fax Number", "NAD+CZ++FX 610222341001+CONSIGNOR NAME+TEST CONSIGNOR ADDRESS 1+TEST CONSIGNOR CITY+NSW+2222+AU", message);
		}

		public void TestProcessHVMShipmentWithCoLoadHVLShipment()
		{
			var consol = CreateConsol();

			var hvmShipment = consol.Shipments[0];
			hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			hvmShipment.JS_HouseBill = "HVMSHIPMENT";

			var hvlShipment = consol.Shipments.AddNew();
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;
			hvlShipment.JS_HouseBill = "HVLSHIPMENT";

			Factory.Save();

			var generator = new TraxonMessageGenerator(new TraxonConsolStatus(consol));

			generator.GenerateSendMessages();
			AssertEquals("ISAC message sent", 1, consol.Messages.Count);
			var message = consol.Messages[0].EM_FormattedMessageText.ToString().Trim();
			CombineAssertions(() =>
			{
				AssertContains("HVM Shipment is included", "CNI+1+HVMSHIPMENT", message);
				AssertNotContains("HVL Shipment is not included", "CNI+2+HVLSHIPMENT", message);
			});
		}

		public void TestSendFailureWhenRegistryNotConfigured()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.SetCountry(Core.Constants.CountryCodes.HongKong);
			Factory.Save();

			var consol = CreateConsol();

			var branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = consol.ReceivingForwarder.PK;

			Factory.Save();

			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027"))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				var sender = new TraxonMessageSender(consol);
				sender.Process(null, new CancellationToken());

				AssertNoExceptionThrown("No ZSaveExceptions", () => Factory.Save());

				AssertEquals("ISAC message not sent", 0, consol.Messages.Count);
				AssertEquals("Error notification sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				Assert("Has recipient", email.Recipients.Contains("hktraxontest@edi.com.au"));
				AssertEquals("Error title", "ISAC Send Message Error", email.Subject);
				AssertContains("Error message", "ISAC Message sending configuration has not been completed in the registry", email.Body);
			}
		}

		ForwardingConsol CreateConsol() => CreateConsol(Factory);

		internal static ForwardingConsol CreateConsol(BusinessObjectFactory factory)
		{
			var address = factory.NewWithValidTestData<OrgAddress>();

			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "435";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.JK_OA_ReceivingForwarderAddress = address.PK;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "23";
			transport.JW_ETA = new ZDateTime(2012, 12, 21);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "123";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_OuterPacks = 100;
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";

			var consignee = shipment.ConsigneeDocumentaryAddress;
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "CONSIGNEE NAME";
			consignee.E2_Address1 = "TEST CONSIGNEE ADDRESS 1";
			consignee.E2_Address2 = "TEST CONSIGNEE ADDRESS 2";
			consignee.E2_City = "TEST CONSIGNEE CITY";
			consignee.E2_Postcode = "9999";
			consignee.E2_RN_NKCountryCode = "HK";
			consignee.E2_Phone = "8522345678";
			consignee.E2_Fax = "8621310489";

			var consignor = shipment.ConsignorDocumentaryAddress;
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "CONSIGNOR NAME";
			consignor.E2_Address1 = "TEST CONSIGNOR ADDRESS 1";
			consignor.E2_City = "TEST CONSIGNOR CITY";
			consignor.E2_Postcode = "2222";
			consignor.E2_State = "NSW";
			consignor.E2_RN_NKCountryCode = "AU";
			consignor.E2_Phone = "+61 02 22340008";
			consignor.E2_Fax = "+61 02 22341001";

			factory.Save();

			return consol;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "hktraxontest@edi.com.au";

			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);

			var link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;

			Factory.Save();

			HKDataRegistry.Instance.GroupToCopyTraxonResponseEmailsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}
	}
}
