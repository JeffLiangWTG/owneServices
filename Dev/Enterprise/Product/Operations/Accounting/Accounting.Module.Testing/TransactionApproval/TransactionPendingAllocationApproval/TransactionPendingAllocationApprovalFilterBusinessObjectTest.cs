using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalFilterBusinessObject))]
	public class TransactionPendingAllocationApprovalFilterBusinessObjectTest : TransactionApprovalFilterBusinessObjectTest
	{
		FilterStripBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();
			FilterBO = GetNewFilterStripBusinessObject();
		}

		public override void TestApprovalStatusFilterList()
		{
			var expectedList = ExpectedList;
			AssertApprovalStatusList(expectedList);
		}

		public void TestApprovalStatusFilterList_WhenITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProviderImplemented()
		{
			var expectedList = ExpectedList;
			expectedList.AddPair(Constants.GenApprovalRequestApprovalStatus.ApprovalRequested, "Approval Requested");
			expectedList.AddPair(Constants.GenApprovalRequestApprovalStatus.RejectionRequested, "Rejection Requested");

			TestObjectCreator.MockCountryFactoryForTPAAeInvoicing();
			AssertApprovalStatusList(expectedList);
		}

		void AssertApprovalStatusList(CodeDescriptionPairList expectedList)
		{
			var filter = FilterBO["Approval Status"] as ModuleTextFilter;
			var list = (ICodeDescriptionPairList)filter.List;
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TransactionPendingAllocationApprovalFilterBusinessObject();

		public static CodeDescriptionPairList ExpectedList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Requested, "Requested");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Cancelled, "Canceled");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Rejected, "Rejected");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Approved, "Approved");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Posted, "Posted");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Error, "Validation Errors");

				return list;
			}
		}

		(TransactionPendingAllocationApprovalRequest firstRequest, TransactionPendingAllocationApprovalRequest secondRequest) CreateTestObjectsForFilterTests()
		{
			var transactionABC = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionABC.AH_TransactionNum = "ABC1234567";
			transactionABC.AH_ComplianceSubType = "ABC";
			transactionABC.AH_OH = TestObjectCreator.Creditor1.PK;
			transactionABC.AH_InvoiceDate = new ZDate(2023, 3, 15);

			var requestABC = Factory.New<TransactionPendingAllocationApprovalRequest>();
			requestABC.Initialize(transactionABC);

			var transactionXYZ = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			transactionXYZ.AH_TransactionNum = "XYZ9876543";
			transactionXYZ.AH_ComplianceSubType = "XYZ";
			transactionXYZ.AH_OH = TestObjectCreator.Creditor2.PK;
			transactionXYZ.AH_InvoiceDate = new ZDate(2023, 9, 15);

			var requestXYZ = Factory.New<TransactionPendingAllocationApprovalRequest>();
			requestXYZ.Initialize(transactionXYZ);

			Factory.Save();

			return (requestABC, requestXYZ);
		}

		public void TestComplianceSubTypeFilterWhenNotSupported()
		{
			var countryComplianceFactoryIntegrationMock = new Mock<ICountryComplianceFactoryIntegration>();
			countryComplianceFactoryIntegrationMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(null as IComplianceSubTypeCodeProvider);
			ObjectFactory.Substitute(countryComplianceFactoryIntegrationMock.Object);

			var complianceSubTypefilter = FilterBO["Compliance Sub Type"] as ModuleTextFilter;
			AssertNull("Compliance Sub Type Filter", complianceSubTypefilter);
		}

		public void TestComplianceSubTypeFilter()
		{
			var fakeComplianceSubTypeList = new ComplianceSubTypeList { new ComplianceSubType("MOQ", () => (NoResString)"Mock Sub Type", () => "", () => "") };

			var complianceSubTypeCodeProviderMock = new Mock<IComplianceSubTypeCodeProvider>();
			complianceSubTypeCodeProviderMock.Setup(p => p.GetComplianceSubTypes()).Returns(fakeComplianceSubTypeList);

			var countryComplianceFactoryIntegrationMock = new Mock<ICountryComplianceFactoryIntegration>();
			countryComplianceFactoryIntegrationMock.Setup(c => c.GetIComplianceSubTypeCodeProvider(It.IsAny<ZString>())).Returns(complianceSubTypeCodeProviderMock.Object);
			ObjectFactory.Substitute(countryComplianceFactoryIntegrationMock.Object);

			var complianceSubTypefilter = FilterBO["Compliance Sub Type"] as ModuleTextFilter;
			AssertNotNull("Compliance Sub Type Filter", complianceSubTypefilter);
			AssertNotNull("Compliance Sub Type Filter Lookup List", complianceSubTypefilter.List as CodeDescriptionPairList);
			AssertEquals("Compliance Sub Type Filter Lookup List contents", "MOQ", ((CodeDescriptionPairList)complianceSubTypefilter.List).CodesAsString);

			var (requestABC, requestXYZ) = CreateTestObjectsForFilterTests();

			complianceSubTypefilter.IsActive = true;
			complianceSubTypefilter.Property = "MOQ";
			var filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			AssertEquals("Expecting collection not to contain anything", 0, filteredCollection.Count);

			complianceSubTypefilter.Property = "ABC";
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection to contain requestABC", filteredCollection.Contains(requestABC));
			Assert("Expecting collection not to contain requestXYZ", !filteredCollection.Contains(requestXYZ));

			complianceSubTypefilter.Property = "XYZ";
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection not to contain requestABC", !filteredCollection.Contains(requestABC));
			Assert("Expecting collection to contain requestXYZ", filteredCollection.Contains(requestXYZ));
		}

		public void TestCreditorFilter()
		{
			var creditorFilter = FilterBO["Creditor"] as ModuleGuidFilter;
			AssertNotNull("Creditor Filter", creditorFilter);

			var (requestABC, requestXYZ) = CreateTestObjectsForFilterTests();

			creditorFilter.IsActive = true;
			creditorFilter.Property = ZGuid.Empty;
			var filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			AssertEquals("Expecting collection to contain everything", 2, filteredCollection.Count);

			creditorFilter.Property = TestObjectCreator.Creditor1.PK;
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection to contain requestABC", filteredCollection.Contains(requestABC));
			Assert("Expecting collection not to contain requestXYZ", !filteredCollection.Contains(requestXYZ));

			creditorFilter.Property = TestObjectCreator.Creditor2.PK;
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection not to contain requestABC", !filteredCollection.Contains(requestABC));
			Assert("Expecting collection to contain requestXYZ", filteredCollection.Contains(requestXYZ));

			creditorFilter.Property = TestObjectCreator.Creditor6.PK;
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			AssertEquals("Expecting collection not to contain anything", 0, filteredCollection.Count);
		}

		public void TestInvoiceDateFilter()
		{
			var invoiceDateFilter = FilterBO["Invoice Date"] as ModuleDateFilter;
			AssertNotNull("Invoice Date Filter", invoiceDateFilter);

			var (requestABC, requestXYZ) = CreateTestObjectsForFilterTests();

			invoiceDateFilter.IsActive = true;
			invoiceDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			invoiceDateFilter.Property1 = new ZDateTime(1976, 7, 23);
			invoiceDateFilter.Property2 = new ZDateTime(2023, 3, 14);
			var filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			AssertEquals("Expecting collection not to contain anything", 0, filteredCollection.Count);

			invoiceDateFilter.Property2 = new ZDateTime(2023, 12, 15);
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			AssertEquals("Expecting collection to contain everything", 2, filteredCollection.Count);

			invoiceDateFilter.Property1 = new ZDateTime(2023, 6, 15);
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection not to contain requestABC", !filteredCollection.Contains(requestABC));
			Assert("Expecting collection to contain requestXYZ", filteredCollection.Contains(requestXYZ));

			invoiceDateFilter.Property1 = new ZDateTime(2023, 1, 15);
			invoiceDateFilter.Property2 = new ZDateTime(2023, 6, 15);
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection to contain requestABC", filteredCollection.Contains(requestABC));
			Assert("Expecting collection not to contain requestXYZ", !filteredCollection.Contains(requestXYZ));
		}

		public void TestTransactionNumberFilter()
		{
			var transactionNumberfilter = FilterBO["Transaction Number"] as ModuleTextFilter;
			AssertNotNull("Transaction Number Filter", transactionNumberfilter);

			var (requestABC, requestXYZ) = CreateTestObjectsForFilterTests();

			transactionNumberfilter.IsActive = true;
			transactionNumberfilter.Property = "NOP4567890";
			var filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			AssertEquals("Expecting collection not to contain anything", 0, filteredCollection.Count);

			transactionNumberfilter.Property = "ABC1234567";
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection to contain requestABC", filteredCollection.Contains(requestABC));
			Assert("Expecting collection not to contain requestXYZ", !filteredCollection.Contains(requestXYZ));

			transactionNumberfilter.Property = "XYZ9876543";
			filteredCollection = new TransactionPendingAllocationApprovalRequestCollection(Factory, FilterBO.Filter);
			Assert("Expecting collection not to contain requestABC", !filteredCollection.Contains(requestABC));
			Assert("Expecting collection to contain requestXYZ", filteredCollection.Contains(requestXYZ));
		}
	}
}
