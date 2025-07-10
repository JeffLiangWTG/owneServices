using System;
using System.Data;
using System.Reflection;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPEPrintBatch))]
	sealed class UPEPrintBatchTest : EnterpriseBusinessObjectTestCase
	{
		#region New Properties

		public void TestIsPrinted()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			Callout callout = Factory.NewWithValidTestData<Callout>();
			printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), callout);
			AssertEquals("IsPrinted before printing", false, printBatch.IsPrinted);
			printBatch.Print();
			AssertEquals("IsPrinted after printing", true, printBatch.IsPrinted);
		}

		public void TestNumberOfPrintItems()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			Callout callout = Factory.NewWithValidTestData<Callout>();
			TestUPEPrintBatch printBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), callout);
			printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), callout);
			AssertEquals("NumberOfPrintItems when Items is loaded", 2, printBatch.NumberOfPrintItems);
			printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), callout);
			Factory.Save();
			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			TestUPEPrintBatch loadedPrintBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(loadingFactory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Items should not be loaded for this test", null, typeof(UPEPrintBatch).InvokeMember("fPrintItems", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, loadedPrintBatch, null));
			AssertEquals("NumberOfPrintItems when Items is not loaded", 3, loadedPrintBatch.NumberOfPrintItems);
		}

		#endregion
		#region Related Business Objects
		public void TestItems()
		{
			UPEPrintBatch printBatch = Factory.New<UPEPrintBatch>();
			AssertNotNull(printBatch.PrintItems);
		}

		#endregion
		#region QueueForBatchPrintAndSave

		public void TestQueueForBatchPrintAndSave()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatchItem printBatchItem = currentPrintBatch.QueueForBatchPrintAndSave(DocumentSupportable, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertEquals("Factory should be saved after print item queued", false, DocumentSupportable.HasChanges);
			AssertEquals("Factory should be saved after print item queued", false, printBatchItem.HasChanges);
			AssertEquals("PrintBatchItemQueued should be fired with the correct UPEPrintBatchItem in the event args", printBatchItem, LastPrintBatchItemQueuedEventArgs.PrintItem);
			AssertEquals("PrintBatchItemQueued should be fired with the NotifyUser=true", true, LastPrintBatchItemQueuedEventArgs.NotifyUser);
			currentPrintBatch.QueueForBatchPrintAndSave(DocumentSupportable, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK, false);
			AssertEquals("PrintBatchItemQueued should be fired with the NotifyUser=false", false, LastPrintBatchItemQueuedEventArgs.NotifyUser);
		}

		public void TestQueueForBatchPrintAndSave_WhenQueuingTwice()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			UPEPrintBatchItem printItem1 = currentPrintBatch.QueueForBatchPrintAndSave(DocumentSupportable, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertEquals("Should be queued into batch #1", 1, printItem1.PrintBatch.T7_BatchNumber);
			printItem1.PrintBatch.T7_LastPrintedDate = ZDateTime.Now;
			AssertEquals("Batch #1 should be marked as printed for the test", true, printItem1.PrintBatch.IsPrinted);
			currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			UPEPrintBatchItem printItem2 = currentPrintBatch.QueueForBatchPrintAndSave(DocumentSupportable, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
			AssertEquals("Should be queued in batch #2, because batch #1 was printed", 2, printItem2.PrintBatch.T7_BatchNumber);
			AssertEquals("The print batch item in batch #1 should be deleted because it has been re-queued in batch #2", true, printItem1.IsDeleted);
		}

		#endregion
		#region Print

		public void TestLastPrintedDate_UpdatedAfterBatchPrint()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPEPrintBatchItem item1 = CreatePrintBatchItem();
			UPEPrintBatchItem item2 = CreatePrintBatchItem();
			Factory.Save();
			PrintBatch.Print();
			AssertEquals("Last printed date should be populated after a batch is printed", true, PrintBatch.T7_LastPrintedDate.IsValid);
		}

		public void TestInvoiceBatchPrintedCounts_UpdatedAfterBatchPrint()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPEPrintBatchItem item1 = CreatePrintBatchItem();
			UPEPrintBatchItem item2 = CreatePrintBatchItem();
			Factory.Save();
			PrintBatch.Print();
			AssertEquals("InvoiceBatchPrintedCount should be incremented", 1, PrintBatch.T7_PrintCount);
			PrintBatch.Print();
			AssertEquals("InvoiceBatchPrintedCount should be incremented", 2, PrintBatch.T7_PrintCount);
		}

		public void TestPrintOrder()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			Callout callout1 = Factory.NewWithValidTestData<Callout>();
			callout1.BillToAccountNumber = "AccountID1";
			callout1.InvoiceNumber = "InvoiceNumber1";
			QueueTaxInvoiceForBatchPrintAndSave(callout1);
			Callout callout2 = Factory.NewWithValidTestData<Callout>();
			callout2.BillToAccountNumber = "AccountID2";
			callout2.InvoiceNumber = "InvoiceNumber1";
			QueueTaxInvoiceForBatchPrintAndSave(callout2);
			Callout callout3 = Factory.NewWithValidTestData<Callout>();
			callout3.BillToAccountNumber = "AccountID2";
			callout3.InvoiceNumber = "InvoiceNumber2";
			QueueTaxInvoiceForBatchPrintAndSave(callout3);
			UPEPrintBatchItem[] queueBatchItems = Factory.Load<UPEPrintBatchItem>(new ZQuery());
			AssertEquals("3 invoice documents should be printed", 3, queueBatchItems.Length);
		}

		void QueueTaxInvoiceForBatchPrintAndSave(Callout callout)
		{
			UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			currentPrintBatch.QueueForBatchPrintAndSave(callout, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
		}

		TestUPEPrintBatch PrintBatch
		{
			get
			{
				if (fPrintBatch == null)
				{
					fPrintBatch = (TestUPEPrintBatch)new TestUPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
				}

				return fPrintBatch;
			}
		}

		TestUPEPrintBatch fPrintBatch;
		UPEPrintBatchItem CreatePrintBatchItem()
		{
			Callout callout = Factory.NewWithValidTestData<Callout>();
			UPEPrintBatchItem result = PrintBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), callout);
			return result;
		}

		#endregion
		#region Implementation
		UPECusHAWB DocumentSupportable
		{
			get
			{
				if (fDocumentSupportable == null)
				{
					fDocumentSupportable = Factory.NewWithValidTestData<UPECusHAWB>();
					fDocumentSupportable.PrintBatchItemQueued += new PrintBatchItemQueuedEventHandler(OnDocumentSupportable_PrintBatchItemQueued);
				}

				return fDocumentSupportable;
			}
		}

		UPECusHAWB fDocumentSupportable;
		PrintBatchItemQueuedEventArgs LastPrintBatchItemQueuedEventArgs;
		void OnDocumentSupportable_PrintBatchItemQueued(object sender, PrintBatchItemQueuedEventArgs e)
		{
			LastPrintBatchItemQueuedEventArgs = e;
		}

		public override void TestFetchForLoad()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			base.TestFetchForLoad();
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
		#endregion
	}

	#region UPEPrintBatchTest_NonTransactioned

	[UseSnapshotProtection]
	class UPEPrintBatchTest_NonTransactioned : TestCase
	{
		[TestDate(2000, 1, 1)]
		public void TestCreateOrLoadLatestBatch_WhenNewBatchCreatedBy2UsersConcurrently()
		{
			ZDateTime nowAccessedEarlyToPreventThreadingProblem = ZDateTime.Now;
			GlbStaff currentUserAccessedEarlyToPreventThreadingProblem = GlbStaff.CurrentUser;
			BusinessObjectFactory factory = new BusinessObjectFactory();

			int initialPrintBatchCount = factory.GetDatabaseCount(typeof(UPEPrintBatch));
			AssertEquals(
				"There must be no " + ClientPrintBatchSchema.Constants.TableName + " records initially for this test, if there were they would be deleted at the end of this test",
				0, initialPrintBatchCount);

			ThreadSafeAccessTestCase.RunTestOnMultipleThreads(delegate
			{
				TestCreateOrLoadLatestBatch_WhenNewBatchCreatedBy2UsersConcurrently_ThreadStart();
			}, 2, ThreadSafeAccessTestCase.EndThreadTestAction.Join);

			if (ExceptionInOtherThread != null)
			{
				throw new InvalidOperationException(ExceptionInOtherThread.Message, ExceptionInOtherThread);
			}
			int printBatchCount = factory.GetDatabaseCount(typeof(UPEPrintBatch));
			AssertEquals("Only 1 print batch should be created", 1, printBatchCount);
		}

		void TestCreateOrLoadLatestBatch_WhenNewBatchCreatedBy2UsersConcurrently_ThreadStart()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				BusinessObjectFactory factory;
				lock (Env.Registry)
				{
					factory = new BusinessObjectFactory(connection);
					NonRegistryAccessingUPEPrintBatch printBatch =
						(NonRegistryAccessingUPEPrintBatch)new NonRegistryAccessingUPEPrintBatch.Loader(factory).CreateOrLoadLatestBatch(
								UPEPrintBatchTypes.Codes.TaxInvoice);
				}

				try
				{
					factory.Save();
				}
				catch (Exception ex)
				{
					ExceptionInOtherThread = ex;
				}
			}
		}

		[TestDate(2000, 1, 1)]
		public void TestCreateOrLoadLatestBatch_WhenNewBatchCreatedBy2UsersConcurrentlyWithCache()
		{
			ZDateTime nowAccessedEarlyToPreventThreadingProblem = ZDateTime.Now;
			GlbStaff currentUserAccessedEarlyToPreventThreadingProblem = GlbStaff.CurrentUser;
			BusinessObjectFactory factory = new BusinessObjectFactory();

			UPEPrintBatch newPrintBatch = factory.New<UPEPrintBatch>();
			newPrintBatch.T7_BatchType = "INV";
			newPrintBatch.T7_BatchNumber = 1;
			newPrintBatch.T7_LastPrintedDate = ZDateTime.Now;
			newPrintBatch.T7_PrintCount = 1;

			factory.Save();

			int initialPrintBatchCount = factory.GetDatabaseCount(typeof(UPEPrintBatch));
			AssertEquals(
				"There must be one " + ClientPrintBatchSchema.Constants.TableName + " record initially for this test, if there were they would be deleted at the end of this test",
				1, initialPrintBatchCount);

			ThreadSafeAccessTestCase.RunTestOnMultipleThreads(cancellationToken => TestCreateOrLoadLatestBatch_WhenNewBatchCreatedBy2UsersConcurrentlyWithCache_ThreadStart(), 2, ThreadSafeAccessTestCase.EndThreadTestAction.Join);
			if (ExceptionInOtherThread != null)
			{
				throw new InvalidOperationException(ExceptionInOtherThread.Message, ExceptionInOtherThread);
			}
			int printBatchCount = factory.GetDatabaseCount(typeof(UPEPrintBatch));
			AssertEquals("Only 1 print batch should be created (excluding the one created for this test)", 2, printBatchCount);
		}

		void TestCreateOrLoadLatestBatch_WhenNewBatchCreatedBy2UsersConcurrentlyWithCache_ThreadStart()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				lock (Env.Registry)
				{
					var factory = new BusinessObjectFactoryForTestingCache(connection);
					factory.RowFactoryForTest.DisableQueryCacheReset = true;

					try
					{
						NonRegistryAccessingUPEPrintBatch printBatch =
							(NonRegistryAccessingUPEPrintBatch)new NonRegistryAccessingUPEPrintBatchForTestingCache.Loader(factory).CreateOrLoadLatestBatch(
									UPEPrintBatchTypes.Codes.TaxInvoice);
					}
					catch (Exception ex)
					{
						ExceptionInOtherThread = ex;
					}
				}
			}
		}

		#region Test Classes

		class NonRegistryAccessingUPEPrintBatch : UPEPrintBatch
		{
			public NonRegistryAccessingUPEPrintBatch(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new class Loader : UPEPrintBatch.Loader
			{
				public Loader(BusinessObjectFactory factory)
					: base(factory, typeof(NonRegistryAccessingUPEPrintBatch))
				{
				}

				protected override int PrintBatchMaxCount
				{
					get { return 200; }
				}
			}
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
		class NonRegistryAccessingUPEPrintBatchForTestingCache : UPEPrintBatch
		{
			public NonRegistryAccessingUPEPrintBatchForTestingCache(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new class Loader : UPEPrintBatch.Loader
			{
				public Loader(BusinessObjectFactory factory)
					: base(factory, typeof(NonRegistryAccessingUPEPrintBatch))
				{
				}

				protected override int PrintBatchMaxCount
				{
					get { return 200; }
				}

				public new UPEPrintBatch CreateOrLoadLatestBatch(ZString batchType)
				{
					UPEPrintBatch result;
					try
					{
						result = TryCreateOrLoadLatestBatchForTest(batchType);
					}
					catch (ZSaveException ex)
					{
						if (ex.IndexNameIfUniqueIndexViolation == "NR_UX__T7_BatchType_T7_BatchNumber")
						{
							((BusinessObjectFactoryForTestingCache)Factory).RowFactoryForTest.DisableQueryCacheReset = false;
							Factory.ClearQueryCache(ClientPrintBatchSchema.Constants.TableName);
							result = TryCreateOrLoadLatestBatchForTest(batchType);
						}
						else
						{
							throw;
						}
					}
					return result;
				}
			}
		}

		class BusinessObjectFactoryForTestingCache : BusinessObjectFactory
		{
			public BusinessObjectFactoryForTestingCache(DbConnection connection) : base(connection) { }

			public RowFactory RowFactoryForTest { get { return _rowFactoryDoNotUseDirectly; } }
		}

		#endregion

		#region Implementation

		Exception ExceptionInOtherThread;
		bool PrintBatchTableExisted;
		bool PrintBatchItemTableExisted;

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.SetUp();
			PrintBatchTableExisted = ExistsDbTable(ClientPrintBatchSchema.Constants.TableName);
			PrintBatchItemTableExisted = ExistsDbTable(ClientPrintBatchItemSchema.Constants.TableName);
			if (!PrintBatchTableExisted)
			{
				ExecuteNonQuery(GetTableCreateScript(ClientPrintBatchSchema.Constants.TableName).CreateScript);
			}
			if (!PrintBatchItemTableExisted)
			{
				ExecuteNonQuery(GetTableCreateScript(ClientPrintBatchItemSchema.Constants.TableName).CreateScript);
			}
		}

		DatabaseObjectCreateScript GetTableCreateScript(string tableName)
		{
			foreach (var script in ClientHookLoader.Instance.ClientHook.DbSchemaExtensionObjects.TableCreationScripts)
			{
				if (script.ObjectName == tableName)
				{
					return script;
				}
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Baseline")]
		void ExecuteNonQuery(string sQL)
		{
			DbCommand command = Db.Connection.Command(sQL);
			command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Baseline")]
		bool ExistsDbTable(string tableName)
		{
			DbCommand command = Db.Connection.Command(string.Format("SELECT name FROM sys.objects WHERE name='" + tableName + "'"));
			object result = command.ExecuteScalar();
			return result != null;
		}

		#endregion
	}

	#endregion

	#region TestUPEPrintBatch

	public class TestUPEPrintBatch : UPEPrintBatch
	{
		public TestUPEPrintBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : UPEPrintBatch.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory, typeof(TestUPEPrintBatch))
			{
			}
		}

		public StmPrintQueue PrintQueue
		{
			get
			{
				if (fPrintQueue == null)
				{
					fPrintQueue = LoadOrCreateTestQueue();
				}
				return fPrintQueue;
			}
		}
		StmPrintQueue fPrintQueue;

		StmPrintQueue LoadOrCreateTestQueue()
		{
			StmPrintQueue testQueue = null;

			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName);
			var matchingQuery = new ZDBOnlyQuery(typeof(StmPrintQueue));
			matchingQuery.AddSubQuery(serverSubQuery, JoinCondition.And);
			matchingQuery.AddToFilter(StmPrintQueueSchema.SQ_DisplayName, "UPEPrintBatch_DisplayName");
			matchingQuery.AddToFilter(StmPrintQueueSchema.SQ_QueueName, "UPEPrintBatch_QueueName");

			StmPrintQueue[] queues = Factory.Load<StmPrintQueue>(matchingQuery);

			if (queues.Length > 0)
			{
				testQueue = queues[0];
			}
			else
			{
				testQueue = Factory.New<StmPrintQueue>();
				testQueue.SQ_ServerName = System.Environment.MachineName;
				testQueue.SQ_DisplayName = "UPEPrintBatch_DisplayName";
				testQueue.SQ_QueueName = "UPEPrintBatch_QueueName";
				Factory.Save();
			}
			return testQueue;
		}

		public bool PrintCalled;
		public override void Print()
		{
			base.Print();
			PrintCalled = true;
		}

		protected override DeliveryInstructions GetHardCopyDeliveryInstructions()
		{
			DeliveryInstructions result = new DeliveryInstructions();
			result.Destination = DeliveryInstructionDestination.Print;
			result.PrinterDelivery.PrintQueuePK = PrintQueue.PK;
			result.PrinterDelivery.NumberOfCopies = 1;
			return result;
		}
	}

	#endregion
}


