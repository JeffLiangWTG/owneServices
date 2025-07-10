using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class LinkedDocumentMessageProcessorTest : TestCaseWithFactory
	{
		#region TestAddLog

		public void TestAddLog_Unsaved()
		{
			var dummy = Factory.New<DummyWithLogs>();
			var menuItem = CreateMenuItem(
				addToEDocs: false,
				Events.CustomisableEvent00Code,
				"Hello <typeof(@data).Name>");

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var log = CreateDataLinkedLog(dummy.PK, DummyBizoSchema.Constants.TableName);

			processor.Process(dummy, log);

			var logs = GetLogs(dummy, Events.CustomisableEvent00Code);
			AssertEquals("no logs were yet created", 0, logs.Length);

			Factory.Save();

			logs = GetLogsFromDb(dummy, Events.CustomisableEvent00Code);

			AssertEquals("created 1 log", 1, logs.Length);
			AssertEquals("Log reference was created from macro", "Hello DummyWithLogs", logs[0].SL_Reference);
			Assert("Log set to defer fire workflow", logs[0].SL_FireWorkflow);
		}

		public void TestAddLog_Unsaved_MultipleLogs()
		{
			var dummy = Factory.New<DummyWithLogs>();
			var menuItem = CreateMenuItem(
				addToEDocs: false,
				Events.CustomisableEvent00Code,
				"Hello <typeof(@data).Name>");

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var log1 = CreateDataLinkedLog(dummy.PK, DummyBizoSchema.Constants.TableName);
			var log2 = CreateDataLinkedLog(dummy.PK, DummyBizoSchema.Constants.TableName);

			processor.Process(dummy, log1);
			processor.Process(dummy, log2);

			var logs = GetLogs(dummy, Events.CustomisableEvent00Code);
			AssertEquals("no logs were yet created", 0, logs.Length);

			Factory.Save();

			logs = GetLogsFromDb(dummy, Events.CustomisableEvent00Code);

			AssertEquals("created 2 logs", 2, logs.Length);
			AssertEquals("Log reference was created from macro", "Hello DummyWithLogs", logs[0].SL_Reference);
			AssertEquals("Log reference was created from macro", "Hello DummyWithLogs", logs[1].SL_Reference);
			Assert("Log set to defer fire workflow", logs[0].SL_FireWorkflow);
			Assert("Log set to defer fire workflow", logs[1].SL_FireWorkflow);
		}

		public void TestAddLog_Saved()
		{
			var dummy = Factory.New<DummyWithLogs>();
			var menuItem = CreateMenuItem(
				addToEDocs: false,
				Events.CustomisableEvent00Code,
				"Hello <typeof(@data).Name>");

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var log = CreateDataLinkedLog(dummy.PK, DummyBizoSchema.Constants.TableName);

			Factory.Save();

			processor.Process(dummy, log);

			var logs = GetLogs(dummy, Events.CustomisableEvent00Code);

			AssertEquals("created 1 log", 1, logs.Length);
			AssertEquals("Log reference was created from macro", "Hello DummyWithLogs", logs[0].SL_Reference);
			Assert("Log set to defer fire workflow", logs[0].SL_FireWorkflow);
		}

		#endregion

		#region TestAddEDoc

		public void TestAddEDoc_Unsaved()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var menuItem = CreateMenuItem(
				addToEDocs: true,
				ZString.Empty,
				ZString.Empty);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var log = CreateDataLinkedLog(shipment.PK, JobShipmentSchema.Constants.TableName);

			processor.Process((IBusiness)shipment, log);

			StmPrintJob[] GetPrintJobsFromDB()
			{
				var printJobsQuery = new ZQuery();
				printJobsQuery.IgnoreDbQueryCache = true;
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobShipmentSchema.Constants.TableName);
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, shipment.PK);
				return Factory.Load<StmPrintJob>(printJobsQuery);
			}

			AssertEquals("no print jobs were created before factory save", 0, GetPrintJobsFromDB().Length);

			var factoriesBeforeSave = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories();

			Factory.Save();

			var factoriesAfterSave = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories();

			var printJobs = GetPrintJobsFromDB();
			AssertEquals("one print job was created after factory save", 1, printJobs.Length);

			bool FactoryHasMenuItem(BusinessObjectFactory f) => f.GetBizOsForPK(menuItem.PK.ToGuid()).Length > 0;
			bool FactoryHasPrintJob(BusinessObjectFactory f) => f.GetBizOsForPK(printJobs[0].PK.ToGuid()).Length > 0;

			var newFactoriesContainingMenuItemOrPrintJob = factoriesAfterSave
				.Except(factoriesBeforeSave)
				.Where(f => FactoryHasMenuItem(f) || FactoryHasPrintJob(f))
				.ToArray();

			Assert("the eDoc was created on the same factory as linked document was processed",
				newFactoriesContainingMenuItemOrPrintJob.Length == 1);
		}

		public void TestAddEDoc_Unsaved_MultipleLogs()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var menuItem = CreateMenuItem(
				addToEDocs: true,
				ZString.Empty,
				ZString.Empty);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var log1 = CreateDataLinkedLog(shipment.PK, JobShipmentSchema.Constants.TableName);
			var log2 = CreateDataLinkedLog(shipment.PK, JobShipmentSchema.Constants.TableName);

			processor.Process((IBusiness)shipment, log1);
			processor.Process((IBusiness)shipment, log2);

			void AssertPrintJobs(string message, int expectedShipmentPrintJobs)
			{
				var printJobsQuery = new ZQuery();
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobShipmentSchema.Constants.TableName);
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, shipment.PK);

				var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);
				AssertEquals(message, expectedShipmentPrintJobs, printJobs.Length);
			}

			AssertPrintJobs("no print jobs were created before factory save", 0);

			Factory.Save();

			AssertPrintJobs("two print job were created after factory save, one for each DLI log", 2);
		}

		public void TestAddEDoc_Saved()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var menuItem = CreateMenuItem(
				addToEDocs: true,
				ZString.Empty,
				ZString.Empty);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var log = CreateDataLinkedLog(shipment.PK, JobShipmentSchema.Constants.TableName);

			Factory.Save();

			processor.Process((IBusiness)shipment, log);

			void AssertPrintJobs(string message, int expectedShipmentPrintJobs)
			{
				var printJobsQuery = new ZQuery();
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, JobShipmentSchema.Constants.TableName);
				printJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, shipment.PK);

				var printJobs = Factory.Load<StmPrintJob>(printJobsQuery);
				AssertEquals(message, expectedShipmentPrintJobs, printJobs.Length);
			}

			AssertPrintJobs("one print job was created", 1);
		}

		#endregion

		#region TestAddingLogFromDocTypeTriggersWorkflow

		public void TestAddingLogFromDocTypeTriggersWorkflow()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();

			var milestone = ((IWorkflowProvider)consol).WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;

			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<JK_MasterBillIssueDate>";
			action.PQ_FieldValue = "<Now>";

			var menuItem = CreateMenuItem(
				addToEDocs: true,
				Events.BookingConfirmedCode,
				string.Empty);

			Factory.Save();

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK
			};

			var biz = (IBusiness)consol;
			var logParent = (IStmALogParent)consol;

			var log = CreateDataLinkedLog(consol.PK, JobConsolSchema.Constants.TableName);

			processor.Process(biz, log);

			var logs = GetLogs(logParent, Events.BookingConfirmedCode);
			AssertEquals("no logs were yet created", 0, logs.Length);

			var saveCount = Factory.SaveCount;
			Factory.Save();
			AssertEquals("no additional saves are done", Factory.SaveCount, saveCount + 1);

			logs = GetLogsFromDb(logParent, Events.BookingConfirmedCode);
			AssertEquals("created 1 log", 1, logs.Length);
		}

		#endregion

		#region TestReportConcurrencyException

		[TestDate(2022, 5, 31)]
		public void TestReportConcurrencyException()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "SEA";

			var milestone = ((IWorkflowProvider)consol).WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;

			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<JK_MasterBillIssueDate>";
			action.PQ_FieldValue = "<Now>";

			var menuItem = CreateMenuItem(
				addToEDocs: true,
				Events.BookingConfirmedCode,
				string.Empty);

			AssertEquals("prerequisite: JK_MasterBillIssueDate is not set", ZDateTime.Empty, consol.JK_MasterBillIssueDate);

			Factory.Save();

			void UpdateConsolCausingConcurrency(BusinessObjectFactory factory)
			{
				var consolInLinkerFactory = (Forwarding.IForwardingConsol)factory.GetBizOsForPK(consol.PK.ToGuid()).Single(); // consol has been loaded to process eDocs

				consolInLinkerFactory.JK_MasterBillIssueDate = ZDateTime.Today.AddDays(1);
				ConcurrencyInfo.SetConcurrencyPolicy((BusinessObject)consolInLinkerFactory, JobConsolSchema.Constants.JK_MasterBillIssueDate, ConcurrencyPolicy.Strict);

				var otherUserFactory = new BusinessObjectFactory();
				otherUserFactory.RefreshEnabled = false;

				var otherUserConsol = otherUserFactory.Load<Forwarding.IForwardingConsol>(consol.PK);
				otherUserConsol.JK_MasterBillIssueDate = ZDateTime.Today.AddDays(2);

				otherUserFactory.Save();
			}

			var processor = new DummyLinkedDocumentMessageProcessor
			{
				ShouldProcessImpl = _ => true,
				MenuItemPKImpl = () => menuItem.PK,
				OnBeforeSaveImpl = UpdateConsolCausingConcurrency
			};

			var biz = (IBusiness)consol;
			var logParent = (IStmALogParent)consol;

			var log = CreateDataLinkedLog(consol.PK, JobConsolSchema.Constants.TableName);

			processor.Process(biz, log);
			Factory.Save();
			((BusinessObject)consol).Reload();

			AssertEquals("It should attempt 3 times more in failing", 4, processor.SaveCallCounter);
			AssertEquals("JK_MasterBillIssueDate in db is the one set by another user", ZDateTime.Today.AddDays(2), consol.JK_MasterBillIssueDate);
			AssertEquals("Issue was reported", "LinkedDocumentMessageProcessor-CreateLogAndEDoc-FailedAfter3Attempts", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			// in practice the processor is invoked from service task when UniversalXml import is creating a DIL log
			Globals.IsUserInteractive = false;
		}

		sealed class DummyLinkedDocumentMessageProcessor : LinkedDocumentMessageProcessor
		{
			public Func<IEDIMessage, bool> ShouldProcessImpl { get; set; }
			public int SaveCallCounter { get; set; }
			public Func<ZGuid> MenuItemPKImpl { get; set; }
			public Action<BusinessObjectFactory> OnBeforeSaveImpl { get; set; }

			protected override bool ShouldProcess(IEDIMessage message) => ShouldProcessImpl?.Invoke(message) ?? false;

			protected override ZGuid MenuItemPK => MenuItemPKImpl?.Invoke() ?? ZGuid.Empty;
			protected override ZInt MenuItemDocumentCount => 1;

			protected override void OnBeforeSaveForTest(BusinessObjectFactory factory)
			{
				OnBeforeSaveImpl?.Invoke(factory);
				factory.Saved += Factory_Saved;
			}

			void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				SaveCallCounter++;
			}
		}

		IStmMenuItem CreateMenuItem(bool addToEDocs, ZString eventCode, ZString eventReferenceMacro)
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "menu name";
			menuItem.SU_MenuType = "FRM";

			var template = Factory.New<VisualizerTemplate>();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(tempFileName);
			}

			var docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "ALL";
			docType.RT_DocType = "XXX";
			docType.RT_LogMacro = eventReferenceMacro;
			docType.RT_SE_NKDocumentReceivedEvent = eventCode;
			docType.RT_LogSystemCreatedDocsToEDocs = addToEDocs;

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();
			pivot.SI_DocumentTitle = "doc";
			pivot.SI_DataStoreName = "TestDataStore";
			pivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;
			pivot.SI_RT_DocType = docType.PK;

			Factory.Save();

			return menuItem;
		}

		StmALog CreateDataLinkedLog(ZGuid parentPK, string tableName)
		{
			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.DataLinkedCode;
				log.SL_Parent = parentPK;
				log.SL_Table = tableName;
			}

			return log;
		}

		StmALog[] GetLogs(IStmALogParent logParent, ZString eventCode)
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == eventCode)
				.ToArray();
		}

		StmALog[] GetLogsFromDb(IStmALogParent logParent, ZString eventCode)
		{
			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Table, logParent.LogsParentTableName);
			query.AddToFilter(StmALogSchema.SL_Parent, logParent.LogsParentPK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);

			return logParent.Factory.Load<StmALog>(query);
		}

		#endregion
	}
}
