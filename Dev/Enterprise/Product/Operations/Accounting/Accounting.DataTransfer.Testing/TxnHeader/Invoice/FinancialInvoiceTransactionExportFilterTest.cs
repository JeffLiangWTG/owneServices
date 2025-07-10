using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class FinancialInvoiceTransactionExportFilterTest : TransactionExportFilterTestBase
	{
		public void TestTransactionFilterPicksUpCorrectJobRelatedTransactions()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.Jobs.Add(ObjectCreator.Job1);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection contains AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));

			Assert("Invoice Collection does not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices, ARInvoiceFromAnotherCompany));

			AssertEquals("Invoice Collection Count", 1, invoices.Count);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			invoices.Load(Filter.Filter);

			Assert("Invoice Collection contains AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection does not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices, ARInvoiceFromAnotherCompany));

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
		}

		public void TestTransactionFilterPicksUpNoTransactionsWhenExcludingEverything()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			AssertEquals("Invoices Count", 0, Filter.NumberOfObjects);
		}

		public void TestTransactionFilterPicksUpNoTransactionsWhenNoTransactionsAreSelected()
		{
			IncludeAllTransactionTypes(false);
			AssertEquals("Invoices Count", 0, Filter.NumberOfObjects);
		}

		public void TestTransactionFilterForJobRelatedTransactionsForAR()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);

			Assert("Invoice Collection contains AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should not contain AR Credit Note 1", !BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should not contain AP Invoice 1", !BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection should not contain AP Credit Note 1", !BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));
		}

		public void TestTransactionFilterForNonJobRelatedTransactionsForAR()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should not contain AR Invoice 1", !BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should contain AR Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should not contain AP Invoice 1", !BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection should not contain AP Credit Note 1", !BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));

			AssertEquals("Invoice Collection Count", 2, invoices.Count);
		}

		public void TestTransactionFilterForJobRelatedTransactionsForAP()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should not contain AR Invoice 1", !BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should not contain AR Credit Note 1", !BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should contain AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection should not contain AP Credit Note 1", !BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
		}

		public void TestTransactionFilterForNonJobRelatedTransactionsForAP()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = false;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should not contain AR Invoice 1", !BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should not contain AR Credit Note 1", !BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should not contain AP Invoice 1", !BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection should contain AP Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));

			AssertEquals("Invoice Collection Count", 2, invoices.Count);
		}

		public void TestTransactionFilterForToAndFromDates()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.DateFrom = ZDateTime.Today.AddDays(-1);
			FilterProvider.DateTo = ZDateTime.Today.AddDays(1);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should contain AR Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should contain AP Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));

			AssertEquals("Invoices in Batch", 2, invoices.Count);
		}

		public void TestTransactionFilterForToAndFromDatesOfInvoicdDate()
		{
			FilterProvider.DateFrom = ZDateTime.Today.AddDays(-1);
			FilterProvider.DateTo = ZDateTime.Today.AddDays(1);
			FilterThatUsesInvoiceDateForToAndFromDates = new FilterUsingInvoiceDateForFromAndToDates(Factory, FilterProvider);
			IncludeAllTransactionTypes(true);

			ARCreditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);
			APCreditNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);
			ARAdjustmentNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);
			APAdjustmentNote1.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);
			ARInvoice1.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);
			APInvoice1.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);

			Factory.Save();

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(FilterThatUsesInvoiceDateForToAndFromDates.Filter);
			AssertEquals("Invoices in Batch", 0, invoices.Count);

			ARInvoice1.AH_InvoiceDate = ZDateTime.Today;
			APInvoice1.AH_InvoiceDate = ZDateTime.Today;
			Factory.Save();
			invoices.Load(FilterThatUsesInvoiceDateForToAndFromDates.Filter);
			AssertEquals("Invoices in Batch", 2, invoices.Count);
			Assert("Invoice Collection should contain AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should contain AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
		}

		public void TestTransactionFilterForToAndFromTransactionNumbers()
		{
			ARInvoice1.IsManuallySetTransactionNumber_ForTestOnly = true;
			ARCreditNote1.IsManuallySetTransactionNumber_ForTestOnly = true;
			ARAdjustmentNote1.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARCreditNote1.AH_TransactionNum = "2000";
			APCreditNote1.AH_TransactionNum = "2000";
			ARAdjustmentNote1.AH_TransactionNum = "ABC";
			APAdjustmentNote1.AH_TransactionNum = "ABC";
			ARInvoice1.AH_TransactionNum = "ABC1";
			APInvoice1.AH_TransactionNum = "ABC1";

			SaveWithModifiedTransactionNumber();

			IncludeAllTransactionTypes(true);
			FilterProvider.TransactionNumberFrom = "2500";
			FilterProvider.TransactionNumberTo = "AB1";
			FilterUsingInvoiceDateForFromAndToDates filterThatUsesTransactionNumbersForToAndFromStrings = new FilterUsingInvoiceDateForFromAndToDates(Factory, FilterProvider);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(filterThatUsesTransactionNumbersForToAndFromStrings.Filter);
			AssertEquals("Invoices in Batch", 0, invoices.Count);

			ARAdjustmentNote1.AH_TransactionNum = "2550";
			APAdjustmentNote1.AH_TransactionNum = "2550";
			ARInvoice1.AH_TransactionNum = "AB";
			APInvoice1.AH_TransactionNum = "AB";

			SaveWithModifiedTransactionNumber();

			invoices.Load(filterThatUsesTransactionNumbersForToAndFromStrings.Filter);
			AssertEquals("Invoices in Batch", 4, invoices.Count);
			Assert("Invoice Collection should contain AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should contain AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices, APInvoice1));

			FilterProvider.TransactionNumberFrom = "";
			FilterProvider.TransactionNumberTo = "A";
			filterThatUsesTransactionNumbersForToAndFromStrings = new FilterUsingInvoiceDateForFromAndToDates(Factory, FilterProvider);
			invoices.Load(filterThatUsesTransactionNumbersForToAndFromStrings.Filter);
			AssertEquals("Invoices in Batch", 4, invoices.Count);

			FilterProvider.TransactionNumberFrom = "AB";
			FilterProvider.TransactionNumberTo = "";
			filterThatUsesTransactionNumbersForToAndFromStrings = new FilterUsingInvoiceDateForFromAndToDates(Factory, FilterProvider);
			invoices.Load(filterThatUsesTransactionNumbersForToAndFromStrings.Filter);
			AssertEquals("Invoices in Batch", 2, invoices.Count);

			void SaveWithModifiedTransactionNumber()
			{
				//Intend to change transaction number for test.
				using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					Factory.Save();
				}
			}
		}

		public void TestTransactionFilterForToAndFromPeriods()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			Factory.Save();

			IncludeAllTransactionTypes(true);

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);
			ZInt currentPeriod = periodCalc.GetPeriodFromDate(ZDateTime.Today);

			FilterProvider.PeriodFrom = periodTestHelper.CurrentPeriodInt;
			FilterProvider.PeriodTo = periodTestHelper.CurrentPeriodInt;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should contain AR Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should contain AP Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));

			AssertEquals("Invoices in Batch", 2, invoices.Count);
		}

		public void TestTransactionFilterForOrganisations()
		{
			IncludeAllTransactionTypes(true);

			ARInvoice1.AH_OH = ObjectCreator.AALSHI.PK;
			ARAdjustmentNote1.AH_OH = ObjectCreator.LocalClient.PK;

			APInvoice1.AH_OH = ObjectCreator.ABIGAS.PK;
			APAdjustmentNote1.AH_OH = ObjectCreator.ZECTRA.PK;
			APAdjustmentNote1.Lines[0].AL_AT = ObjectCreator.GST1.PK;

			ObjectCreator.AALSHI.CompanyData.OB_IsDebtor = true;
			WIP1.RelatedJobCharge.JR_OH_SellAccount = ObjectCreator.AALSHI.PK;
			WIP1.AL_OH = ObjectCreator.AALSHI.PK;

			WIP2Reversed.AL_OH = ZGuid.Empty;

			ObjectCreator.LocalClient.CompanyData.OB_IsCreditor = true;
			Accrual1.RelatedJobCharge.JR_OH_CostAccount = ObjectCreator.LocalClient.PK;
			Accrual1.AL_OH = ObjectCreator.LocalClient.PK;
			Accrual2Reversed.AL_OH = ObjectCreator.ZECTRA.PK;

			ARInvoiceFromAnotherCompany.AH_OH = ObjectCreator.ZECTRA.PK;
			WIPFromAnotherCompany.AL_OH = ObjectCreator.ZECTRA.PK;

			Factory.Save();

			FilterProvider.Organisations.Add(ObjectCreator.AALSHI);
			FilterProvider.Organisations.Add(ObjectCreator.ABIGAS);
			FilterProvider.Organisations.Add(ObjectCreator.ZECTRA);

			AssertEquals("PreCondition: Exporter.Organisations has 3 Organisations", 3, FilterProvider.Organisations.Count);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should contain AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should contain AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection should contain AP Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices, APAdjustmentNote1));

			Assert("Invoice Collection shpuld not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices, ARInvoiceFromAnotherCompany));
			AssertNull("Batch Number should not exist for Invoice from other company", ARInvoiceFromAnotherCompany.ExportedBatchSequence);

			AssertEquals("Invoice Collection Count", 3, invoices.Count);
		}

		public void TestTransactionFilterOnlyPicksUpThisCompanysTransactions()
		{
			AssertEquals("Batch Number", 0, FilterProvider.CurrentBatchNo);

			IncludeAllTransactionTypes(true);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 6, invoices.Count);

			Assert("Invoice Collection contains AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection contains AR Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection contains AR Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices, ARAdjustmentNote1));
			Assert("Invoice Collection contains AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection contains AP Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));
			Assert("Invoice Collection contains AP Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices, APAdjustmentNote1));

			Assert("Invoice Collection does not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices, ARInvoiceFromAnotherCompany));
			AssertNull("Batch Should not be Created for Invoice from other company", ARInvoiceFromAnotherCompany.ExportedBatchSequence);

			ObjectCreator.CreateGenExportBatchSequenceHeader(1, ARInvoiceFromAnotherCompany.PK, 1);
			FakeABatch(invoices);

			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 6, invoices.Count);

			foreach (InvoicingBase inv in invoices)
			{
				AssertEquals("Has batch number of 1", 1, inv.ExportedBatchSequence.XB_BatchNumber);
			}

			Assert("Invoice Collection should contain AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices2, ARInvoice1));
			Assert("Invoice Collection should contain AR Credit Note 1", BusinessObjectIsInCollectionByPK(invoices2, ARCreditNote1));
			Assert("Invoice Collection should contain AR Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices2, ARAdjustmentNote1));
			Assert("Invoice Collection should contain AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices2, APInvoice1));
			Assert("Invoice Collection should contain AP Credit Note 1", BusinessObjectIsInCollectionByPK(invoices2, APCreditNote1));
			Assert("Invoice Collection should contain AP Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices2, APAdjustmentNote1));

			Assert("Invoice Collection should not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices2, ARInvoiceFromAnotherCompany));
		}

		public void TestTransactionTypeFilterIncludeARInvoices()
		{
			FilterProvider.IncludeARInvoices = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
			AssertEquals("Invoice Collection contains Invoice 1", ARInvoice1.PK, invoices[0].PK);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices2.Count);
			AssertEquals("Invoice Collection contains Invoice 1", ARInvoice1.PK, invoices2[0].PK);
			AssertEquals("Has Batch no of 1", ARInvoice1.ExportedBatchSequence.XB_BatchNumber, invoices2[0].ExportedBatchSequence.XB_BatchNumber);
		}

		public void TestTransactionTypeFilterIncludeARCreditNotes()
		{
			FilterProvider.IncludeARCreditNotes = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
			AssertEquals("Invoice Collection contains Invoice 1", ARCreditNote1.PK, invoices[0].PK);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices2.Count);
			AssertEquals("Invoice Collection contains Invoice 1", ARCreditNote1.PK, invoices2[0].PK);
			AssertEquals("Has Batch no of 1", ARCreditNote1.ExportedBatchSequence.XB_BatchNumber, invoices2[0].ExportedBatchSequence.XB_BatchNumber);
		}

		public void TestTransactionTypeFilterIncludeARAdjustmentNotes()
		{
			FilterProvider.IncludeARAdjustmentNotes = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
			AssertEquals("Invoice Collection contains Invoice 1", ARAdjustmentNote1.PK, invoices[0].PK);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices2.Count);
			AssertEquals("Invoice Collection contains Invoice 1", ARAdjustmentNote1.PK, invoices2[0].PK);
			AssertEquals("Has Batch no of 1", ARAdjustmentNote1.ExportedBatchSequence.XB_BatchNumber, invoices2[0].ExportedBatchSequence.XB_BatchNumber);
		}

		[SuspendCriticalValidation]
		public void TestFilterExcludesAPInvoicesWithUTCLines()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			UAInvoiceLine uALine = Factory.NewWithValidTestData<UAInvoiceLine>();
			uALine.AL_AH = invoice.PK;
			Factory.Save();

			FilterProvider.IncludeAPInvoices = true;
			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
		}

		public void TestTransactionTypeFilterIncludeAPInvoices()
		{
			FilterProvider.IncludeAPInvoices = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
			AssertEquals("Invoice Collection contains Invoice 1", APInvoice1.PK, invoices[0].PK);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices2.Count);
			AssertEquals("Invoice Collection contains Invoice 1", APInvoice1.PK, invoices2[0].PK);
			AssertEquals("Has Batch no of 1", APInvoice1.ExportedBatchSequence.XB_BatchNumber, invoices2[0].ExportedBatchSequence.XB_BatchNumber);
		}

		public void TestTransactionTypeFilterIncludeAPCreditNotes()
		{
			FilterProvider.IncludeAPCreditNotes = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
			AssertEquals("Invoice Collection contains Invoice 1", APCreditNote1.PK, invoices[0].PK);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices2.Count);
			AssertEquals("Invoice Collection contains Invoice 1", APCreditNote1.PK, invoices2[0].PK);
			AssertEquals("Has Batch no of 1", APCreditNote1.ExportedBatchSequence.XB_BatchNumber, invoices2[0].ExportedBatchSequence.XB_BatchNumber);
		}

		public void TestTransactionTypeFilterIncludeAPAdjustmentNotes()
		{
			FilterProvider.IncludeAPAdjustmentNotes = true;

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices.Count);
			AssertEquals("Invoice Collection contains Invoice 1", APAdjustmentNote1.PK, invoices[0].PK);

			FakeABatch(invoices);
			FilterProvider.CurrentBatchNo = 1;

			InvoicingBaseCollection invoices2 = new InvoicingBaseCollection(Factory);
			invoices2.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 1, invoices2.Count);
			AssertEquals("Invoice Collection contains Invoice 1", APAdjustmentNote1.PK, invoices2[0].PK);
			AssertEquals("Has Batch no of 1", APAdjustmentNote1.ExportedBatchSequence.XB_BatchNumber, invoices2[0].ExportedBatchSequence.XB_BatchNumber);
		}

		void FakeABatch(BusinessObjectCollection collectionOfBizObjsToFakeBatch)
		{
			foreach (BusinessObject bizObj in collectionOfBizObjsToFakeBatch)
			{
				ObjectCreator.CreateGenExportBatchSequenceHeader(1, ((InvoicingBase)bizObj).PK, 1);
			}

			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestConsistentParameterName()
		{
			TestFlag.Flag = true;
			TestFlag.Count = 0;
			TestFlag.Value = DateTime.UtcNow.Add(new TimeSpan(24, 1, 00));

			try
			{
				IncludeAllTransactionTypes(true);
				ExportFilter.GetFilterPks(true);
			}
			finally
			{
				TestFlag.Flag = false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new FinancialInvoiceTransactionExportFilterForTest(Factory, FilterProvider);
		}

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new FinancialInvoiceTransactionExportFilterForTest(Factory, FilterProvider);
		}

		static class TestFlag
		{
			public static bool Flag;
			public static DateTime Value;
			public static int Count;
		}

		internal class FinancialInvoiceTransactionExportFilterForTest : FinancialInvoiceTransactionExportFilter
		{
			public FinancialInvoiceTransactionExportFilterForTest(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider)
				: base(factory, filterProvider)
			{
			}

			protected override ZQuery CreateFilterForBatch()
			{
				if (TestFlag.Flag && TestFlag.Count++ == 2)
				{
					System.Threading.Thread.Sleep(60000);
				}

				var result = base.CreateFilterForBatch();

				if (TestFlag.Flag)
				{
					result.AddToFilter(AccTransactionHeaderSchema.AH_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, new ZDateTime(TestFlag.Value));
				}

				return result;
			}
		}

		protected override int ExpectedNumberOfHighWaterMarkParams
		{
			get
			{
				return (FilterProvider.AtLeastOneTypeOfARIsSelected ? 1 : 0) + (FilterProvider.AtLeastOneTypeOfAPIsSelected ? 1 : 0);
			}
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc; }
		}

		class FilterUsingInvoiceDateForFromAndToDates : FinancialInvoiceTransactionExportFilter
		{
			public FilterUsingInvoiceDateForFromAndToDates(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider)
				: base(factory, filterProvider)
			{
			}

			protected override SchemaDateTimeColumn FromToDatesColumnToFilterOn
			{
				get { return AccTransactionHeaderSchema.AH_InvoiceDate; }
			}
		}

		FinancialInvoiceTransactionExportFilterForTest Filter;
		FilterUsingInvoiceDateForFromAndToDates FilterThatUsesInvoiceDateForToAndFromDates;
	}

	public class FinancialInvoiceTransactionExportFilterForBranchAndDepartmentTest : TransactionExportFilterTestBase
	{
		public void TestTransactionFilterForBranches()
		{
			IncludeAllTransactionTypes(true);

			GlbBranch newBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch2.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch3 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch3.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch4 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch4.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch5 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch5.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch6 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch6.GB_GC = GlbCompany.CurrentCompany.PK;

			ARInvoice1.AH_GB = newBranch1.PK;
			ARCreditNote1.AH_GB = newBranch2.PK;
			ARAdjustmentNote1.AH_GB = newBranch3.PK;

			APInvoice1.AH_GB = newBranch4.PK;
			APCreditNote1.AH_GB = newBranch5.PK;
			APAdjustmentNote1.AH_GB = newBranch6.PK;

			WIP1.AL_GB = newBranch1.PK;
			WIP2Reversed.AL_GB = newBranch2.PK;
			Accrual1.AL_GB = newBranch3.PK;
			Accrual2Reversed.AL_GB = newBranch4.PK;

			Factory.Save();

			FilterProvider.Branches.Add(newBranch1);
			FilterProvider.Branches.Add(newBranch3);
			FilterProvider.Branches.Add(newBranch5);

			AssertEquals("PreCondition: Exporter.Branches has 3 Branches", 3, FilterProvider.Branches.Count);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			Assert("Invoice Collection should contain AR Invoice 1", BusinessObjectIsInCollectionByPK(invoices, ARInvoice1));
			Assert("Invoice Collection should contain AR Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices, ARAdjustmentNote1));
			Assert("Invoice Collection should contain AP Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, APCreditNote1));

			Assert("Invoice Collection should not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices, ARInvoiceFromAnotherCompany));
			AssertNull("Batch Number should not exist for Invoice from other company", ARInvoiceFromAnotherCompany.ExportedBatchSequence);

			AssertEquals("Invoice Collection Count", 3, invoices.Count);
		}

		public void TestTransactionFilterForDepartments()
		{
			IncludeAllTransactionTypes(true);

			GlbDepartment newDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment3 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment4 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment5 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment6 = Factory.NewWithValidTestData<GlbDepartment>();

			ARInvoice1.AH_GE = newDepartment1.PK;
			ARCreditNote1.AH_GE = newDepartment2.PK;
			ARAdjustmentNote1.AH_GE = newDepartment3.PK;

			APInvoice1.AH_GE = newDepartment4.PK;
			APCreditNote1.AH_GE = newDepartment5.PK;
			APAdjustmentNote1.AH_GE = newDepartment6.PK;

			ARInvoiceFromAnotherCompany.AH_GE = newDepartment1.PK;

			WIP1.AL_GE = newDepartment1.PK;
			WIP2Reversed.AL_GE = newDepartment2.PK;
			Accrual1.AL_GE = newDepartment3.PK;
			Accrual2Reversed.AL_GE = newDepartment4.PK;

			Factory.SuspendValidation();
			Factory.Save();
			Factory.ResumeValidation();

			FilterProvider.Departments.Add(newDepartment2);
			FilterProvider.Departments.Add(newDepartment4);
			FilterProvider.Departments.Add(newDepartment6);
			AssertEquals("PreCondition: Exporter.Departmentes has 3 Departments", 3, FilterProvider.Departments.Count);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.Load(Filter.Filter);

			AssertEquals("Invoice Collection Count", 3, invoices.Count);

			Assert("Invoice Collection should contain AR Credit Note 1", BusinessObjectIsInCollectionByPK(invoices, ARCreditNote1));
			Assert("Invoice Collection should contain AP Invoice 1", BusinessObjectIsInCollectionByPK(invoices, APInvoice1));
			Assert("Invoice Collection should contain AP Adjustment Note 1", BusinessObjectIsInCollectionByPK(invoices, APAdjustmentNote1));

			Assert("Invoice Collection should not contain Invoice from other company", !BusinessObjectIsInCollectionByPK(invoices, ARInvoiceFromAnotherCompany));
			AssertNull("Batch Number should not exist for Invoice from other company", ARInvoiceFromAnotherCompany.ExportedBatchSequence);
		}

		public override void TestGetFilterPks()
			=> Assert($"Tested in {nameof(FinancialInvoiceTransactionExportFilterTest)} but doesn't work when IsForceSetupSave is false", true);

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new FinancialInvoiceTransactionExportFilterTest.FinancialInvoiceTransactionExportFilterForTest(Factory, FilterProvider);
		}

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new FinancialInvoiceTransactionExportFilterTest.FinancialInvoiceTransactionExportFilterForTest(Factory, FilterProvider);
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc; }
		}

		protected override bool IsForceSetupSave => false;

		FinancialInvoiceTransactionExportFilterTest.FinancialInvoiceTransactionExportFilterForTest Filter;
	}
}
