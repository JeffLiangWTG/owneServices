using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class TransactionExportFilterProviderTest : TestCaseWithFactory
	{
		public void TestAtLeastOneTypeOfARIsSelected()
		{
			FilterProvider.IncludeARInvoices = false;
			FilterProvider.IncludeARCreditNotes = false;
			FilterProvider.IncludeARAdjustmentNotes = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfARIsSelected", false, FilterProvider.AtLeastOneTypeOfARIsSelected);

			FilterProvider.IncludeARInvoices = true;
			AssertEquals("FilterProvider.AtLeastOneTypeOfARIsSelected", true, FilterProvider.AtLeastOneTypeOfARIsSelected);

			FilterProvider.IncludeARCreditNotes = true;
			FilterProvider.IncludeARInvoices = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfARIsSelected", true, FilterProvider.AtLeastOneTypeOfARIsSelected);

			FilterProvider.IncludeARAdjustmentNotes = true;
			FilterProvider.IncludeARCreditNotes = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfARIsSelected", true, FilterProvider.AtLeastOneTypeOfARIsSelected);

			FilterProvider.IncludeARAdjustmentNotes = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfARIsSelected", false, FilterProvider.AtLeastOneTypeOfARIsSelected);
		}

		public void TestAtLeastOneTypeOfAPIsSelected()
		{
			FilterProvider.IncludeAPInvoices = false;
			FilterProvider.IncludeAPCreditNotes = false;
			FilterProvider.IncludeAPAdjustmentNotes = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfAPIsSelected", false, FilterProvider.AtLeastOneTypeOfAPIsSelected);

			FilterProvider.IncludeAPInvoices = true;
			AssertEquals("FilterProvider.AtLeastOneTypeOfAPIsSelected", true, FilterProvider.AtLeastOneTypeOfAPIsSelected);

			FilterProvider.IncludeAPCreditNotes = true;
			FilterProvider.IncludeAPInvoices = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfAPIsSelected", true, FilterProvider.AtLeastOneTypeOfAPIsSelected);

			FilterProvider.IncludeAPAdjustmentNotes = true;
			FilterProvider.IncludeAPCreditNotes = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfAPIsSelected", true, FilterProvider.AtLeastOneTypeOfAPIsSelected);

			FilterProvider.IncludeAPAdjustmentNotes = false;
			AssertEquals("FilterProvider.AtLeastOneTypeOfAPIsSelected", false, FilterProvider.AtLeastOneTypeOfAPIsSelected);
		}

		public void TestFieldsGetSet()
		{
			FilterProvider.IncludeARInvoices = true;
			Assert(FilterProvider.IncludeARInvoices);

			FilterProvider.IncludeARCreditNotes = true;
			Assert(FilterProvider.IncludeARCreditNotes);

			FilterProvider.IncludeARAdjustmentNotes = true;
			Assert(FilterProvider.IncludeARAdjustmentNotes);

			FilterProvider.IncludeAPInvoices = true;
			Assert(FilterProvider.IncludeAPInvoices);

			FilterProvider.IncludeAPCreditNotes = true;
			Assert(FilterProvider.IncludeAPCreditNotes);

			FilterProvider.IncludeAPAdjustmentNotes = true;
			Assert(FilterProvider.IncludeAPAdjustmentNotes);

			FilterProvider.IncludeWIPsPosting = true;
			Assert(FilterProvider.IncludeWIPsPosting);

			FilterProvider.IncludeWIPsReversing = true;
			Assert(FilterProvider.IncludeWIPsReversing);

			FilterProvider.IncludeAccrualsPosting = true;
			Assert(FilterProvider.IncludeAccrualsPosting);

			FilterProvider.IncludeAccrualsReversing = true;
			Assert(FilterProvider.IncludeAccrualsReversing);

			AssertNotNull(FilterProvider.Jobs);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			Assert(FilterProvider.ExcludeJobRelatedTransactionsForAR);

			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			Assert(FilterProvider.ExcludeNonJobRelatedTransactionsForAR);

			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			Assert(FilterProvider.ExcludeJobRelatedTransactionsForAP);

			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;
			Assert(FilterProvider.ExcludeNonJobRelatedTransactionsForAP);

			ZDateTime dateFrom_TestValue = new ZDateTime(2004, 02, 01);
			FilterProvider.DateFrom = dateFrom_TestValue;
			AssertEquals(dateFrom_TestValue, FilterProvider.DateFrom);

			ZDateTime dateTo_TestValue = new ZDateTime(2004, 05, 12);
			FilterProvider.DateTo = dateTo_TestValue;
			AssertEquals(dateTo_TestValue, FilterProvider.DateTo);

			ZGuid accountgGroup = ZGuid.NewZGuid();
			FilterProvider.AccountGroup = accountgGroup;
			AssertEquals("Account Group ZGuid", accountgGroup, FilterProvider.AccountGroup);

			FilterProvider.PeriodFrom = 5;
			AssertEquals(5, FilterProvider.PeriodFrom);

			FilterProvider.PeriodTo = 1;
			AssertEquals(1, FilterProvider.PeriodTo);

			FilterProvider.TransactionNumberFrom = "F1234567Z";
			AssertEquals("F1234567Z", FilterProvider.TransactionNumberFrom);

			FilterProvider.TransactionNumberTo = "ABC123456Z";
			AssertEquals("ABC123456Z", FilterProvider.TransactionNumberTo);

			AssertNotNull(FilterProvider.Organisations);
			AssertNotNull(FilterProvider.Branches);
			AssertNotNull(FilterProvider.Departments);

			AssertEquals(0, FilterProvider.CurrentBatchNo);

			int batchNo = 100;
			FilterProvider.CurrentBatchNo = batchNo;
			AssertEquals(batchNo, FilterProvider.CurrentBatchNo);
		}

		public void TestCollectionsHaveAdhocRelationship()
		{
			FilterProvider.Jobs.Add(Factory.NewJobForTesting<JobHeader>());
			AssertEquals("Jobs collections should not be read-only", 1, FilterProvider.Jobs.Count);
			FilterProvider.Departments.Add(Factory.New<GlbDepartment>());
			AssertEquals("Jobs collections should not be read-only", 1, FilterProvider.Departments.Count);
			FilterProvider.Branches.Add(Factory.New<GlbBranch>());
			AssertEquals("Jobs collections should not be read-only", 1, FilterProvider.Branches.Count);
			FilterProvider.Organisations.Add(Factory.New<OrgHeader>());
			AssertEquals("Jobs collections should not be read-only", 1, FilterProvider.Organisations.Count);
		}

		public void TestCollectionsAreWritable()
		{
			AssertEquals("Jobs collections should not be read-only", false, FilterProvider.Jobs.ReadOnly);
			AssertEquals("Organisations collections should not be read-only", false, FilterProvider.Organisations.ReadOnly);
			AssertEquals("Branches collections should not be read-only", false, FilterProvider.Branches.ReadOnly);
			AssertEquals("Departments collections should not be read-only", false, FilterProvider.Departments.ReadOnly);
		}

		public void TestCanSaveAndUsingHighWaterMark()
		{
			CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection {
					{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
					{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
					{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", false },
					{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", false },
					{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", false },
					{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", false },
					{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", false },
					{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", false },
					{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", false },
					{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", false },
					{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", false },
					{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", false } };
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

			var exporter = new DummyExporter(Factory);
			var exporterWithHighWaterMarkEnabled = new DummyExporterWithHighWaterMarkEnabled(Factory);
			var exporterWithHighWaterMarkDisabled = new DummyExporterWithHighWaterMarkDisabled(Factory);

			FilterProvider.IncludeARInvoices = true;
			FilterProvider.IncludeAPInvoices = true;
			FilterProvider.Exporter = exporterWithHighWaterMarkEnabled;
			FilterProvider.CurrentBatchNo = 0;
			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.CurrentBatchNo = 1;
			AssertEquals("High water mark cannot be saved because exporter is not creating a batch", false, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.CurrentBatchNo = 0;
			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.Exporter = exporter as ISupportHighWaterMark;
			AssertNull("Exporter is not ISupportHighWaterMark", FilterProvider.Exporter);
			AssertEquals("High water mark cannot be saved because exporter is null", false, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.Exporter = exporterWithHighWaterMarkEnabled;
			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.Exporter = exporterWithHighWaterMarkDisabled;
			AssertEquals("High water mark cannot be saved because exporter has high water mark disabled", false, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.Exporter = exporterWithHighWaterMarkEnabled;
			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			AssertEquals("High water mark cannot be saved because a restrictive filter was set", false, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.ExcludeJobRelatedTransactionsForAP = false;
			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.IncludeARCreditNotes = true;
			AssertEquals("High water mark cannot be saved because filter no longer matches registry", false, FilterProvider.CanSaveHighWaterMark);

			FilterProvider.IncludeARCreditNotes = false;
			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			((CodeDescriptionBool)defaultCollection.FindByCode(ExportTransactionTypes.ARInvoice)).Bool = false;
			((CodeDescriptionBool)defaultCollection.FindByCode(ExportTransactionTypes.APInvoice)).Bool = false;
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

			AssertEquals("High water mark cannot be saved because no transaction types are set in the registry", false, FilterProvider.CanSaveHighWaterMark);

			((CodeDescriptionBool)defaultCollection.FindByCode(ExportTransactionTypes.ARInvoice)).Bool = true;
			((CodeDescriptionBool)defaultCollection.FindByCode(ExportTransactionTypes.APInvoice)).Bool = true;
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

			AssertEquals("High water mark can be saved", true, FilterProvider.CanSaveHighWaterMark);

			AssertEquals("High water mark cannot be used because registry is not set", false, FilterProvider.UsingHighWaterMark);

			using (SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}

			AssertEquals("High water mark can be used because registry is set", true, FilterProvider.UsingHighWaterMark);
		}

		public void TestHasRestrictiveFilters()
		{
			Assert("No restrictive filters set", !FilterProvider.HasRestrictiveFilters);

			SetPropertyAndAssertValue("TransactionNumberFrom", new ZString("1000"));
			SetPropertyAndAssertValue("TransactionNumberTo", new ZString("2000"));
			SetPropertyAndAssertValue("ExcludeJobRelatedTransactionsForAR", ZBool.True);
			SetPropertyAndAssertValue("ExcludeNonJobRelatedTransactionsForAR", ZBool.True);
			SetPropertyAndAssertValue("ExcludeJobRelatedTransactionsForAP", ZBool.True);
			SetPropertyAndAssertValue("ExcludeNonJobRelatedTransactionsForAP", ZBool.True);
			SetPropertyAndAssertValue("DateFrom", ZDateTime.Now);
			SetPropertyAndAssertValue("DateTo", ZDateTime.Now);
			SetPropertyAndAssertValue("PeriodFrom", new ZInt(201201));
			SetPropertyAndAssertValue("PeriodTo", new ZInt(201202));
			SetPropertyAndAssertValue("Jobs", Factory.NewJobWithValidTestDataForTesting<JobHeader>());
			SetPropertyAndAssertValue("Organisations", Factory.NewWithValidTestData<OrgHeader>());
			SetPropertyAndAssertValue("Branches", GlbBranch.CurrentBranch);
			SetPropertyAndAssertValue("Departments", GlbDepartment.CurrentDepartment);

			Assert("No restrictive filters set", !FilterProvider.HasRestrictiveFilters);
		}

		void SetPropertyAndAssertValue(string propertyName, object value)
		{
			var property = (FilterProvider.GetType().GetProperty(propertyName));
			var originalValue = property.GetValue(FilterProvider, null);
			if (originalValue is IBusinessObjectCollection)
			{
				var collection = (IBusinessObjectCollection)originalValue;
				collection.Add((IBusiness)value);
				Assert(string.Format("HasRestrictiveFilters should be true because the following restrictive filter was set: {0}", property.Name), FilterProvider.HasRestrictiveFilters);
				collection.Remove((IBusiness)value);
			}
			else
			{
				property.SetValue(FilterProvider, value, null);
				Assert(string.Format("HasRestrictiveFilters should be true because the following restrictive filter was set: {0}", property.Name), FilterProvider.HasRestrictiveFilters);
				property.SetValue(FilterProvider, originalValue, null);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			FilterProvider = new TransactionExportFilterProvider(Factory);
		}

		TransactionExportFilterProvider FilterProvider;
	}

	internal class DummyExporter : AccountingTransactionsDataExporter
	{
		public DummyExporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void ExportObjectsToEndPoint(BusinessObject bizObj, Enterprise.DataTransfer.Integration.IValueObjectDataAdapter dataAdapter, ZString status)
		{
			throw new NotImplementedException();
		}

		protected override void InitialiseDocumentWriter(System.IO.Stream docStream)
		{
			throw new NotImplementedException();
		}
	}

	internal class DummyExporterWithHighWaterMarkEnabled : DummyExporter, ISupportHighWaterMark
	{
		public DummyExporterWithHighWaterMarkEnabled(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool IsHighWaterMarkEnabled
		{
			get { return true; }
		}

		public DateTimeRegistryItem HighWaterMarkRegistry
		{
			get { return SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark; }
		}
	}

	internal class DummyExporterWithHighWaterMarkDisabled : DummyExporter, ISupportHighWaterMark
	{
		public DummyExporterWithHighWaterMarkDisabled(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool IsHighWaterMarkEnabled
		{
			get { return false; }
		}

		public DateTimeRegistryItem HighWaterMarkRegistry
		{
			get { return SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark; }
		}
	}
}
