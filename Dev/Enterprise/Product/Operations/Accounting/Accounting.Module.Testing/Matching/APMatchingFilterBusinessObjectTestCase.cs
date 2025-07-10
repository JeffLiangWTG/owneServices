using CargoWise.Common;
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
	[TestedType(typeof(APMatchingFilterBusinessObject))]
	public class APMatchingFilterBusinessObjectTestCase : AccountingFilterStripBusinessObjectTestCase
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

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote1", FilterBO.TransactionHeaders.Contains(APCreditNote1));
			Assert("Expecting collection not to contain APInvoice2", !FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote1", !FilterBO.TransactionHeaders.Contains(APCreditNote1));
			Assert("Expecting collection to contain APInvoice2", FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote1", FilterBO.TransactionHeaders.Contains(APCreditNote1));
			Assert("Expecting collection not to contain APInvoice2", !FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote1", !FilterBO.TransactionHeaders.Contains(APCreditNote1));
			Assert("Expecting collection to contain APInvoice2", FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			APInvoice1.AH_TransactionNum = "11100011";
			APCreditNote2.AH_TransactionNum = "22000222";

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

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			APInvoice1.AH_ChequeOrReference = "11100011";
			APCreditNote2.AH_ChequeOrReference = "22000222";

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

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[AccountingConstants.NumberFilterTypes.ConsolidationNumber];

			APInvoice1.AH_ConsolidatedInvoiceRef = "11100011";
			APCreditNote2.AH_ConsolidatedInvoiceRef = "22000222";

			Factory.Save();

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "11100011";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			APInvoice1.AH_DueDate = new ZDateTime(2000, 1, 1);
			APCreditNote1.AH_DueDate = new ZDateTime(2000, 1, 1);
			APInvoice2.AH_DueDate = new ZDateTime(2000, 2, 2);
			APCreditNote2.AH_DueDate = new ZDateTime(2000, 2, 2);

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

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			APInvoice1.AH_PostDate = new ZDateTime(2000, 1, 1);
			APCreditNote1.AH_PostDate = new ZDateTime(2000, 1, 1);
			APInvoice2.AH_PostDate = new ZDateTime(2000, 2, 2);
			APCreditNote2.AH_PostDate = new ZDateTime(2000, 2, 2);

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

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			APInvoice1.AH_InvoiceDate = new ZDateTime(2000, 1, 1);
			APCreditNote1.AH_InvoiceDate = new ZDateTime(2000, 1, 1);
			APInvoice2.AH_InvoiceDate = new ZDateTime(2000, 2, 2);
			APCreditNote2.AH_InvoiceDate = new ZDateTime(2000, 2, 2);

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

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			MatchLinkI1.AP_MatchDate = new ZDateTime(2000, 1, 1);
			MatchLinkC2.AP_MatchDate = new ZDateTime(2000, 2, 2);

			//exclude irrelevant records
			MatchLinkI2.AP_MatchGroupNum = "M001";
			MatchLinkC1.AP_MatchGroupNum = "M002";

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

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			APInvoice1.AH_OH = Organisation1.PK;
			APCreditNote2.AH_OH = Organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Organisation"];

			filter.Property = Organisation1.PK;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection not to contain MatchGroup2", !FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection not to contain APCreditNote2", !FilterBO.TransactionHeaders.Contains(APCreditNote2));

			filter.Property = Organisation2.PK;
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection not to contain MatchGroup1", !FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection not to contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			filter.Property = "INV";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection to not contain APInvoice1", !FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APInvoice2", FilterBO.TransactionHeaders.Contains(APInvoice2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);

			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote1", FilterBO.TransactionHeaders.Contains(APCreditNote1));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);

			Assert("Expecting collection to contain APInvoice2", FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote1", FilterBO.TransactionHeaders.Contains(APCreditNote1));
			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);
			Assert("Expecting collection to contain APInvoice2", FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

			FilterBO.LedgerFilterValue = "AP";
			AssertEquals("Ledger Filter is now AP", "AP", FilterBO.LedgerFilterValue);

			FilterCollection.SetFilterHelper(FilterBO.MatchGroupFilter);
			FilterCollection.Load();

			Assert("Expecting collection to contain MatchGroup1", FilterCollection.ContainsMatchGroupNumber(MatchGroup1.MatchGroupNum));
			Assert("Expecting collection to contain MatchGroup2", FilterCollection.ContainsMatchGroupNumber(MatchGroup2.MatchGroupNum));

			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup1);
			Assert("Expecting collection to contain APInvoice1", FilterBO.TransactionHeaders.Contains(APInvoice1));
			Assert("Expecting collection to contain APCreditNote1", FilterBO.TransactionHeaders.Contains(APCreditNote1));
			FilterBO.SetCurrentTransactionsForMatchGroup(MatchGroup2);
			Assert("Expecting collection to contain APInvoice2", FilterBO.TransactionHeaders.Contains(APInvoice2));
			Assert("Expecting collection to contain APCreditNote2", FilterBO.TransactionHeaders.Contains(APCreditNote2));

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
				APPayment aPPayment = unmatchRow.Factory.New<APPayment>();
				TransactionMatchLink matchLink = ((IMatching)aPPayment).CurrentMatchGroup.AddNew();
				matchLink.AP_AH = aPPayment.PK;
				matchLink.AP_MatchGroupNum = matchNum;
				Business.TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
				unmatchRow.MatchLinks.Add(matchLink);
			}

			Factory.Save();

			FilterBO.SetCurrentTransactionsForMatchGroup(unmatchRow);

			AssertEquals(1000, FilterBO.TransactionHeaders.Count);
		}

		public void TestSuspendListChangedWhenSetCurrentTransactionsForMatchGroup()
		{
			var filterBO = new APMatchingFilterBusinessObject_ForTest();
			var matchGroup = new UnmatchingRow(Factory);
			var transactionHeaders = filterBO.TransactionHeaders;
			AssertNotNull("Pre-condition: transactionHeaders should not be null.", transactionHeaders);
			var callTimesBefore = filterBO.TransactionHeaders_ForTest.CallSuspenderTimes;

			filterBO.SetCurrentTransactionsForMatchGroup(matchGroup);
			AssertEquals("The CallSuspenderTimes should be 1 as the suspender was called.", 1, filterBO.TransactionHeaders_ForTest.CallSuspenderTimes - callTimesBefore);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APMatchingFilterBusinessObject();
		}

		APInvoice APInvoice1;
		APInvoice APInvoice2;

		APCreditNote APCreditNote1;
		APCreditNote APCreditNote2;

		UnmatchingRow MatchGroup1;
		UnmatchingRow MatchGroup2;

		TransactionMatchLink MatchLinkI1;
		TransactionMatchLink MatchLinkI2;
		TransactionMatchLink MatchLinkC1;
		TransactionMatchLink MatchLinkC2;

		OrgHeader Organisation1;
		OrgHeader Organisation2;
		OrgHeader Organisation3;

		UnmatchingRowCollection FilterCollection;
		APMatchingFilterBusinessObject FilterBO;

		class APMatchingFilterBusinessObject_ForTest : APMatchingFilterBusinessObject
		{
			public TransactionHeaderCollection_ForTest TransactionHeaders_ForTest => (TransactionHeaderCollection_ForTest)TransactionHeaders;

			protected override TransactionHeaderCollection TransactionHeadersCore
			{
				get
				{
					if (transactionHeaders == null)
					{
						transactionHeaders = new TransactionHeaderCollection_ForTest(Factory);
						transactionHeaders.SetReadOnlyIncludingChildren(true);
					}

					return transactionHeaders;
				}
			}

			TransactionHeaderCollection_ForTest transactionHeaders;

			public class TransactionHeaderCollection_ForTest : TransactionHeaderCollection
			{
				public TransactionHeaderCollection_ForTest(BusinessObjectFactory factory) : base(factory)
				{ }

				public int CallSuspenderTimes => callSuspenderTimes;
				int callSuspenderTimes;
				protected override DisposableList GetAdditionalListChangedSuspenders()
				{
					callSuspenderTimes++;
					return base.GetAdditionalListChangedSuspenders();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			MatchGroup1 = new UnmatchingRow(Factory);
			MatchGroup2 = new UnmatchingRow(Factory);

			APInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice2 = Factory.NewWithValidTestData<APInvoice>();

			APCreditNote1 = Factory.NewWithValidTestData<APCreditNote>();
			APCreditNote2 = Factory.NewWithValidTestData<APCreditNote>();

			MatchLinkI1 = ((IMatching)APInvoice1).CurrentMatchGroup.AddNew();
			MatchLinkI2 = ((IMatching)APInvoice2).CurrentMatchGroup.AddNew();

			MatchLinkC1 = ((IMatching)APCreditNote1).CurrentMatchGroup.AddNew();
			MatchLinkC2 = ((IMatching)APCreditNote2).CurrentMatchGroup.AddNew();

			Organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			MatchLinkI1.AP_AH = APInvoice1.PK;
			MatchLinkI2.AP_AH = APInvoice2.PK;

			MatchLinkC1.AP_AH = APCreditNote1.PK;
			MatchLinkC2.AP_AH = APCreditNote2.PK;

			Business.TestObjectCreator.SetupMatchLinkMatchDate(APInvoice1);
			Business.TestObjectCreator.SetupMatchLinkMatchDate(APInvoice2);
			Business.TestObjectCreator.SetupMatchLinkMatchDate(APCreditNote1);
			Business.TestObjectCreator.SetupMatchLinkMatchDate(APCreditNote2);

			MatchGroup1.MatchGroupNum = "11100011";
			MatchGroup2.MatchGroupNum = "22000222";

			MatchLinkI1.AP_MatchGroupNum = MatchGroup1.MatchGroupNum;
			MatchLinkI2.AP_MatchGroupNum = MatchGroup2.MatchGroupNum;

			MatchLinkC1.AP_MatchGroupNum = MatchGroup1.MatchGroupNum;
			MatchLinkC2.AP_MatchGroupNum = MatchGroup2.MatchGroupNum;

			FilterCollection = new UnmatchingRowCollection(Factory);
			FilterBO = (APMatchingFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
