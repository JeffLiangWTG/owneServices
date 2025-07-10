using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	[TestedType(typeof(Accrual))]
	public class AccrualTest : BaseWIPAccrualTest
	{
		public void TestErrorReportingIf_AL_OH_IsEmpty()
		{
			var charge = Factory.New<BaseCharge>();
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_OH_CostAccount = ZGuid.Empty;

			var creator = new TestObjectCreator(Factory);
			var localClient = creator.CreateOrgHeader("ZUB", false, true, false, false, false, false);
			var job = creator.CreateJob(localClient, 5.00m, null, 0m);

			var line = Factory.New<Accrual>();
			line.SetValues(job, charge);

			var expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Cost Account PK: {ZGuid.Empty}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
ACR branch Company PK: {GlbCompany.CurrentCompany.PK}
Is OrgCompanyData found: No
Is Validation Suspended on Charge: No";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when cost account is empty", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			var creditor = creator.Creditor1;
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_GC = ZGuid.Empty;

			line = Factory.New<Accrual>();
			line.SetValues(job, charge);

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Cost Account PK: {creditor.PK}
Charge Company PK: {ZGuid.Empty}
ACR branch Company PK: {GlbCompany.CurrentCompany.PK}
Is OrgCompanyData found: No
Is Validation Suspended on Charge: No";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when charge company is empty", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			var org = creator.CreateOrgHeader("XXX", false, false);
			charge.JR_OH_CostAccount = org.PK;
			charge.JR_GC = GlbCompany.CurrentCompany.PK;

			line = Factory.New<Accrual>();
			line.SetValues(job, charge);

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Cost Account PK: {org.PK}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
ACR branch Company PK: {GlbCompany.CurrentCompany.PK}
Is Creditor from charge.CostAccount: No
Is OrgCompanyData found: Yes
Is Creditor from dbo.OrgCompanyData: No
Is Validation Suspended on Charge: No";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when org is not a Creditor", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			charge.JR_GB = ZGuid.Empty;

			line = Factory.New<Accrual>();
			line.SetValues(job, charge);

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Cost Account PK: {org.PK}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
ACR branch Company PK: 
Is Creditor from charge.CostAccount: No
Is OrgCompanyData found: Yes
Is Creditor from dbo.OrgCompanyData: No
Is Validation Suspended on Charge: No";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when org is not a Creditor", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			charge.JR_GB = GlbBranch.CurrentBranch.PK;

			line = Factory.New<Accrual>();
			using (charge.GetValidationSuspender())
			{
				line.SetValues(job, charge);
			}

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Cost Account PK: {org.PK}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
ACR branch Company PK: {GlbCompany.CurrentCompany.PK}
Is Creditor from charge.CostAccount: No
Is OrgCompanyData found: Yes
Is Creditor from dbo.OrgCompanyData: No
Is Validation Suspended on Charge: Yes";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when org is not a Creditor, validation on charge is suspended", collectedInfo, expectedInfo);
		}

		public void TestGetRelatedChargeCore()
		{
			Accrual line = Factory.NewWithValidTestData<Accrual>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AL_APLine = line.PK;
			AssertNotNull(line.RelatedJobCharge);
			AssertEquals(charge, line.RelatedJobCharge);
		}

		public void TestDescriptionDefault()
		{
			AssertEquals("Default Accrual Description", "Accrual", ConcreteWipAccrual.AL_Desc);
		}

		public void TestUpdateJobChargeLink()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = creator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var job = creator.CreateJob(shipment, false);
			Factory.Save();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, new ApportionmentListing(Factory, consol));
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			consolCost.E6_AH_APInvoice = apInvoice.PK;
			AssertEquals(true, consolCost.IsPosted);
			var charge = consolCost.ApportionmentCharges.AddNew();
			var accrual = Factory.New<Accrual>();
			charge.JR_AL_APLine = accrual.PK;
			accrual.UpdateJobChargeLink(Factory.Load<Charge>(charge.PK));
			AssertEquals(false, consolCost.IsDeleted);
			job.Dispose();
		}

		public override void TestForeignCurrencyAndAmount()
		{
			ConcreteWipAccrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
			ConcreteAccrual.AL_ReverseDate = ZDateTime.Empty;
			AssertEquals(ConcreteAccrual.ForeignAmountValue, 0m);
			AssertEquals(ConcreteAccrual.RelatedCurrencyCodeValue, ZString.Empty);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			JobCharge jobCh = Factory.New<JobCharge>();
			jobCh.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			jobCh.JR_OSCostAmt = 125m;
			jobCh.JR_AL_APLine = ConcreteAccrual.PK;
			AssertEquals(ConcreteAccrual.ForeignAmountValue, jobCh.JR_OSCostAmt);
			AssertEquals(ConcreteAccrual.RelatedCurrencyCodeValue, jobCh.JR_RX_NKCostCurrency);
		}

		public override void TestProcessConsolCostAfterReversingAccrual()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			ConcreteAccrual.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
			ConcreteAccrual.AL_ReverseDate = ZDateTime.Empty;
			ConcreteAccrual.AL_JH = TestObjectCreator.Job1.PK;
			ConcreteAccrual.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(ConcreteAccrual);
			charge.JR_E6 = cost.PK;
			Factory.Save();
			ZGuid costPK = cost.PK;
			ConcreteAccrual.Reverse();
			Factory.Save();
			AssertEquals("JR_E6", ZGuid.Empty, charge.JR_E6);
			AssertNull(Factory.Load<JobConsolCost>(costPK));
		}

		public void TestShouldReverseDefaults()
		{
			Assert("Default value for new object should be true.", ConcreteAccrual.ShouldReverese);
			Factory.Save();
			Accrual testAccrual = new BusinessObjectFactory().Load<Accrual>(ConcreteAccrual.PK);
			Assert("Default value for loaded object should be true.", testAccrual.ShouldReverese);
		}

		public void TestRaiseRecalculateAmountsEvent()
		{
			new WIPAccrualCollection(Factory).Add(ConcreteAccrual);
			AccrualCollection testCollection = new AccrualCollection(Factory);
			testCollection.Add(ConcreteAccrual);
			testCollection.RecalculateAmounts += new EventHandler(testCollection_RecalculateAmounts);
			RecalculateAmountsEventRaiseAmount = 0;
			ConcreteAccrual.ShouldReverese = !ConcreteAccrual.ShouldReverese;
			AssertEquals("RecalculateAmounts event should be rised.", 1, RecalculateAmountsEventRaiseAmount);
			RecalculateAmountsEventRaiseAmount = 0;
			ConcreteAccrual.ShouldReverese = ConcreteAccrual.ShouldReverese;
			AssertEquals("RecalculateAmounts event should not be rised without property value changes.", 0, RecalculateAmountsEventRaiseAmount);
		}

		public void TestUpdateAL_ReverseDate_UpdateLastAccrualReverseDate()
		{
			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = "ALL";
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			var shipment = creator.CreateShipment("S001");
			var expectedAccrualReverseDate = ZDateTime.Today;
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(-2);
			shipment.JS_E_ARV = expectedAccrualReverseDate;
			var testJob = creator.CreateJob(shipment, false);
			var charge = creator.CreateCharge(testJob, creator.CC2, "Charge 2", creator.AUD, 1m, creator.CreateOrgHeader("Two", true, false), creator.AUD, 1m, creator.CreateOrgHeader("AgentTwo", false, true));
			charge.JR_APInvoiceNum = "inv123";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			Factory.Save();
			var accrual = charge.Accrual;
			AssertNotNull("Precondition: accrual must be created", accrual);
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			creator.PostJobAsBillingTab(testJob, JobInvoicingPostingOption.Costs);
			Factory.Save();
			var cost = charge.Cost;
			AssertNotNull("Precondition: cost must be created", cost);
			AssertEquals("Precondition: cost AL_PostDate", expectedAccrualReverseDate, cost.AL_ReverseDate);
			AssertEquals("Accrual must be reversed with cost revenue recognition date", cost.AL_ReverseDate, accrual.AL_ReverseDate);
		}

		void testCollection_RecalculateAmounts(object sender, EventArgs e)
		{
			RecalculateAmountsEventRaiseAmount++;
		}

		int RecalculateAmountsEventRaiseAmount;
		Accrual ConcreteAccrual
		{
			get
			{
				return (Accrual)ConcreteWipAccrual;
			}
		}

		protected override TransactionLine CreateNewLine()
		{
			TransactionLine line = base.CreateNewLine();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AL_APLine = line.PK;
			line.AL_AC = charge.JR_AC;
			line.AL_JH = charge.JR_JH;
			line.AL_GB = charge.JR_GB;
			line.AL_GE = charge.JR_GE;
			return line;
		}
	}
}
