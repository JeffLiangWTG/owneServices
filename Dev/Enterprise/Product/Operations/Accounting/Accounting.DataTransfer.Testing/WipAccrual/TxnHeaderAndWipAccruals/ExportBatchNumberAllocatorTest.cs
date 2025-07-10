using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class ExportBatchNumberAllocatorTest : TestCaseWithFactory
	{
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeARInvoices()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);

			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("tst 001", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateARInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Test line", 100m);
			TestObjectCreator.CreateJobCharge(invoice.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_FullyPaidDate = invoice.AH_OutstandingAmount == 0 ? ZDateTime.Now : ZDateTime.Empty;
			invoice.Validation.ValidateAll();
			AssertNoErrors(invoice);
			Factory.Save();

			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeARInvoices = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeUnallocatedAPInvoices()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);

			var invoice = TestObjectCreator.CreateTransactionPendingAllocation("tst 001", TestObjectCreator.AALSHI, 100m);
			invoice.Validation.ValidateAll();
			AssertNoErrors(invoice);
			Factory.Save();

			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeUnallocatedAPInvoices = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeUnallocatedAPCreditNotes()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);

			var creditNote = TestObjectCreator.CreateTransactionPendingAllocation("tst 001", TestObjectCreator.AALSHI, -100m);
			creditNote.Validation.ValidateAll();
			AssertNoErrors(creditNote);
			Factory.Save();

			AccountingNumberFountainNonVoucher numberFountainWrapper = AccountingNumberFountainWrapperFactory.Instance.TransactionExportBatchNo;
			ZInt newBatchNumber = ZInt.Parse(numberFountainWrapper.PeekPreliminary(Factory));

			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeUnallocatedAPCreditNotes = true;

			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", newBatchNumber, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeUnallocatedAPInvoicesAndCreditNotes()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);

			var invoice = TestObjectCreator.CreateTransactionPendingAllocation("tst 001", TestObjectCreator.AALSHI, 100m);
			invoice.Validation.ValidateAll();
			AssertNoErrors(invoice);

			var creditNote = TestObjectCreator.CreateTransactionPendingAllocation("tst 001", TestObjectCreator.AALSHI, -100m);
			creditNote.Validation.ValidateAll();
			AssertNoErrors(creditNote);
			Factory.Save();

			AccountingNumberFountainNonVoucher numberFountainWrapper = AccountingNumberFountainWrapperFactory.Instance.TransactionExportBatchNo;
			ZInt newBatchNumber = ZInt.Parse(numberFountainWrapper.PeekPreliminary(Factory));

			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeUnallocatedAPInvoices = true;
			exporter.FilterProvider.IncludeUnallocatedAPCreditNotes = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", newBatchNumber, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeWIPsPosting()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);
			WIP wip = TestObjectCreator.CreateWIP();
			wip.AL_AC = TestObjectCreator.CC1.PK;
			wip.AL_OSExTaxAmount = 100m;
			wip.RelatedJobCharge.JR_LocalSellAmt = 100m;
			wip.RelatedJobCharge.JR_OSSellAmt = 100m;
			wip.Validation.ValidateAll();
			AssertNoErrors(wip);
			Factory.Save();
			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeWIPsPosting = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeWIPsReversing()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);
			WIP wip = TestObjectCreator.CreateWIP();
			wip.AL_AC = TestObjectCreator.CC1.PK;
			wip.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
			wip.AL_OSExTaxAmount = 100m;
			wip.Validation.ValidateAll();
			AssertNoErrors(wip);
			Factory.Save();
			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeWIPsReversing = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeAccrualsPosting()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);
			Job job = TestObjectCreator.CreateJobHeader();
			Accrual accrual = TestObjectCreator.CreateAccrual(job);
			accrual.AL_AC = TestObjectCreator.CC1.PK;
			accrual.RelatedJobCharge.JR_LocalCostAmt = 100m;
			accrual.RelatedJobCharge.JR_OSCostAmt = 100m;
			accrual.AL_OSExTaxAmount = 100m;
			accrual.Validation.ValidateAll();
			AssertNoErrors(accrual);
			Factory.Save();
			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeAccrualsReversing()
		{
			TestObjectCreator.CreateTestPeriods(ZDate.Today);
			Job job = TestObjectCreator.CreateJobHeader();
			Accrual accrual = TestObjectCreator.CreateAccrual(job);
			accrual.AL_AC = TestObjectCreator.CC1.PK;
			accrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
			accrual.AL_OSExTaxAmount = 100m;
			accrual.Validation.ValidateAll();
			AssertNoErrors(accrual);
			Factory.Save();
			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberGetWorksInItransactionParticipant_IncludeWIPAccrualsPostingAndReversing()
		{
			var job = TestObjectCreator.CreateJobHeader();

			TestObjectCreator.CreateTestPeriods(ZDate.Today);
			WIP wip = TestObjectCreator.CreateWIP();
			wip.AL_AC = TestObjectCreator.CC1.PK;
			wip.AL_OSExTaxAmount = 100m;
			wip.RelatedJobCharge.JR_LocalSellAmt = 100m;
			wip.RelatedJobCharge.JR_OSSellAmt = 100m;
			wip.Validation.ValidateAll();
			AssertNoErrors(wip);

			wip = TestObjectCreator.CreateWIP();
			wip.AL_AC = TestObjectCreator.CC1.PK;
			wip.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
			wip.AL_OSExTaxAmount = 100m;
			wip.Validation.ValidateAll();
			AssertNoErrors(wip);

			Accrual accrual = TestObjectCreator.CreateAccrual(job);
			accrual.AL_AC = TestObjectCreator.CC1.PK;
			accrual.RelatedJobCharge.JR_LocalCostAmt = 100m;
			accrual.RelatedJobCharge.JR_OSCostAmt = 100m;
			accrual.AL_OSExTaxAmount = 100m;
			accrual.Validation.ValidateAll();
			AssertNoErrors(accrual);

			accrual = TestObjectCreator.CreateAccrual(job);
			accrual.AL_AC = TestObjectCreator.CC1.PK;
			accrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
			accrual.AL_OSExTaxAmount = 100m;
			accrual.Validation.ValidateAll();
			AssertNoErrors(accrual);
			Factory.Save();

			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			AssertEquals("Should get a valid batch number back from allocator", 1, allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		public void TestBatchNumberWithFailedAllocation()
		{
			var header = TestObjectCreator.InsertTransaction("IPA");
			header.AH_TransactionNum = "TEST";
			header.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			header.Validation.ValidateAll();
			AssertNoErrors(header);
			Factory.Save();
			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(Factory);
			exporter.FilterProvider.IncludeUnallocatedAPInvoices = true;
			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			allocator.isDeleteTestInvoice = true;
			AssertExceptionThrown<Exception>(() => allocator.GetBatchNumberAndAllocateToTransactionsToBeExported());
		}

		void updateAccTransactionLinePK(ZGuid oldPk, ZGuid newPk)
		{
			string sql = @"
							SELECT " + JobChargeSchema.Constants.PK + ", " + JobChargeSchema.Constants.JR_AL_APLine + ", " + JobChargeSchema.Constants.JR_AL_ARLine + @"
							INTO #temp
							FROM " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + @"
							WHERE " + JobChargeSchema.Constants.JR_AL_APLine + " = '" + oldPk.ToString() + @"' OR " + JobChargeSchema.Constants.JR_AL_ARLine + " = '" + oldPk.ToString() + @"'

							UPDATE " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + @"
							SET " +
								JobChargeSchema.Constants.JR_AL_APLine + @" = NULL,
								JR_SystemLastEditTimeUtc = GETUTCDATE(),
								JR_SystemLastEditUser = '~BP'
							WHERE " +
								JobChargeSchema.Constants.JR_AL_APLine + " = '" + oldPk.ToString() + @"'
							
							UPDATE " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + @"
							SET " +
								JobChargeSchema.Constants.JR_AL_ARLine + @" = NULL,
								JR_SystemLastEditTimeUtc = GETUTCDATE(),
								JR_SystemLastEditUser = '~BP'	
							WHERE " +
								JobChargeSchema.Constants.JR_AL_ARLine + " = '" + oldPk.ToString() + @"'

							UPDATE " + AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + @"
							SET " +
								AccTransactionLinesSchema.Constants.PK + " = '" + newPk.ToString() + @"',
								 " + AccTransactionLinesSchema.Constants.AL_SystemLastEditTimeUtc + @" = GETUTCDATE(),
								 " + AccTransactionLinesSchema.Constants.AL_SystemLastEditUser + @" = 'TST'
							WHERE " +
								AccTransactionLinesSchema.Constants.PK + " = '" + oldPk.ToString() + @"'
									
							UPDATE jc
							SET " +
								JobChargeSchema.Constants.JR_AL_APLine + " = '" + newPk.ToString() + @"',
								JR_SystemLastEditTimeUtc = GETUTCDATE(),
								JR_SystemLastEditUser = '~BP'
							FROM #temp t
							INNER JOIN " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + @" jc
								ON t." + JobChargeSchema.Constants.PK + " = jc." + JobChargeSchema.Constants.PK + @"
							WHERE
								t." + JobChargeSchema.Constants.JR_AL_APLine + " = '" + oldPk.ToString() + @"'
							
							UPDATE jc
							SET " +
								JobChargeSchema.Constants.JR_AL_ARLine + " = '" + newPk.ToString() + @"',
								JR_SystemLastEditTimeUtc = GETUTCDATE(),
								JR_SystemLastEditUser = '~BP'
							FROM #temp t
							INNER JOIN " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + @" jc
								ON t." + JobChargeSchema.Constants.PK + " = jc." + JobChargeSchema.Constants.PK + @"
							WHERE
								t." + JobChargeSchema.Constants.JR_AL_ARLine + " = '" + oldPk.ToString() + @"'
							
							DROP TABLE #temp";
			Db.Connection.ExecuteNonQuery(sql); // this is for test only.
		}

		public void TestCheckWipAccrualSequence()
		{
			var job = TestObjectCreator.CreateJobHeader();
			Accrual accrual = TestObjectCreator.CreateAccrual(job, x => x.JR_OSSellAmt = 0m);
			accrual.AL_PostDate = ZDateTime.Today;

			Accrual accrualRev = TestObjectCreator.CreateAccrual(job, x => x.JR_OSSellAmt = 0m);
			accrualRev.AL_PostDate = ZDateTime.Today;

			BaseCharge linkedCharge = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_APLine, accrualRev.PK));

			linkedCharge.ReverseAccrual(ZDateTime.Today);

			WIP wip = TestObjectCreator.CreateWIP();
			WIP wipRev = TestObjectCreator.CreateWIP();
			wip.AL_PostDate = ZDateTime.Today;
			wipRev.AL_PostDate = ZDateTime.Today;
			linkedCharge = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, wipRev.PK));
			linkedCharge.JR_OSSellAmt = 0m;

			linkedCharge.ReverseWIP(ZDateTime.Today);

			Factory.Save();

			ZGuid wipRevPK = new ZGuid("B0823D8B-70B9-4602-8382-06E9D062BA35");
			ZGuid accrualPK = new ZGuid("C668C5C5-3ECE-4341-AEAE-6E85DC9BB2C0");
			ZGuid accrualRevPK = new ZGuid("93FF4397-2114-4478-8886-84E8B868EE2A");
			ZGuid wipPK = new ZGuid("6A0CA46F-ECD1-44B3-8BBF-E72B7D247EEB");
			updateAccTransactionLinePK(wipRev.PK, wipRevPK);
			updateAccTransactionLinePK(accrual.PK, accrualPK);
			updateAccTransactionLinePK(accrualRev.PK, accrualRevPK);
			updateAccTransactionLinePK(wip.PK, wipPK);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			AccountingTransactionsDataExporter exporter = new AccountingTransactionsDataExporterTest.TestTransactionExporter(factory2);
			exporter.FilterProvider.IncludeAccrualsPosting = true;
			exporter.FilterProvider.IncludeAccrualsReversing = true;
			exporter.FilterProvider.IncludeWIPsPosting = true;
			exporter.FilterProvider.IncludeWIPsReversing = true;

			ExportBatchNumberAllocator allocator = new ExportBatchNumberAllocator(exporter);
			int batchNumber = allocator.GetBatchNumberAndAllocateToTransactionsToBeExported();
			AssertEquals("Batch Number", 1, batchNumber);

			accrual = factory2.Load<Accrual>(accrualPK);
			accrualRev = factory2.Load<Accrual>(accrualRevPK);
			wip = factory2.Load<WIP>(wipPK);
			wipRev = factory2.Load<WIP>(wipRevPK);

			AssertEquals("accrual.AL_Sequence", 3, accrual.ExportBatchSequencePostedObject.XB_Sequence);
			AssertEquals("wip.AL_Sequence", 6, wip.ExportBatchSequencePostedObject.XB_Sequence);
			AssertEquals("accrualRev.AL_Sequence", 4, accrualRev.ExportBatchSequencePostedObject.XB_Sequence);
			AssertEquals("wipRev.AL_Sequence", 1, wipRev.ExportBatchSequencePostedObject.XB_Sequence);
			AssertEquals("accrualRev.AL_PercentageOfPeriod", 5, accrualRev.ExportBatchSequenceReversedObject.XB_Sequence);
			AssertEquals("wipRev.AL_PercentageOfPeriod", 2, wipRev.ExportBatchSequenceReversedObject.XB_Sequence);
		}
	}
}
