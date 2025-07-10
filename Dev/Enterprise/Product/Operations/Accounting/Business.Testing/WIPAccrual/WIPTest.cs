using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	[TestedType(typeof(WIP))]
	public class WIPTest : BaseWIPAccrualTest
	{
		public void TestErrorReportingIf_AL_OH_IsEmpty()
		{
			var charge = Factory.New<BaseCharge>();
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_OH_SellAccount = ZGuid.Empty;

			var creator = new TestObjectCreator(Factory);
			var localClient = creator.CreateOrgHeader("ZUB", false, true, false, false, false, false);
			var job = creator.CreateJob(localClient, 5.00m, null, 0m);

			var line = Factory.New<WIP>();
			line.SetValues(job, charge);

			var expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Sell Account PK: {ZGuid.Empty}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
WIP branch Company PK: {GlbCompany.CurrentCompany.PK}
Is OrgCompanyData found: No
Is Validation Suspended on Charge: No";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when sell account is empty", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			var debtor = creator.Debtor1;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_GC = ZGuid.Empty;

			line = Factory.New<WIP>();
			line.SetValues(job, charge);

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Sell Account PK: {debtor.PK}
Charge Company PK: {ZGuid.Empty}
WIP branch Company PK: {GlbCompany.CurrentCompany.PK}
Is OrgCompanyData found: No
Is Validation Suspended on Charge: No";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when charge company is empty", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			var org = creator.CreateOrgHeader("XXX", false, false);
			using (charge.GetValidationSuspender())
			{
				charge.JR_OH_SellAccount = org.PK;
			}
			charge.JR_GC = GlbCompany.CurrentCompany.PK;

			line = Factory.New<WIP>();
			line.SetValues(job, charge);

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Sell Account PK: {org.PK}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
WIP branch Company PK: {GlbCompany.CurrentCompany.PK}
Is Debtor from charge.SellAccount: No
Is OrgCompanyData found: Yes
Is Debtor from dbo.OrgCompanyData: No
Is Validation Suspended on Charge: No";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when org is not a Debtor", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			charge.JR_GB = ZGuid.Empty;

			line = Factory.New<WIP>();
			line.SetValues(job, charge);

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Sell Account PK: {org.PK}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
WIP branch Company PK: 
Is Debtor from charge.SellAccount: No
Is OrgCompanyData found: Yes
Is Debtor from dbo.OrgCompanyData: No
Is Validation Suspended on Charge: No";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when org is not a Debtor", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();

			charge.JR_GB = GlbBranch.CurrentBranch.PK;

			line = Factory.New<WIP>();
			using (charge.GetValidationSuspender())
			{
				line.SetValues(job, charge);
			}

			expectedInfo =
$@"
WIPACROrganizationIsNotSet:
Sell Account PK: {org.PK}
Charge Company PK: {GlbCompany.CurrentCompany.PK}
WIP branch Company PK: {GlbCompany.CurrentCompany.PK}
Is Debtor from charge.SellAccount: No
Is OrgCompanyData found: Yes
Is Debtor from dbo.OrgCompanyData: No
Is Validation Suspended on Charge: Yes";

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationIsNotSet);
			AssertContainsInOrder("Error reporting when org is not a Debtor, validation on charge is suspended", collectedInfo, expectedInfo);
		}

		public void TestReverseWIPsWhenRelatedToApportionedCost()
		{
			WIP wip = Factory.NewWithValidTestData<WIP>();
			JobConsolCost cost = Factory.NewWithValidTestData<JobConsolCost>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_E6 = cost.PK;
			charge.JR_AL_ARLine = wip.PK;
			wip.Reverse(false);
			Assert(wip.IsReversed);
		}

		public void TestGetRelatedChargeCore()
		{
			WIP aRline = Factory.NewWithValidTestData<WIP>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AL_ARLine = aRline.PK;
			AssertNotNull(aRline.RelatedJobCharge);
			AssertEquals(charge, aRline.RelatedJobCharge);
		}

		public override void TestForeignCurrencyAndAmount()
		{
			ConcreteWipAccrual.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
			ConcreteWipAccrual.AL_ReverseDate = ZDateTime.Empty;
			AssertEquals(ConcreteWIP.ForeignAmountValue, 0m);
			AssertEquals(ConcreteWIP.RelatedCurrencyCodeValue, ZString.Empty);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			JobCharge jobCh = Factory.New<JobCharge>();
			jobCh.JR_RX_NKSellCurrency = creator.GBP.RX_Code;
			jobCh.JR_OSSellAmt = 300m;
			jobCh.JR_AL_ARLine = ConcreteWIP.PK;
			AssertEquals(ConcreteWIP.ForeignAmountValue, jobCh.JR_OSSellAmt);
			AssertEquals(ConcreteWIP.RelatedCurrencyCodeValue, jobCh.JR_RX_NKSellCurrency);
		}

		WIP ConcreteWIP
		{
			get
			{
				return (WIP)ConcreteWipAccrual;
			}
		}

		public void TestDescriptionDefault()
		{
			AssertEquals("Default WIP Description", "WIP", ConcreteWipAccrual.AL_Desc);
		}

		protected override TransactionLine CreateNewLine()
		{
			TransactionLine line = base.CreateNewLine();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AL_ARLine = line.PK;
			line.AL_AC = charge.JR_AC;
			line.AL_JH = charge.JR_JH;
			line.AL_GB = charge.JR_GB;
			line.AL_GE = charge.JR_GE;
			return line;
		}

		public void TestSetValues()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader localClient = creator.CreateOrgHeader("ZUB", false, true, false, false, false, false);
			Job job = creator.CreateJob(localClient, 5.00m, null, 0m);
			creator.CreateExchangeRate(job, creator.USD, 0.7m);
			AccChargeCode mRG100Code = creator.CreateChargeCode("MRG100", "Margin 100 Charge Code", Core.Constants.ChargeType.Margin, 100m, null, null, "ALL");
			ChargeWithCost charge = creator.CreateCharge(job, mRG100Code, "DESCRIPTION", creator.LocalCurrency, 100m, null, "", creator.USD, 350m, localClient);
			AssertEquals("CFX Value on charge before WIP created", 26.32m, charge.JR_CFXAmt);
			AssertEquals("Value of charge local sell", 526.32m, charge.JR_LocalSellAmt);
			AssertEquals("WIP Value should be zero", 0m, ConcreteWipAccrual.AL_OSExTaxAmount);
			ConcreteWipAccrual.SetValues(job, charge);
			AssertEquals("Sequence should be 1", (ZShort)1, ConcreteWipAccrual.AL_Sequence);
			AssertEquals("Line Amount", -500.00m, ConcreteWipAccrual.AL_LineAmount);
			AssertEquals("OS Ex Tax Amount", 500.00m, ConcreteWipAccrual.AL_OSExTaxAmount);
		}

		public void TestSetValuesWithValidDebtor()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader localClient = creator.CreateOrgHeader("ARTICKED", false, true, false, false, false, false); // Ticked as AR
			Job job = creator.CreateJob(localClient, 5.00m, null, 0m);
			creator.CreateExchangeRate(job, creator.USD, 0.7m);
			AccChargeCode mRG100Code = creator.CreateChargeCode("MRG100", "Margin 100 Charge Code", Core.Constants.ChargeType.Margin, 100m, null, null, "ALL");
			ChargeWithCost charge = creator.CreateCharge(job, mRG100Code, "DESCRIPTION", creator.LocalCurrency, 100m, null, "", creator.USD, 350m, localClient);
			AssertEquals("WIP Organisation", ZGuid.Empty, ConcreteWipAccrual.AL_OH);
			ConcreteWipAccrual.SetValues(job, charge);
			AssertEquals("WIP Organisation", localClient.PK, ConcreteWipAccrual.AL_OH);
		}

		public void TestSetValuesWithInvalidDebtor()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			OrgHeader localClient = creator.CreateOrgHeader("NOTTICKED", false, false, false, false, false, false); // Not Ticked as AR
			Job job = creator.CreateJob(localClient, 5.00m, null, 0m);
			creator.CreateExchangeRate(job, creator.USD, 0.7m);
			AccChargeCode mRG100Code = creator.CreateChargeCode("MRG100", "Margin 100 Charge Code", Core.Constants.ChargeType.Margin, 100m, null, null, "ALL");
			ChargeWithCost charge = creator.CreateCharge(job, mRG100Code, "DESCRIPTION", creator.LocalCurrency, 100m, null, "", creator.USD, 350m, localClient);
			AssertEquals("WIP Organisation", ZGuid.Empty, ConcreteWipAccrual.AL_OH);
			ConcreteWipAccrual.SetValues(job, charge);
			AssertEquals("WIP Organisation", ZGuid.Empty, ConcreteWipAccrual.AL_OH);
		}
	}
}
