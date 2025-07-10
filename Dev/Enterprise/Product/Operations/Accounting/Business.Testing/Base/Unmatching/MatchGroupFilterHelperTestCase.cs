using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using AccConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Base.Unmatching.Testing
{
	public abstract class MatchGroupFilterHelperTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			FilterHelper = (MatchGroupFilterHelper)CachedBusinessObject;
		}

		protected MatchGroupFilterHelper FilterHelper;

		protected TransactionMatchLink Matchlink1;
		protected TransactionMatchLink Matchlink2;
		protected TransactionMatchLink Matchlink3;
		protected TransactionMatchLink Matchlink4;
		protected TransactionMatchLink Matchlink5;

		protected ARInvoice ARINV1;
		protected ARInvoice ARINV2;
		protected ARCreditNote ARCRD1;
		protected ARCreditNote ARCRD2;

		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;

		#region SetupDataSet

		public void SetupDataSet(string arInv1TransactionNum = null, string arInv2TransactionNum = null, string arCrd1TransactionNum = null, string arCrd2TransactionNum = null)
		{
			ARINV1 = Factory.New<ARInvoice>();
			if (arInv1TransactionNum != null)
			{
				ARINV1.AH_TransactionNum = arInv1TransactionNum;
				ARINV1.IsManuallySetTransactionNumber_ForTestOnly = true;
			}
			Factory.Save();

			ARCRD1 = Factory.New<ARCreditNote>();
			if (arCrd1TransactionNum != null)
			{
				ARCRD1.AH_TransactionNum = arCrd1TransactionNum;
				ARCRD1.IsManuallySetTransactionNumber_ForTestOnly = true;
			}
			Factory.Save();

			ARCRD2 = Factory.New<ARCreditNote>();
			if (arCrd2TransactionNum != null)
			{
				ARCRD2.AH_TransactionNum = arCrd2TransactionNum;
				ARCRD2.IsManuallySetTransactionNumber_ForTestOnly = true;
			}
			Factory.Save();

			ARINV2 = Factory.New<ARInvoice>();
			if (arInv2TransactionNum != null)
			{
				ARINV2.AH_TransactionNum = arInv2TransactionNum;
				ARINV2.IsManuallySetTransactionNumber_ForTestOnly = true;
			}
			Factory.Save();

			Matchlink1 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = ARINV1.PK;
			Matchlink1.AP_MatchGroupNum = "M00001000";

			Matchlink2 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = ARCRD1.PK;
			Matchlink2.AP_MatchGroupNum = "M00001000";

			Matchlink3 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink3.AP_AH = ARINV2.PK;
			Matchlink3.AP_MatchGroupNum = "M00001001";

			Matchlink4 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink4.AP_AH = ARCRD2.PK;
			Matchlink4.AP_MatchGroupNum = "M00001001";
			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);
		}

		#endregion

		protected void SetupDataForFilteringTest()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			ZDateTime cachedNow = ZDateTime.Now;

			// Group 1
			ARInvoice testARInv1 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv1.AH_OH = TestOrg1.PK;
			Matchlink1 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = testARInv1.PK;
			Matchlink1.AP_MatchGroupNum = "M00001334";
			Matchlink1.AP_MatchDate = cachedNow;

			APInvoice testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv2.AH_OH = TestOrg1.PK;
			Matchlink2 = ((IMatching)testARInv1).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = testAPInv2.PK;
			Matchlink2.AP_MatchGroupNum = "M00001334";
			Matchlink2.AP_MatchDate = cachedNow;

			// Group 2
			ARInvoice testARInv3 = Factory.NewWithValidTestData<ARInvoice>();
			testARInv3.AH_OH = TestOrg1.PK;
			Matchlink3 = ((IMatching)testARInv3).CurrentMatchGroup.AddNew();
			Matchlink3.AP_AH = testARInv3.PK;
			Matchlink3.AP_MatchGroupNum = "M00001442";
			Matchlink3.AP_MatchDate = cachedNow;

			ARCreditNote testARCrd4 = Factory.NewWithValidTestData<ARCreditNote>();
			testARCrd4.AH_OH = TestOrg2.PK;
			Matchlink4 = ((IMatching)testARInv3).CurrentMatchGroup.AddNew();
			Matchlink4.AP_AH = testARCrd4.PK;
			Matchlink4.AP_MatchGroupNum = "M00001442";
			Matchlink4.AP_MatchDate = cachedNow;

			// Group 3
			APInvoice testAPInv5 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv5.AH_OH = TestOrg1.PK;
			Matchlink5 = ((IMatching)testAPInv5).CurrentMatchGroup.AddNew();
			Matchlink5.AP_AH = testAPInv5.PK;
			Matchlink5.AP_MatchGroupNum = "M00001523";
			Matchlink5.AP_MatchDate = cachedNow;

			APCreditNote testAPCrd6 = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd6.AH_OH = TestOrg1.PK;
			TransactionMatchLink matchlink6 = ((IMatching)testAPInv5).CurrentMatchGroup.AddNew();
			matchlink6.AP_AH = testAPCrd6.PK;
			matchlink6.AP_MatchGroupNum = "M00001523";
			matchlink6.AP_MatchDate = cachedNow;

			// Group 4
			APInvoice testAPInv7 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv7.AH_OH = TestOrg1.PK;
			TransactionMatchLink matchlink7 = ((IMatching)testAPInv7).CurrentMatchGroup.AddNew();
			matchlink7.AP_AH = testAPInv7.PK;
			matchlink7.AP_MatchGroupNum = "M00009332";
			matchlink7.AP_MatchDate = cachedNow;

			APAdjustmentNote testAPAdj8 = Factory.NewWithValidTestData<APAdjustmentNote>();
			testAPAdj8.AH_OH = TestOrg2.PK;
			TransactionMatchLink matchlink8 = ((IMatching)testAPInv7).CurrentMatchGroup.AddNew();
			matchlink8.AP_AH = testAPAdj8.PK;
			matchlink8.AP_MatchGroupNum = "M00009332";
			matchlink8.AP_MatchDate = cachedNow;

			Factory.Save();
		}

		public void SetupDataForDoesNotExcludePaymentTests()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			APPayment aPPay1 = Factory.NewWithValidTestData<APPayment>();
			aPPay1.AH_OH = TestOrg1.PK;
			Matchlink1 = ((IMatching)aPPay1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = aPPay1.PK;
			Matchlink1.AP_MatchGroupNum = "M00002243";

			APInvoice aPInv2 = Factory.NewWithValidTestData<APInvoice>();
			aPInv2.AH_OH = TestOrg1.PK;
			Matchlink2 = ((IMatching)aPPay1).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = aPInv2.PK;
			Matchlink2.AP_MatchGroupNum = "M00002243";

			ARPayment aRPay3 = Factory.NewWithValidTestData<ARPayment>();
			aRPay3.AH_OH = TestOrg1.PK;
			Matchlink3 = ((IMatching)aPPay1).CurrentMatchGroup.AddNew();
			Matchlink3.AP_AH = aRPay3.PK;
			Matchlink3.AP_MatchGroupNum = "M00002551";

			APInvoice aPInv4 = Factory.NewWithValidTestData<APInvoice>();
			aPInv4.AH_OH = TestOrg1.PK;
			Matchlink4 = ((IMatching)aPPay1).CurrentMatchGroup.AddNew();
			Matchlink4.AP_AH = aPInv4.PK;
			Matchlink4.AP_MatchGroupNum = "M00002551";
			TestObjectCreator.SetupMatchLinkMatchDate(aPPay1);

			Factory.Save();

			FilterHelper.Organisation = TestOrg1.PK;
		}

		#endregion

		#region TestNoParameters

		public void TestNoParameters()
		{
			ARINV1 = Factory.NewWithValidTestData<ARInvoice>();
			ARINV1.AH_LocalExTaxAmount = 10M;
			ARINV1.AH_GB = GlbBranch.CurrentBranch.PK;

			APInvoice aPINV1 = Factory.NewWithValidTestData<APInvoice>();
			aPINV1.AH_LocalExTaxAmount = 10M;
			aPINV1.AH_GB = GlbBranch.CurrentBranch.PK;

			Matchlink1 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = ARINV1.PK;
			Matchlink1.AP_MatchGroupNum = "M00001234";
			Matchlink2 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = aPINV1.PK;
			Matchlink2.AP_MatchGroupNum = "M00001234";
			TestObjectCreator.SetupMatchLinkMatchDate(ARINV1);

			Factory.Save();
			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);

			AssertEquals("There should be 1 ViewMatchGroup", 1, dynBizOs.Count);
			DynamicBusinessObject dynBizO = dynBizOs[0];
			AssertEquals("Match Group Number should be M00001234", "M00001234", dynBizO[UnmatchingRow.Schema.MatchGroupNum]);
		}

		#endregion

		#region TestToDateFilter

		public void TestToDateFilter()
		{
			ARINV1 = Factory.NewWithValidTestData<ARInvoice>();
			ARINV1.AH_LocalExTaxAmount = 100M;
			ARINV1.AH_GB = GlbBranch.CurrentBranch.PK;

			Matchlink1 = ((IMatching)ARINV1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = ARINV1.PK;
			Matchlink1.AP_MatchGroupNum = "M00001356";
			Matchlink1.AP_MatchDate = new ZDateTime(2005, 3, 6, 13, 23, 0);

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_LocalExTaxAmount = 90M;
			aPInv.AH_GB = GlbBranch.CurrentBranch.PK;

			Matchlink2 = ((IMatching)aPInv).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = aPInv.PK;
			Matchlink2.AP_MatchGroupNum = "M00001485";
			Matchlink2.AP_MatchDate = new ZDateTime(2005, 3, 6, 14, 56, 0);

			Factory.Save();

			FilterHelper.DateType = AccConstants.DateFilterTypes.MatchDate;
			FilterHelper.FromDateFilter = new ZDateTime(2005, 3, 6);
			FilterHelper.ToDateFilter = new ZDateTime(2005, 3, 6);

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);

			AssertEquals("There should be 1 ViewMatchGroup", 1, dynBizOs.Count);
		}

		#endregion

		#region TestOrganisationFilterText

		public void TestOrganisationFilterText()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();
			FilterHelper.Organisation = ZGuid.Empty;
			Assert("Organisation Filter string should be empty", FilterHelper.OrganisationFilterString_ForTestOnly.IsEmpty);
			//Assert("Should not be a ZSQLParameter for MG_OH", FilterHelper.FilterParameters.Contains(

			FilterHelper.Organisation = TestOrg1.PK;
			Assert("Organisation Filter string should not be empty", !FilterHelper.OrganisationFilterString_ForTestOnly.IsEmpty);

			Assert("Overall filter string should contain WHERE clause for Org",
				FilterHelper.PlainFilterWithoutParamValues.Contains($"{AutoViewMatchGroup.Schema.MG_OH} = @Organisation"));
		}

		#endregion

		public void TestMaximumRows()
		{
			ARInvoice aRINV1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice aRINV2 = Factory.NewWithValidTestData<ARInvoice>();
			APInvoice aPINV1 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice aPINV2 = Factory.NewWithValidTestData<APInvoice>();

			Matchlink1 = ((IMatching)aRINV1).CurrentMatchGroup.AddNew();
			Matchlink1.AP_AH = aRINV1.PK;
			Matchlink1.AP_MatchGroupNum = "M00001000";

			Matchlink2 = ((IMatching)aRINV2).CurrentMatchGroup.AddNew();
			Matchlink2.AP_AH = aRINV2.PK;
			Matchlink2.AP_MatchGroupNum = "M00001001";

			Matchlink3 = ((IMatching)aRINV1).CurrentMatchGroup.AddNew();
			Matchlink3.AP_AH = aPINV1.PK;
			Matchlink3.AP_MatchGroupNum = "M00001000";
			TestObjectCreator.SetupMatchLinkMatchDate(aRINV1);

			Matchlink4 = ((IMatching)aRINV2).CurrentMatchGroup.AddNew();
			Matchlink4.AP_AH = aPINV2.PK;
			Matchlink4.AP_MatchGroupNum = "M00001001";
			TestObjectCreator.SetupMatchLinkMatchDate(aRINV2);
			Factory.Save();

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 2 ViewMatchGroup", 2, dynBizOs.Count);

			FilterHelper.MaximumRows = 1;

			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 ViewMatchGroup", 1, dynBizOs.Count);
		}
	}
}
