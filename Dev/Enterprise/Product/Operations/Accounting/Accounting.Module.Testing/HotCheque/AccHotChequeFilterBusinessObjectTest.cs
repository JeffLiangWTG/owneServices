using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccHotChequeFilterBusinessObject))]
	public class AccHotChequeFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Number Filter Tests

		public void TestMasterBillNumberFilter()
		{
			Cheque1.AQ_MasterBill = "11100011";
			Cheque2.AQ_MasterBill = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Master Bill #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestHouseBillNumberFilter()
		{
			Cheque1.AQ_HouseBill = "11100011";
			Cheque2.AQ_HouseBill = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["House Bill #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestChequeNumberFilter()
		{
			Cheque1.AQ_ChequeNumber = "11100011";
			Cheque2.AQ_ChequeNumber = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Check #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		#endregion

		#region Reference Filter Tests

		public void TestCreditorFilter()
		{
			Cheque1.AQ_OH = Organisation1.PK;
			Cheque2.AQ_OH = Organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Creditor"];

			filter.Property = Organisation1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property = Organisation3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestStaffFilter()
		{
			Cheque1.AQ_GS_NKResponsibleStaff = Staff1.GS_Code;
			Cheque2.AQ_GS_NKResponsibleStaff = Staff2.GS_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Staff"];

			filter.Property = Staff1.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property = Staff3.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestLocalJobReferenceFilter()
		{
			Cheque1.AQ_JH = Job1.PK;
			Cheque2.AQ_JH = Job2.PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Job Local Reference"];

			filter.Property = Job1.JH_JobLocalReference;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property = "Denys";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestJobFilter()
		{
			Cheque1.AQ_JH = Job1.PK;
			Cheque2.AQ_JH = Job2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job"];

			AssertEquals("Job header list", typeof(JobHeaderCollection), filter.List.GetType());

			filter.Property = Job1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property = Job3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestCurrencyFilter()
		{
			Account1.AB_RX_NKAccountCurrency = Currency1.RX_Code;
			Account2.AB_RX_NKAccountCurrency = Currency2.RX_Code;

			ChequeBook1.AK_AB = Account1.PK;
			ChequeBook2.AK_AB = Account2.PK;

			Cheque1.AQ_AK = ChequeBook1.PK;
			Cheque2.AQ_AK = ChequeBook2.PK;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Currency"];

			filter.Property = Currency1.RX_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property = Currency3.RX_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		#endregion

		#region Other Filters Tests

		public void TestAmountFilter()
		{
			Cheque1.AQ_Amount = 10.0;
			Cheque2.AQ_Amount = 100.0;

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["Amount"];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expected collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property1 = 10.0;
			filter.Property2 = 100.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expected collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.Property1 = 20.0;
			filter.Property2 = 90.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expected collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.Property1 = 200.0;
			filter.Property2 = 300.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expected collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestChequePayeeFilter()
		{
			Cheque1.AQ_ChequePayee = "AAAXXXAA";
			Cheque2.AQ_ChequePayee = "BBXXXBBB";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Check Payee"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "AAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BBXXXBBB";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "XXX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "YYY";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
		}

		public void TestStatusFilter()
		{
			Cheque1.AQ_Cancelled = ZBool.True;
			Cheque2.AQ_Cancelled = ZBool.False;
			Cheque3.AQ_Cancelled = ZBool.False;

			Cheque1.AQ_AH = Transaction1.PK;
			Cheque2.AQ_AH = Transaction2.PK;
			Cheque3.AQ_AH = ZGuid.Empty;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Status"];

			filter.Property = "ACTIVE";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Cheque1", !FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
			Assert("Expecting collection to contain Cheque3", FilterCollection.Contains(Cheque3));

			filter.Property = "POSTED";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection to contain Cheque2", FilterCollection.Contains(Cheque2));
			Assert("Expecting collection not to contain Cheque3", !FilterCollection.Contains(Cheque3));

			filter.Property = "CANCELLED";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Cheque1", FilterCollection.Contains(Cheque1));
			Assert("Expecting collection not to contain Cheque2", !FilterCollection.Contains(Cheque2));
			Assert("Expecting collection not to contain Cheque3", !FilterCollection.Contains(Cheque3));
		}

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccHotChequeFilterBusinessObject();
		}

		AccHotCheque Cheque1;
		AccHotCheque Cheque2;
		AccHotCheque Cheque3;

		OrgHeader Organisation1;
		OrgHeader Organisation2;
		OrgHeader Organisation3;

		GlbStaff Staff1;
		GlbStaff Staff2;
		GlbStaff Staff3;

		JobHeader Job1;
		JobHeader Job2;
		JobHeader Job3;

		AccChequeBook ChequeBook1;
		AccChequeBook ChequeBook2;
		AccChequeBook ChequeBook3;

		AccBankAccount Account1;
		AccBankAccount Account2;

		RefCurrency Currency1;
		RefCurrency Currency2;
		RefCurrency Currency3;

		AccTransactionHeader Transaction1;
		AccTransactionHeader Transaction2;

		AccHotChequeCollection FilterCollection;
		AccHotChequeFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Cheque1 = Factory.NewWithValidTestData<AccHotCheque>();
			Cheque2 = Factory.NewWithValidTestData<AccHotCheque>();
			Cheque3 = Factory.NewWithValidTestData<AccHotCheque>();

			ChequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook3 = Factory.NewWithValidTestData<AccChequeBook>();

			ChequeBook1.AK_GB = GlbBranch.CurrentBranch.PK;
			ChequeBook2.AK_GB = GlbBranch.CurrentBranch.PK;
			ChequeBook3.AK_GB = GlbBranch.CurrentBranch.PK;

			Cheque1.AQ_AK = ChequeBook1.PK;
			Cheque2.AQ_AK = ChequeBook2.PK;
			Cheque3.AQ_AK = ChequeBook3.PK;

			Account1 = Factory.NewWithValidTestData<AccBankAccount>();
			Account2 = Factory.NewWithValidTestData<AccBankAccount>();

			Currency1 = Factory.NewWithValidTestData<RefCurrency>();
			Currency2 = Factory.NewWithValidTestData<RefCurrency>();
			Currency3 = Factory.NewWithValidTestData<RefCurrency>();

			Transaction1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			Transaction2 = Factory.NewWithValidTestData<AccTransactionHeader>();

			Organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			Staff1 = Factory.NewWithValidTestData<GlbStaff>();
			Staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Staff3 = Factory.NewWithValidTestData<GlbStaff>();

			Job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			FilterCollection = new AccHotChequeCollection(Factory);
			FilterBO = (AccHotChequeFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
