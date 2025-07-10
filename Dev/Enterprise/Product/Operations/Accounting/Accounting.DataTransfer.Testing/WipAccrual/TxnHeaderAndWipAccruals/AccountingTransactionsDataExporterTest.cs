using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.WipsAndAccruals;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	#region NonTransactionedTestingForDeadlock

	[UseSnapshotProtection]
	public class NonTransactionedTestingForDeadlock : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();
			_ = factory.New<ARInvoice>();

			factory.Save();
		}

		public void TestNoDeadlocksExistDuringExport()
		{
			using (var testConnection = Db.NewExtraConnectionToMainDb())
			{
				testConnection.BeginTransaction();
				testConnection.ExecuteNonQuery("UPDATE dbo.AccTransactionHeader SET AH_FullyPaidDate = NULL, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST'");
				Db.Connection.DefaultCommandTimeOutInSeconds = 20;

				using (var otherConnection = Db.Connection)
				{
					Assert("Different Connections", !Object.ReferenceEquals(testConnection, otherConnection));
					AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(new BusinessObjectFactory(otherConnection));

					exporter.FilterProvider.CurrentBatchNo = 0;
					exporter.FilterProvider.IncludeARInvoices = true;

					try
					{
						using (TempFile document = TempFile.New())
						{
							using (MemoryStream stream = new MemoryStream())
							{
								exporter.Export(stream);
							}
						}
					}
					catch (System.Data.Common.DbException ex)
					{
						Assert("There Should Be No DeadLock TimeOut", !ex.Message.StartsWith("Timeout expired.  The timeout period elapsed prior to completion of the operation"));
					}

					AssertEquals("There Should be 1 Invoice in Batch", exporter.NumberOfInvoicesInBatch, 1);
				}
				testConnection.RollbackTransaction();
				testConnection.CloseConnection();
			}
		}
	}

	#endregion

	#region AccountingTransactionsDataExporterTest

	[TestedType(typeof(TestTransactionExporter))]
	public class AccountingTransactionsDataExporterTest : AccountingTransactionsDataExporterTestCase
	{
		public void TestExportWritingObjectsInChunks()
		{
			CreateTestData();
			Factory.Save();
			var exporter = (TestTransactionExporter)NewExporter;

			exporter.FilterProvider.IncludeAPInvoices = true;
			exporter.FilterProvider.IncludeARInvoices = true;
			exporter.FilterProvider.IncludeAPCreditNotes = true;
			exporter.FilterProvider.IncludeARCreditNotes = true;
			exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
			exporter.FilterProvider.IncludeARAdjustmentNotes = true;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			exporter.FilterProvider.IncludeUnallocatedAPInvoices = true;
			exporter.FilterProvider.IncludeUnallocatedAPCreditNotes = true;

			exporter.FilterProvider.CurrentBatchNo = 1;
			AssertEquals("Number Of Invoices Transaction In Batch", 3, exporter.InvoiceBatchFilterPks.Count);
			AssertEquals("Number Of WIPAccrual Transaction In Batch", 2, exporter.WIPAccPostBatchFilterPks.Count);
			AssertEquals("Number Of Reverse WIPAccrual Transaction In Batch", 2, exporter.WIPAccReverseBatchFilterPks.Count);
			AssertEquals("Number Of UnAllocated Transaction InBatch", 1, exporter.UnallocatedTransactionBatchFilterPks.Count);

			exporter.SetWriteObjectsChunkSizeTestOnlyExposed(2);
			using (TempFile document = TempFile.New())
			{
				using (MemoryStream stream = new MemoryStream())
				{
					exporter.Export(stream);
				}

				ZString message = ExpectedMessageWhenThereIsNoError();
				AssertMultilineASCIIEquals("Message To Display when there are no errors with the export", message, exporter.GetMessageToDisplayWhenExportIsFinished());
			}
		}

		public void TestTestTransactionExporter_BizObjIsInvoice()
		{
			APInvoice anAPInvoice = Factory.Load<APInvoice>(Invoice.PK);
			TestTransactionExporter exporter = new TestTransactionExporter(Factory);
			BusinessObject correctlyTypedInvoice = exporter.LoadCorrectTypeOfBusinessObject(new ExportFinancialInvoiceDataAdapter(), anAPInvoice, String.Empty);

			AssertEquals(typeof(APInvoice), correctlyTypedInvoice.GetType());
		}

		public void TestTestTransactionExporter_BizObjIsWIP_Posted()
		{
			TestTransactionExporter exporter = new TestTransactionExporter(Factory);
			BusinessObject correctlyTypedWip = exporter.LoadCorrectTypeOfBusinessObject(new WIPAndAccrualDataAdapter(), WIP, nameof(Xsd.WipOrAccrualPostOrReverse.P));

			AssertEquals(typeof(WIPAccrualPRBusinessObject), correctlyTypedWip.GetType());
			AssertEquals(Xsd.WipOrAccrualPostOrReverse.P, ((WIPAccrualPRBusinessObject)correctlyTypedWip).PostedOrReverseStatus);
		}

		public void TestTestTransactionExporter_BizObjIsWIP_Reversed()
		{
			TestTransactionExporter exporter = new TestTransactionExporter(Factory);
			BusinessObject correctlyTypedWip = exporter.LoadCorrectTypeOfBusinessObject(new WIPAndAccrualDataAdapter(), WIP, nameof(Xsd.WipOrAccrualPostOrReverse.R));

			AssertEquals(typeof(WIPAccrualPRBusinessObject), correctlyTypedWip.GetType());
			AssertEquals(Xsd.WipOrAccrualPostOrReverse.R, ((WIPAccrualPRBusinessObject)correctlyTypedWip).PostedOrReverseStatus);
		}

		public void TestNotificationBufferHasInner()
		{
			TestTransactionExporter exporter = new TestTransactionExporter(Factory);
			AssertNotNull(exporter.ExporterNotification.Inner);
			AssertEquals(typeof(NotificationBuffer), exporter.ExporterNotification.Inner.GetType());
		}

		public void TestNumberOfBusinessObjectsExported()
		{
			string aRInvoiceBusinessObjectFullName = typeof(ARInvoice).FullName;
			string aPInvoiceBusinessObjectFullName = typeof(APInvoice).FullName;
			string wIPBusinessObjectFullName = typeof(WIP).FullName;

			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					AccountingTransactionsDataExporter exporter = NewExporter;
					ExporterToTestTransactionsProcessed = exporter;
					exporter.ProcessingProgressed += new EventHandler(Exporter_ProcessingProgressed);
					exporter.FilterProvider.IncludeARInvoices = true;
					exporter.FilterProvider.IncludeAPInvoices = true;
					exporter.FilterProvider.IncludeWIPsPosting = true;
					exporter.Export(streamToFile);
					AssertEquals("Number of Transaction Processed should be reset to 0", 0, exporter.PercentageComplete);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool aRInvoiceBusinessObjectFullNameFound = false;
					bool aPInvoiceBusinessObjectFullNameFound = false;
					bool wIPBusinessObjectFullNameFound = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.StartsWith(aRInvoiceBusinessObjectFullName))
						{
							aRInvoiceBusinessObjectFullNameFound = true;
						}

						if (lineFromFile.StartsWith(aPInvoiceBusinessObjectFullName))
						{
							aPInvoiceBusinessObjectFullNameFound = true;
						}

						if (lineFromFile.StartsWith(wIPBusinessObjectFullName))
						{
							wIPBusinessObjectFullNameFound = true;
						}
					}

					Assert("ARInvoice BusinessObject was never Exported", aRInvoiceBusinessObjectFullNameFound);
					Assert("APInvoice BusinessObject was never Exported", aPInvoiceBusinessObjectFullNameFound);
					Assert("WIP BusinessObject was never Exported", wIPBusinessObjectFullNameFound);
				}
			}
		}
		AccountingTransactionsDataExporter ExporterToTestTransactionsProcessed;

		public void TestExportingBusinessObjectsFromExistingBatch()
		{
			string aRInvoiceBusinessObjectFullName = typeof(ARInvoice).FullName;
			string wIPBusinessObjectFullName = typeof(WIP).FullName;

			AccountingTransactionsDataExporter exporter = NewExporter;
			ExporterToTestTransactionsProcessed = exporter;
			exporter.ProcessingProgressed += new EventHandler(Exporter_ProcessingProgressed);

			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					Assert(!exporter.IsTransactionsExistInBatch);

					exporter.FilterProvider.IncludeARInvoices = true;
					exporter.FilterProvider.IncludeWIPsPosting = true;

					exporter.Export(streamToFile);

					AssertEquals("Number of Transaction Processed should be reset to 0", 0, exporter.PercentageComplete);
					AssertEquals("Last Batch Number Of Transaction Should be 3 After export", 3, exporter.LastBatchNumberOfTransactions);
					AssertEquals("Should Export With Batch Number", 1, exporter.FilterProvider.CurrentBatchNo);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool aRInvoiceBusinessObjectFullNameFound = false;
					bool wIPBusinessObjectFullNameFound = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.StartsWith(aRInvoiceBusinessObjectFullName))
						{
							aRInvoiceBusinessObjectFullNameFound = true;
						}

						if (lineFromFile.StartsWith(wIPBusinessObjectFullName))
						{
							wIPBusinessObjectFullNameFound = true;
						}
					}

					Assert("ARInvoice BusinessObject was Exported", aRInvoiceBusinessObjectFullNameFound);
					Assert("WIP BusinessObject was Exported", wIPBusinessObjectFullNameFound);
				}
			}

			NumberOfTransactionsProcessed = 0;

			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					exporter.FilterProvider.IncludeARInvoices = true;
					exporter.FilterProvider.IncludeWIPsPosting = true;
					exporter.FilterProvider.CurrentBatchNo = 1;

					Assert("Transactions Exist in Batch 1", exporter.IsTransactionsExistInBatch);
					exporter.Export(streamToFile);

					AssertEquals("Number of Transaction Processed should be reset to 0", 0, exporter.PercentageComplete);
					AssertEquals("Last Batch Number Of Transaction Should be 3 After export", 3, exporter.LastBatchNumberOfTransactions);
					AssertEquals("Should Export With Batch Number", 1, exporter.FilterProvider.CurrentBatchNo);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool aRInvoiceBusinessObjectFullNameFound = false;
					bool wIPBusinessObjectFullNameFound = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.StartsWith(aRInvoiceBusinessObjectFullName))
						{
							aRInvoiceBusinessObjectFullNameFound = true;
						}

						if (lineFromFile.StartsWith(wIPBusinessObjectFullName))
						{
							wIPBusinessObjectFullNameFound = true;
						}
					}

					Assert("ARInvoice BusinessObject was Exported", aRInvoiceBusinessObjectFullNameFound);
					Assert("WIP BusinessObject was Exported", wIPBusinessObjectFullNameFound);
				}
			}
		}

		public void TestResetFiltersPksCacheForEachBatch()
		{
			string aRInvoiceBusinessObjectFullName = typeof(ARInvoice).FullName;
			string aPInvoiceBusinessObjectFullName = typeof(APInvoice).FullName;
			string wIPBusinessObjectFullName = typeof(WIP).FullName;

			AccountingTransactionsDataExporter exporter = NewExporter;
			ExporterToTestTransactionsProcessed = exporter;
			exporter.ProcessingProgressed += new EventHandler(Exporter_ProcessingProgressed);

			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					exporter.FilterProvider.IncludeARInvoices = true;
					exporter.FilterProvider.IncludeWIPsPosting = true;
					exporter.Export(streamToFile);

					AssertEquals("Number of Transaction Processed should be reset to 0", 0, exporter.PercentageComplete);
					AssertEquals("Last Batch Number Of Transaction Should be 3 After export", 3, exporter.LastBatchNumberOfTransactions);
					AssertEquals("Should Export With Batch Number", 1, exporter.FilterProvider.CurrentBatchNo);

					NumberOfTransactionsProcessed = 0;
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool aRInvoiceBusinessObjectFullNameFound = false;
					bool wIPBusinessObjectFullNameFound = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.StartsWith(aRInvoiceBusinessObjectFullName))
						{
							aRInvoiceBusinessObjectFullNameFound = true;
						}

						if (lineFromFile.StartsWith(wIPBusinessObjectFullName))
						{
							wIPBusinessObjectFullNameFound = true;
						}
					}

					Assert("ARInvoice BusinessObject was Exported", aRInvoiceBusinessObjectFullNameFound);
					Assert("WIP BusinessObject was Exported", wIPBusinessObjectFullNameFound);
				}
			}

			NumberOfTransactionsProcessed = 0;

			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					exporter.FilterProvider.IncludeAPInvoices = true;
					exporter.FilterProvider.CurrentBatchNo = 0;

					exporter.Export(streamToFile);

					AssertEquals("Number of Transaction Processed should be reset to 0", 0, exporter.PercentageComplete);
					AssertEquals("Last Batch Number Of Transaction Should be 1 After export", 1, exporter.LastBatchNumberOfTransactions);
					AssertEquals("Should Export With Batch Number", 2, exporter.FilterProvider.CurrentBatchNo);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool aPInvoiceBusinessObjectFullNameFound = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.StartsWith(aPInvoiceBusinessObjectFullName))
						{
							aPInvoiceBusinessObjectFullNameFound = true;
						}
					}

					Assert("APInvoice BusinessObject was Exported", aPInvoiceBusinessObjectFullNameFound);
				}
			}
		}

		int NumberOfTransactionsProcessed;
		void Exporter_ProcessingProgressed(object sender, EventArgs e)
		{
			NumberOfTransactionsProcessed++;
			int percentageComplete = Convert.ToInt32(NumberOfTransactionsProcessed / (float)ExporterToTestTransactionsProcessed.LastBatchNumberOfTransactions * 100);
			AssertEquals("Number of transactions were not incremented", NumberOfTransactionsProcessed, ExporterToTestTransactionsProcessed.NumberOfTransactionsProcessed);
			AssertEquals("Percentage Complete was not the same", percentageComplete, ExporterToTestTransactionsProcessed.PercentageComplete);
		}

		public void TestExportAlwaysResetFiltersPKCache()
		{
			AccountingTransactionsDataExporter exporter = new XmlAccountingTransactionExporter(Factory);

			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					exporter.Export(streamToFile);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool transactionLineExists = false;
					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.Contains("TxnNumber"))
						{
							transactionLineExists = true;
						}
					}
					Assert("First run - should not contain any transactions", !transactionLineExists);
				}
			}

			exporter.FilterProvider.IncludeARInvoices = true;
			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					exporter.Export(streamToFile);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool transactionLineExists = false;
					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile.Contains("TxnNumber"))
						{
							transactionLineExists = true;
						}
					}
					Assert("Second run - should contain at leaset 1 transaction because batchInvoicePKs has been reset to null so it will be loaded again", transactionLineExists);
				}
			}
		}

		public void TestBeforeAndAfterDocumentBuildExecuted()
		{
			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					NewExporter.Export(streamToFile);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool beforeTagFound = false;
					bool afterTagFound = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile == TestTransactionExporter.BeforeDocumentBuildString)
						{
							beforeTagFound = true;
						}

						if (lineFromFile == TestTransactionExporter.AfterDocumentBuildString)
						{
							afterTagFound = true;
						}
					}

					Assert("BeforeDocumentBuild was never called", beforeTagFound);
					Assert("AfterDocumentBuild was never called", afterTagFound);
				}
			}
		}

		public void TestDocumentInitialisedAndDeInitialised()
		{
			using (TempFile document = TempFile.New())
			{
				using (FileStream streamToFile = new FileStream(document.Filename, FileMode.Create, FileAccess.ReadWrite))
				{
					AccountingTransactionsDataExporter exporter = NewExporter;
					exporter.FilterProvider.IncludeARInvoices = true;
					exporter.FilterProvider.IncludeARCreditNotes = true;
					exporter.FilterProvider.IncludeWIPsPosting = true;
					exporter.Export(streamToFile);
					Assert("Stream was not DeInitialised", !streamToFile.CanRead);
				}

				using (StreamReader reader = new StreamReader(document.Filename))
				{
					bool initialised = false;
					bool deInitialised = false;

					string lineFromFile = null;
					while ((lineFromFile = reader.ReadLine()) != null)
					{
						if (lineFromFile == TestTransactionExporter.InitialiseDocumentWriterString)
						{
							initialised = true;
						}

						if (lineFromFile == TestTransactionExporter.DeInitialiseDocumentWriterString)
						{
							deInitialised = true;
						}
					}

					Assert("Initialised was never called", initialised);
					Assert("DeInitialised was never called", deInitialised);
				}
			}
		}

		public void TestDecideAdapterByBusinessObjectType()
		{
			TestTransactionExporter testExporter = new TestTransactionExporter(Factory);

			Assert("Adapter should be WIPAndAccrualDataAdapter", testExporter.DecideAdapterByBusinessObjectType(Factory.New<WIP>()) is WIPAndAccrualDataAdapter);
			Assert("Adapter should be WIPAndAccrualDataAdapter", testExporter.DecideAdapterByBusinessObjectType(Factory.New<Accrual>()) is WIPAndAccrualDataAdapter);
			try
			{
				testExporter.DecideAdapterByBusinessObjectType(Factory.New<AccTransactionLines>());
				Fail("Should throw a NotSupportedException exception");
			}
			catch (NotSupportedException e)
			{
				AssertEquals("Could not load DataAdapter for BizObj Type ( " + typeof(AccTransactionLines).FullName + ")", e.Message);
			}
			catch (Exception e)
			{
				Fail("Did not expect any other exceptions but was a " + e.GetType().FullName + " with the following Message :" + e.Message);
			}

			Assert("Adapter should be ExportFinancialInvoiceDataAdapter", testExporter.DecideAdapterByBusinessObjectType(Factory.New<APInvoice>()) is ExportFinancialInvoiceDataAdapter);
			Assert("Adapter should be UnallocatedTransactionDataAdapter", testExporter.DecideAdapterByBusinessObjectType(Factory.New<TransactionPendingAllocation>()) is UnallocatedTransactionDataAdapter);
			try
			{
				testExporter.DecideAdapterByBusinessObjectType(Factory.New<AccTransactionHeader>());
				Fail("Should throw a NotSupportedException exception");
			}
			catch (NotSupportedException e)
			{
				AssertEquals("Could not load DataAdapter for BizObj Type ( " + typeof(AccTransactionHeader).FullName + ")", e.Message);
			}
			catch (Exception e)
			{
				Fail("Did not expect any other exceptions but was a " + e.GetType().FullName + " with the following Message :" + e.Message);
			}
		}

		#region MutexTest

		public void TestBatchExporterMethodFailureWhenFailtoObtainMutex()
		{
			OtherMutex = new ZGlobalMutex(MutexIDs.BatchXMLExportNumber, Env.CurrentCompany.PK.ToString());
			Assert(OtherMutex.Lock());

			var testExporter = (TestTransactionExporter)NewExporter;
			bool result = testExporter.CreateNewBatch();

			AssertEquals("Exporter Should Fail Due To Mutex using CurrentCompany PK.", false, result);
			AssertContains("Correct error message returned", "Batch Exporter update has been canceled because a Batch Exporter update is currently being run by user 'CargoWise Support'", testExporter.BatchExportResult);
			Assert("FailedToAquireMutex property should be true", testExporter.FailedToAquireMutex);
		}

		public void TestBatchExporterMethodFailureWhenFailtoObtainMutexAndLockInfoIsNull()
		{
			OtherMutex = new ZGlobalMutex(MutexIDs.BatchXMLExportNumber, Env.CurrentCompany.PK.ToString());
			Assert(OtherMutex.Lock());

			var testExporter = (TestTransactionExporter)NewExporter;
			testExporter.ReleaseLock_ForTest = () => OtherMutex.Unlock();
			bool result = testExporter.CreateNewBatch();

			AssertEquals("Aggregation Should Fail Due To Mutex using CurrentCompany PK.", false, result);
			AssertEquals("Correct error message returned", "Batch Exporter update has been canceled because a Batch Exporter update is currently being run by user '*unknown user*' since *unknown time*", testExporter.BatchExportResult);
			Assert("FailedToAquireMutex property should be true", testExporter.FailedToAquireMutex);
		}

		public void TestBatchExporterMethodUsesCompanySpecificMutex()
		{
			OtherMutex = new ZGlobalMutex(MutexIDs.BatchXMLExportNumber, ZGuid.NewZGuid().ToString());
			Assert(OtherMutex.Lock());

			ARInvoice testARInvoice = Factory.New<ARInvoice>();
			Factory.Save();

			var testExporter = (TestTransactionExporter)NewExporter;
			testExporter.FilterProvider.IncludeARInvoices = true;
			bool result = testExporter.CreateNewBatch();

			Assert("Batch Exporter Should Succeed Due To Company Specific Mutex.", result);
			AssertEquals("FailedToAquireMutex property should be false", false, testExporter.FailedToAquireMutex);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestBatchExporterMethodFailureWhenFailtoObtainMutexWithNullUserInLockInfo()
		{
			Db.Connection.BeginTransaction();
			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "UPDATE dbo.GlbStaff SET GS_CODE  = 'TT', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetUtcDate() WHERE GS_Code = '{0}'", GlbStaff.CurrentUser.GS_Code));
			Db.Connection.CommitTransaction();

			OtherMutex = new ZGlobalMutex(MutexIDs.BatchXMLExportNumber, Env.CurrentCompany.PK.ToString());
			Assert(OtherMutex.Lock());

			var testExporter = (TestTransactionExporter)NewExporter;
			bool result = testExporter.CreateNewBatch();

			AssertEquals("Exporter Should Fail Due To Mutex using CurrentCompany PK.", false, result);
			AssertContains("Correct error message returned", "Batch Exporter update has been canceled because a Batch Exporter update is currently being run by user '*unknown user*'", testExporter.BatchExportResult);
			Assert("FailedToAquireMutex property should be true", testExporter.FailedToAquireMutex);
		}

		protected override void TearDown()
		{
			if (OtherMutex != null && OtherMutex.IsLocked)
			{
				OtherMutex.Unlock();
			}
		}

		#endregion

		#region Implementation

		protected override AccountingTransactionsDataExporter NewExporter
		{
			get { return new TestTransactionExporter(Factory); }
		}

		protected override bool ShouldTestMessagesToDisplayWhenExportIsFinished()
		{
			return true;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TestTransactionExporter(Factory);
		}

		protected ZGlobalMutex OtherMutex;

		#region Setup

		public class TestTransactionExporter : AccountingTransactionsDataExporter
		{
			public TestTransactionExporter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
			{
				DocWriter.WriteLine(bizObj.GetType().FullName + " " + bizObj.PK.ToString());
				BusinessObject bizObjToReport = LoadCorrectTypeOfBusinessObject(dataAdapter, bizObj, status);
				IncreaseStandardTransactionsProcessedCount(bizObjToReport);
			}

			public NotificationBuffer ExporterNotification
			{
				get { return Notify; }
			}

			protected override void InitialiseDocumentWriter(Stream exportFile)
			{
				Document = exportFile;
				DocWriter = new StreamWriter(Document);
				DocWriter.WriteLine(InitialiseDocumentWriterString);
			}

			protected override void DeInitialiseDocumentWriter()
			{
				DocWriter.WriteLine(DeInitialiseDocumentWriterString);
				DocWriter.Flush();
				base.DeInitialiseDocumentWriter();
			}

			protected override void BeforeDocumentBuild()
			{
				base.BeforeDocumentBuild();
				DocWriter.WriteLine(BeforeDocumentBuildString);
			}

			protected override void AfterDocumentBuild()
			{
				base.AfterDocumentBuild();
				DocWriter.WriteLine(AfterDocumentBuildString);
			}

			public new BusinessObject LoadCorrectTypeOfBusinessObject(IValueObjectDataAdapter dataAdapter, BusinessObject bizObj, ZString status)
			{
				return base.LoadCorrectTypeOfBusinessObject(dataAdapter, bizObj, status);
			}

			public new IValueObjectDataAdapter DecideAdapterByBusinessObjectType(BusinessObject bizObj)
			{
				return base.DecideAdapterByBusinessObjectType(bizObj);
			}

			public void SetWriteObjectsChunkSizeTestOnlyExposed(int value)
			{
				SetWriteObjectsChunkSizeForTestOnly(value);
			}

			public const string InitialiseDocumentWriterString = "InitialiseDocumentWriter";
			public const string DeInitialiseDocumentWriterString = "DeInitialiseDocumentWriter";
			public const string BeforeDocumentBuildString = "BeforeDocumentBuild";
			public const string AfterDocumentBuildString = "AfterDocumentBuild";

			StreamWriter DocWriter;
		}

		#endregion

		#endregion
	}

	#endregion

	#region AccountingTransactionsDataExporterTestCase

	public abstract class AccountingTransactionsDataExporterTestCase : NonPersistentBusinessObjectTestCase
	{
		protected abstract AccountingTransactionsDataExporter NewExporter { get; }

		public void TestGetMessageToDisplayWhenExportIsFinishedWhenErrorOccurs()
		{
			if (ShouldTestMessagesToDisplayWhenExportIsFinished())
			{
				CreateTestData();
				Factory.Save();

				AccountingTransactionsDataExporter exporter = NewExporter;
				exporter.FilterProvider.CurrentBatchNo = 1;

				using (TempFile document = TempFile.New())
				{
					using (MemoryStream stream = new MemoryStream())
					{
						exporter.Export(stream);
					}

					SimulateErrorInExport();
					Factory.Save();
					ZString message = ExpectedMessageWhenErrorOccurs();
					AssertMultilineASCIIEquals("Message To Display when there are errors with the export", message, exporter.GetMessageToDisplayWhenExportIsFinished());
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestLedgerSpecificTransactionsInBatch()
		{
			CreateTestData();
			CreateApTestData();
			Factory.Save();
			AccountingTransactionsDataExporter exporter = NewExporter;
			exporter.FilterProvider.CurrentBatchNo = 1;
			AssertEquals("NumberOfArInvoicesInBatch", 1, exporter.NumberOfLedgerSpecificInvoicesInBatch(ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("NumberOfApInvoicesInBatch", 1, exporter.NumberOfLedgerSpecificInvoicesInBatch(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("NumberOfArCreditNotesInBatch", 1, exporter.NumberOfLedgerSpecificCreditNotesInBatch(ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("NumberOfApCreditNotessInBatch", 1, exporter.NumberOfLedgerSpecificCreditNotesInBatch(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("NumberOfArAdjustmentNotesInBatch", 1, exporter.NumberOfLedgerSpecificAdjustmentNotesInBatch(ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("NumberOfApAdjustmentNotesInBatch", 1, exporter.NumberOfLedgerSpecificAdjustmentNotesInBatch(ZArchitecture.Core.LedgerTypes.AccountsPayable));
		}

		public void TestBusinessObjectFilterPks()
		{
			CreateTestData();
			Factory.Save();
			AccountingTransactionsDataExporter exporter = NewExporter;

			exporter.FilterProvider.IncludeARInvoices = true;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;
			exporter.FilterProvider.IncludeUnallocatedAPInvoices = true;

			exporter.FilterProvider.CurrentBatchNo = 1;
			AssertEquals("Number Of Invoices Transaction In Batch", 3, exporter.InvoiceBatchFilterPks.Count);
			AssertEquals("Number Of WIPAccrual Transaction In Batch", 2, exporter.WIPAccPostBatchFilterPks.Count);
			AssertEquals("Number Of Reverse WIPAccrual Transaction In Batch", 2, exporter.WIPAccReverseBatchFilterPks.Count);
			AssertEquals("Number Of UnAllocated Transaction InBatch", 1, exporter.UnallocatedTransactionBatchFilterPks.Count);
		}

		public void TestGetMessageToDisplayWhenExportIsFinishedWhenThereIsNoError()
		{
			if (ShouldTestMessagesToDisplayWhenExportIsFinished())
			{
				CreateTestData();
				Factory.Save();

				AccountingTransactionsDataExporter exporter = NewExporter;
				exporter.FilterProvider.CurrentBatchNo = 1;

				exporter.FilterProvider.IncludeAPInvoices = true;
				exporter.FilterProvider.IncludeARInvoices = true;
				exporter.FilterProvider.IncludeAPCreditNotes = true;
				exporter.FilterProvider.IncludeARCreditNotes = true;
				exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
				exporter.FilterProvider.IncludeARAdjustmentNotes = true;
				exporter.FilterProvider.IncludeWIPsPosting = true;
				exporter.FilterProvider.IncludeWIPsReversing = true;
				exporter.FilterProvider.IncludeAccrualsPosting = true;
				exporter.FilterProvider.IncludeAccrualsReversing = true;
				exporter.FilterProvider.IncludeUnallocatedAPInvoices = true;
				exporter.FilterProvider.IncludeUnallocatedAPCreditNotes = true;

				using (TempFile document = TempFile.New())
				{
					using (MemoryStream stream = new MemoryStream())
					{
						exporter.Export(stream);
					}

					ZString message = ExpectedMessageWhenThereIsNoError();
					AssertMultilineASCIIEquals("Message To Display when there are no errors with the export", message, exporter.GetMessageToDisplayWhenExportIsFinished());
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestGetMessageToDisplayWhenMessageParameterIsUsed()
		{
			AccountingTransactionsDataExporter exporter = NewExporter;
			string message = "this is the message that should be at the beginning of the Display Message";
			Assert("Message to Display did not Start with the Message Parameter", exporter.GetMessageToDisplayWhenExportIsFinished(message).StartsWith(message));
		}

		protected virtual void CreateTestData()
		{
			InvoiceForExport = PopulateTransactionHeaderAndSetBatchNumber(typeof(ARInvoice), 1);
			CreditNoteForExport = PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote), 1);
			AdjustmentNoteForExport = PopulateTransactionHeaderAndSetBatchNumber(typeof(ARAdjustmentNote), 1);

			FillWIPAccrualBizObjWithTestData(typeof(WIP), ObjectCreator.CC1, 100M);
			WipForExport = WIP;
			WipForExport.RelatedJobCharge.ReverseWIP(ZDateTime.Today);
			ObjectCreator.CreateGenExportBatchSequencePostLine(1, WipForExport.PK, 1);
			ObjectCreator.CreateGenExportBatchSequenceReverseLine(1, WipForExport.PK, 1);

			FillWIPAccrualBizObjWithTestData(typeof(Accrual), ObjectCreator.CC1, 100M);
			AccrualForExport = WIP;
			AccrualForExport.RelatedJobCharge.ReverseAccrual(ZDateTime.Today);
			ObjectCreator.CreateGenExportBatchSequencePostLine(1, AccrualForExport.PK, 1);
			ObjectCreator.CreateGenExportBatchSequenceReverseLine(1, AccrualForExport.PK, 1);

			TransactionPendingAllocationForExport = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			TransactionPendingAllocationForExport.AH_PostDate = PostDate;
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, TransactionPendingAllocationForExport.PK, 1);

			TransactionPendingAllocationForExport.AH_RX_NKTransactionCurrency = ObjectCreator.AUD.RX_Code;
			TransactionPendingAllocationForExport.AH_OSTotal = 100m;
			TransactionPendingAllocationForExport.AH_InvoiceAmount = 100m;
		}

		protected virtual void CreateApTestData()
		{
			InvoiceForExport = PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice), 1);
			CreditNoteForExport = PopulateTransactionHeaderAndSetBatchNumber(typeof(APCreditNote), 1);
			AdjustmentNoteForExport = PopulateTransactionHeaderAndSetBatchNumber(typeof(APAdjustmentNote), 1);
		}

		protected virtual void SimulateErrorInExport()
		{
			// Reset the batch number on these transactions to cause an error
			InvoiceForExport.ExportedBatchSequence.Delete();
			CreditNoteForExport.ExportedBatchSequence.Delete();
			AdjustmentNoteForExport.ExportedBatchSequence.Delete();
			WipForExport.ExportBatchSequencePostedObject.Delete();
			WipForExport.ExportBatchSequenceReversedObject.Delete();
			AccrualForExport.ExportBatchSequencePostedObject.Delete();
			AccrualForExport.ExportBatchSequenceReversedObject.Delete();
			TransactionPendingAllocationForExport.ExportedBatchSequence.Delete();
		}

		protected abstract bool ShouldTestMessagesToDisplayWhenExportIsFinished();

		[TestDate(2005, 01, 01, 12, 0, 0)]
		public void TestExportExistingBatch()
		{
			AccountingTransactionsDataExporter exporter = NewExporter;
			exporter.FilterProvider.CurrentBatchNo = 0;
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
			exporter.FilterProvider.IncludeAPCreditNotes = true;
			exporter.FilterProvider.IncludeAPInvoices = true;
			exporter.FilterProvider.IncludeARAdjustmentNotes = true;
			exporter.FilterProvider.IncludeARCreditNotes = true;
			exporter.FilterProvider.IncludeARInvoices = true;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;

			int lastBatchNumber = 0;
			var assertHighWaterMark = exporter is ISupportHighWaterMark && ((ISupportHighWaterMark)exporter).IsHighWaterMarkEnabled;

			AssertEquals("Precondition: High water mark registry value is not set", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

			using (TempFile firstBatchTempFile = TempFile.New())
			{
				using (FileStream exportedXmlFileStream = new FileStream(firstBatchTempFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					exporter.Export(exportedXmlFileStream);
					if (assertHighWaterMark)
					{
						AssertEquals("High water mark registry value should have been set because a new batch was exported", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
					}
					else
					{
						AssertEquals("High water mark registry value should not have been set because it is not applicable", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
					}
				}

				using (TempFile tempTestFile = TempFile.New())
				{
					using (FileStream exportedXmlFileStream = new FileStream(tempTestFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					{
						SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
						WIP.Reload();
						AccountingTransactionsDataExporter reExporter = NewExporter;
						reExporter.FilterProvider.CurrentBatchNo = WIP.ExportBatchSequencePostedObject.XB_BatchNumber;
						reExporter.Export(exportedXmlFileStream);
						AssertEquals("High water mark registry value should not have been set because an existing batch was exported", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

						Invoice.Reload();
						WIP.Reload();
						AssertNotNull("Batch Exist After export", Invoice.ExportedBatchSequence);
						AssertNotNull("Batch number Exist after export", WIP.ExportBatchSequencePostedObject);

						lastBatchNumber = WIP.ExportBatchSequencePostedObject.XB_BatchNumber;
					}

					using (StreamReader firstDocumentReader = new StreamReader(firstBatchTempFile.Filename))
					{
						using (StreamReader secondDocumentReader = new StreamReader(tempTestFile.Filename))
						{
							AssertDocumentsAreTheSame(firstDocumentReader, secondDocumentReader);
						}
						Invoice.Reload();
						WIP.Reload();
						AssertEquals("a new batch number was specified when one should not have", lastBatchNumber, Invoice.ExportedBatchSequence.XB_BatchNumber);
						AssertEquals("a new batch number was specified when one should not have", lastBatchNumber, WIP.ExportBatchSequencePostedObject.XB_BatchNumber);
					}
				}
			}
		}

		protected virtual void AssertDocumentsAreTheSame(StreamReader firstDocument, StreamReader secondDocument)
		{
			AssertEquals("First batch document and second batch document are not the same", firstDocument.ReadToEnd(), secondDocument.ReadToEnd());
		}

		[TestDate(2005, 01, 01, 12, 0, 0)]
		public void TestExportNewBatch()
		{
			AccountingTransactionsDataExporter exporter = NewExporter;
			exporter.FilterProvider.CurrentBatchNo = 0;
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
			exporter.FilterProvider.IncludeAPCreditNotes = true;
			exporter.FilterProvider.IncludeAPInvoices = true;
			exporter.FilterProvider.IncludeARAdjustmentNotes = true;
			exporter.FilterProvider.IncludeARCreditNotes = true;
			exporter.FilterProvider.IncludeARInvoices = true;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;

			var assertHighWaterMark = exporter is ISupportHighWaterMark && ((ISupportHighWaterMark)exporter).IsHighWaterMarkEnabled;

			AssertEquals("Precondition: High water mark registry value is not set", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

			using (TempFile xmlTempFile = TempFile.New())
			{
				using (FileStream exportedXmlFileStream = new FileStream(xmlTempFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					exporter.Export(exportedXmlFileStream);
					if (assertHighWaterMark)
					{
						AssertEquals("High water mark registry value should have been set because a new batch was exported", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
					}
					else
					{
						AssertEquals("High water mark registry value should not have been set because it is not applicable", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
					}
				}

				using (FileStream exportedXmlFileStream = new FileStream(xmlTempFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					Assert("Nothing was writen to the resulting file", exportedXmlFileStream.Length > 0);
				}

				Invoice.Reload();
				InvoiceLineWIP.Reload();
				WIPsAccrual.Reload();
				WIP.Reload();
				AssertNotNull("Batch number was created for export", Invoice.ExportedBatchSequence);
				AssertNotNull("Batch number was created for export", InvoiceLineWIP.ExportBatchSequencePostedObject);
				AssertNotNull("Batch number was created for export", WIP.ExportBatchSequencePostedObject);
				Assert("Sequence Number was not set", InvoiceLineWIP.ExportBatchSequencePostedObject.XB_Sequence > 0);
				Assert("Sequence Number was not set", WIPsAccrual.ExportBatchSequencePostedObject.XB_Sequence > 0);
				Assert("Sequence Number was not set", WIP.ExportBatchSequencePostedObject.XB_Sequence > 0);
			}
		}

		public void TestKnownExportersThatHaveHighWaterMarkEnabled()
		{
			//Add to this list if an exporter implements ISupportHighWaterMark and IsHighWaterMarkEnabled returns true
			var knownExporters = new[]
			{
				"Enterprise.Accounting.DataTransfer.XmlAccountingTransactionExporter",
				"Enterprise.ServiceManager.Tasks.XMLAutomation.AccountingTransactionExporter"
			};

			var exporter = GetNewBusinessObject() as ISupportHighWaterMark;
			var shouldBeHighWaterMarkEnabled = knownExporters.Contains(GetExpectedBusinessObjectType().FullName);

			AssertEquals(string.Format("Exporter {0} should{1} be high water mark enabled. If you have enabled high water mark support on your exporter, add it to the list of known exporters.",
				GetExpectedBusinessObjectType().FullName, shouldBeHighWaterMarkEnabled ? "" : " not"),
				shouldBeHighWaterMarkEnabled, exporter != null && exporter.IsHighWaterMarkEnabled);
		}

		public void TestExportingBatchEndUseCompanyPK()
		{
			AccountingTransactionsDataExporter exporter = NewExporter;
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
			exporter.FilterProvider.IncludeAPCreditNotes = true;
			exporter.FilterProvider.IncludeAPInvoices = true;
			exporter.FilterProvider.IncludeARAdjustmentNotes = true;
			exporter.FilterProvider.IncludeARCreditNotes = true;
			exporter.FilterProvider.IncludeARInvoices = true;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;

			using (TempFile firstBatchTempFile = TempFile.New())
			{
				using (FileStream exportedXmlFileStream = new FileStream(firstBatchTempFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					exporter.Export(exportedXmlFileStream);
					AssertEquals(2, exporter.NumberOfInvoicesInBatch);
					var lastQuery = SqlEventTracker.Instance.LastSqlQuery;
					AssertContains("The query should have AH_GC", "AH_GC = ", lastQuery);

					AssertEquals(2, exporter.NumberOfWipPostingsInBatch);
					lastQuery = SqlEventTracker.Instance.LastSqlQuery;
					AssertContains("The query should have AL_GC", "AL_GC = ", lastQuery);
				}
			}
		}

		[RequireZeroLocalCostAmountInSetup]
		public void TestSetSequenceNumbersOnWIPsAndAccruals()
		{
			AccountingTransactionsDataExporter exporter = NewExporter;
			exporter.FilterProvider.CurrentBatchNo = 0;
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			exporter.FilterProvider.IncludeAPAdjustmentNotes = false;
			exporter.FilterProvider.IncludeAPCreditNotes = false;
			exporter.FilterProvider.IncludeAPInvoices = false;
			exporter.FilterProvider.IncludeARAdjustmentNotes = false;
			exporter.FilterProvider.IncludeARCreditNotes = false;
			exporter.FilterProvider.IncludeARInvoices = false;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;

			WIP wip1 = Factory.NewWithValidTestData<WIP>();
			setupWIPAccrual(wip1, ZDateTime.Now);
			WIP wip2 = Factory.NewWithValidTestData<WIP>();
			setupWIPAccrual(wip2, ZDateTime.Empty);
			WIP wip3 = Factory.NewWithValidTestData<WIP>();
			setupWIPAccrual(wip3, ZDateTime.Empty);

			Accrual accrual1 = Factory.NewWithValidTestData<Accrual>();
			setupWIPAccrual(accrual1, ZDateTime.Now);
			Accrual accrual2 = Factory.NewWithValidTestData<Accrual>();
			setupWIPAccrual(accrual2, ZDateTime.Empty);
			Accrual accrual3 = Factory.NewWithValidTestData<Accrual>();
			setupWIPAccrual(accrual3, ZDateTime.Empty);

			Factory.Save();

			using (TempFile xmlTempFile = TempFile.New())
			{
				using (FileStream exportedXmlFileStream = new FileStream(xmlTempFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					exporter.Export(exportedXmlFileStream);
				}

				using (FileStream exportedXmlFileStream = new FileStream(xmlTempFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					Assert("Nothing was written to the resulting file", exportedXmlFileStream.Length > 0);
				}

				WIP.Reload();
				InvoiceLineWIP.Reload();
				WIPsAccrual.Reload();
				wip1.Reload();
				wip2.Reload();
				wip3.Reload();
				accrual1.Reload();
				accrual2.Reload();
				accrual3.Reload();

				Assert("Batch number was not set after export", wip1.ExportBatchSequencePostedObject.XB_BatchNumber > 0);
				Assert("Batch number was not set after export", wip2.ExportBatchSequencePostedObject.XB_BatchNumber > 0);
				Assert("Batch number was not set after export", wip3.ExportBatchSequencePostedObject.XB_BatchNumber > 0);
				Assert("Reverse Batch number was not set after export", wip1.ExportBatchSequenceReversedObject.XB_BatchNumber > 0);

				Assert("Batch number was not set after export", accrual1.ExportBatchSequencePostedObject.XB_BatchNumber > 0);
				Assert("Batch number was not set after export", accrual2.ExportBatchSequencePostedObject.XB_BatchNumber > 0);
				Assert("Batch number was not set after export", accrual3.ExportBatchSequencePostedObject.XB_BatchNumber > 0);
				Assert("Reverse Batch number was not set after export", accrual1.ExportBatchSequenceReversedObject.XB_BatchNumber > 0);

				Assert("Sequence Number wasn't set", wip1.ExportBatchSequencePostedObject.XB_Sequence > 0);
				Assert("Sequence Number wasn't set", wip1.ExportBatchSequenceReversedObject.XB_Sequence > 0);
				Assert("Sequence Number wasn't set", wip2.ExportBatchSequencePostedObject.XB_Sequence > 0);
				Assert("Sequence Number wasn't set", wip3.ExportBatchSequencePostedObject.XB_Sequence > 0);

				Assert("Sequence Number wasn't set", accrual1.ExportBatchSequencePostedObject.XB_Sequence > 0);
				Assert("Sequence Number wasn't set", accrual1.ExportBatchSequenceReversedObject.XB_Sequence > 0);
				Assert("Sequence Number wasn't set", accrual2.ExportBatchSequencePostedObject.XB_Sequence > 0);
				Assert("Sequence Number wasn't set", accrual3.ExportBatchSequencePostedObject.XB_Sequence > 0);

				int sequenceCheckSum = wip1.ExportBatchSequencePostedObject.XB_Sequence + wip1.ExportBatchSequenceReversedObject.XB_Sequence +
					wip2.ExportBatchSequencePostedObject.XB_Sequence +
					wip3.ExportBatchSequencePostedObject.XB_Sequence +
					accrual1.ExportBatchSequencePostedObject.XB_Sequence + accrual1.ExportBatchSequenceReversedObject.XB_Sequence +
					accrual2.ExportBatchSequencePostedObject.XB_Sequence +
					accrual3.ExportBatchSequencePostedObject.XB_Sequence +
					WIP.ExportBatchSequencePostedObject.XB_Sequence +
					InvoiceLineWIP.ExportBatchSequencePostedObject.XB_Sequence +
					WIPsAccrual.ExportBatchSequencePostedObject.XB_Sequence;

				AssertEquals(66, sequenceCheckSum);
			}
		}

		void setupWIPAccrual(BaseWIPAccrual wipAccrual, ZDateTime reverseDate)
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			JobHeader job = charge.Job;

			wipAccrual.AL_AC = chargeCode.PK;
			wipAccrual.AL_JH = job.PK;
			wipAccrual.AL_PostDate = ZDateTime.Now;
			wipAccrual.AL_ReverseDate = reverseDate;
			wipAccrual.AL_GB = GlbBranch.CurrentBranch.PK;
			wipAccrual.AL_GE = GlbDepartment.CurrentDepartment.PK;
			if (reverseDate.IsEmpty)
			{
				if (wipAccrual.AL_LineType == TransactionLineTypes.WIP)
				{
					charge.JR_AL_ARLine = wipAccrual.PK;
					charge.SetChargeValuesFromLinkedARLineForTests();
				}
				if (wipAccrual.AL_LineType == TransactionLineTypes.Accrual)
				{
					charge.JR_AL_APLine = wipAccrual.PK;
					charge.SetChargeValuesFromLinkedAPLineForTests();
				}
			}
		}

		#region Implementation

		protected virtual ZString ExpectedMessageWhenErrorOccurs()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("BATCH ERROR: There were problems exporting Batch 1.");
			result.Append("BATCH ERROR: Please do not use the files exported and contact support." + System.Environment.NewLine);
			result.Append("ERROR: There were problems exporting the following transactions: Invoices");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Credit Notes");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Adjustment Notes");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: WIP Posting");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: WIP Reversing");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Accrual Posting");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Accrual Reversing");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("ERROR: There were problems exporting the following transactions: Unallocated Transactions");
			result.Append(" - Transactions in Batch: 0");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			return result.ToStringWithNewLineBetweenAppends();
		}

		protected virtual ZString ExpectedMessageWhenThereIsNoError()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("Batch 1 was exported successfully.");
			result.Append("");
			result.Append("The following transactions were exported successfully: Invoices");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Credit Notes");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Adjustment Notes");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: WIP Posting");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: WIP Reversing");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Accrual Posting");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Accrual Reversing");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Unallocated Transactions");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			return result.ToStringWithNewLineBetweenAppends();
		}

		void AddTransactionsToDataBase()
		{
			Invoice = Factory.New<APInvoice>();
			Invoice.AH_OH = Header.PK;

			Invoice.AH_TransactionNum = "000010004";
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_ConsolidatedInvoiceRef = Shipment.JS_UniqueConsignRef;

			InvoicingLineBase line1 = (InvoicingLineBase)Invoice.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)Invoice.Lines.AddNew();

			var chargeCode = ObjectCreator.CC2;
			line1.AL_AC = chargeCode.PK;

			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = 35.00M;
			line1.AL_OSTaxAmount = 3.50M;
			line1.AL_AT = ObjectCreator.GST1.PK;
			line2.AL_OSWHTAmount = 5.00M;
			line2.AL_AW = Factory.NewWithValidTestData(typeof(AccWithholding)).PK;

			line2.AL_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			line2.AL_OSExTaxAmount = 100.00M;
			line2.AL_OSTaxAmount = 10.00M;

			var charge = ObjectCreator.CreateJobCharge(line1, Job, chargeCode, ObjectCreator.AUD);
			charge.JR_OSSellAmt = charge.JR_OSCostAmt;
			WIP.RelatedJobCharge.JR_OSCostAmt = WIP.RelatedJobCharge.JR_OSSellAmt;

			Factory.Save();
			InvoiceLineWIP = line1.RelatedJobCharge.WIP;
			AssertNotNull(InvoiceLineWIP);
			WIPsAccrual = WIP.RelatedJobCharge.Accrual;
			AssertNotNull(WIPsAccrual);
		}

		protected virtual InvoicingBase PopulateTransactionHeaderAndSetBatchNumber(Type typeToCreate, int batchNumber)
		{
			InvoicingBase invoiceForExport = PopulateInvoice(typeToCreate, 100.00M, 10.00M, 0.50M, ObjectCreator.USD);
			ObjectCreator.CreateGenExportBatchSequenceHeader(batchNumber, invoiceForExport.PK, 1);
			return invoiceForExport;
		}

		protected virtual InvoicingBase PopulateInvoice(Type type, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, RefCurrency currency)
		{
			Invoice = (InvoicingBase)Factory.NewWithValidTestData(type);

			Invoice.AH_OH = Header.PK;
			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Invoice.AH_Desc = "This is a test description to see how the XML Export works";
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Invoice.AH_Ledger = type.Name.Substring(0, 2) == "AP" ? "AP" : "AR";
			Invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			Invoice.AH_ExchangeRate = exchangeRate;
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_InvoiceTerm = "INV";
			Invoice.AH_InvoiceTermDays = 1;
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_TransactionReference = @"Shipment ABC123";

			InvoicingLineBase line1 = (InvoicingLineBase)Invoice.Lines.AddNew();
			PopulateInvoiceLine(line1, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), exchangeRate, Invoice.PK);

			InvoicingLineBase line2 = (InvoicingLineBase)Invoice.Lines.AddNew();
			PopulateInvoiceLine(line2, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), exchangeRate, Invoice.PK);
			//if (ExchangeRate == 1)
			//{
			//    Invoice.AH_InvoiceAmount = Invoice.AH_OSTotal - Invoice.AH_GSTAmount;
			//    Invoice.AH_OutstandingAmount = Invoice.AH_OSTotal;
			//}
			Factory.Save();

			return Invoice;
		}

		void PopulateInvoiceLine(InvoicingLineBase line, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, ZGuid header)
		{
			line.AL_AC = ObjectCreator.CC2.PK;
			line.AL_AH = header;
			line.AL_AT = ObjectCreator.GST1.PK;
			line.AL_Desc = "Transaction Line Description";
			line.AL_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.AL_OSTaxAmount = oSTaxAmount;
			line.AL_PostDate = PostDate;
			line.AL_Sequence = 1;
			line.AL_JH = ObjectCreator.Job2.PK;
			line.AL_OSTaxAmount = oSTaxAmount;
			if (line.TransactionHeader.AH_Ledger == "AR")
			{
				line.AL_GSTVAT = GlbCompany.CurrentCompany.GetExchangeRate().ForeignToLocal(oSTaxAmount, exchangeRate);
			}

			var charge = ObjectCreator.CreateJobCharge(line, ObjectCreator.Job2, ObjectCreator.CC2, ObjectCreator.USD);

			if (RequireZeroLocalCostAmountInSetupAttribute.IsActive)
			{
				charge.JR_LocalCostAmt = 0m;
			}
		}

		ZDateTime PostDate
		{
			get { return new ZDateTime(2005, 01, 01, 10, 30, 0); }
		}

		protected virtual BaseWIPAccrual FillWIPAccrualBizObjWithTestData(Type type, AccChargeCode chargeCode, decimal localExTaxAmount)
		{
			WIP = (BaseWIPAccrual)Factory.NewWithValidTestData(type);
			WIP.AL_AC = chargeCode.PK;
			WIP.AL_LocalExTaxAmount = localExTaxAmount;
			WIP.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			WIP.AL_PostDate = PostDate;
			WIP.AL_AC = chargeCode.PK;
			WIP.AL_Desc = "Description of Accrual or WIP";
			WIP.AL_JH = Job1.PK;

			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			if (WIP.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = WIP.PK;
				charge.SetChargeValuesFromLinkedARLineForTests();
			}
			if (WIP.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = WIP.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
			}

			return WIP;
		}

		protected virtual TimeSpan ExpectedHighWaterMarkBuffer
		{
			get { return new TimeSpan(48, 0, 0); }
		}

		protected Job Job1;
		protected BaseWIPAccrual WIP;
		protected Accrual WIPsAccrual;
		protected ForwardingConsol Consol;
		protected ForwardingShipment Shipment;
		protected ForwardingConsol Consol1;
		protected ForwardingShipment Shipment1;
		protected Job Job;
		protected InvoicingBase Invoice;
		protected WIP InvoiceLineWIP;
		protected TestObjectCreator ObjectCreator;
		protected OrgHeader Header;

		protected InvoicingBase InvoiceForExport;
		protected InvoicingBase CreditNoteForExport;
		protected InvoicingBase AdjustmentNoteForExport;
		protected BaseWIPAccrual WipForExport;
		protected BaseWIPAccrual AccrualForExport;
		protected TransactionPendingAllocation TransactionPendingAllocationForExport;

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			Header = Factory.NewWithValidTestData<OrgHeader>();
			Header.OH_FullName = @"The Fullname of the Organisation";

			ObjectCreator = new TestObjectCreator(Factory);

			Consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol.JK_MasterBillNum = "ABCDEFGH";

			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_INCO = "FOB";
			Shipment.JS_UniqueConsignRef = "S12341234";
			Shipment.JS_HouseBill = "UVWXYZ";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			Job = Factory.NewJobForTesting<Job>();
			Job.JH_ParentTableCode = "JS";
			Job.JH_ParentID = Shipment.PK;
			Job.JH_GB = GlbBranch.CurrentBranch.PK;
			Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job.JH_JobNum = Shipment.JS_UniqueConsignRef;

			PopulateInvoice(typeof(ARInvoice), 100.00M, 10.00M, 0.50M, ObjectCreator.USD);

			Consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol1.JK_MasterBillNum = "HAHAHAHHA";

			Shipment1 = Consol1.Shipments.AddNew();
			Shipment1.JS_INCO = "FOB";
			Shipment1.JS_UniqueConsignRef = "S00001234";
			Shipment1.JS_HouseBill = "UVWwwXYZ";
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_RL_NKDestination = "USLAX";

			Job1 = Factory.NewJobForTesting<Job>();
			Job1.JH_ParentTableCode = "JS";
			Job1.JH_ParentID = Shipment1.PK;
			Job1.JH_GB = GlbBranch.CurrentBranch.PK;
			Job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job1.JH_JobNum = Shipment1.JS_UniqueConsignRef;

			Factory.Save();

			FillWIPAccrualBizObjWithTestData(typeof(WIP), ObjectCreator.CC4, 100.0M);

			AddTransactionsToDataBase();

			Factory.Save();
		}

		#endregion

		internal sealed class RequireZeroLocalCostAmountInSetupAttribute : TestSetupAttribute
		{
			public override void SetUp(TestCase testCase)
			{
				IsActive = true;
			}

			public override void TearDown(TestCase testCase)
			{
				IsActive = false;
			}

			[ThreadStatic]
			public static bool IsActive;
		}
	}

	#endregion
}
