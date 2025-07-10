using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class EDocSerializerTest : TestCaseWithFactory
	{
		public void TestNoDocumentPK_InTriggeringLogReference()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var mode = organisation.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@test.com";
			mode.EK_Filename = "eDoc from Organisation [(*JobNumber*)].txt";

			var trigger = organisation.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImported.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;

			AssertNoErrors(string.Format("Action trigger-type {0} should be valid", action.PQ_TriggerType), action.PQ_TriggerTypeInfo);

			organisation.Logs.AddNew(AutoEvents.DocumentImported, "TEST REFERENCE");

			Factory.Save();

			var wteLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode));
			AssertEquals("WHEN adding DDI Log, 1 workflow trigger event should be created", 1, wteLogs.Length);

			var delivery = new XmlMessageDeliver(
				modes: GetModes(new[] { mode }),
				bizObjToDeliver: organisation,
				dataAdapter: new StorageDocsBaseValueObjectDataAdatper(),
				action: action,
				eventInfoProvider: new EventInfoProvider(new QueuedLogForTesting(wteLogs[0], trigger), trigger, trigger.GetJob()));
			var notifications = new NotificationBuffer();

			delivery.Process(notifications);
			Factory.Save();

			AssertMultilineASCIIEquals(
				"GIVEN DDI event was manually created with reference has no document PK, WHEN executing process, THEN warning should be logged",
				"0 eDocs to deliver.\r\nTriggering event: reference 'TEST REFERENCE', parent table 'OrgHeader'. Event reference does not contain document information, the event may have been created manually.",
				notifications.AsString);
		}

		public void TestNoSave()
		{
			using (var env = MasterFilesTestHelper.SetupOrgProxy(Factory, (org, branch) =>
			{
				var mode = org.EDICommunicationsModes.AddNew();
				var company = Factory.NewWithValidTestData<GlbCompany>();

				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_Destination = "test@test.com";
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;
				mode.EK_Module = "ORG";
				mode.EK_Filename = "eDoc from Organisation [(*JobNumber*)].txt";
			}))
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var trigger = organisation.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.DocumentImported.Code;
				trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
				trigger.TriggerConditions.TriggerConditionValue = Core.Constants.RefDocTypes.EntryPrint;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;
				action.PQ_Calc_TriggerParty = "ORP";

				Factory.Save();

				// Import Document
				var docFactory = ObjectFactory.Get<IDocumentFactoryProviderForTest>().GetFactory(Factory);
				using (docFactory as IDisposable)
				{
					var eprDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.EntryPrint));
					eprDocType.RT_ReferenceType = Constants.ReferenceTypes.All;

					Factory.Save();

					docFactory.Import(new byte[] { 0x1, 0x2 }, "EPR Document 1", Core.Constants.DocManagerCodes.Organisation, organisation.PK, eprDocType.RT_DocType, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
					docFactory.Save();

					var ddaQuery = new ZQuery(
						new ZQuery(StmALogSchema.SL_Parent, organisation.PK),
						new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentImportedCode));
					var ddaLogs = Factory.Load<StmALog>(ddaQuery);
					AssertEquals("Pre-condition: 1 DDI event", 1, ddaLogs.Length);

					var storageMain = ((BusinessObjectFactory)docFactory).LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, organisation.PK));
					var eprDoc = (BusinessObject)storageMain.AllEDocs[0];
					var epr1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, string.Concat("EPR|", eprDoc.PK.ToString())));
					AssertEquals("Pre-condition: 1 DDI event with EPR reference", 1, Factory.Load<StmALog>(epr1DocDDAQuery).Length);

					docFactory.Save();

					var wteLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode));
					AssertEquals("1 workflow trigger event for EPR eDoc should be created", 1, wteLogs.Length);

					MasterFilesTestHelper.EnableFactorySaveAlerterInTesting = true;
					var message = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
				}
			}
		}

		public void TestDeletedDocument()
		{
			var originalValue = SystemDataRegistry.Instance.DDIDocumentSource.Value;
			try
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var mode = organisation.EDICommunicationsModes.AddNew();

				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "eDoc from Organisation [(*JobNumber*)].txt";

				var trigger = organisation.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.DocumentImported.Code;
				trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
				trigger.TriggerConditions.TriggerConditionValue = Core.Constants.RefDocTypes.EntryPrint;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;

				Factory.Save();

				// Import Document
				var docFactory = ObjectFactory.Get<IDocumentFactoryProviderForTest>().GetFactory(Factory);
				using (docFactory as IDisposable)
				{
					var eprDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.EntryPrint));
					eprDocType.RT_ReferenceType = Constants.ReferenceTypes.All;

					Factory.Save();

					docFactory.Import(new byte[] { 0x1, 0x2 }, "EPR Document 1", Core.Constants.DocManagerCodes.Organisation, organisation.PK, eprDocType.RT_DocType, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
					docFactory.Save();

					var ddaQuery = new ZQuery(
						new ZQuery(StmALogSchema.SL_Parent, organisation.PK),
						new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentImportedCode));
					var ddaLogs = Factory.Load<StmALog>(ddaQuery);
					AssertEquals("Pre-condition: 1 DDI event", 1, ddaLogs.Length);

					var storageMain = ((BusinessObjectFactory)docFactory).LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, organisation.PK));
					var eprDoc = (BusinessObject)storageMain.AllEDocs[0];
					var epr1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, string.Concat("EPR|", eprDoc.PK.ToString())));
					AssertEquals("Pre-condition: 1 DDI event with EPR reference", 1, Factory.Load<StmALog>(epr1DocDDAQuery).Length);

					eprDoc.Delete();

					docFactory.Save();

					var wteLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode));
					AssertEquals("1 workflow trigger event for EPR eDoc should be created", 1, wteLogs.Length);

					var delivery = new XmlMessageDeliver(
						modes: GetModes(new[] { mode }),
						bizObjToDeliver: organisation,
						dataAdapter: new StorageDocsBaseValueObjectDataAdatper(),
						action: action,
						eventInfoProvider: new EventInfoProvider(new QueuedLogForTesting(wteLogs[0], trigger), trigger, trigger.GetJob()));
					var notifications = new NotificationBuffer();

					delivery.Process(notifications);
					Factory.Save();

					AssertMultilineASCIIEquals(
						"GIVEN imported eDoc was deleted, WHEN executing process, THEN warning should be logged",
						string.Format("0 eDocs to deliver.\r\nTriggering event: reference 'EPR|{0}', parent table 'OrgHeader'. Cannot find matched document, it may have been deleted.", eprDoc.PK),
						notifications.AsString);
				}
			}
			finally
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleDDIEventsWithOnlyOneMatch()
		{
			var originalValue = SystemDataRegistry.Instance.DDIDocumentSource.Value;
			try
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var mode = organisation.EDICommunicationsModes.AddNew();

				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "eDoc from Organisation [(*JobNumber*)].txt";

				var trigger = organisation.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.DocumentImported.Code;
				trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
				trigger.TriggerConditions.TriggerConditionValue = Core.Constants.RefDocTypes.EntryPrint;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;

				Factory.Save();

				ImportEDocs(organisation);

				DeliverAndAssert(organisation, mode, trigger, action);
			}
			finally
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleDDAEventsWithOnlyOneMatch()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var mode = organisation.EDICommunicationsModes.AddNew();

			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@test.com";
			mode.EK_Filename = "eDoc from Organisation [(*JobNumber*)].txt";

			var trigger = organisation.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = Core.Constants.RefDocTypes.EntryPrint + "*";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;

			Factory.Save();

			AllocateEDocs(organisation);

			DeliverAndAssert(organisation, mode, trigger, action);
		}

		public void TestShouldNotDeliverIfTriggeringLogDoesNotExist()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var dataAdapter = new StorageDocsBaseValueObjectDataAdatper();
			var mode = organisation.EDICommunicationsModes.AddNew();

			var ddaTrigger = organisation.WorkflowItems.Triggers.AddNew();
			ddaTrigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			organisation.Logs.AddNew(Events.DocumentAllocated);

			var action = ddaTrigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;

			Factory.Save();

			var delivery = new XmlMessageDeliver(GetModes(new[] { mode }), organisation, dataAdapter, action);
			AssertNotNull(delivery);

			var notifications = new NotificationBuffer();
			delivery.Process(notifications);
			Factory.Save();

			AssertEquals("Shouldn't create an email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestGetBizObjSerializerForInitialisationHandlesEXLEvent()
		{
			using (Factory.AddDisposableService())
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				organisation.OH_Code = "ABC123";
				var dataAdapter = new StorageDocsBaseValueObjectDataAdatper();

				var eDoc = (organisation as IDocManagerSupport).DocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "SomeFilename", "MSC");

				var mode = organisation.EDICommunicationsModes.AddNew();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				mode.EK_Destination = "test@test.com";
				mode.EK_Filename = "eDoc from Organisation [(*JobNumber*)].txt";

				var trigger = organisation.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Document Imported";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Enterprise.ZArchitecture.Business.AutoEvents.DocumentImportedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEDocXml;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				AssertNoErrors(string.Format("Action trigger-type {0} should be valid", action.PQ_TriggerType), action.PQ_TriggerTypeInfo);

				organisation.Logs.AddNew(Enterprise.ZArchitecture.Business.AutoEvents.DocumentImported, eDoc.UniqueKey.ToString());
				organisation.Logs.AddNew(Enterprise.ZArchitecture.Business.AutoEvents.Received);

				Factory.Save();

				var wteLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

				var wteLogs = Factory.Load<StmALog>(wteLogsQuery);
				AssertEquals(1, wteLogs.Length);

				foreach (var wteLog in wteLogs)
				{
					var queuedLog = new QueuedLogForTesting(wteLog, trigger);
					var eventInfo = new EventInfoProvider(queuedLog, action.Parent, trigger.GetJob());
					var delivery = new XmlMessageDeliver(GetModes(new[] { mode }), organisation, organisation, dataAdapter, action, eventInfo);
					AssertNotNull(delivery);

					var notifications = new NotificationBuffer();
					delivery.Process(notifications);
					Factory.Save();

					AssertEquals("EDocSerializer should have been used for the SendEDocXml event", typeof(EDocSerializer), delivery.Serializer.GetType());
				}

				AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachment count on sent email", 1, createdEmail.Attachments.Count);
				var attachment = createdEmail.Attachments[0];
				AssertEquals("Sent Email Attachment Name", "eDoc from Organisation [ABC123].txt", attachment.DisplayName);
				var attachmentContent = Encoding.UTF8.GetString(attachment.Data);
				AssertContains("Attachment Content", "<Document>", attachmentContent);
				AssertContains("Attachment Content", "<DocumentType>MSC</DocumentType>", attachmentContent);
				AssertContains("Attachment Content", "<FileName>SomeFilename</FileName>", attachmentContent);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTriggerConditionReference()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var trigger = organisation.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Event.Reference.Contains(\"EPR Document 2\")";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, organisation.WorkflowItems.Triggers.FirstOrDefault().PK);

			AssertEquals("Initially no WTE events", 0, Factory.Load<StmALog>(query).Length);

			AllocateEDocs(organisation);

			AssertEquals("GIVEN trigger with Condition RelatedInformation.Contains('EPR Document 2'), should only fire on DDA where related-information(file description + filename) contains text 'EPR Document 2')", 1, Factory.Load<StmALog>(query).Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2015, 7, 14)]
		public void TestTriggerConditionReference_WithWildcard()
		{
			TestDateAttribute.UseUNLOCO = true;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var trigger = organisation.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "*R Document *";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			var documentAllocatedEvent = organisation.Logs.Find(l => l.SL_SE_NKEvent == Events.DocumentAllocatedCode).SingleOrDefault();

			CombineAssertions("The DDA trigger has not yet fired", () =>
			{
				AssertEquals("P9_ActualDate", ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
				AssertNull("DDA event", documentAllocatedEvent);
			});

			AllocateEDocs(organisation);

			documentAllocatedEvent = organisation.Logs.Find(l => l.SL_SE_NKEvent == Events.DocumentAllocatedCode && l.SL_Reference.Contains("Document 2")).SingleOrDefault();

			CombineAssertions("GIVEN trigger condition `*R Document *`, should only fire on DDA where related information (file description + filename) contains text 'R Document '", () =>
			{
				AssertEquals("P9_ActualDate", ZDateTimeOffset.Now, trigger.P9_ActualDateForBinding);
				AssertNotNull("DDA event", documentAllocatedEvent);
			});

			AssertContains("EPR|EPR Document 2|", documentAllocatedEvent.SL_Reference);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2015, 7, 14)]
		public void TestTriggerConditionReference_LimitNameLength()
		{
			TestDateAttribute.UseUNLOCO = true;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var longDocName = (ZString)@"Really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really long doc name";

			var trigger = organisation.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "*really*";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			var documentAllocatedEvent = organisation.Logs.Find(l => l.SL_SE_NKEvent == Events.DocumentAllocatedCode).SingleOrDefault();

			CombineAssertions("The DDA trigger has not yet fired", () =>
			{
				AssertEquals("P9_ActualDate", ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
				AssertNull("DDA event", documentAllocatedEvent);
			});

			var truncatedDocName = longDocName.SubstringSafe(0, 124);
			AllocateEDocs(organisation, longDocName, truncatedDocName);

			documentAllocatedEvent = organisation.Logs.Find(l => l.SL_SE_NKEvent == Events.DocumentAllocatedCode && l.SL_Reference.Contains(truncatedDocName)).FirstOrDefault();

			CombineAssertions("GIVEN trigger condition `*R Document *`, should only fire on DDA where related information (file description + filename) contains text 'R Document '", () =>
			{
				AssertEquals("P9_ActualDate", ZDateTimeOffset.Now, trigger.P9_ActualDateForBinding);
				AssertNotNull("DDA event", documentAllocatedEvent);
			});

			AssertContains("EPR|EPR Really really really really really really really really really", documentAllocatedEvent.SL_Reference);

			var regex = new Regex("[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}", RegexOptions.IgnoreCase);
			Assert("Should contain an entire GUID", regex.IsMatch(documentAllocatedEvent.SL_Reference));
		}

		MessageProcessorCommunicationModesResult GetModes(IList<IEDICommunicationsMode> modes) => new MessageProcessorCommunicationModesResult(modes, null);

		void ImportEDocs(OrgHeader organisation)
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProviderForTest>();
			var docFactory = documentFactoryProvider.GetFactory(Factory);
			using (docFactory as IDisposable)
			{
				var eprDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.EntryPrint));
				eprDocType.RT_ReferenceType = Constants.ReferenceTypes.All;

				var hblDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.HouseBill));
				hblDocType.RT_ReferenceType = Constants.ReferenceTypes.All;

				Factory.Save();

				docFactory.Import(new byte[] { 0x1, 0x2 }, "EPR Document 1", Core.Constants.DocManagerCodes.Organisation, organisation.PK, eprDocType.RT_DocType, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
				docFactory.Save();

				var storageMain = ((BusinessObjectFactory)docFactory).LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, organisation.PK));
				var epr1Doc = (BusinessObject)storageMain.AllEDocs[0];

				TestDateAttribute.Date = ZDateTime.Now.AddSeconds(1).ToDateTime();
				docFactory.Import(new byte[] { 0x1, 0x2 }, "HBL Document", Core.Constants.DocManagerCodes.Organisation, organisation.PK, hblDocType.RT_DocType, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
				docFactory.Save();
				var hblDoc = (BusinessObject)storageMain.AllEDocs[1];

				TestDateAttribute.Date = ZDateTime.Now.AddSeconds(1).ToDateTime();
				docFactory.Import(new byte[] { 0x1, 0x2 }, "EPR Document 2", Core.Constants.DocManagerCodes.Organisation, organisation.PK, eprDocType.RT_DocType, "DEF", ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
				docFactory.Save();
				var epr2Doc = (BusinessObject)storageMain.AllEDocs[2];

				docFactory.Save();

				var ddaQuery = new ZQuery(new ZQuery(StmALogSchema.SL_Parent, organisation.PK), new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentImportedCode));
				var ddaLogs = Factory.Load<StmALog>(ddaQuery);
				AssertEquals(3, ddaLogs.Length);

				var epr1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, string.Concat("EPR|", epr1Doc.PK.ToString())));
				AssertEquals(1, Factory.Load<StmALog>(epr1DocDDAQuery).Length);

				var hBL1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, string.Concat("HBL|", hblDoc.PK.ToString())));
				AssertEquals(1, Factory.Load<StmALog>(hBL1DocDDAQuery).Length);

				var epr2DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, string.Concat("EPR|", epr2Doc.PK.ToString())));
				AssertEquals(1, Factory.Load<StmALog>(epr2DocDDAQuery).Length);
			}
		}

		void AllocateEDocs(OrgHeader organisation, string baseDocName = "Document", string trimmedDocName = null)
		{
			var masterFactory = (organisation as IDocManagerSupport).DocManagerInfo.MasterFactory;
			var printJobPKAsString = ZGuid.NewZGuid().ToString();

			var eprDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.EntryPrint));
			eprDocType.RT_ReferenceType = Constants.ReferenceTypes.All;
			eprDocType.RT_SaveVersions = true;

			var hblDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.HouseBill));
			hblDocType.RT_ReferenceType = Constants.ReferenceTypes.All;

			Factory.Save();

			var epr1Doc = masterFactory.CreateAndAllocateDocument(
				organisation.PK,
				Core.Constants.DocManagerCodes.Organisation,
				BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\5pages.tif",
				eprDocType.RT_DocType,
				$"EPR {baseDocName} 1",
				false, "", "", "", printJobPKAsString);

			TestDateAttribute.Date = ZDateTime.Now.AddSeconds(1).ToDateTime();
			var hblDoc = masterFactory.CreateAndAllocateDocument(
				organisation.PK,
				Core.Constants.DocManagerCodes.Organisation,
				BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\5pages.tif",
				hblDocType.RT_DocType,
				$"HBL {baseDocName}",
				false, "", "", "", printJobPKAsString);

			TestDateAttribute.Date = ZDateTime.Now.AddSeconds(1).ToDateTime();
			var epr2Doc = masterFactory.CreateAndAllocateDocument(
				organisation.PK,
				Core.Constants.DocManagerCodes.Organisation,
				BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif",
				eprDocType.RT_DocType,
				$"EPR {baseDocName} 2",
				false, "", "", "", printJobPKAsString);

			masterFactory.Save();

			var ddaQuery = new ZQuery(new ZQuery(StmALogSchema.SL_Parent, organisation.PK), new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocatedCode));
			var ddaLogs = Factory.Load<StmALog>(ddaQuery);
			AssertEquals(3, ddaLogs.Length);

			if (trimmedDocName == null)
			{
				var epr1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, $"EPR|EPR {baseDocName} 1|{epr1Doc.PK}|{printJobPKAsString}"));
				AssertEquals(1, Factory.Load<StmALog>(epr1DocDDAQuery).Length);

				var hBL1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, $"HBL|HBL {baseDocName}|{hblDoc.PK}|{printJobPKAsString}"));
				AssertEquals(1, Factory.Load<StmALog>(hBL1DocDDAQuery).Length);

				var epr2DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, $"EPR|EPR {baseDocName} 2|{epr2Doc.PK}|{printJobPKAsString}"));
				AssertEquals(1, Factory.Load<StmALog>(epr2DocDDAQuery).Length);
			}
			else
			{
				var epr1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, $"EPR|EPR {trimmedDocName}|{epr1Doc.PK}|{printJobPKAsString}"));
				AssertEquals(1, Factory.Load<StmALog>(epr1DocDDAQuery).Length);

				var hBL1DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, $"HBL|HBL {trimmedDocName}|{hblDoc.PK}|{printJobPKAsString}"));
				AssertEquals(1, Factory.Load<StmALog>(hBL1DocDDAQuery).Length);

				var epr2DocDDAQuery = new ZQuery(ddaQuery, new ZQuery(StmALogSchema.SL_Reference, $"EPR|EPR {trimmedDocName}|{epr2Doc.PK}|{printJobPKAsString}"));
				AssertEquals(1, Factory.Load<StmALog>(epr2DocDDAQuery).Length);
			}
		}

		void DeliverAndAssert(OrgHeader organisation, EDICommunicationsMode mode, ProcessTask trigger, ProcessTaskNotification action)
		{
			using (Factory.AddDisposableService())
			{
				var dataAdapter = new StorageDocsBaseValueObjectDataAdatper();
				var wteLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

				var wteLogs = Factory.Load<StmALog>(wteLogsQuery);
				AssertEquals("2 WTE for EPR eDoc should be created, no WTE for HBL", 2, wteLogs.Length);

				foreach (var wteLog in wteLogs)
				{
					var queuedLog = new QueuedLogForTesting(wteLog, trigger);
					var eventInfo = new EventInfoProvider(queuedLog, action.Parent, trigger.GetJob());
					var delivery = new XmlMessageDeliver(GetModes(new[] { mode }), organisation, dataAdapter, action, eventInfo);
					AssertNotNull(delivery);

					var notifications = new NotificationBuffer();
					delivery.Process(notifications);
					Factory.Save();

					AssertEquals("EDocSerializer should have been used for the SendEDocXml event", typeof(EDocSerializer), delivery.Serializer.GetType());
				}

				AssertEquals("Should create an email", 2, Env.OutgoingMailManager.EmailsCreated.Count);

				for (var i = 0; i < Env.OutgoingMailManager.EmailsCreated.Count; i++)
				{
					var createdEmail = Env.OutgoingMailManager.EmailsCreated[i];
					AssertEquals("Attachment count on sent email", 1, createdEmail.Attachments.Count);
					var attachment = createdEmail.Attachments[0];
					AssertEquals("Sent Email Attachment Name", "eDoc from Organisation [XVBQP68SIYXQ].txt", attachment.DisplayName);
					var attachmentContent = Encoding.UTF8.GetString(attachment.Data);
					AssertContains("Attachment Content", "<Document>", attachmentContent);
					AssertNotContains("Attachment Content", "<DocumentType>HBL</DocumentType>", attachmentContent);
					AssertContains("Attachment Content", "<DocumentType>EPR</DocumentType>", attachmentContent);
					AssertContains("Attachment Content", string.Format("<FileName>EPR Document {0}</FileName>", i + 1), attachmentContent);
				}
			}
		}
	}
}
