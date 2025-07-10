using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmPrintQueue))]
	sealed class StmPrintQueueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLogsWhenSQ_QueueDeletedChanged()
		{
			var bizo = Factory.New<StmPrintQueue>();
			using (Env.Instance.TemporaryServiceTaskContext("PJM", true))
			{
				bizo.SQ_QueueDeleted = ZDateTime.Now;
				var log = bizo.Logs.MostRecentLogByEventTime(Events.EditedARecord, "Set it to offline, Service Task: PJM");
				AssertNotNull("Offline log", log);

				bizo.SQ_QueueDeleted = ZDateTime.Empty;
				log = bizo.Logs.MostRecentLogByEventTime(Events.EditedARecord, "Set it to online, Service Task: PJM");
				AssertNotNull("Online log", log);
			}
		}

		public void TestConcurrencyPolicy()
		{
			var bizo = Factory.New<StmPrintQueue>();
			var row = ((INeedRow)bizo).Row;
			var lastUsedDateTimeUtcConcurrencyPolicy = ConcurrencyInfo.Get(row, row.Table.Columns[nameof(StmPrintQueue.SQ_LastUsedDateTimeUtc)]);
			var printQueueStateChangedConcurrencyPolicy = ConcurrencyInfo.Get(row, row.Table.Columns[nameof(StmPrintQueue.SQ_PrintQueueStateChanged)]);
			AssertEquals(ConcurrencyPolicy.Ignore, lastUsedDateTimeUtcConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Ignore, printQueueStateChangedConcurrencyPolicy);
		}

		public void TestSQ_LastUsedDateTimeUtcReadonly()
		{
			AssertEquals("SQ_LastUsedDateTimeUtc ReadOnly should be true", true, Queue.SQ_LastUsedDateTimeUtcInfo.ReadOnly);
		}

		public void TestSecurityIsAllowed()
		{
			Assert("Seurity is granted by default for a new Print Queue", Queue.IsPrintAllowed);

			Env.Security.GetPrintQueueCheckPoint(Queue.PK.ToGuid(), Queue.SQ_DisplayName).IsAllowed = false;
			Assert("Seurity is denied when checkpoint is denied", !Queue.IsPrintAllowed);

			Env.Security.GetPrintQueueCheckPoint(Queue.PK.ToGuid(), Queue.SQ_DisplayName).IsAllowed = true;
			Assert("Seurity is granted when checkpoint is granted", Queue.IsPrintAllowed);
		}

		public void TestHumanReadableName()
		{
			Queue.SQ_DisplayName = "NotEmpty";
			Assert(Queue.HumanReadableName.Contains(Queue.SQ_DisplayName));
		}

		public void TestPrintLanguageList()
		{
			CodeDescriptionPairList printLanguageList = Queue.SQ_PrintLanguage_List;
			AssertNotNull(printLanguageList);
		}

		public void TestHasDefaultValues()
		{
			Assert("New Print Queue has default values", Queue.HasDefaultValues());

			Queue.SQ_QueueName = "TestQueue";
			Queue.SQ_ServerName = "TestServer";
			Queue.SQ_DisplayName = Queue.GetDefaultDisplayName();
			Queue.SQ_QueueDeleted = ZDateTime.Now;
			Factory.Save();
			Queue.Reload();

			Assert("With changed Queue and Server names and set Deleted datetime Queue has default values", Queue.HasDefaultValues());

			Queue.SQ_DisplayName = "TestDisplayName";
			Factory.Save();
			Queue.Reload();

			AssertEquals("With changed Display names Queue doesn't have default values", false, Queue.HasDefaultValues());

			Queue.SQ_DisplayName = Queue.GetDefaultDisplayName();
			Factory.Save();
			Queue.Reload();

			Assert("Restore default Display Name and Queue has default values", Queue.HasDefaultValues());

			Queue.SQ_SupressLetterhead = true;
			Factory.Save();
			Queue.Reload();

			AssertEquals("With changed property Queue doesn't have default values", false, Queue.HasDefaultValues());
		}

		public void TestGetDefaultDisplayName()
		{
			Queue.SQ_QueueName = "TestQueue";

			AssertEquals("Equals Queue Name", "TestQueue", Queue.GetDefaultDisplayName());

			Queue.SQ_QueueName = @"\\TestServer\TestQueue";

			AssertEquals("Get substring of Queue Name", "TestQueue", Queue.GetDefaultDisplayName());
		}

		public void TestGetDefaultPrintLanguage()
		{
			AssertEquals("Get default print language", "N/A", Queue.SQ_PrintLanguage);
		}

		public void TestCodeAndDescription()
		{
			Queue.SQ_DisplayName = "Queue";
			Queue.SQ_QueueName = @"\\Q\Queue";

			var codeDescription = (ICodeDescription)Queue;
			AssertEquals("PK", Queue.PK, codeDescription.PK);
			AssertEquals("Code", "Queue", codeDescription.Code);
			AssertEquals("Description", @"\\Q\Queue", codeDescription.Description);

			AssertEquals("CodePropertyAttribute", AutoStmPrintQueue.Schema.SQ_DisplayName, ((CodePropertyAttribute)TypeDescriptor.GetAttributes(Queue)[typeof(CodePropertyAttribute)]).PropertyName);
			AssertEquals("DescriptionPropertyAttribute", AutoStmPrintQueue.Schema.SQ_QueueName, ((DescriptionPropertyAttribute)TypeDescriptor.GetAttributes(Queue)[typeof(DescriptionPropertyAttribute)]).PropertyName);
		}

		public void TestOnSaving()
		{
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			ZGuid previousStamp = ZGuid.NewZGuid();
			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_QueueName = "hello";
			queue.SQ_PrintQueueStateChanged = previousStamp;

			Factory.Save();
			Assert("OnSaving, the queue should update the stamp", previousStamp != queue.SQ_PrintQueueStateChanged);

			previousStamp = queue.SQ_PrintQueueStateChanged;
			queue.SQ_Scale = 90;
			Factory.Save();
			Assert("OnSaving, the queue should update the stamp again", previousStamp != queue.SQ_PrintQueueStateChanged);
		}

		public void TestGetSerialisablePrintQueue()
		{
			Queue.SQ_QueueName = "abc";
			Queue.SQ_DisplayName = "def";
			Queue.SQ_ColumnScale = 1.1;
			Queue.SQ_RowScale = 1.2;
			Queue.SQ_Scale = 80;
			Queue.SQ_LeftMargin = 12;
			Queue.SQ_TopMargin = 34;
			Queue.SQ_PrintLanguage = "ghi";
			Queue.SQ_PrintQueueStateChanged = Guid.NewGuid();
			Queue.SQ_SupressLetterhead = true;
			Queue.SQ_XLSTemplateForPrintSettings = new byte[] { 9, 8, 7 };

			SerialisablePrintQueue serialisableQueue = Queue.GetSerialisablePrintQueue();
			AssertEquals(Queue.SQ_QueueName, serialisableQueue.Name);
			AssertEquals(Queue.SQ_DisplayName, serialisableQueue.DisplayName);
			AssertEquals(Queue.SQ_ColumnScale, serialisableQueue.ColumnScale);
			AssertEquals(Queue.SQ_RowScale, serialisableQueue.RowScale);
			AssertEquals(Queue.SQ_Scale, serialisableQueue.Scale);
			AssertEquals(Queue.SQ_LeftMargin, serialisableQueue.LeftMargin);
			AssertEquals(Queue.SQ_TopMargin, serialisableQueue.TopMargin);
			AssertEquals(Queue.SQ_PrintLanguage, serialisableQueue.PrintLanguage);
			AssertEquals(Queue.PK, serialisableQueue.QueuePk);
			AssertEquals(Queue.SQ_PrintQueueStateChanged, serialisableQueue.StateChangedStamp);
			AssertEquals(Queue.SQ_SupressLetterhead, serialisableQueue.SuppressLetterhead);
			AssertEquals(Queue.SQ_XLSTemplateForPrintSettings, serialisableQueue.XlsTemplate);
		}

		#region Test IStmPrintQueue Members

		public void TestIStmPrintQueue_PK()
		{
			AssertEquals(Queue.PK, ((Enterprise.Integration.DocumentEngine.IStmPrintQueue)Queue).PK);
		}

		public void TestIStmPrintQueue_QueueName()
		{
			var istmPrintQueue = (Enterprise.Integration.DocumentEngine.IStmPrintQueue)Queue;
			Queue.SQ_QueueName = "TEST";
			AssertEquals("TEST", istmPrintQueue.QueueName);

			istmPrintQueue.QueueName = "TEST1";
			AssertEquals("TEST1", Queue.SQ_QueueName);
		}

		#endregion

		public void TestDelete_DeleteAllUnusedStmData()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = "server";
			queue.SQ_QueueName = queue.SQ_DisplayName = "testqueueforhelpmsg";
			queue.SQ_Scale = 99;

			for (var i = 0; i < 9; i++)
			{
				var stmData = Factory.New<StmData>();
				stmData.SD_Name = "ShowHelpForPrinter" + queue.PK.ToGuid().ToString("N") + Guid.NewGuid().ToString("N");
			}

			Factory.Save();
			var stmDataCollection = new StmDataCollection(Factory, new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, $"ShowHelpForPrinter{queue.PK.ToGuid():N}"));

			AssertEquals("9 StmData records found", 9, stmDataCollection.Count);

			queue.Delete();

			AssertEquals("No StmData records found", 0, stmDataCollection.Count);
		}

		public void TestDelete_DeleteUnusedPrintQueueSecurity()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = "server";
			queue.SQ_QueueName = queue.SQ_DisplayName = "testqueueforhelpmsg";
			queue.SQ_Scale = 99;

			var security = Factory.New<GlbSecurity>();
			security.GU_SecurityRight = "StmPrintQueue";
			security.GU_ItemGUID = queue.PK.ToGuid();

			Factory.Save();

			var securityCollection = new GlbSecurityCollection(Factory);
			var filter1 = new ZQuery(GlbSecuritySchema.GU_SecurityRight, "StmPrintQueue");
			var filter2 = new ZQuery(GlbSecuritySchema.GU_ItemGUID, queue.PK.ToGuid());
			var filter = new ZQuery(filter1, filter2);
			securityCollection.Load(filter);

			AssertEquals("1 GlbSecurity records found", 1, securityCollection.Count);

			queue.Delete();

			AssertEquals("No GlbSecurity records found", 0, securityCollection.Count);
		}

		public void TestDelete_ClearsPlainPaperPrinterForeignKeys()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = "server";
			queue.SQ_QueueName = queue.SQ_DisplayName = "testqueueforhelpmsg";
			queue.SQ_Scale = 99;

			var ppQueue1 = Factory.New<StmPrintQueue>();
			ppQueue1.SQ_ServerName = "plainPaper";
			ppQueue1.SQ_QueueName = queue.SQ_DisplayName = "testqueuefoplain";
			ppQueue1.SQ_Scale = 99;
			ppQueue1.SQ_SQ_PlainPaperPrinter = queue.PK;

			var ppQueue2 = Factory.New<StmPrintQueue>();
			ppQueue2.SQ_ServerName = "plainPaper2";
			ppQueue2.SQ_QueueName = queue.SQ_DisplayName = "testqueuefoplain2";
			ppQueue2.SQ_Scale = 95;
			ppQueue2.SQ_SQ_PlainPaperPrinter = queue.PK;

			Factory.Save();

			AssertEquals("Precondition: FK exists", true, ppQueue1.SQ_SQ_PlainPaperPrinter.IsValid);
			AssertEquals("Precondition: FK exists", true, ppQueue2.SQ_SQ_PlainPaperPrinter.IsValid);

			queue.Delete();

			AssertEquals("Queue deleted gone", true, queue.IsDeleted);
			AssertEquals("FK1 gone", ZGuid.Empty, ppQueue1.SQ_SQ_PlainPaperPrinter);
			AssertEquals("FK2 gone", ZGuid.Empty, ppQueue2.SQ_SQ_PlainPaperPrinter);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Queue = Factory.New(typeof(StmPrintQueue)) as StmPrintQueue;
		}

		StmPrintQueue Queue;

		#endregion
	}
}
