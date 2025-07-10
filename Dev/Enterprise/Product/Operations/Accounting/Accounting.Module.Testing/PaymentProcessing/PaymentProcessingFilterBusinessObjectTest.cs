using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class PaymentProcessingFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestPaymentApprovalReferenceFilter()
		{
			var approval1InCurrentCompany = CreatePaymentApproval();
			var approval2InCurrentCompany = CreatePaymentApproval();
			Factory.Save();

			PaymentApprovalWithAuthorisation approval1InNonCurrentCompany;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				approval1InNonCurrentCompany = CreatePaymentApproval();
				approval1InNonCurrentCompany.AV_GC = TestObjectCreator.NonCurrentCompany.PK;
				approval1InNonCurrentCompany.AV_GB = TestObjectCreator.NonCurrentCompany.FirstActiveBranch.PK;
				Factory.Save();
			}

			AssertEquals("Precondition", approval1InCurrentCompany.AV_PaymentApprovalReference, approval1InNonCurrentCompany.AV_PaymentApprovalReference);

			var filter = (ModuleNumberFilter)FilterBO["Payment Approval Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = approval1InCurrentCompany.AV_PaymentApprovalReference;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Filter collection must contain approval1InCurrentCompany", FilterCollection.Contains(approval1InCurrentCompany));
			Assert("Filter collection must not contain approval2InCurrentCompany", !FilterCollection.Contains(approval2InCurrentCompany));
			Assert("Filter collection must not contain approval1InNonCurrentCompany", !FilterCollection.Contains(approval1InNonCurrentCompany));
		}

		protected abstract Type GetPaymentApprovalType();

		PaymentApprovalWithAuthorisation CreatePaymentApproval()
		{
			var approval = Factory.New(GetPaymentApprovalType()) as PaymentApprovalWithAuthorisation;
			approval.AV_GB = GlbBranch.CurrentBranch.PK;
			approval.AV_GC = GlbCompany.CurrentCompany.PK;
			approval.AV_PostDate = ZDateTime.Today;
			approval.AV_OH = TestObjectCreator.AALSHI.PK;
			approval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval.AV_Amount = 300m;
			return approval;
		}

		public void TestBranchFilter()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

			Approval1.AV_GB = branch.PK;

			Approval1.AV_ChequeOrReference = "AXXXA";
			Approval2.AV_ChequeOrReference = "BXXXB";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Check or Reference"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "XXX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));
		}

		public void TestPaymentRejectionReasonFilter()
		{
			Approval1.AV_RejectionReasonCode = "ICH";
			Approval2.AV_RejectionReasonCode = "IRA";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO["Rejection Reason"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ICH";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to NOT contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestCheckBookBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			AccChequeBook chequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook1.AK_GB = branch1.PK;
			AccChequeBook chequeBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook2.AK_GB = branch2.PK;

			Approval1.AV_AK = chequeBook1.PK;
			Approval2.AV_AK = chequeBook2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Check Book Branch"];
			filter.Property = branch2.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));
		}

		public void TestLedgerCodeFilter()
		{
			Approval1.AV_Ledger = "YY";

			Approval1.AV_ChequeOrReference = "AXXXA";
			Approval2.AV_ChequeOrReference = "BXXXB";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Check or Reference"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "XXX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));
		}

		#region EPayment Filter Tests

		public void TestEPaymentFilterAvailability()
		{
			AssertEPaymentFilterAvailability(true);
			AssertEPaymentFilterAvailability(false);
		}

		void AssertEPaymentFilterAvailability(bool isOFXEPaymentEnabled)
		{
			var ePaymentFilterNames = new string[] { "E-Payment Status", "E-Payment Last Response Received Date", "E-Payment Provider Reference", "E-Payment Submitted Date" };

			using (Business.TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			{
				foreach (var filterName in ePaymentFilterNames)
				{
					var filter = FilterBO.GetModuleFiltersCore_ForTestOnly()[filterName];
					if (isOFXEPaymentEnabled)
					{
						AssertNotNull($"{filterName} Filter should be available when E-Payment functionality is enabled.", filter);
					}
					else
					{
						AssertNull($"{filterName} Filter should not be available when E-Payment functionality is disabled.", filter);
					}
				}
			}
		}

		public void TestEPaymentStatusFilter()
		{
			using (Business.TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				PrepareEPaymentDealData();

				Factory.Save();

				var filter = (ModuleTextFilter)FilterBO["E-Payment Status"];

				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "ACP";
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);
				Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
				Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "DEC";
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);
				Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
				Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
			}
		}

		public void TestEPaymentLastResponseReceivedDateFilter()
		{
			using (Business.TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				PrepareEPaymentDealData();
				Factory.Save();

				((PaymentApprovalBase)Approval1).CurrentDeal.AED_LastResponseReceivedUtc = new ZDateTime(2021, 6, 10);
				((PaymentApprovalBase)Approval2).CurrentDeal.AED_LastResponseReceivedUtc = new ZDateTime(2021, 6, 20);
				Factory.Save();

				var filter = (ModuleDateFilter)FilterBO["E-Payment Last Response Received Date"];

				filter.Property1 = new ZDateTime(2021, 6, 8);
				filter.Property2 = new ZDateTime(2021, 6, 18);
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);

				Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
				Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
			}
		}

		public void TestEPaymentProviderReferenceFilter()
		{
			using (Business.TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				PrepareEPaymentDealData();
				Factory.Save();

				((PaymentApprovalBase)Approval1).CurrentDeal.AED_ProviderReference = "1000";
				((PaymentApprovalBase)Approval2).CurrentDeal.AED_ProviderReference = "1005";
				Factory.Save();

				var filter = (ModuleTextFilter)FilterBO["E-Payment Provider Reference"];

				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "1000";
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);

				Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
				Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
			}
		}

		public void TestEPaymentSubmittedDateFilter()
		{
			using (Business.TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				PrepareEPaymentDealData();
				Factory.Save();

				Factory.Save();

				var filter = (ModuleDateFilter)FilterBO["E-Payment Submitted Date"];

				filter.Property1 = new ZDateTime(2021, 6, 1);
				filter.Property2 = new ZDateTime(2021, 6, 9);
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);

				Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
				Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));
			}
		}

		void PrepareEPaymentDealData()
		{
			Approval1 = TestObjectCreator.CreatePaymentApproval(GetPaymentApprovalType(), ReceiptTypes.EFT, Account1, ChequeBook1);
			Approval1.AV_Amount = 60m;

			var deal1_1 = TestObjectCreator.CreateEPaymentDeal(Approval1 as PaymentApprovalBase);
			deal1_1.AED_Status = "DEC";
			deal1_1.AED_LastResponseReceivedUtc = new ZDateTime(2021, 6, 8);
			deal1_1.AED_SystemCreateTimeUtc = new ZDateTime(2021, 6, 8);

			var deal1_2 = TestObjectCreator.CreateEPaymentDeal(Approval1 as PaymentApprovalBase);
			deal1_2.AED_Status = "ACP";
			deal1_2.AED_SystemCreateTimeUtc = new ZDateTime(2021, 6, 10);
			deal1_2.AED_LastResponseReceivedUtc = new ZDateTime(2021, 6, 9);
			deal1_2.AED_ProviderReference = "1000";

			Approval2 = TestObjectCreator.CreatePaymentApproval(GetPaymentApprovalType(), ReceiptTypes.EFT, Account1, ChequeBook1);
			Approval2.AV_Amount = 80m;

			var deal2 = TestObjectCreator.CreateEPaymentDeal(Approval2 as PaymentApprovalBase);
			deal2.AED_Status = "FAL";
			deal2.AED_LastResponseReceivedUtc = new ZDateTime(2021, 6, 10);
			deal2.AED_SystemCreateTimeUtc = new ZDateTime(2021, 6, 5);
			deal2.AED_ProviderReference = "1000";
		}

		#endregion

		#region Number Filter Tests

		public void TestChequeOrReferenceFilter()
		{
			Approval1.AV_ChequeOrReference = "AXXXA";
			Approval2.AV_ChequeOrReference = "BXXXB";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Check or Reference"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "AXX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "BXXXB";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "XXX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		#endregion

		#region Status Filter Tests

		public void TestStatusListProperty()
		{
			AssertNotNull(FilterBO.AV_StatusList);
			var codeList = FilterBO.AV_StatusList.ToArray().Select(x => x.Code);

			AssertCollectionContains("Expecting list to contain AwaitingApproval item", PaymentApprovalStatus.AwaitingApproval, codeList);
			AssertCollectionContains("Expecting list to contain FullyApproved item", PaymentApprovalStatus.FullyApproved, codeList);
			AssertCollectionContains("Expecting list to contain Posted item", PaymentApprovalStatus.Posted, codeList);
			AssertCollectionContains("Expecting list to contain Rejected item", PaymentApprovalStatus.Rejected, codeList);
		}

		public void TestStatusFilter()
		{
			Approval1.AV_Status = ZArchitecture.Core.PaymentApprovalStatus.AwaitingApproval;
			Approval2.AV_Status = ZArchitecture.Core.PaymentApprovalStatus.FullyApproved;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Status"];

			filter.Property = ZArchitecture.Core.PaymentApprovalStatus.FullyApproved;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));

			filter.Property = ZArchitecture.Core.PaymentApprovalStatus.Posted;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		#endregion

		#region Organisation/Staff Filter Tests

		public void TestOHListProperty()
		{
			AssertNotNull(FilterBO.AV_OHList);
		}

		public void TestOrganisationFilter()
		{
			Approval1.AV_OH = Organisation1.PK;
			Approval2.AV_OH = Organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO[FilterBO.OrganisationFilterName_ForTestOnly];

			filter.Property = Organisation1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Organisation3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestFirstApprovalListProperty()
		{
			AssertNotNull(FilterBO.AV_GS_FirstApprovalList);
		}

		public void TestFirstApprovalFilter()
		{
			Approval1.AV_GS_NKApproval1st = Resource1.GS_Code;
			Approval2.AV_GS_NKApproval1st = Resource2.GS_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["First Approval"];

			filter.Property = Resource1.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Resource3.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestSecondApprovalListProperty()
		{
			AssertNotNull(FilterBO.AV_GS_SecondApprovalList);
		}

		public void TestSecondApprovalFilter()
		{
			Approval1.AV_GS_NKApproval2nd = Resource1.GS_Code;
			Approval2.AV_GS_NKApproval2nd = Resource2.GS_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Second Approval"];

			filter.Property = Resource1.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Resource3.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestThirdApprovalListProperty()
		{
			AssertNotNull(FilterBO.AV_GS_ThirdApprovalList);
		}

		public void TestThirdApprovalFilter()
		{
			Approval1.AV_GS_NKApproval3rd = Resource1.GS_Code;
			Approval2.AV_GS_NKApproval3rd = Resource2.GS_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Third Approval"];

			filter.Property = Resource1.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Resource3.GS_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		#endregion

		#region Other Filter Tests

		public void TestABListProperty()
		{
			AssertNotNull(FilterBO.AV_ABList);
		}

		public void TestBankAccountFilter()
		{
			Approval1.AV_AB = Account1.PK;
			Approval2.AV_AB = Account2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Bank"];

			filter.Property = Account1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Account3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestAKListProperty()
		{
			AssertNotNull(FilterBO.AV_AKList);
		}

		public void TestChequeBookFilter()
		{
			Approval1.AV_AK = ChequeBook1.PK;
			Approval2.AV_AK = ChequeBook2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Check"];

			filter.Property = ChequeBook1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = ChequeBook3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestRXListProperty()
		{
			AssertNotNull(FilterBO.AV_RXList);
		}

		public void TestCurrencyFilter()
		{
			Approval1.AV_RX_NKPaymentCurrency = Currency1.RX_Code;
			Approval2.AV_RX_NKPaymentCurrency = Currency2.RX_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterBO["Currency"];

			filter.Property = Currency1.RX_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property = Currency3.RX_Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestAmountFilter()
		{
			Approval1.AV_Amount = 10.0;
			Approval2.AV_Amount = 100.0;

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["Amount"];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property1 = 10.0;
			filter.Property2 = 100.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Approval1", FilterCollection.Contains(Approval1));
			Assert("Expecting collection to contain Approval2", FilterCollection.Contains(Approval2));

			filter.Property1 = 20.0;
			filter.Property2 = 90.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));

			filter.Property1 = 200.0;
			filter.Property2 = 300.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Approval1", !FilterCollection.Contains(Approval1));
			Assert("Expecting collection not to contain Approval2", !FilterCollection.Contains(Approval2));
		}

		public void TestOrgAndAddressFilter()
		{
			Approval1.AV_OH = Organisation1.PK;
			Approval2.AV_OH = Organisation2.PK;
			Approval2.AV_OA_AddressOverride = ZGuid.Empty;
			var defaultAddress = Organisation2.Addresses[0].PK;
			var otherAddress = TestObjectCreator.CreateAddress(Organisation2, OrgAddressType.Office, isMain: false).PK;

			Factory.Save();

			AssertEquals(Organisation2.AddressForSendingARDocuments.PK, defaultAddress);
			AssertEquals(Organisation2.AddressForSendingAPDocuments.PK, defaultAddress);

			var filter = (OrgWithAddressFilter)FilterBO[FilterBO.OrganisationAndAddressFilterName_ForTestOnly];

			filter.Organization = ZGuid.Empty;
			filter.Address = ZGuid.Empty;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert(FilterCollection.Contains(Approval1));
			Assert(FilterCollection.Contains(Approval2));

			filter.Organization = Organisation1.PK;
			filter.Address = ZGuid.Empty;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert(FilterCollection.Contains(Approval1));
			Assert(!FilterCollection.Contains(Approval2));

			filter.Organization = Organisation2.PK;
			filter.Address = defaultAddress;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert(!FilterCollection.Contains(Approval1));
			Assert(FilterCollection.Contains(Approval2)); //default address

			filter.Organization = Organisation2.PK;
			filter.Address = otherAddress;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert(!FilterCollection.Contains(Approval1));
			Assert(!FilterCollection.Contains(Approval2));

			Approval2.AV_OA_AddressOverride = otherAddress;
			Factory.Save();

			filter.Organization = Organisation2.PK;
			filter.Address = otherAddress;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert(!FilterCollection.Contains(Approval1));
			Assert(FilterCollection.Contains(Approval2)); //override address
		}

		#endregion

		protected GlbStaff Resource1;
		protected GlbStaff Resource2;
		protected GlbStaff Resource3;

		protected RefCurrency Currency1;
		protected RefCurrency Currency2;
		protected RefCurrency Currency3;

		protected AccBankAccount Account1;
		protected AccBankAccount Account2;
		protected AccBankAccount Account3;

		protected AccChequeBook ChequeBook1;
		protected AccChequeBook ChequeBook2;
		protected AccChequeBook ChequeBook3;

		protected OrgHeader Organisation1;
		protected OrgHeader Organisation2;
		protected OrgHeader Organisation3;

		protected AccPaymentApproval Approval1;
		protected AccPaymentApproval Approval2;

		protected AccPaymentApprovalCollection FilterCollection;
		protected PaymentProcessingFilterBusinessObject FilterBO;

		protected ZString GetLedgerCode()
		{
			PropertyInfo propertyInfo = typeof(PaymentProcessingFilterBusinessObject).GetProperty("LedgerCode", BindingFlags.Instance | BindingFlags.NonPublic);
			return (ZString)propertyInfo.GetValue(FilterBO, null);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Resource1 = Factory.NewWithValidTestData<GlbStaff>();
			Resource2 = Factory.NewWithValidTestData<GlbStaff>();
			Resource3 = Factory.NewWithValidTestData<GlbStaff>();

			Currency1 = Factory.NewWithValidTestData<RefCurrency>();
			Currency2 = Factory.NewWithValidTestData<RefCurrency>();
			Currency3 = Factory.NewWithValidTestData<RefCurrency>();

			Account1 = Factory.NewWithValidTestData<AccBankAccount>();
			Account2 = Factory.NewWithValidTestData<AccBankAccount>();
			Account3 = Factory.NewWithValidTestData<AccBankAccount>();

			ChequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook3 = Factory.NewWithValidTestData<AccChequeBook>();

			Organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			Approval1 = Factory.NewWithValidTestData<AccPaymentApproval>();
			Approval2 = Factory.NewWithValidTestData<AccPaymentApproval>();

			Approval1.AV_GB = GlbBranch.CurrentBranch.PK;
			Approval2.AV_GB = GlbBranch.CurrentBranch.PK;

			FilterCollection = new AccPaymentApprovalCollection(Factory);
			FilterBO = (PaymentProcessingFilterBusinessObject)GetNewFilterStripBusinessObject();

			Approval1.AV_Ledger = GetLedgerCode();
			Approval2.AV_Ledger = GetLedgerCode();
		}
	}
}
