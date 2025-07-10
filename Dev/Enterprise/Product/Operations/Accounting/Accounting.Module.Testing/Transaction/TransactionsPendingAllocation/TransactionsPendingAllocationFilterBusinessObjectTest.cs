using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(TransactionsPendingAllocationFilterBusinessObject))]
	public class TransactionsPendingAllocationFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TransactionsPendingAllocationFilterBusinessObject();
		}

		#region TestBranchManagementCodeFilter

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", description: null, value: true);
			codeCollection.Add("BRB", description: null, value: true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";

			var invoice1 = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoice1.AH_GB = branch1.PK;
			var invoice2 = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			invoice2.AH_GB = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterBO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collection = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice1", new[] { invoice1 }, collection);

			branchManagementCodeFilter.Property = "BRB";
			collection = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain invoice2", new[] { invoice2 }, collection);
		}

		#endregion

		public void TestCreditorWithAddressFilter()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.OA_Code = "XYZ";
			testAddress.OA_OH = testOrg.PK;

			TransactionPendingAllocation testInvoice1 = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			testInvoice1.AH_OH = testOrg.PK;
			testInvoice1.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			testInvoice1.AH_TransactionNum = "00002544";

			TransactionPendingAllocation testInvoice2 = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			testInvoice2.AH_OH = testOrg.PK;
			testInvoice2.AH_OA_InvoiceAddressOverride = testAddress.PK;
			testInvoice2.AH_ConsolidatedInvoiceRef = "S0002544/A";

			Factory.Save();

			OrgWithAddressFilter orgWithAddressFilter;
			orgWithAddressFilter = ((OrgWithAddressFilter)FilterBO["Creditor and Address"]);
			orgWithAddressFilter.Organization = testOrg.PK;
			orgWithAddressFilter.Address = testAddress.PK;
			orgWithAddressFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice 2", testTransactions.Contains(testInvoice2.PK));

			orgWithAddressFilter.Address = ZGuid.Empty;
			testTransactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 2 invoice", 2, testTransactions.Count);
			Assert("Collection contains invoice 1", testTransactions.Contains(testInvoice1.PK));
			Assert("Collection contains invoice 2", testTransactions.Contains(testInvoice2.PK));

			orgWithAddressFilter.Address = testOrg.AddressForSendingARDocuments.PK;
			testTransactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice 1", testTransactions.Contains(testInvoice1.PK));
		}

		public void TestDocumentReceivedDateFilter()
		{
			DirectReceipt transaction1 = Factory.NewWithValidTestData<DirectReceipt>();
			ARPayment transaction2 = Factory.NewWithValidTestData<ARPayment>();

			transaction1.AH_DocumentReceivedDate = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			transaction2.AH_DocumentReceivedDate = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Document Received Date"];

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

		public void TestDueDateFilter()
		{
			DirectReceipt transaction1 = Factory.NewWithValidTestData<DirectReceipt>();
			ARPayment transaction2 = Factory.NewWithValidTestData<ARPayment>();

			transaction1.AH_DueDate = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			transaction2.AH_DueDate = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Due Date"];

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

		public void TestPostDateFilter()
		{
			DirectReceipt transaction1 = Factory.NewWithValidTestData<DirectReceipt>();
			ARPayment transaction2 = Factory.NewWithValidTestData<ARPayment>();

			transaction1.AH_PostDate = new ZDateTime(2000, 1, 1, 11, 0, 0);     // 2 Jan 11:00
			transaction2.AH_PostDate = new ZDateTime(2000, 2, 2, 22, 0, 0);     // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Post Date"];

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

		public void TestActiveStatusFilterNotShown()
		{
			TransactionsPendingAllocationModule module = new TransactionsPendingAllocationModule();

			try
			{
				AssertNull(((TransactionsPendingAllocationFilterBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject).ModuleFilters["Active Status"]);
			}
			finally
			{
				module.Dispose();
			}
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

		public void TestCreditorFilter()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			ARPayment transaction1 = Factory.NewWithValidTestData<ARPayment>();
			APReceipt transaction2 = Factory.NewWithValidTestData<APReceipt>();

			transaction1.AH_OH = organisation1.PK;
			transaction2.AH_OH = organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Creditor"];

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

		public void TestTransactionNumberFilter()
		{
			DirectReceipt transaction1 = Factory.NewWithValidTestData<DirectReceipt>();
			BankTransfer bt = new BankTransfer(Factory, null);
			BankTransferFromRow transaction2 = bt.TransferRowFrom;
			Factory.Save();

			bt.TransactionNumber = "121315";
			//BankTransfer transaction number will be reset when save.
			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Factory.Save();
			}

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Transaction Number"];

			filter.Property = "00001000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));

			filter.Property = "2345";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !FilterCollection.Contains(transaction1));
			Assert("Expecting collection not to contain Transaction2", !FilterCollection.Contains(transaction2));
		}

		public void TestComplianceSubTypeFilter()
		{
			var countryComplianceFactoryIntegrationMock = new Mock<ICountryComplianceFactoryIntegration>();
			countryComplianceFactoryIntegrationMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(null as IComplianceSubTypeCodeProvider);
			ObjectFactory.Substitute(countryComplianceFactoryIntegrationMock.Object);

			var filter = FilterBO["Compliance Sub Type"] as ModuleTextFilter;
			AssertNull("Compliance Sub Type Filter", filter);
		}

		public void TestComplianceSubTypeFilterWhenAvailable()
		{
			var fakeComplianceSubTypeList = new ComplianceSubTypeList { new ComplianceSubType("MOQ", () => (NoResString)"Mock Sub Type", () => "", () => "") };

			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(p => p.GetComplianceSubTypes()).Returns(fakeComplianceSubTypeList);

			var countryComplianceFactoryIntegrationMock = new Mock<ICountryComplianceFactoryIntegration>();
			countryComplianceFactoryIntegrationMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);
			ObjectFactory.Substitute(countryComplianceFactoryIntegrationMock.Object);

			var filter = FilterBO["Compliance Sub Type"] as ModuleTextFilter;
			AssertNotNull("Compliance Sub Type Filter", filter);

			var transactionPIN = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPIN.AH_ComplianceSubType = "PIN";

			var transactionPIC = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionPIC.AH_ComplianceSubType = "PIC";

			filter.IsActive = true;
			filter.Property = "MOQ";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain transactionPIN", !FilterCollection.Contains(transactionPIN));
			Assert("Expecting collection not to contain transactionPIC", !FilterCollection.Contains(transactionPIC));

			filter.Property = "PIN";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain transactionPIN", FilterCollection.Contains(transactionPIN));
			Assert("Expecting collection not to contain transactionPIC", !FilterCollection.Contains(transactionPIC));

			filter.Property = "PIC";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain transactionPIN", !FilterCollection.Contains(transactionPIN));
			Assert("Expecting collection to contain transactionPIC", FilterCollection.Contains(transactionPIC));
		}

		public void TestApprovalStatusFilterOverMostRecentApproval()
		{
			CreateTransactionWithRequests("JUSTCAN", "CAN");

			CreateTransactionWithRequests("ONLYREJ", "REJ");
			CreateTransactionWithRequests("ONLYAPP", "APP");
			CreateTransactionWithRequests("ONLYPST", "PST");
			CreateTransactionWithRequests("ONLYERR", "ERR");
			CreateTransactionWithRequests("ONLYARQ", "ARQ");
			CreateTransactionWithRequests("ONLYRRQ", "RRQ");
			CreateTransactionWithRequests("ONLYREQ", "REQ");

			CreateTransactionWithRequests("LASTREJ", "CAN", "REJ");
			CreateTransactionWithRequests("LASTAPP", "CAN", "APP");
			CreateTransactionWithRequests("LASTPST", "CAN", "PST");
			CreateTransactionWithRequests("LASTERR", "CAN", "ERR");
			CreateTransactionWithRequests("LASTARQ", "CAN", "ARQ");
			CreateTransactionWithRequests("LASTRRQ", "CAN", "RRQ");
			CreateTransactionWithRequests("LASTREQ", "CAN", "REQ");

			CreateTransactionWithRequests("TWICECN", "CAN", "CAN", "REQ");
			CreateTransactionWithRequests("THRICEC", "CAN", "CAN", "CAN", "REQ");

			Factory.Save();

			AssertResultCount(SQLComparisonOperator.Equal, "CAN", 1);
			AssertResultCount(SQLComparisonOperator.Equal, "REJ", 2);
			AssertResultCount(SQLComparisonOperator.Equal, "APP", 2);
			AssertResultCount(SQLComparisonOperator.Equal, "PST", 2);
			AssertResultCount(SQLComparisonOperator.Equal, "ERR", 2);
			AssertResultCount(SQLComparisonOperator.Equal, "ARQ", 2);
			AssertResultCount(SQLComparisonOperator.Equal, "RRQ", 2);
			AssertResultCount(SQLComparisonOperator.Equal, "REQ", 4);

			AssertResultCount(SQLComparisonOperator.NotEqual, "REQ", 13);
			AssertResultCount(SQLComparisonOperator.NotEqual, "REJ", 15);
			AssertResultCount(SQLComparisonOperator.NotEqual, "APP", 15);
			AssertResultCount(SQLComparisonOperator.NotEqual, "PST", 15);
			AssertResultCount(SQLComparisonOperator.NotEqual, "ERR", 15);
			AssertResultCount(SQLComparisonOperator.NotEqual, "ARQ", 15);
			AssertResultCount(SQLComparisonOperator.NotEqual, "RRQ", 15);
			AssertResultCount(SQLComparisonOperator.NotEqual, "CAN", 16);

			AssertResultCount(SQLComparisonOperator.NotEqual, "HUH", 17);

			void AssertResultCount(SQLComparisonOperator equal, string status, int expectedCount)
			{
				var filter = FilterBO["Approval Status"] as ModuleTextFilter;
				AssertNotNull("Approval Status Filter", filter);

				filter.IsActive = true;
				filter.SqlComparisonOperator = equal;
				filter.Property = status;

				FilterCollection.Load(FilterBO.Filter);

				var found = string.Join(", ", FilterCollection.Select(tpa => tpa.AH_JobNumber).ToArray());

				AssertEquals($"Expecting correct filtering for {equal} {status} but found: {found}", expectedCount, FilterCollection.Count);
			}

			TransactionPendingAllocation CreateTransactionWithRequests(string jobNumber, params string[] approvalStatusList)
			{
				var creationTime = ZDateTime.Now.AddHours(-1);

				var transaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();
				transaction.AH_JobNumber = jobNumber;

				foreach (var approvalStatus in approvalStatusList)
				{
					var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
					request.Initialize(transaction);
					request.XP_ApprovalStatus = approvalStatus;
					request.XP_SystemCreateTimeUtc = creationTime = creationTime.AddMinutes(1);
				}

				return transaction;
			}
		}

		TransactionsPendingAllocationFilterBusinessObject FilterBO;
		TransactionHeaderCollection FilterCollection;

		protected override void SetUp()
		{
			base.SetUp();
			FilterCollection = new TransactionHeaderCollection(Factory);
			FilterBO = (TransactionsPendingAllocationFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
