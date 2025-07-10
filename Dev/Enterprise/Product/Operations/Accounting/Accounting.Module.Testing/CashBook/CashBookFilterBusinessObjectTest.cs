using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CashBookFilterBusinessObject))]
	public class CashBookFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Number Filter Tests

		public void TestTransactionNumberFilter()
		{
			OpeningPayment transaction1 = Factory.NewWithValidTestData<OpeningPayment>();
			OpeningReceipt transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();
			ARReceipt transaction3 = Factory.NewWithValidTestData<ARReceipt>();

			transaction1.IsManuallySetTransactionNumber_ForTestOnly = true;
			transaction2.IsManuallySetTransactionNumber_ForTestOnly = true;
			transaction3.IsManuallySetTransactionNumber_ForTestOnly = true;

			transaction1.AH_TransactionNum = "11111111";
			transaction2.AH_TransactionNum = "22000222";
			transaction3.AH_TransactionNum = "33330003";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		public void TestChequeReferenceNumberFilter()
		{
			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			DirectReceipt transaction2 = Factory.NewWithValidTestData<DirectReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction3 = bt.TransferRowFrom;

			transaction1.AH_ChequeOrReference = "11111111";
			transaction2.AH_ChequeOrReference = "22000222";
			transaction3.AH_ChequeOrReference = "33330003";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[Business.AccountingUtils.NumberFilterTypes.ChequeReferenceNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		public void TestDepositBatchNumberFilter()
		{
			ARReceipt transaction1 = Factory.NewWithValidTestData<ARReceipt>();
			DirectReceipt transaction2 = Factory.NewWithValidTestData<DirectReceipt>();
			ARReceipt transaction3 = Factory.NewWithValidTestData<ARReceipt>();

			transaction1.AH_ReceiptBatchNo = "11111111";
			transaction2.AH_ReceiptBatchNo = "22000222";
			transaction3.AH_ReceiptBatchNo = "33330003";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		public void TestDDRBatchNumberFilter()
		{
			APReceipt transaction1 = Factory.NewWithValidTestData<APReceipt>();
			DirectReceipt transaction2 = Factory.NewWithValidTestData<DirectReceipt>();
			APPayment transaction3 = Factory.NewWithValidTestData<APPayment>();

			transaction1.AH_ReceiptBatchNo = "11111111";
			transaction2.AH_ReceiptBatchNo = "22000222";
			transaction3.AH_ReceiptBatchNo = "33330003";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[Business.AccountingUtils.NumberFilterTypes.DDRBatchNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		#endregion

		#region Type Filter Tests

		public void TestTransactionTypesProperty()
		{
			AssertNotNull(FilterBO.TransactionTypes);
			AssertEquals(9, FilterBO.TransactionTypes.Count);
		}

		public void TestTransactionTypeFilter()
		{
			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			OpeningReceipt transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction3 = bt.TransferRowFrom;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Transaction Type"];

			filter.Property = ZArchitecture.Core.TransactionTypes.DirectPayment;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = ZArchitecture.Core.TransactionTypes.OpeningReceipt;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.Property = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		public void TestTransactionCategoryFilter()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 1.5m);

			var transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			var transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();

			var unrealizedExchangeDiff1 = TestObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 100M, TestObjectCreator.USD.Code);
			var unrealizedExchangeDiff2 = TestObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 200M, TestObjectCreator.USD.Code);
			var unrealizedExchangeDiff3 = TestObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 300M, TestObjectCreator.USD.Code);

			var realizedExchangeDiff1 = TestObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 100M, TestObjectCreator.USD.Code);
			var realizedExchangeDiff2 = TestObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 200M, TestObjectCreator.USD.Code);
			realizedExchangeDiff1.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;
			realizedExchangeDiff2.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Transaction Category"];
			filter.IsActive = true;

			filter.Property = "ALL";
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection to contain all transactions", 7, FilterCollection.Count);

			filter.Property = Core.Constants.TransactionCategory.Codes.UnrealizedExchangeGainLoss;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection to contain 3 transactions", 3, FilterCollection.Count);
			Assert("Expecting collection to contain unrealizedExchangeDiff1", FilterCollection.Contains(unrealizedExchangeDiff1));
			Assert("Expecting collection to contain unrealizedExchangeDiff2", FilterCollection.Contains(unrealizedExchangeDiff2));
			Assert("Expecting collection to contain unrealizedExchangeDiff3", FilterCollection.Contains(unrealizedExchangeDiff3));

			filter.Property = Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection to contain 2 transactions", 2, FilterCollection.Count);
			Assert("Expecting collection to contain realizedExchangeDiff1", FilterCollection.Contains(realizedExchangeDiff1));
			Assert("Expecting collection to contain realizedExchangeDiff2", FilterCollection.Contains(realizedExchangeDiff2));
		}

		public void TestNotBatchedFilter()
		{
			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			OpeningReceipt transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction3 = bt.TransferRowFrom;

			transaction1.AH_IsCancelled = ZBool.False;
			transaction2.AH_IsCancelled = ZBool.False;
			transaction3.AH_IsCancelled = ZBool.True;

			transaction1.AH_ReceiptBatchNo = ZString.Empty;
			transaction2.AH_ReceiptBatchNo = "111";
			transaction3.AH_ReceiptBatchNo = ZString.Empty;

			Factory.Save();

			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterBO["Not Batched"];

			filter.Property0 = ZBool.True;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property0 = ZBool.False;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.Property0 = ZBool.True;
			filter.IsActive = false;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));
		}

		public void TestPositivePayExportStatus()
		{
			string originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

				DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
				APPayment transaction2 = Factory.NewWithValidTestData<APPayment>();
				BankTransfer bt = new BankTransfer(Factory, null);
				BankTransferFromRow transaction3 = bt.TransferRowFrom;

				GenExportBatchSequence batch = Factory.New<GenExportBatchSequence>();
				batch.XB_BatchNumber = 1000;
				batch.XB_ParentID = transaction2.PK;
				batch.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				batch.XB_Sequence = 1;
				batch.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.PositivePayFile;

				Factory.Save();

				ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Positive Pay Export Status"];

				filter.Property = "ALL";
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);

				Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
				Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
				Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

				filter.Property = "EXP";
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);

				Assert("Expecting collection to not contain Transaction1", !FilterCollection.Contains(transaction1));
				Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
				Assert("Expecting collection to not contain Transaction3", !FilterCollection.Contains(transaction3));

				filter.Property = "NOT";
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);

				Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
				Assert("Expecting collection to not contain Transaction2", !FilterCollection.Contains(transaction2));
				Assert("Expecting collection to not contain Transaction3", !FilterCollection.Contains(transaction3));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestCheckPaymentExportStatus()
		{
			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			APPayment transaction2 = Factory.NewWithValidTestData<APPayment>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction3 = bt.TransferRowFrom;

			GenExportBatchSequence batch = Factory.New<GenExportBatchSequence>();
			batch.XB_BatchNumber = 1000;
			batch.XB_ParentID = transaction2.PK;
			batch.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			batch.XB_Sequence = 1;
			batch.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.PositivePayFile;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Check Payment Export Status"];

			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.Property = "EXP";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to not contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection to not contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = "NOT";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to not contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection to not contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		#endregion

		#region Reference Filter Tests

		public void TestFindBoxCollections()
		{
			AssertNotNull(FilterBO.Branches);
			AssertNotNull(FilterBO.Currencies);
			AssertNotNull(FilterBO.Departments);
			AssertNotNull(FilterBO.BankAccounts);
			AssertNotNull(FilterBO.Organisations);
		}

		public void TestCurrencyFilter()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency3 = Factory.NewWithValidTestData<RefCurrency>();

			DirectReceipt transaction1 = Factory.NewWithValidTestData<DirectReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction2 = bt.TransferRowFrom;

			transaction1.AH_RX_NKTransactionCurrency = currency1.RX_Code;
			transaction2.AH_RX_NKTransactionCurrency = currency2.RX_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Currency"];

			filter.Property = currency1.RX_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.Property = currency3.RX_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestBankAccountFilter()
		{
			AccBankAccount account1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount account2 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount account3 = Factory.NewWithValidTestData<AccBankAccount>();

			OpeningPayment transaction1 = Factory.NewWithValidTestData<OpeningPayment>();
			DirectReceipt transaction2 = Factory.NewWithValidTestData<DirectReceipt>();

			transaction1.AH_AB = account1.PK;
			transaction2.AH_AB = account2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Bank Account"];

			filter.Property = account1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.Property = account3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestOrganisationFilter()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			ARPayment transaction1 = Factory.NewWithValidTestData<ARPayment>();
			APReceipt transaction2 = Factory.NewWithValidTestData<APReceipt>();

			transaction1.AH_OH = organisation1.PK;
			transaction2.AH_OH = organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Debitor/Creditor"];

			filter.Property = organisation1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.Property = organisation3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestDepartmentFilter()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department3 = Factory.NewWithValidTestData<GlbDepartment>();

			OpeningReceipt transaction1 = Factory.NewWithValidTestData<OpeningReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction2 = bt.TransferRowFrom;

			transaction1.AH_GE = department1.PK;
			transaction2.AH_GE = department2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Department"];

			filter.Property = department1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.Property = department3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();

			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;

			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			OpeningReceipt transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction3 = bt.TransferRowFrom;

			transaction1.AH_GB = branch1.PK;
			transaction2.AH_GB = branch2.PK;
			transaction3.AH_GB = branch3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Branch"];

			filter.Property = branch1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = branch3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));
		}

		public void TestTaxBranchFilter()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			FilterBO = (CashBookFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNull("Tax Branch filter should be null when registry is not enabled.", FilterBO["Tax Branch"]);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FilterBO = (CashBookFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNull("Tax Branch filter should be here when registry is enabled.", FilterBO["Tax Branch"]);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			FilterBO = (CashBookFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNull("Tax Branch filter should be null when registry is not enabled.", FilterBO["Tax Branch"]);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FilterBO = (CashBookFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull("Tax Branch filter should be here when registry is enabled.", FilterBO["Tax Branch"]);

			var transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			var branch1 = TestObjectCreator.CreateBranch("T01", GlbCompany.CurrentCompany);
			transaction1.AH_GB_TaxBranch = branch1.PK;

			var transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();
			var branch2 = TestObjectCreator.CreateBranch("T02", GlbCompany.CurrentCompany);
			transaction2.AH_GB_TaxBranch = branch2.PK;

			var transaction3 = new BankTransfer(Factory, null).TransferRowFrom;
			var branch3 = TestObjectCreator.CreateBranch("T03", GlbCompany.CurrentCompany);
			transaction3.AH_GB_TaxBranch = branch3.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterBO["Tax Branch"];

			filter.Property = branch1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = branch3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection to contain Transaction3", FilterCollection.Contains(transaction3));
		}

		#endregion

		#region Date Filter Tests

		public void TestPostDateFilter()
		{
			DirectReceipt transaction1 = Factory.NewWithValidTestData<DirectReceipt>();
			ARPayment transaction2 = Factory.NewWithValidTestData<ARPayment>();

			transaction1.AH_PostDate = new ZDateTime(2000, 1, 1, 11, 0, 0);     // 2 Jan 11:00
			transaction2.AH_PostDate = new ZDateTime(2000, 2, 2, 22, 0, 0);     // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.PostDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestTransactionDateFilter()
		{
			OpeningPayment transaction1 = Factory.NewWithValidTestData<OpeningPayment>();
			APReceipt transaction2 = Factory.NewWithValidTestData<APReceipt>();

			transaction1.AH_InvoiceDate = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			transaction2.AH_InvoiceDate = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.TransactionDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestDateShownInStatementFilter()
		{
			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction2 = bt.TransferRowFrom;

			transaction1.AH_DateClearedInCashbook = new ZDateTime(2000, 1, 1, 11, 0, 0);        // 2 Jan 11:00
			transaction2.AH_DateClearedInCashbook = new ZDateTime(2000, 2, 2, 22, 0, 0);        // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.DateShownInStatement];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		#endregion

		#region Other Filter Tests

		public void TestPaymentReceiptMethodProperty()
		{
			AssertNotNull(FilterBO.PaymentReceiptMethods);
			Assert("CHQ should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.Cheque));
			Assert("CSH should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.Cash));
			Assert("CCD should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.CreditCard));
			Assert("DCR/DDR should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.DirectCredit + "/" + ReceiptTypes.DirectDebit));
			Assert("EFT should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.EFT));
			Assert("SFT should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.ScheduledEFT));
			Assert("CRQ should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.CollectionRequest));
			Assert("ENC should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.eNettDirectCredit));
			Assert("END should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.eNettDirectDebit));
			Assert("ECC should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.eNettCreditCard));
			Assert("AMF should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.AccountMaintenanceFee));
			Assert("BDT should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.BankDebitTax));
			Assert("BDP should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.BankDepositFee));
			Assert("INT should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.InterestPaid));
			Assert("INR should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.InterestReceived));
			Assert("PPY should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.PeriodicPayment));
			Assert("STD should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.StampDuty));
			Assert("MSR should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.MiscellaneousReceipt));
			Assert("MSF should exist in the list of filters", FilterBO.PaymentReceiptMethods.ContainsCode(ReceiptTypes.MiscellaneousFees));
		}

		public void TestPaymentReceiptMethodFilter()
		{
			DirectPayment transaction1 = Factory.NewWithValidTestData<DirectPayment>();
			OpeningReceipt transaction2 = Factory.NewWithValidTestData<OpeningReceipt>();
			ARPayment transaction3 = Factory.NewWithValidTestData<ARPayment>();

			transaction1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			transaction2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			transaction3.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Payment/Receipt Method"];

			filter.Property = ZArchitecture.Core.ReceiptTypes.Cash;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = ZArchitecture.Core.ReceiptTypes.Cheque;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property = ZArchitecture.Core.ReceiptTypes.CreditCard;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expecting collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		public void TestAmountFilter()
		{
			ARReceipt transaction1 = Factory.NewWithValidTestData<ARReceipt>();
			DirectPayment transaction2 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 50m, 0m, 50m, 0m);
			OpeningReceipt transaction3 = Factory.NewWithValidTestData<OpeningReceipt>();

			transaction1.AH_OSTotal = 10.0;
			transaction1.AH_InvoiceAmount = 10.0;
			transaction1.AH_OutstandingAmount = 10.0;
			transaction3.AH_OSTotal = -100.0;
			transaction3.AH_InvoiceAmount = -100.0;
			transaction3.AH_OutstandingAmount = -100.0;

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["Amount"];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expected collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expected collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property1 = 100.0;
			filter.Property2 = 200.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expected collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expected collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.Property1 = 10.0;
			filter.Property2 = 100.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expected collection to contain Transaction2", FilterCollection.Contains(transaction2));
			Assert("Expected collection to contain Transaction3", FilterCollection.Contains(transaction3));

			filter.Property1 = 20.0;
			filter.Property2 = 90.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expected collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expected collection not to contain Transaction3", !FilterCollection.Contains(transaction3));

			filter.Property1 = 200.0;
			filter.Property2 = 300.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expected collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
			Assert("Expected collection not to contain Transaction3", !FilterCollection.Contains(transaction3));
		}

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CashBookFilterBusinessObject();
		}

		CashBookFilterBusinessObject FilterBO;
		CashbookTransactionCollection FilterCollection;

		protected override void SetUp()
		{
			base.SetUp();
			FilterCollection = new CashbookTransactionCollection(Factory);
			FilterBO = (CashBookFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
