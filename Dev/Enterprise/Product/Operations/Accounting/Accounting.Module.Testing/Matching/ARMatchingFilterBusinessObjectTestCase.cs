using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using AccountingConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARMatchingFilterBusinessObject))]
	public class ARMatchingFilterBusinessObjectTestCase : AccountingFilterStripBusinessObjectTestCase
	{
		#region Number Filter Tests

		public void TestMatchGroupNumberFilter()
		{
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[AccountingConstants.NumberFilterTypes.MatchGroupNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment1", FilterBO.TransactionHeaders.Contains(ARPayment1));
			Assert("Expecting collection not to contain ARReceipt2", !FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment1", !FilterBO.TransactionHeaders.Contains(ARPayment1));
			Assert("Expecting collection to contain ARReceipt2", FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment1", FilterBO.TransactionHeaders.Contains(ARPayment1));
			Assert("Expecting collection not to contain ARReceipt2", !FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment1", !FilterBO.TransactionHeaders.Contains(ARPayment1));
			Assert("Expecting collection to contain ARReceipt2", FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		public void TestTransactionNumberFilter()
		{
			ARReceipt1.IsManuallySetTransactionNumber_ForTestOnly = true;
			ARPayment2.IsManuallySetTransactionNumber_ForTestOnly = true;

			ARReceipt1.AH_TransactionNum = "11100011";
			ARPayment2.AH_TransactionNum = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[AccountingConstants.NumberFilterTypes.TransactionNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		public void TestChequeOrReferenceNumberFilter()
		{
			ARReceipt1.AH_ChequeOrReference = "11100011";
			ARPayment2.AH_ChequeOrReference = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[AccountingConstants.NumberFilterTypes.ChequeReferenceNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		public void TestConsolidationNumberFilter()
		{
			ARReceipt1.AH_ConsolidatedInvoiceRef = "11100011";
			ARPayment2.AH_ConsolidatedInvoiceRef = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[AccountingConstants.NumberFilterTypes.ConsolidationNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		#endregion

		#region Date Filter Tests

		public void TestDueDateFilter()
		{
			ARReceipt1.AH_DueDate = new ZDateTime(2000, 1, 1);
			ARPayment1.AH_DueDate = new ZDateTime(2000, 1, 1);
			ARReceipt2.AH_DueDate = new ZDateTime(2000, 2, 2);
			ARPayment2.AH_DueDate = new ZDateTime(2000, 2, 2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.DueDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		public void TestPostDateFilter()
		{
			ARReceipt1.AH_PostDate = new ZDateTime(2000, 1, 1);
			ARPayment1.AH_PostDate = new ZDateTime(2000, 1, 1);
			ARReceipt2.AH_PostDate = new ZDateTime(2000, 2, 2);
			ARPayment2.AH_PostDate = new ZDateTime(2000, 2, 2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.PostDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		public void TestTransactionDateFilter()
		{
			ARReceipt1.AH_InvoiceDate = new ZDateTime(2000, 1, 1);
			ARPayment1.AH_InvoiceDate = new ZDateTime(2000, 1, 1);
			ARReceipt2.AH_InvoiceDate = new ZDateTime(2000, 2, 2);
			ARPayment2.AH_InvoiceDate = new ZDateTime(2000, 2, 2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.TransactionDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		public void TestMatchDateFilter()
		{
			MatchLinkR1.AP_MatchDate = new ZDateTime(2000, 1, 1);
			MatchLinkP2.AP_MatchDate = new ZDateTime(2000, 2, 2);

			//exclude irrelevant records
			MatchLinkR2.AP_MatchGroupNum = "M001";
			MatchLinkP1.AP_MatchGroupNum = "M002";

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.MatchDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		#endregion

		#region Organisation Filter Tests

		public void TestOrganisationFilter()
		{
			ARReceipt1.AH_OH = Organisation1.PK;
			ARPayment2.AH_OH = Organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Organisation"];

			filter.Property = Organisation1.PK;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection not to contain ARPayment2", !FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.Property = Organisation2.PK;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.Property = Organisation3.PK;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		#endregion

		#region Transaction Type Filter Test

		public void TestTransactionTypeFilter()
		{
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[AccountingConstants.ModesAndTypesFilterTypes.TransactionType];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "REC";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection to not contain ARReceipt1", !FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARReceipt2", FilterBO.TransactionHeaders.Contains(ARReceipt2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment1", FilterBO.TransactionHeaders.Contains(ARPayment1));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection to contain ARReceipt2", FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "TRF";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		#endregion

		#region Ledger Filter Test

		public void TestLedgerFilter()
		{
			Factory.Save();

			AssertEquals("Ledger Filter is empty by default", ZString.Empty, FilterBO.LedgerFilterValue);

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);
			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment1", FilterBO.TransactionHeaders.Contains(ARPayment1));
			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);
			Assert("Expecting collection to contain ARReceipt2", FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.LedgerFilterValue = "AR";
			AssertEquals("Ledger Filter is now AR", "AR", FilterBO.LedgerFilterValue);

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);
			Assert("Expecting collection to contain ARReceipt1", FilterBO.TransactionHeaders.Contains(ARReceipt1));
			Assert("Expecting collection to contain ARPayment1", FilterBO.TransactionHeaders.Contains(ARPayment1));
			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);
			Assert("Expecting collection to contain ARReceipt2", FilterBO.TransactionHeaders.Contains(ARReceipt2));
			Assert("Expecting collection to contain ARPayment2", FilterBO.TransactionHeaders.Contains(ARPayment2));

			FilterBO.LedgerFilterValue = "XX";
			AssertEquals("Ledger Filter is now XX", "XX", FilterBO.LedgerFilterValue);

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to not contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to not contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));
		}

		#endregion

		#region Other Tests

		public void TestFilterDoesntBreakWithManyRows()
		{
			ZString matchNum = "Test";
			UnmatchingRow unmatchRow = new UnmatchingRow(Factory);

			for (int count = 0; count < 1000; count++)
			{
				ARInvoice aRInvoice = unmatchRow.Factory.New<ARInvoice>();
				TransactionMatchLink matchLink = ((IMatching)aRInvoice).CurrentMatchGroup.AddNew();
				matchLink.AP_AH = aRInvoice.PK;
				Business.TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
				matchLink.AP_MatchGroupNum = matchNum;
				unmatchRow.MatchLinks.Add(matchLink);
			}

			Factory.Save();

			FilterBO.SetCurrentTransactionsForMatchGroup(unmatchRow);

			AssertEquals(1000, FilterBO.TransactionHeaders.Count);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARMatchingFilterBusinessObject();
		}

		ARReceipt ARReceipt1;
		ARReceipt ARReceipt2;

		ARPayment ARPayment1;
		ARPayment ARPayment2;

		UnmatchingRow MatchGroup1;
		UnmatchingRow MatchGroup2;

		TransactionMatchLink MatchLinkR1;
		TransactionMatchLink MatchLinkR2;
		TransactionMatchLink MatchLinkP1;
		TransactionMatchLink MatchLinkP2;

		OrgHeader Organisation1;
		OrgHeader Organisation2;
		OrgHeader Organisation3;

		UnmatchingRowCollection FilterCollection;
		ARMatchingFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			MatchGroup1 = new UnmatchingRow(Factory);
			MatchGroup2 = new UnmatchingRow(Factory);

			ARReceipt1 = Factory.NewWithValidTestData<ARReceipt>();
			ARReceipt2 = Factory.NewWithValidTestData<ARReceipt>();

			ARPayment1 = Factory.NewWithValidTestData<ARPayment>();
			ARPayment2 = Factory.NewWithValidTestData<ARPayment>();

			MatchLinkR1 = ((IMatching)ARReceipt1).CurrentMatchGroup.AddNew();
			MatchLinkR2 = ((IMatching)ARReceipt2).CurrentMatchGroup.AddNew();

			MatchLinkP1 = ((IMatching)ARPayment1).CurrentMatchGroup.AddNew();
			MatchLinkP2 = ((IMatching)ARPayment1).CurrentMatchGroup.AddNew();

			Organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			MatchLinkR1.AP_AH = ARReceipt1.PK;
			MatchLinkR2.AP_AH = ARReceipt2.PK;

			MatchLinkP1.AP_AH = ARPayment1.PK;
			MatchLinkP2.AP_AH = ARPayment2.PK;

			Business.TestObjectCreator.SetupMatchLinkMatchDate(ARReceipt1);
			Business.TestObjectCreator.SetupMatchLinkMatchDate(ARReceipt2);
			Business.TestObjectCreator.SetupMatchLinkMatchDate(ARPayment1);
			Business.TestObjectCreator.SetupMatchLinkMatchDate(ARPayment2);

			MatchGroup1.MatchGroupNum = "11100011";
			MatchGroup2.MatchGroupNum = "22000222";

			MatchLinkR1.AP_MatchGroupNum = MatchGroup1.MatchGroupNum;
			MatchLinkR2.AP_MatchGroupNum = MatchGroup2.MatchGroupNum;

			MatchLinkP1.AP_MatchGroupNum = MatchGroup1.MatchGroupNum;
			MatchLinkP2.AP_MatchGroupNum = MatchGroup2.MatchGroupNum;

			FilterCollection = new UnmatchingRowCollection(Factory);
			FilterBO = (ARMatchingFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
