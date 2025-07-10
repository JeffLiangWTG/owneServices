using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public abstract class TransactionExportFilterTestBase : TestCaseWithFactory
	{
		#region TestGetFilterPks

		public virtual void TestGetFilterPks()
		{
			int numberOfObjectsInDb = 0;
			IncludeAllTransactionTypes(false);
			List<ZGuid> pks = ExportFilter.GetFilterPks(true);
			AssertEquals("Number Of Pks Should Be Zero Because No transaction types are selected.", 0, pks.Count);
			pks = ExportFilter.GetFilterPks(false);
			AssertEquals("Number Of Pks Should Be Zero Because No transaction types are selected.", 0, pks.Count);

			IncludeAllTransactionTypes(true);
			numberOfObjectsInDb = Factory.GetDatabaseCount(GetBizoTypeForTableName(), ExportFilter.Filter);
			AssertNotEquals("Precondition: NumberOfObjectsInDb should have a value.", 0, numberOfObjectsInDb);
			pks = ExportFilter.GetFilterPks(true);
			AssertEquals("Number Of Pks Should Be 1 Because transaction types are selected.", 1, pks.Count);
			pks = ExportFilter.GetFilterPks(false);
			AssertEquals("Number Of Pks Should not Be Zero Because transaction types are selected.", numberOfObjectsInDb, pks.Count);
		}

		public void TestAddHighWaterMarkFilterIfApplicable()
		{
			CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection {
					{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
					{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
					{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", true },
					{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", true },
					{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", true },
					{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", true },
					{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", true },
					{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", true },
					{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", true },
					{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", true },
					{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", true },
					{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", true } };
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

			using (SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}

			IncludeAllTransactionTypes(true);

			var filter = GetNewExportFilter();

			if (FilterProvider.Exporter != null && FilterProvider.Exporter.IsHighWaterMarkEnabled)
			{
				AssertHighWaterMarkFilter(filter, ExpectedNumberOfHighWaterMarkParams);

				FilterProvider.CurrentBatchNo = 1;
				filter = GetNewExportFilter();
				AssertHighWaterMarkFilter(filter, 0);
			}
			else
			{
				filter = GetNewExportFilter();
				AssertHighWaterMarkFilter(filter, 0);
			}
		}

		void AssertHighWaterMarkFilter(TransactionExportFilter filter, int expectedNumberOfHighWaterMarkParams)
		{
			var highWaterMark = SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value;
			var truncatedDate = new DateTime(highWaterMark.Year, highWaterMark.Month, highWaterMark.Day, highWaterMark.Hour, highWaterMark.Minute, 0, 0);
			var matches = filter.Filter.Params.Where(p => p.SchemaColumn == filter.SystemLastEditTimeColumn && p.ComparisonOperator == SQLComparisonOperator.GreaterThan && ((DateTime)p.Value) == truncatedDate);
			AssertEquals("Number of high water mark parameters in filter", expectedNumberOfHighWaterMarkParams, matches.Count());
		}

		public void TestSystemLastEditTimeColumn()
		{
			AssertEquals("SystemLastEditTimeColumn", ExpectedSystemLastEditTimeColumn, GetNewExportFilter().SystemLastEditTimeColumn);
		}

		protected virtual int ExpectedNumberOfHighWaterMarkParams
		{
			get { return 0; }
		}

		protected abstract SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn { get; }

		protected virtual Type GetBizoTypeForTableName()
		{
			return typeof(APInvoice);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);
			ObjectCreator.USD.ExchangeRates.DeleteAll();
			ExchangeRateReader.GetReaderInstance().ClearCache();
			Factory.Save();

			var exRate = ObjectCreator.USD.ExchangeRates.AddNew();
			exRate.RE_StartDate = ZDateTime.Now.AddMonths(-1);
			exRate.RE_ExpiryDate = ZDateTime.Now.AddMonths(1);
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			exRate.RE_SellRate = 1.0m;
			Factory.Save();

			FilterProvider = new TransactionExportFilterProvider(Factory);
			AddTransactionsToDataBase();
		}

		#endregion

		#region Helpers

		protected bool BusinessObjectIsInCollectionByPK(BusinessObjectCollection bizObjCollection, BusinessObject bizObj)
		{
			bool result = false;

			foreach (BusinessObject bizo in bizObjCollection)
			{
				if (bizo.PK == bizObj.PK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected void AddInvoiceLineWithTestValues(InvoicingBase invoiceHeader, ZGuid jobPK)
		{
			invoiceHeader.AH_JH = jobPK;

			InvoicingLineBase line = (InvoicingLineBase)invoiceHeader.Lines.AddNew();

			line.AL_AH = invoiceHeader.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AT = ObjectCreator.GST1.PK;
			line.AL_Desc = "Transaction Line Description";
			line.AL_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			line.AL_OSExTaxAmount = 100.0M;
			line.AL_OSTaxAmount = 10.0M;
			line.AL_PostDate = ZDateTime.Now;
			line.AL_Sequence = 1;
			line.AL_AG = ObjectCreator.GLHeader1.PK;

			if (!jobPK.IsValid)
			{
				line.AL_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			}
			else
			{
				line.AL_AC = ObjectCreator.CC1.PK;
				line.AL_JH = jobPK;

				JobCharge charge = invoiceHeader.Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = jobPK;
				if (line.AL_LineType == TransactionLineTypes.Revenue)
				{
					charge.JR_AL_ARLine = line.PK;
					line.AL_LocalExTaxAmount = charge.JR_OSSellAmt;
				}
				else
				{
					charge.JR_AL_APLine = line.PK;
					line.AL_LocalExTaxAmount = charge.JR_OSCostAmt;
				}
			}
		}

		protected void IncludeAllTransactionTypes(bool value)
		{
			FilterProvider.IncludeARInvoices = value;
			FilterProvider.IncludeARCreditNotes = value;
			FilterProvider.IncludeARAdjustmentNotes = value;

			FilterProvider.IncludeAPInvoices = value;
			FilterProvider.IncludeAPCreditNotes = value;
			FilterProvider.IncludeAPAdjustmentNotes = value;

			FilterProvider.IncludeWIPsPosting = value;
			FilterProvider.IncludeWIPsReversing = value;

			FilterProvider.IncludeAccrualsPosting = value;
			FilterProvider.IncludeAccrualsReversing = value;

			FilterProvider.IncludeUnallocatedAPInvoices = value;
			FilterProvider.IncludeUnallocatedAPCreditNotes = value;
		}

		protected virtual void AddTransactionsToDataBase()
		{
			ARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice1.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum;
			AddInvoiceLineWithTestValues(ARInvoice1, ObjectCreator.Job1.PK);

			ARCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			AddInvoiceLineWithTestValues(ARCreditNote1, ZGuid.Empty);

			ARAdjustmentNote1 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			AddInvoiceLineWithTestValues(ARAdjustmentNote1, ZGuid.Empty);

			APInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice1.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "/A";
			AddInvoiceLineWithTestValues(APInvoice1, ObjectCreator.Job2.PK);

			APCreditNote1 = Factory.NewWithValidTestData<APCreditNote>();
			AddInvoiceLineWithTestValues(APCreditNote1, ZGuid.Empty);

			APAdjustmentNote1 = Factory.NewWithValidTestData<APAdjustmentNote>();
			AddInvoiceLineWithTestValues(APAdjustmentNote1, ZGuid.Empty);

			ARInvoiceFromAnotherCompany = Factory.NewWithValidTestData<ARInvoice>();
			ZQuery anotherCompanyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(anotherCompanyFilter);
			ARInvoiceFromAnotherCompany.AH_GB = anotherCompany.Branches[0].PK;
			AddInvoiceLineWithTestValues(ARInvoiceFromAnotherCompany, ObjectCreator.Job1.PK);
			ARInvoiceFromAnotherCompany.Lines[0].AL_GB = anotherCompany.Branches[0].PK;
			ARInvoiceFromAnotherCompany.AH_ConsolidatedInvoiceRef = ObjectCreator.Job1.JH_JobNum + "/B";

			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			ObjectCreator.JobHeader1.JH_JobNum = "007";
			charge.JR_JH = ObjectCreator.JobHeader1.PK;
			WIP1 = Factory.NewWithValidTestData<WIP>();
			WIP1.AL_GB = GlbBranch.CurrentBranch.PK;
			WIP1.AL_JH = ObjectCreator.JobHeader1.PK;
			WIP1.AL_OH = Guid.Empty;
			WIP1.AL_AC = charge.JR_AC;
			WIP1.AL_AG = ObjectCreator.GLHeader1.PK;
			charge.JR_AL_ARLine = WIP1.PK;
			charge.SetChargeValuesFromLinkedARLineForTests();

			WIP2Reversed = Factory.NewWithValidTestData<WIP>();
			WIP2Reversed.AL_GB = GlbBranch.CurrentBranch.PK;
			WIP2Reversed.AL_ReverseDate = ZDateTime.Now;
			WIP2Reversed.AL_JH = ObjectCreator.Job2.PK;
			WIP2Reversed.AL_OH = ZGuid.Empty;
			WIP2Reversed.AL_AG = ObjectCreator.GLHeader1.PK;

			Accrual1 = Factory.NewWithValidTestData<Accrual>();
			Accrual1.AL_GB = GlbBranch.CurrentBranch.PK;
			Accrual1.AL_JH = ObjectCreator.JobHeader1.PK;
			Accrual1.AL_OH = ZGuid.Empty;
			Accrual1.AL_AC = charge.JR_AC;
			Accrual1.AL_AG = ObjectCreator.GLHeader1.PK;
			charge.JR_AL_APLine = Accrual1.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();

			Accrual2Reversed = Factory.NewWithValidTestData<Accrual>();
			Accrual2Reversed.AL_GB = GlbBranch.CurrentBranch.PK;
			Accrual2Reversed.AL_ReverseDate = ZDateTime.Now;
			Accrual2Reversed.AL_JH = ObjectCreator.Job2.PK;
			Accrual2Reversed.AL_OH = ZGuid.Empty;
			Accrual2Reversed.AL_AG = ObjectCreator.GLHeader1.PK;

			WIPFromAnotherCompany = Factory.NewWithValidTestData<WIP>();
			WIPFromAnotherCompany.AL_GB = anotherCompany.Branches[0].PK;
			WIPFromAnotherCompany.AL_ReverseDate = ZDateTime.Now;
			WIPFromAnotherCompany.AL_OH = ZGuid.Empty;
			WIPFromAnotherCompany.AL_AG = ObjectCreator.GLHeader1.PK;

			ARInvoice1.AH_PostDate = ZDateTime.Today.AddDays(-40);
			APInvoice1.AH_PostDate = ZDateTime.Today.AddDays(-40);
			ARCreditNote1.AH_PostDate = ZDateTime.Today;
			APCreditNote1.AH_PostDate = ZDateTime.Today;
			ARAdjustmentNote1.AH_PostDate = ZDateTime.Today.AddDays(40);
			APAdjustmentNote1.AH_PostDate = ZDateTime.Today.AddDays(40);

			WIP1.AL_PostDate = ZDateTime.Today.AddDays(-40);
			WIP2Reversed.AL_PostDate = ZDateTime.Today.AddDays(-40);
			WIP2Reversed.AL_ReverseDate = ZDateTime.Today;
			Accrual1.AL_PostDate = ZDateTime.Today;
			Accrual2Reversed.AL_PostDate = ZDateTime.Today.AddDays(30);
			Accrual2Reversed.AL_ReverseDate = ZDateTime.Today.AddDays(30);

			ARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			if (IsForceSetupSave)
			{
				Factory.SuspendValidation();
				Factory.Save();
				Factory.ResumeValidation();
			}
		}

		protected virtual bool IsForceSetupSave => true;

		#endregion

		#region Properties

		protected TransactionExportFilterProvider FilterProvider;
		protected TestObjectCreator ObjectCreator;

		protected ARInvoice ARInvoice1;
		protected ARCreditNote ARCreditNote1;
		protected ARAdjustmentNote ARAdjustmentNote1;

		protected TransactionPendingAllocation unAllocatedInvoice;
		protected TransactionPendingAllocation unAllocatedCreditNote;

		protected APInvoice APInvoice1;
		protected APCreditNote APCreditNote1;
		protected APAdjustmentNote APAdjustmentNote1;

		protected ARInvoice ARInvoiceFromAnotherCompany;
		protected WIP WIPFromAnotherCompany;

		protected WIP WIP1;
		protected WIP WIP2Reversed;
		protected Accrual Accrual1;
		protected Accrual Accrual2Reversed;

		protected TransactionExportFilter ExportFilter
		{
			get { return exportFilter ?? (exportFilter = GetNewExportFilter()); }
		}
		TransactionExportFilter exportFilter;

		protected abstract TransactionExportFilter GetNewExportFilter();

		#endregion
	}
}
