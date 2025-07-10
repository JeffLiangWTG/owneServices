using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class CashAdvanceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCashAdvanceRequestLinesAreNotLoaded()
		{
			FilterCollection.Load(FilterBO.Filter);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertEquals(0, CashAdvanceRequestHeader1.Lines.Count);
		}

		public void TestCashAdvanceRequestLinesAreLoaded()
		{
			AccCashAdvanceRequestLine line1;
			line1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			CashAdvanceRequestHeader1.Lines.Add(line1);
			FilterCollection.Load(FilterBO.Filter);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertEquals(1, CashAdvanceRequestHeader1.Lines.Count);
		}

		public void TestLoadOnlyCurrentCompany()
		{
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);
		}

		#region Organisation/Staff Filter Tests		

		public void TestOrganisationListProperty()
		{
			AssertNotNull(FilterBO.CAH_OH_OrganizationList);
		}

		public void TestOrganisationFilter()
		{
			CashAdvanceRequestHeader1.CAH_OH_Organization = Organisation1.PK;
			CashAdvanceRequestHeader2.CAH_OH_Organization = Organisation1.PK;
			CashAdvanceRequestHeader3.CAH_OH_Organization = Organisation2.PK;

			Factory.Save();

			SetupOrgFilter();

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);
		}

		protected virtual void SetupOrgFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Organisation"];
			filter.Property = Organisation1.PK;
			filter.IsActive = true;
		}
		#endregion

		#region Currency Filter Tests	

		public void TestCurrencyListProperty()
		{
			AssertNotNull(FilterBO.CAH_RXList);
		}

		public void TestCurrencyFilter()
		{
			CashAdvanceRequestHeader1.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			CashAdvanceRequestHeader2.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.India;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Currency"];

			filter.Property = Core.Constants.CurrencyCodes.Australia;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader2, FilterCollection);

			filter.Property = Core.Constants.CurrencyCodes.India;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader2, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader1, FilterCollection);
		}

		#endregion

		#region OS Amount Filter Tests

		public virtual void TestOSAmountFilter()
		{
			CashAdvanceRequestHeader3.CAH_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["OS Amount"];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader2, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);

			filter.Property1 = 100.0;
			filter.Property2 = 150.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertCollectionNotContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader2, CashAdvanceRequestHeader3 }, FilterCollection);
		}

		#endregion

		#region Status Filter Tests

		public void TestStatusListProperty()
		{
			AssertNotNull(FilterBO.CAH_StatusList);
			var codeList = FilterBO.CAH_StatusList.GetAllCodes();

			AssertCollectionNotContains("Expecting list to contain Pending item", CashAdvanceStatusCodes.RequestHeader.Pending, codeList);
			AssertCollectionContains("Expecting list to contain Requested item", CashAdvanceStatusCodes.RequestHeader.Requested, codeList);
			AssertCollectionContains("Expecting list to contain Paid item", CashAdvanceStatusCodes.RequestHeader.Paid, codeList);
			AssertCollectionNotContains("Expecting list to contain PartiallyPaid item", CashAdvanceStatusCodes.RequestHeader.PartiallyPaid, codeList);
			AssertCollectionContains("Expecting list to contain Invoiced item", CashAdvanceStatusCodes.RequestHeader.Invoiced, codeList);
			AssertCollectionContains("Expecting list to contain PartiallyInvoiced item", CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, codeList);
			AssertCollectionContains("Expecting list to contain Cancelled item", CashAdvanceStatusCodes.RequestHeader.Cancelled, codeList);
		}

		public virtual void TestStatusFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Status"];

			filter.Property = CashAdvanceStatusCodes.RequestHeader.Requested;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);

			filter.Property = CashAdvanceStatusCodes.RequestHeader.Invoiced;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		#endregion

		#region Number and References Filter Tests

		public void TestCashAdvanceNumberFilter()
		{
			CashAdvanceRequestHeader1.CAH_RequestReferenceNumber = "00001000";
			CashAdvanceRequestHeader2.CAH_RequestReferenceNumber = "00001001";
			CashAdvanceRequestHeader3.CAH_RequestReferenceNumber = "00011001";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Advance Payment #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "0000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "00001000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1001";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionNotContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertCollectionContains(CashAdvanceRequestHeader2, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		public void TestJobNumberFilter()
		{
			CashAdvanceRequestHeader1.CAH_JH_Job = Job1.PK;
			CashAdvanceRequestHeader2.CAH_JH_Job = Job2.PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Job #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "S000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "S00001000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "0000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "1234";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);
		}

		#endregion

		#region Branch and Department Filter Tests

		public void TestCashAdvanceJobBranchFilter()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			Job1.JH_GB = branch1.PK;
			Job2.JH_GB = branch2.PK;

			CashAdvanceRequestHeader1.CAH_JH_Job = Job1.PK;
			CashAdvanceRequestHeader2.CAH_JH_Job = Job2.PK;

			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Branch"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZGuid.NewZGuid();
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Job1.JH_GB;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1 }, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Job2.JH_GB;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals(1, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader2 }, FilterCollection);
		}

		public void TestCashAdvanceJobDepartmentFilter()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Job1.JH_GE = department1.PK;
			Job2.JH_GE = department2.PK;

			CashAdvanceRequestHeader1.CAH_JH_Job = Job1.PK;
			CashAdvanceRequestHeader2.CAH_JH_Job = Job2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Department"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZGuid.NewZGuid();
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(0, FilterCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Job1.JH_GE;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1 }, FilterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Job2.JH_GE;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader2 }, FilterCollection);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			Organisation1 = ObjectCreator.CreateOrgHeader("Org1", true, true);
			Organisation2 = ObjectCreator.CreateOrgHeader("Org2", true, true);

			Job1 = ObjectCreator.CreateJob("S00001000", null, 0, null, 0);
			Job2 = ObjectCreator.CreateJob("S00001111", null, 0, null, 0);

			CashAdvanceRequestHeader1 = ObjectCreator.CreateCashAdvanceRequestHeader(Job1.PK, ObjectCreator.LocalClient.PK, LedgerType, 10m, 10m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader2 = ObjectCreator.CreateCashAdvanceRequestHeader(Job2.PK, ObjectCreator.LocalClient.PK, LedgerType, 100m, 100m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			CashAdvanceRequestHeader3 = ObjectCreator.CreateCashAdvanceRequestHeader(Job2.PK, ObjectCreator.LocalClient.PK, LedgerType, 150m, 150m, ObjectCreator.AUD.Code, CashAdvanceStatusCodes.RequestHeader.Paid);

			var newCompany = ObjectCreator.CreateNewCompany("NEW");
			CashAdvanceRequestHeader3.CAH_GC_Company = newCompany.PK;

			FilterCollection = new AccCashAdvanceRequestHeaderCollection(Factory);
			FilterBO = (CashAdvanceFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		protected TestObjectCreator ObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected AccCashAdvanceRequestHeaderCollection FilterCollection;
		protected CashAdvanceFilterBusinessObject FilterBO;
		protected AccCashAdvanceRequestHeader CashAdvanceRequestHeader1;
		protected AccCashAdvanceRequestHeader CashAdvanceRequestHeader2;
		protected AccCashAdvanceRequestHeader CashAdvanceRequestHeader3;
		protected OrgHeader Organisation1;
		protected OrgHeader Organisation2;
		protected JobHeader Job1;
		protected JobHeader Job2;

		protected abstract string LedgerType { get; set; }
	}
}
