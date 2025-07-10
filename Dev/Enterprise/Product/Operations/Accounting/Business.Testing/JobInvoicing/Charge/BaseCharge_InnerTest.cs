using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(BaseCharge))]
	public abstract class BaseCharge_InnerTest : JobChargeTest
	{
		#region Tax Branch

		public virtual void TestJR_GB_CostTaxBranch_ReadOnly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, true, false, false, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, true, false, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, false, true, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, false, false, true, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, false, false, false, true, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, false, false, false, false, true);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(false, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, false, false, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_CostTaxBranchInfo.ReadOnly), isCost: true, false, false, false, false, false, false, false);
		}

		public virtual void TestJR_GB_SellTaxBranch_ReadOnly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, true, false, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, false, true, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, false, false, true, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, false, false, false, true, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, false, false, false, false, true);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(false, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, false, false, false, false, false);
			TestTaxBranchReadonly(consol, shipmentJob, (charge) => AssertEquals(true, charge.JR_GB_SellTaxBranchInfo.ReadOnly), isCost: false, false, false, false, false, false, false, false);
		}

		void TestTaxBranchReadonly(ForwardingConsol consol, Job job, Action<Charge> action, bool isCost,
			bool isApportioned, bool isTaxBranchNotApplicable, bool isAccountGSTNotRegistered, bool isPosted, bool isInDatabaseAndReadyForPosting, bool isInDatabaseAndReadyForFinancialClosureWithoutModifySecurity, bool isSecurityAllowed = true)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			Factory.Save();

			var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 100, testObjectCreator.Creditor1);
			charge.JR_E6 = isApportioned ? consolCost.PK : ZGuid.Empty;
			AssertEquals(isApportioned, charge.JR_IsApportioned);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, !isTaxBranchNotApplicable);
			AssertEquals(isTaxBranchNotApplicable, !AccountingMasterFilesUtils.IsTaxBranchApplicable);

			if (!isTaxBranchNotApplicable)
			{
				var securityName = isCost ? SecurityCore.AllowOverrideCostTaxBranch : SecurityCore.AllowOverrideSellTaxBranch;
				Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, securityName).IsAllowed = isSecurityAllowed;
			}

			if (isCost)
			{
				TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = isAccountGSTNotRegistered ?
					AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code :
					AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				AssertEquals(isAccountGSTNotRegistered, !charge.IsCostGSTApplicable);
			}
			else
			{
				TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = isAccountGSTNotRegistered ?
					AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code :
					AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				AssertEquals(isAccountGSTNotRegistered, !charge.IsSellGSTApplicable);
			}

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			if (isPosted)
			{
				line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				line.AL_LineType = isCost ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue;
				if (isCost)
				{
					charge.JR_AL_APLine = line.PK;
				}
				else
				{
					charge.JR_AL_ARLine = line.PK;
				}
			}
			AssertEquals(isPosted, isCost ? charge.IsCostPosted : charge.IsRevenuePosted);

			if (isInDatabaseAndReadyForPosting)
			{
				job.JH_Status = isCost ? JobHeaderStatus.JobReadyForCostPosting.Code : JobHeaderStatus.JobReadyForRevenuePosting.Code;
				AssertEquals(isInDatabaseAndReadyForPosting, isCost ? charge.IsInDatabaseAndReadyForCostPosting : charge.IsInDatabaseAndReadyForRevenuePosting);
			}
			else if (isInDatabaseAndReadyForFinancialClosureWithoutModifySecurity)
			{
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = !isInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
				AssertEquals(isInDatabaseAndReadyForFinancialClosureWithoutModifySecurity, charge.IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity);
			}
			else
			{
				job.JH_Status = JobHeaderStatus.Working.Code;
			}

			action(charge);

			consolCost.Delete();
			line.Delete();
			charge.Delete();
		}

		#endregion

		public void TestJR_AT_CostGSTRateReportsIssueWhenChargeHasContextAutoJRJAndWhenCostTaxRateIsNotEmpty_WhenConsolTaxRateEmpty()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			TestObjectCreator.CreateJob(shipment);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			consolCost.E6_OH_Creditor = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			AssertEquals(ZGuid.Empty, consolCost.E6_AT_TaxRate);

			var charge = consolCost.ApportionmentCharges[0];
			AssertEquals(ZGuid.Empty, charge.JR_AT_CostGSTRate);
			Assert(!charge.HasContext(BusinessContext.AutoJobRevenueJournal));
			AssertContains("Should not report error", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal));

			charge.SetContext(BusinessContext.AutoJobRevenueJournal);
			charge.JR_AT_CostGSTRate = ZGuid.NewZGuid();
			Assert(charge.JR_AT_CostGSTRate.IsValid);
			AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ("Should report an error as Cost Tax Rate is NOT empty", "", charge, "", "");

			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			Assert(charge.JR_AT_CostGSTRate.IsValid);
			AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ("Should report an error as Cost Tax Rate is NOT empty", "", charge, "", "ZZGST1");

			charge.JR_AT_CostGSTRate = TestObjectCreator.GST2.PK;
			Assert(charge.JR_AT_CostGSTRate.IsValid);
			AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ("Should report an error as Cost Tax Rate is NOT empty", "", charge, "ZZGST1", "ZZGST2");

			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			Assert(!charge.JR_AT_CostGSTRate.IsValid);
			AssertContains("Should not report error", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal));
		}

		public void TestJR_AT_CostGSTRateReportsIssueWhenChargeHasContextAutoJRJAndWhenCostTaxRateIsChangedToNotEmpty_WhenConsolTaxRateNotEmpty()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			TestObjectCreator.CreateJob(shipment);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			consolCost.E6_OH_Creditor = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			AssertEquals(TestObjectCreator.GST1.PK, consolCost.E6_AT_TaxRate);

			var charge = consolCost.ApportionmentCharges[0];
			AssertEquals(consolCost.E6_AT_TaxRate, charge.JR_AT_CostGSTRate);
			Assert(!charge.HasContext(BusinessContext.AutoJobRevenueJournal));
			AssertContains("Should not report error", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal));

			charge.SetContext(BusinessContext.AutoJobRevenueJournal);
			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			Assert(!charge.JR_AT_CostGSTRate.IsValid);
			AssertContains("Should not report error", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal));

			charge.JR_AT_CostGSTRate = ZGuid.NewZGuid();
			Assert(charge.JR_AT_CostGSTRate.IsValid);
			AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ("Should report an error as Cost Tax Rate is NOT empty", "ZZGST1", charge, "", "");

			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			Assert(charge.JR_AT_CostGSTRate.IsValid);
			AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ("Should report an error as Cost Tax Rate is NOT empty", "ZZGST1", charge, "", "ZZGST1");

			charge.JR_AT_CostGSTRate = TestObjectCreator.GST2.PK;
			Assert(charge.JR_AT_CostGSTRate.IsValid);
			AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ("Should report an error as Cost Tax Rate is NOT empty", "ZZGST1", charge, "ZZGST1", "ZZGST2");
		}

		void AssertJR_AT_CostGSTRateReportsErrorWhenChargeHasContextAutoJRJ(string errorMessage, string consolTaxRate, ApportionSplitCharge charge, string previousValue, string newValue)
		{
			var expectedInfo =
$@"
JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal:
Job Consol Cost Tax Rate : '{consolTaxRate}'
Charge's Tax Rate is changed from '{previousValue}' to '{newValue}'.
Call stack:
";
			AssertContains(errorMessage, expectedInfo, CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal));
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).ClearServiceCache();
		}

		public void TestDataRefreshBusUpdateSkippedExceptionIsNotThrownWhenChargeReloaderReloadsChargeWithSkipDataRefreshBusUpdateBusinessContexts()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 200m);
			Factory.Save();

			var shipment1Charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment1.Job.PK));
			var testCharge = shipment1Charges.First(c => c.JR_DisplaySequence != 1);
			testCharge.JR_DisplaySequence = 1;
			Factory.Save();

			var subscriberFactory = new BusinessObjectFactory();
			var publisherFactory = new BusinessObjectFactory();

			var subscriberCosnolCost = subscriberFactory.Load<JobConsolCost>(testCharge.JR_E6);
			var subscriberCharge = subscriberFactory.Load<JobCharge>(testCharge.PK);
			var publisherCharge = publisherFactory.Load<JobCharge>(testCharge.PK);

			using (subscriberCharge.SuspendSettingHasChanges())
			{
				subscriberCharge.JR_DisplaySequence = 2;
			}

			publisherCharge.JR_OSSellAmt = 134m;
			publisherCharge.JR_DisplaySequence = 1;
			publisherFactory.Save();

			AssertNoExceptionThrown(() => subscriberFactory.Save());
		}

		public void TestJR_OH_CostAccountReportsIssue()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);

			var appotionmentCharge = consolCost.ApportionmentCharges[0];
			appotionmentCharge.JR_OH_CostAccount = ZGuid.NewZGuid();
			AssertIssueReport(TestObjectCreator.Creditor1.PK, appotionmentCharge.JR_OH_CostAccount);

			consolCost.E6_OH_Creditor = ZGuid.Empty;
			var charge = Factory.Load<Charge>(appotionmentCharge.PK);
			charge.JR_OH_CostAccount = ZGuid.NewZGuid();
			AssertIssueReport(ZGuid.Empty, charge.JR_OH_CostAccount);

			void AssertIssueReport(ZGuid consolCostOrgPK, ZGuid chargeOrgPK)
			{
				AssertContains("LastMessageReported",
$@"Charge account '{chargeOrgPK}' Consol Cost account '{consolCostOrgPK}'.
Call stack:
"
					, CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(appotionmentCharge.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost));
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).ClearServiceCache();
			}
		}

		public void TestUpdateOsCostTaxAmount_TaxDate()
		{
			TestCharge.JR_OSCostAmt = 100;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(14.98m, TestCharge.JR_OSCostGSTAmt_Calc);

			TestCharge.JR_CostTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(9.5m, TestCharge.JR_OSCostGSTAmt_Calc);
		}

		public void TestUpdateOsSellTaxAmountFromLocalTaxAmount_TaxDate()
		{
			var arTaxAmounts = TestCharge as IReceivablesTaxAmountCalculation;
			if (arTaxAmounts == null)
			{
				Assert("This test is not applicable.", true);
				return;
			}

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			TestCharge.JR_OSSellAmt = 100;
			TestCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
			TestCharge.JR_OSSellExRate = 2;
			AssertNotEquals("Precondition: JR_OSSellGSTAmt is not calculated properly", 0m, TestCharge.JR_OSSellGSTAmt_Calc);
			arTaxAmounts.RecalculateOsTaxAmount();
			AssertEquals(14.98m, TestCharge.JR_OSSellGSTAmt_Calc);

			TestCharge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			arTaxAmounts.RecalculateOsTaxAmount();
			AssertEquals(9.5m, TestCharge.JR_OSSellGSTAmt_Calc);
		}

		public void TestUpdateOsSellTaxAmount_TaxDate()
		{
			TestCharge.JR_OSSellAmt = 100;
			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(14.98m, TestCharge.JR_OSSellGSTAmt_Calc);

			TestCharge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(9.5m, TestCharge.JR_OSSellGSTAmt_Calc);
		}

		public void TestJR_Sell_LocalGSTAmount_TaxDate()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestCharge.JR_OSSellAmt = 100;
			TestCharge.JR_LocalSellAmt = 100;
			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(14.98m, TestCharge.JR_Sell_LocalGSTAmount);

			TestCharge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(9.5m, TestCharge.JR_Sell_LocalGSTAmount);
		}

		public void TestJR_Cost_LocalGSTAmountHighPrecision_TaxDate()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestCharge.JR_OSCostAmt = 100;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(14.975m, TestCharge.JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly);

			TestCharge.JR_CostTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(9.5m, TestCharge.JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly);
		}

		public void TestJR_Cost_LocalGSTAmountHighPrecisionWithIsCostTaxAmountOverridden()
		{
			foreach (var isOverridden in new[] { true, false })
			{
				TestCharge.JR_OSCostExRate = 0.5m;
				TestCharge.JR_OSCostAmt = 1000.3;
				TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
				using (TestCharge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender())
				{
					TestCharge.JR_IsCostTaxAmountOverridden = isOverridden;
					if (isOverridden)
					{
						TestCharge.JR_OSCostGSTAmt_Calc = 100.02m;
						AssertEquals("local cost GST amount when override is ticked", 200.04m, TestCharge.JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly);
					}
					else
					{
						AssertEquals("local cost GST amount when override is not ticked", 200.06m, TestCharge.JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly);
					}
				}
			}
		}

		public void TestJR_Calc_OSSellGSTAmt_TaxDate()
		{
			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			TestCharge.JR_OSSellAmt = 100;
			AssertEquals(5m, TestCharge.JR_Calc_OSSellGSTAmt);

			TestCharge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(0m, TestCharge.JR_Calc_OSSellGSTAmt);
		}

		public void TestJR_Calc_OSCostGSTAmt_TaxDate()
		{
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			TestCharge.JR_OSCostAmt = 100;
			AssertEquals(5m, TestCharge.JR_Calc_OSCostGSTAmt);

			TestCharge.JR_CostTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRate;
			AssertEquals(0m, TestCharge.JR_Calc_OSCostGSTAmt);
		}

		public void TestJR_Calc_LocalSellExtraTaxAmt_TaxDate()
		{
			TestCharge.JR_OSSellAmt = 100;
			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(9.98m, TestCharge.JR_Calc_LocalSellExtraTaxAmt);

			TestCharge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoExtraRate;
			AssertEquals(0m, TestCharge.JR_Calc_LocalSellExtraTaxAmt);
		}

		public void TestJR_Calc_OSSellExtraTaxAmt_TaxDate()
		{
			TestCharge.JR_OSSellAmt = 100;
			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(9.98m, TestCharge.JR_Calc_OSSellExtraTaxAmt);

			TestCharge.JR_SellTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoExtraRate;
			AssertEquals(0m, TestCharge.JR_Calc_OSSellExtraTaxAmt);
		}

		public void TestJR_Calc_OSCostExtraTaxAmt_TaxDate()
		{
			TestCharge.JR_OSCostAmt = 100;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(9.98m, TestCharge.JR_Calc_OSCostExtraTaxAmt);

			TestCharge.JR_CostTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoExtraRate;
			AssertEquals(0m, TestCharge.JR_Calc_OSCostExtraTaxAmt);
		}

		public void TestJR_Calc_OSCostExtraTaxAmt_NullCurrency()
		{
			TestCharge.JR_OSCostAmt = 100;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			TestCharge.JR_RX_NKCostCurrency = ZString.Empty;
			var oSCostExtraTaxAmt = 1000m;

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => oSCostExtraTaxAmt = TestCharge.JR_Calc_OSCostExtraTaxAmt);
				AssertEquals(0m, oSCostExtraTaxAmt);
			});
		}

		public void TestJR_Calc_LocalCostExtraTaxAmt_TaxDate()
		{
			TestCharge.JR_OSCostAmt = 100;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(9.98m, TestCharge.JR_Calc_LocalCostExtraTaxAmt);

			TestCharge.JR_CostTaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoRateAndExtraRate;
			AssertEquals(0m, TestCharge.JR_Calc_LocalCostExtraTaxAmt);
		}

		public void TestRelatedJobNumberReadOnly()
		{
			var creator = new TestObjectCreator(Factory);
			var gatewayConsol = creator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);

			var acc = creator.CreateChargeCode("GTB");

			using (var consolJobHeader = creator.CreateJob(gatewayConsol))
			{
				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "TAX1";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(5);

				var charge = consolJobHeader.Charges.AddNew();
				charge.JR_AC = acc.PK;
				charge.JR_OSSellAmt = 600m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_OH_SellAccount = creator.Debtor.PK;
				charge.JR_SellRatingOverride = false;

				Assert(!charge.IsRevenuePosted);
				Assert(!charge.JR_Calc_RelatedJobNumberInfo.ReadOnly);

				var transactionHeader = Factory.New<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				var transactionLine = (ARInvoiceLine)Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
				transactionLine.AL_AH = transactionHeader.PK;
				transactionLine.AL_LineType = TransactionLineTypes.Revenue;
				transactionLine.AL_LineAmount = -600;
				transactionLine.AL_OSAmount = -600;
				transactionLine.AL_RX_NKTransactionCurrency = "AUD";
				transactionLine.AL_RevRecognitionType = "IMM";

				charge.JR_AL_ARLine = transactionLine.PK;
				charge.JR_AT_CostGSTRate = taxRate.PK;

				Assert(charge.IsRevenuePosted);
				Assert(charge.JR_Calc_RelatedJobNumberInfo.ReadOnly);
			}
		}

		public void TestSellGSTTaxShouldNotChangeAfterPosted()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			TestObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC11, 100M);

			Factory.Save();

			var charge = job.Charges[0];
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			var expectedTaxRate = TestObjectCreator.GSTANDQST1.PK;
			charge.JR_AT_SellGSTRate = expectedTaxRate;
			charge.JR_OSSellAmt = 100M;

			Factory.Save();

			TestObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Revenue);

			Factory.Save();

			Assert("Precondition: Cost should not be posted.", !charge.IsCostPosted);
			Assert("Precondition: Revenue should be posted.", charge.IsRevenuePosted);
			AssertEquals("Precondition: SellGSTRates should be changed.", expectedTaxRate, charge.JR_AT_SellGSTRate);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.AALSHI);
			invoice.SubmittedFromInvoicingForm = true;

			var invoiceConsolCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC11, 750);
			invoiceConsolCost.RelatedConsolCostPK = cost.PK;
			var expectedBranch = TestObjectCreator.NonCurrentBranch.PK;
			AssertNotEquals("Precondition: Branch should be changed", invoiceConsolCost.ApportionmentCharges[0].JR_GB, expectedBranch);
			invoiceConsolCost.ApportionmentCharges[0].JR_GB = expectedBranch;
			AssertNotEquals("Precondition: Tax rate should be different to what is expected after posting", invoiceConsolCost.ApportionmentCharges[0].JR_AT_SellGSTRate, expectedTaxRate);
			invoice.ImportAllApportionmentsFromCosting();

			Factory.Save();

			Assert("Cost should be posted.", charge.IsCostPosted);
			Assert("Revenue should be posted.", charge.IsRevenuePosted);
			AssertEquals("Branch should be changed.", expectedBranch, charge.JR_GB);
			AssertEquals("SellGSTRates should not change after posted.", expectedTaxRate, charge.JR_AT_SellGSTRate);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesBaseCharge()
		{
			var charge = Factory.NewWithValidTestData<BaseCharge>();

			var localList = new List<string>
				{
					nameof(charge.JR_CFXAmt),
					nameof(charge.JR_LocalSellAmt),
					nameof(charge.JR_Calc_LocalCostExtraTaxAmt),
					nameof(charge.JR_Calc_LocalSellExtraTaxAmt),
					nameof(charge.JR_Cost_LocalGSTAmount),
					nameof(charge.JR_Cost_LocalWHTAmount),
					nameof(charge.TotalLocalAmountWithGSTOnInvForJob),
					nameof(charge.JR_Calc_LocalCostAmtWithGST),
					nameof(charge.JR_Sell_LocalGSTAmount),
					nameof(charge.JR_Sell_LocalWHTAmount),
					nameof(charge.JR_LocalCostAmt),
				};
			var companyLocalList = new List<string>
				{
					nameof(charge.JR_LocalSellInvoiceAmt),
				};

			var osCostList = new List<string>
				{
					nameof(charge.JR_OSCostAmtWithGSTAmt),
					nameof(charge.JR_OSCostAmt),
					nameof(charge.JR_OSCostGSTAmt),
					nameof(charge.JR_OSCostGSTAmt_Calc),
					nameof(charge.JR_OSCostWHTAmt),
					nameof(charge.JR_Calc_OSCostExtraTaxAmt),
					nameof(charge.JR_Calc_OSCostGSTAmt),
					nameof(charge.JR_Calc_OSCostAmtWithGST),
				};

			var osSellList = new List<string>
				{
					nameof(charge.JR_OSSellWHTAmt),
					nameof(charge.JR_OSSellAmt),
					nameof(charge.JR_Calc_OSSellExtraTaxAmt),
					nameof(charge.JR_Calc_OSSellGSTAmt),
					nameof(charge.JR_Calc_OSSellAmtWithGST),
				};

			var invForJobList = new List<string>
				{
					nameof(charge.TotalTaxOnInvForJob),
					nameof(charge.TotalAmountOnInvForJob),
					nameof(charge.LineTotalAmountOnInvoiceForJob),
					nameof(charge.LineGSTAmountOnInvoiceForJob),
					nameof(charge.LineWHTAmountOnInvoiceForJob),
					nameof(charge.LineExtraTaxAmountOnInvoiceForJob),
				};

			var percentList = new List<string>
				{
					nameof(charge.MarginPercentage)
				};

			var weightList = new List<string>
				{
					nameof(charge.JR_Chargeable),
					nameof(charge.JR_ActualWeight)
				};

			var tester = new DecimalPlacesAttributeTester(charge, charge.Company);
			tester.CheckLocalCurrency(localList, nameof(charge.LocalCurrencyDecimals));
			tester.CheckCompanyLocalCurrency(companyLocalList, nameof(charge.CompanyLocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osCostList, nameof(charge.OSCostCurrencyDecimals), nameof(charge.JR_RX_NKCostCurrency), charge);
			tester.CheckNonLocalCurrency(osSellList, nameof(charge.OSSellCurrencyDecimals), nameof(charge.JR_RX_NKSellCurrency), charge);
			tester.CheckNonLocalCurrency(invForJobList, nameof(charge.CurrencyOnInvForJobDecimals), nameof(charge.JR_RX_NKCostCurrency), charge);
			tester.CheckConstant(percentList, nameof(charge.PercentageDecimals), Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
			tester.CheckConstant(weightList, nameof(charge.WeightVolumeDecimals), DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits);
		}

		public void TestBaseChargeTypeDeciderValidation()
		{
			var newCharge = Factory.NewWithValidTestData<BaseCharge>();
			Assert("New BaseCharge is Charge", newCharge is Charge);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedCharge = newFactory.Load<BaseCharge>(newCharge.PK);
			Assert("Loaded BaseCharge is Charge", loadedCharge is Charge);
		}

		public void TestOSSellGSTHasWarning()
		{
			var charge = Factory.New<BaseCharge>();
			charge.JR_OSSellAmt = 100;
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;

			var warning = "This value is not precise and is for reference only. It will be recalculated during posting with higher precision due to Calculate Tax at Header Level rules.";

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			charge.JR_AL_ARLine = line.PK;
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			charge = Factory.New<BaseCharge>();
			charge.JR_OSSellAmt = 100;

			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			charge.Validation.ValidateAll();
			AssertHasWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			charge.JR_AL_ARLine = line.PK;
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			charge.Validation.ValidateAll();
			AssertNoWarning(charge.JR_OSSellGSTAmt_CalcInfo, warning);
		}

		public void TestHasDeferredConfiguration()
		{
			var chargeCode1 = TestObjectCreator.CC1;
			var chargeCode2 = TestObjectCreator.CC2;
			chargeCode1.AC_ChargeGroup = chargeCode2.AC_ChargeGroup = "FRT";

			var orgHeader = TestObjectCreator.ABIGAS;
			var invoiceType = orgHeader.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_ServiceDirection = "ALL";
			invoiceType.PI_TransportMode = "ALL";
			invoiceType.PI_RS_NKServiceLevel = "STD";
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;

			var shipment = TestObjectCreator.CreateShipment("S1234");
			shipment.JS_RS_NKServiceLevel = "STD";
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode1.PK;
			charge.JR_OH_SellAccount = orgHeader.PK;
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice_Batching, charge.JR_InvoiceType);
			charge.JR_AC = chargeCode2.PK;
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice_Batching, charge.JR_InvoiceType);

			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			var deferredCharge = invoiceType.DeferredCharges.AddNew();
			deferredCharge.PO_AC = chargeCode1.PK;
			charge.JR_AC = chargeCode1.PK;
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice_Batching, charge.JR_InvoiceType);
			charge.JR_AC = chargeCode2.PK;
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);

			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.EXC;
			deferredCharge.PO_AC = chargeCode1.PK;
			charge.JR_AC = chargeCode1.PK;
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);
			charge.JR_AC = chargeCode2.PK;
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice_Batching, charge.JR_InvoiceType);
		}

		#region JR_EstimatedCost

		public virtual void TestJR_EstimatedCost_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			SetupChargeForTestJR_EstimatedCost_ReadOnly(charge);
			var shipmentAllowOverrideEstimatedCost = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideEstimatedCost);
			shipmentAllowOverrideEstimatedCost.IsAllowed = false;
			Assert(charge.JR_EstimatedCostInfo.ReadOnly);
			shipmentAllowOverrideEstimatedCost.IsAllowed = true;
			Assert(!charge.JR_EstimatedCostInfo.ReadOnly);
		}

		protected virtual void SetupChargeForTestJR_EstimatedCost_ReadOnly(BaseCharge charge)
		{
		}

		public void TestJR_OSCostAmountSetJR_EstimatedCost_Suspender()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("JR_EstimatedCost", 0m, charge.JR_EstimatedCost);
			decimal expectedValue = 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			var initialExpectedValue = expectedValue;

			using (charge.SetEstimatedCostSuspender.GetSuspender())
			{
				expectedValue += 10;
				charge.JR_OSCostAmt = expectedValue;
				AssertNotEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);
				AssertEquals("JR_EstimatedCost", initialExpectedValue, charge.JR_EstimatedCost);
			}

			expectedValue += 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);
		}

		public void TestJR_OSCostAmountSetJR_EstimatedCost_PostedLine()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("JR_EstimatedCost", 0m, charge.JR_EstimatedCost);
			decimal expectedValue = 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			expectedValue += 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			var initialExpectedValue = expectedValue;
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 10);
			charge.JR_AL_APLine = line.PK;
			Assert("Precondition: IsCostPosted", charge.IsCostPosted);
			expectedValue += 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertNotEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);
			AssertEquals("JR_EstimatedCost", initialExpectedValue, charge.JR_EstimatedCost);
		}

		public void TestJR_OSCostAmountSetJR_EstimatedCost_SavedLine()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("JR_EstimatedCost", 0m, charge.JR_EstimatedCost);
			decimal expectedValue = 10;
			charge.JR_OSCostAmt = expectedValue;
			charge.JR_LocalCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			expectedValue += 10;
			charge.JR_OSCostAmt = expectedValue;
			charge.JR_LocalCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			var initialExpectedValue = expectedValue;
			Factory.Save();
			Assert("Precondition: IsInDatabase", charge.IsInDatabase);
			expectedValue += 10;
			charge.JR_OSCostAmt = expectedValue;
			charge.JR_LocalCostAmt = expectedValue;
			AssertNotEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);
			AssertEquals("JR_EstimatedCost", initialExpectedValue, charge.JR_EstimatedCost);
		}

		public void TestCurencyAndExRateChangeForJR_EstimatedCost()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("JR_EstimatedCost", 0m, charge.JR_EstimatedCost);
			decimal expectedValue = 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = "WW";
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
			charge.JR_OSCostExRate = 0m;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			charge.JR_OSCostExRate = 2m;
			AssertEquals("JR_EstimatedCost", expectedValue * 2, charge.JR_EstimatedCost);

			charge.JR_OSCostExRate = 0m;
			AssertEquals("JR_EstimatedCost", expectedValue * 2, charge.JR_EstimatedCost);

			charge.JR_OSCostExRate = 3m;
			AssertEquals("JR_EstimatedCost", expectedValue * 3, charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.Code;
			charge.JR_OSCostExRate = 1m;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);
		}

		public void TestCurencyAndExRateChangeInReciprocalCompanyForJR_EstimatedCost()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			AssertEquals("JR_EstimatedCost", 0m, charge.JR_EstimatedCost);
			decimal expectedValue = 10;
			charge.JR_OSCostAmt = expectedValue;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = "WW";
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
			charge.JR_OSCostExRate = 0m;
			AssertEquals("JR_EstimatedCost", expectedValue, charge.JR_EstimatedCost);

			charge.JR_OSCostExRate = 2m;
			AssertEquals("JR_EstimatedCost", expectedValue / 2, charge.JR_EstimatedCost);

			charge.JR_OSCostExRate = 0m;
			AssertEquals("JR_EstimatedCost", expectedValue / 2, charge.JR_EstimatedCost);

			charge.JR_OSCostExRate = 3m;
			AssertEquals("JR_EstimatedCost", Utilities.Round(expectedValue / 3, JobChargeSchema.JR_EstimatedCost.Scale), charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.Code;
			charge.JR_OSCostExRate = 1m;
			AssertEquals("JR_EstimatedCost", Utilities.Round(expectedValue / 3, JobChargeSchema.JR_EstimatedCost.Scale) * 3, charge.JR_EstimatedCost);
		}

		public void TestCurencyAndExRateChangeForJR_EstimatedCost_Rounding()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0, 0);

			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;

			AssertNotNull(charge.CostExchangeRate);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.87m);

			AssertEquals("Postcondition: JR_OSCostExRate", 0.87m, charge.JR_OSCostExRate);
			charge.JR_OSCostAmt = 100M;
			AssertEquals("JR_EstimatedCost", 100m, charge.JR_EstimatedCost);

			charge.JR_RX_NKCostCurrency = TestObjectCreator.GBP.Code;

			AssertNotNull(charge.CostExchangeRate);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.91m);

			AssertEquals("Postcondition: JR_OSCostExRate", 0.91m, charge.JR_OSCostExRate);
			AssertEquals("JR_EstimatedCost", 104.60m, Utilities.Round(charge.JR_EstimatedCost, 2));

			charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.Code;

			AssertNull(charge.CostExchangeRate);

			AssertEquals("Postcondition: JR_OSCostExRate", 1m, charge.JR_OSCostExRate);
			AssertEquals("JR_EstimatedCost", 114.94m, Utilities.Round(charge.JR_EstimatedCost, 2));

			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;

			AssertNotNull(charge.CostExchangeRate);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.87m);

			AssertEquals("Postcondition: JR_OSCostExRate", 0.87m, charge.JR_OSCostExRate);
			AssertEquals("JR_EstimatedCost", 100m, Utilities.Round(charge.JR_EstimatedCost, 2));
		}

		#endregion

		public void TestJR_GB_InternalBranch_EnableAutoJobRevenueJournals_OffDoesNotRedefaultOrgProxyCostsAcounts()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var currentOrgProxy = TestObjectCreator.CreateOrgHeader("Confused", true, true, "AUSYD");
			var currentBranch = TestObjectCreator.CreateBranch("YYY", "YYYBranch", GlbCompany.CurrentCompany, currentOrgProxy);

			var anotherOrgProxy = TestObjectCreator.CreateOrgHeader("Sleepy", true, true, "AUMEL");
			TestObjectCreator.CreateBranch("ZZZ", "ZZZBranch", GlbCompany.CurrentCompany, anotherOrgProxy);
			Factory.Save();

			using (currentBranch.SetAsTemporaryContext())
			{
				var charge = (BaseCharge)GetNewBusinessObject();
				charge.JR_JH = charge.Factory.NewJobForTesting<Job>().PK;
				charge.JR_OH_CostAccount = anotherOrgProxy.PK;

				AssertEquals("Cost account should be the set org", charge.JR_OH_CostAccount, anotherOrgProxy.PK);

				charge.JR_GB_InternalBranch = currentBranch.PK;

				AssertEquals("Should not redefault org proxy as the registry setting if off", charge.JR_OH_CostAccount, anotherOrgProxy.PK);
			}
		}

		public void TestJR_GB_InternalBranch_EnableAutoJobRevenueJournalsOnRedefaultsOrgProxyCostsAcounts()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var currentOrgProxy = TestObjectCreator.CreateOrgHeader("Confused", true, true, "AUSYD");
			var currentBranch = TestObjectCreator.CreateBranch("YYY", "YYYBranch", GlbCompany.CurrentCompany, currentOrgProxy);

			var anotherOrgProxy = TestObjectCreator.CreateOrgHeader("Sleepy", true, true, "AUMEL");
			var anotherBranch = TestObjectCreator.CreateBranch("ZZZ", "ZZZBranch", GlbCompany.CurrentCompany, anotherOrgProxy);
			Factory.Save();

			using (currentBranch.SetAsTemporaryContext())
			{
				var charge = (BaseCharge)GetNewBusinessObject();
				charge.JR_JH = charge.Factory.NewJobForTesting<Job>().PK;
				charge.JR_OH_CostAccount = anotherOrgProxy.PK;

				AssertEquals("Cost account should be the set org", charge.JR_OH_CostAccount, anotherOrgProxy.PK);

				charge.JR_GB_InternalBranch = currentBranch.PK;

				AssertEquals("Should have redefaulted the org proxy with the org proxy of the internal branch", charge.JR_OH_CostAccount, currentOrgProxy.PK);

				charge.JR_GB_InternalBranch = anotherBranch.PK;

				AssertEquals("Should have redefaulted the org proxy with the org proxy of the internal branch", charge.JR_OH_CostAccount, anotherOrgProxy.PK);
			}
		}

		public void TestJR_GB_InternalBranchValidation()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var activeBranchInCurrentCompany = TestObjectCreator.CreateBranch("YYY", "YYYBranch", currentCompany);
			var inactiveBranchInCurrentCompany = TestObjectCreator.CreateBranch("ZZZ", "ZZZBranch", currentCompany);
			inactiveBranchInCurrentCompany.GB_IsActive = false;
			var branchInOtherCompany = TestObjectCreator.NonCurrentCompanyBranch;
			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = Factory.NewJobForTesting<Job>().PK;
			charge.Validation.ValidateJR_GB_InternalBranch();
			AssertNoErrors("a brand new charge should have no errors.", charge.JR_GB_InternalBranchInfo);
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

			charge.JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			charge.Validation.ValidateJR_GB_InternalBranch();
			AssertEquals("Internal Branch should not have defaulted because there is more than one branch for this creditor.", charge.Job.JH_GB, charge.JR_GB_InternalBranch);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertHasWarning(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");

			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK; // it doesn't default because there is more than one branch.
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

			charge.JR_GB_InternalBranch = ZGuid.Empty;
			AssertEquals("Cost Account should NOT be reset to empty", GlbBranch.CurrentBranch.OrgProxy.PK, charge.JR_OH_CostAccount);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertHasWarning(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");

			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_OH_SellAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			charge.Validation.ValidateJR_GB_InternalBranch();
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertHasWarning(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");

			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			charge.JR_GB_InternalBranch = ZGuid.NewZGuid();
			AssertHasError(charge.JR_GB_InternalBranchInfo, "Enter a valid Internal Branch.");
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

			charge.JR_GB_InternalBranch = activeBranchInCurrentCompany.PK;
			AssertEquals("Cost Account should be defaulted to org proxy of activeBranchInCurrentCompany", activeBranchInCurrentCompany.GB_OH_OrgProxy, charge.JR_OH_CostAccount);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

			charge.JR_GB_InternalBranch = inactiveBranchInCurrentCompany.PK;
			AssertEquals("Cost Account should NOT be defaulted to org proxy of inactiveBranchInCurrentCompany", activeBranchInCurrentCompany.GB_OH_OrgProxy, charge.JR_OH_CostAccount);
			AssertHasError(charge.JR_GB_InternalBranchInfo, "This Internal Branch is inactive - it may not be used.");
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

			charge.JR_GB_InternalBranch = branchInOtherCompany.PK;
			AssertEquals("Cost Account should NOT be defaulted to org proxy of branchInOtherCompany", activeBranchInCurrentCompany.GB_OH_OrgProxy, charge.JR_OH_CostAccount);
			AssertHasError(charge.JR_GB_InternalBranchInfo, "Enter a valid Internal Branch.");
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);

			charge.JR_GB_InternalBranch = activeBranchInCurrentCompany.PK; // this will set the cost account.
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.Validation.ValidateJR_GB_InternalBranch();
			AssertEquals(ZGuid.Empty, charge.JR_GB_InternalBranch);
			AssertNoErrors(charge.JR_GB_InternalBranchInfo);
			AssertNoWarnings(charge.JR_GB_InternalBranchInfo);
		}

		public void TestJR_SellReferenceValidation()
		{
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_SellReference = "ABC";
			AssertHasWarningContaining(charge.JR_SellReferenceInfo, "The system will group and post multiple invoices by Sell Reference when a value is entered.");
		}

		public void TestJR_GB_InternalBranchValidationOfOrgProxyBranches()
		{
			(var org1, var org2, var branch1, var branch2) = GetSetupForProxyBranches();
			var charge = GetNewBusinessObject() as BaseCharge;
			charge.JR_JH = charge.Factory.NewJobForTesting<Job>().PK;
			AssertJR_GB_InternalBranchValidationOfOrgProxyBranches(charge, org1, org2, branch1);
		}

		public void TestJR_GB_InternalBranchValidationOfOrgProxyBranches_Gateway_OrgsAreNotAgents()
		{
			(var org1, var org2, var branch1, var branch2) = GetSetupForProxyBranches();

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			using (var job = TestObjectCreator.CreateJob(consol))
			{
				job.JH_GB = branch2.PK;
				AssertJR_GB_InternalBranchValidationOfOrgProxyBranches(job.Charges.AddNew(), org1, org2, branch1);
			}
		}

		public void TestJR_GB_InternalBranchValidationOfOrgProxyBranches_Gateway_DoubleBranchOrgIsNotJobBranchProxy()
		{
			(var org1, var org2, var branch1, var branch2) = GetSetupForProxyBranches();

			var consol = TestObjectCreator.CreateGatewayConsol(sendingGatewayAgent: org1, receivingGatewayAgent: org2);
			using (var job = TestObjectCreator.CreateJob(consol))
			{
				job.JH_GB = branch1.PK;
				AssertJR_GB_InternalBranchValidationOfOrgProxyBranches(job.Charges.AddNew(), org1, org2, branch1, branch1);
			}
		}

		public void TestJR_GB_InternalBranchValidationOfOrgProxyBranches_Gateway_DoubleBranchOrgIsJobBranchProxy()
		{
			(var org1, var org2, var branch1, var branch2) = GetSetupForProxyBranches();

			var consol = TestObjectCreator.CreateGatewayConsol(sendingGatewayAgent: org1, receivingGatewayAgent: org2);
			using (var job = TestObjectCreator.CreateJob(consol))
			{
				job.JH_GB = branch2.PK;
				AssertJR_GB_InternalBranchValidationOfOrgProxyBranches(job.Charges.AddNew(), org1, org2, branch1, branch2);
			}
		}

		(OrgHeader org1, OrgHeader org2, GlbBranch branch1, GlbBranch branch2) GetSetupForProxyBranches()
		{
			var org1 = TestObjectCreator.CreateOrgHeader("Org1", false, false);
			var org2 = TestObjectCreator.CreateOrgHeader("Org2", false, false);
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, org1);
			var branch2 = TestObjectCreator.CreateBranch("B2A", "Branch2A", GlbCompany.CurrentCompany, org2);
			TestObjectCreator.CreateBranch("B2B", "Branch2B", GlbCompany.CurrentCompany, org2);

			var port1 = org1.AppointedGatewayAgentPorts.AddNew();
			port1.O5_OA_AgentOfficeAddress = org1.MainAddress.PK;
			port1.O5_PortOrCountry = "AUSYD";
			port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var port2 = org2.AppointedGatewayAgentPorts.AddNew();
			port2.O5_OA_AgentOfficeAddress = org2.MainAddress.PK;
			port2.O5_PortOrCountry = "NZAKL";
			port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			return (org1, org2, branch1, branch2);
		}

		public void AssertJR_GB_InternalBranchValidationOfOrgProxyBranches(BaseCharge charge, OrgHeader singleBranchProxy, OrgHeader doubleBranchProxy, GlbBranch singleBranch, GlbBranch expectedBranchWhenDoubleProxyOrgIsUsed = null)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompany.PK);

			using (singleBranch.SetAsTemporaryContext())
			{
				foreach ((ZPropertyInfo accountInfo, ZString accountName) in new[] { (charge.JR_OH_SellAccountInfo, "Sell"), (charge.JR_OH_CostAccountInfo, "Cost") })
				{
					AssertNoErrors(charge.JR_GB_InternalBranchInfo);

					accountInfo.Value = singleBranchProxy.PK;
					if (accountName == "Cost" &&
						(charge.Job?.Parent.GatewayAgent().sendingAgent?.PK == singleBranchProxy.PK || charge.Job?.Parent.GatewayAgent().receivingAgent?.PK == doubleBranchProxy.PK))   // If the creditor is the Sending/Receiving Gateway Agent, the Internal Fields will be set to blank (WI00494549)
					{
						AssertEquals(ZGuid.Empty, charge.JR_GB_InternalBranch);
						AssertNoErrors(charge.JR_GB_InternalBranchInfo);
						AssertHasWarningContaining(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");
					}
					else
					{
						AssertEquals(singleBranch.PK, charge.JR_GB_InternalBranch);
						AssertNoErrors(charge.JR_GB_InternalBranchInfo);
						AssertNoWarnings(charge.JR_GB_InternalBranchInfo);
						charge.JR_GB_InternalBranch = ZGuid.Empty;
						accountInfo.Value = doubleBranchProxy.PK;

						if (expectedBranchWhenDoubleProxyOrgIsUsed == null)
						{
							AssertEquals(ZGuid.Empty, charge.JR_GB_InternalBranch);
							AssertHasWarningContaining(charge.JR_GB_InternalBranchInfo, "Job Revenue Journal is posted upon Save only when Internal Branch is specified.");
							AssertNoErrors(charge.JR_GB_InternalBranchInfo);
						}
						else
						{
							AssertEquals(expectedBranchWhenDoubleProxyOrgIsUsed.PK, charge.JR_GB_InternalBranch);
							if (expectedBranchWhenDoubleProxyOrgIsUsed == singleBranch)
							{
								AssertHasErrorContaining(charge.JR_GB_InternalBranchInfo, $"The branch you have selected does not match the {accountName} Account.");
							}
							else
							{
								AssertNoErrors(charge.JR_GB_InternalBranchInfo);
							}
						}
					}

					charge.JR_GB_InternalBranch = singleBranch.PK;
					AssertEquals("Setting the branch should default the correct orgProxy", singleBranchProxy.PK, accountInfo.Value);
					AssertNoErrors("Because we have the correct org proxy and branch combination, there should be no error", charge.JR_GB_InternalBranchInfo);
					AssertNoWarnings(charge.JR_GB_InternalBranchInfo);
					accountInfo.Value = ZGuid.Empty;
				}
			}
		}

		public void TestJR_GE_InternalDeptValidation()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var job = TestObjectCreator.CreateJobHeader();
			job.JH_GE = testObjectCreator.GEADepartment.PK;

			Factory.Save();

			var activeDepartment = TestObjectCreator.FESDepartment;
			activeDepartment.GE_IsActive = true;

			var inactiveDepartment = TestObjectCreator.FISDepartment;
			inactiveDepartment.GE_IsActive = false;

			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);

			charge.JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);

			charge.JR_GE_InternalDept = ZGuid.Empty;
			AssertHasWarning(charge.JR_GE_InternalDeptInfo, "Job Revenue Journal is posted upon Save only when Internal Department is specified.");
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);

			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_OH_SellAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			charge.JR_GE_InternalDept = ZGuid.Empty;
			AssertHasWarning(charge.JR_GE_InternalDeptInfo, "Job Revenue Journal is posted upon Save only when Internal Department is specified.");
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);

			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			charge.JR_GE_InternalDept = ZGuid.NewZGuid();
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertHasError(charge.JR_GE_InternalDeptInfo, "Enter a valid Internal Department.");

			charge.JR_GE_InternalDept = activeDepartment.PK;
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);

			charge.JR_GE_InternalDept = inactiveDepartment.PK;
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertHasError(charge.JR_GE_InternalDeptInfo, "This Internal Department is inactive - it may not be used.");

			charge.JR_OH_CostAccount = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, charge.JR_GE_InternalDept);
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertNoErrors(charge.JR_GE_InternalDeptInfo);

			charge.JR_GE_InternalDept = activeDepartment.PK;
			AssertNoWarnings(charge.JR_GE_InternalDeptInfo);
			AssertHasError(charge.JR_GE_InternalDeptInfo, "You can only set the internal department when the Cost or Sell account is an organization proxy for the current company.");
		}

		/*Please create JobHeader via switching user context*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		public void TestJR_JH_InternalJobValidation()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var job = TestObjectCreator.CreateJobHeader();
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			var orgProxyNotGatewayAgent = TestObjectCreator.CreateOrgHeader("Org1", false, false);
			var testBranch1 = TestObjectCreator.CreateBranch("BR1", "Branch1", GlbCompany.CurrentCompany, orgProxyNotGatewayAgent);

			var currentCompanyJob = TestObjectCreator.CreateJobHeader();
			currentCompanyJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var nonCurrentCompanyJob = TestObjectCreator.CreateJobHeader();
			nonCurrentCompanyJob.JH_GC = TestObjectCreator.NonCurrentCompany.PK;

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gatewayJob = TestObjectCreator.CreateJob(consol, false);

			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			charge.JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
			charge.JR_JH_InternalJob = ZGuid.Empty;
			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			charge.JR_GE_InternalDept = ZGuid.Empty;
			AssertHasError(charge.JR_JH_InternalJobInfo, "You must nominate an internal job when the internal branch or department has been set.");

			charge.JR_JH_InternalJob = ZGuid.Empty;
			charge.JR_GB_InternalBranch = ZGuid.Empty;
			charge.JR_GE_InternalDept = TestObjectCreator.FESDepartment.PK;
			AssertHasError(charge.JR_JH_InternalJobInfo, "You must nominate an internal job when the internal branch or department has been set.");

			charge.JR_JH_InternalJob = job.PK;
			AssertEquals("internal branch should be defaulted", job.JH_GB, charge.JR_GB_InternalBranch);
			AssertEquals("internal dept should be defaulted", job.JH_GE, charge.JR_GE_InternalDept);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			charge.JR_JH_InternalJob = ZGuid.Invalid;
			AssertEquals("internal branch should be defaulted", job.JH_GB, charge.JR_GB_InternalBranch);
			AssertEquals("internal dept should be defaulted", job.JH_GE, charge.JR_GE_InternalDept);
			AssertHasError(charge.JR_JH_InternalJobInfo, "Enter a valid Internal Job.");

			charge.JR_JH_InternalJob = charge.JR_JH;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertAllInternalFieldsMatch();
			AssertHasWarning("Warning shown when all internal fields match", charge.JR_JH_InternalJobInfo, "Job Revenue Journal is posted upon Save only when Internal Job/-Branch/-Department is different to Charge Job/-Branch/-Department.");

			charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertNotAllInternalFieldsMatch();
			AssertNoWarnings("No warning when at least one internal field doesn't match", charge.JR_JH_InternalJobInfo);

			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = TestObjectCreator.FESDepartment.PK;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertNotAllInternalFieldsMatch();
			AssertNoWarnings("No warning when at least one internal field doesn't match", charge.JR_JH_InternalJobInfo);

			charge.JR_JH_InternalJob = currentCompanyJob.PK;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertNotAllInternalFieldsMatch();
			AssertNoWarnings("No warning when at least one internal field doesn't match", charge.JR_JH_InternalJobInfo);

			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.JR_GB = ZGuid.Empty;
			charge.JR_GB_InternalBranch = ZGuid.Empty;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertAllInternalFieldsMatch();
			AssertNoWarnings("No warning when at least one internal field is empty", charge.JR_JH_InternalJobInfo);

			using (AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_GB = ZGuid.Empty;
				charge.Validation.ValidateJR_JH_InternalJob();
				CombineAssertions("InternalFields are cleared due to overriding logic in JR_GB", () => {
					AssertInternalFieldsAreEmpty();
					AssertNotAllInternalFieldsMatch();
					AssertNoWarnings("No warning when at least one internal field is empty", charge.JR_JH_InternalJobInfo);
				});
			}

			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE = ZGuid.Empty;
			charge.JR_GE_InternalDept = ZGuid.Empty;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertAllInternalFieldsMatch();
			AssertNoWarnings("No warning when at least one internal field is empty", charge.JR_JH_InternalJobInfo);

			charge.JR_JH = gatewayJob.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.CreateOrgHeader("newOrg", true, true).PK;
			charge.JR_JH_InternalJob = charge.JR_JH;
			charge.JR_GB_InternalBranch = charge.JR_GB;
			charge.JR_GE_InternalDept = charge.JR_GE;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertAllInternalFieldsMatch();
			AssertHasWarning("Warning shown when all internal fields match", charge.JR_JH_InternalJobInfo, "Job Revenue Journal is posted upon Save only when Internal Job/-Branch/-Department is different to Charge Job/-Branch/-Department.");

			charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertAllInternalFieldsMatch();
			AssertNoWarnings("No warning when Debtor is gateway Agent", charge.JR_JH_InternalJobInfo);

			charge.JR_OH_SellAccount = ZGuid.Empty;
			charge.JR_GB_InternalBranch = ZGuid.Empty;
			charge.JR_GE_InternalDept = ZGuid.Empty;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertHasError(charge.JR_JH_InternalJobInfo, "You cannot set the internal job without also setting the internal branch or internal department.");
			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			charge.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_JH_InternalJob = currentCompanyJob.PK;
			charge.JR_GE_InternalDept = ZGuid.Empty;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertHasError(charge.JR_JH_InternalJobInfo, "You can only set the internal job when the Cost or Sell account is an organization proxy for the current company.");

			charge.JR_OH_CostAccount = orgProxyNotGatewayAgent.PK;
			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			charge.Validation.ValidateJR_JH_InternalJob();
			AssertNoErrors(charge.JR_JH_InternalJobInfo);
			AssertNoWarnings(charge.JR_JH_InternalJobInfo);

			charge.JR_JH_InternalJob = nonCurrentCompanyJob.PK;
			AssertHasError(charge.JR_JH_InternalJobInfo, "Enter a valid Internal Job.");

			charge.JR_OH_CostAccount = ZGuid.Empty;
			AssertEquals("internal job should be empty.", ZGuid.Empty, charge.JR_JH_InternalJob);
			AssertEquals("internal branch should be empty.", ZGuid.Empty, charge.JR_GB_InternalBranch);
			AssertEquals("internal dept should be empty.", ZGuid.Empty, charge.JR_GE_InternalDept);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			charge.JR_JH_InternalJob = currentCompanyJob.PK;
			AssertHasErrors(charge.JR_JH_InternalJobInfo);

			void AssertAllInternalFieldsMatch()
			{
				AssertEquals(charge.JR_JH, charge.JR_JH_InternalJob);
				AssertEquals(charge.JR_GB, charge.JR_GB_InternalBranch);
				AssertEquals(charge.JR_GE, charge.JR_GE_InternalDept);
			}

			void AssertNotAllInternalFieldsMatch()
			{
				Assert(charge.JR_JH != charge.JR_JH_InternalJob
				|| charge.JR_GB != charge.JR_GB_InternalBranch
				|| charge.JR_GE != charge.JR_GE_InternalDept);
			}

			void AssertInternalFieldsAreEmpty()
			{
				AssertEquals(ZGuid.Empty, charge.JR_JH_InternalJob);
				AssertEquals(ZGuid.Empty, charge.JR_GB_InternalBranch);
				AssertEquals(ZGuid.Empty, charge.JR_GE_InternalDept);
			}
		}

		public void TestJR_JH_InternalJobForRecognitionType()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			RevenueRecognitionCollection recognitionCollection = new RevenueRecognitionCollection();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, recognitionCollection);

			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			var chargeRevRecOverride1 = chargeCode.RevenueRecOverrides.AddNew();
			chargeRevRecOverride1.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeRevRecOverride1.AE_Direction = Constants.FreightShipmentDirection.Code.All;
			chargeRevRecOverride1.AE_Mode = Constants.TransportModes.Sea;
			chargeRevRecOverride1.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			var chargeRevRecOverride2 = chargeCode.RevenueRecOverrides.AddNew();
			chargeRevRecOverride2.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeRevRecOverride2.AE_Direction = Constants.FreightShipmentDirection.Code.All;
			chargeRevRecOverride2.AE_Mode = Constants.TransportModes.AirSea;
			chargeRevRecOverride2.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

			Factory.Save();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			Job job1 = TestObjectCreator.CreateJob(shipment1, false);

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			Job job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Job job3 = TestObjectCreator.CreateJob(shipment3, false);

			var charge = TestObjectCreator.CreateCharge(job3, chargeCode, "Charge Code 1", TestObjectCreator.AUD, 100M, GlbBranch.CurrentBranch.OrgProxy);
			charge.JR_GB_InternalBranch = job3.JH_GB;
			charge.JR_GE_InternalDept = TestObjectCreator.FESDepartment.PK;
			charge.JR_JH_InternalJob = job1.PK;
			AssertHasError(charge.JR_JH_InternalJobInfo, string.Format("You have not setup Revenue Recognition for this job type. Go to Registry -> {0} to configure Revenue Recognition.", ((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup).Location));

			charge.JR_GB_InternalBranch = job3.JH_GB;
			charge.JR_GE_InternalDept = TestObjectCreator.FESDepartment.PK;
			charge.JR_JH_InternalJob = job2.PK;
			AssertHasError(charge.JR_JH_InternalJobInfo, "Job Revenue Journal cannot be posted until the 'Actual/Estimated Arrival Date' for this internal job is recorded. This internal job and charge code combination requires this date for revenue recognition purposes.");
		}

		public void TestJR_JH_InternalJobValidation_NotAllChargeHasJRJ()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid()))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment1 = consol.Shipments.AddNew();
				var shipment2 = consol.Shipments.AddNew();
				Factory.Save();

				var apps = new ApportionmentListing(Factory, consol);
				try
				{
					var consolCost = apps.CostsCollection.TryAddNew();
					var testObjectCreator = new TestObjectCreator(Factory);
					consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
					consolCost.E6_OSCostAmount = 5163.96;
					consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
					AssertEquals(consolCost.ApportionmentCharges.Count, 2);
					var job1 = TestObjectCreator.CreateJobHeader();
					var job2 = TestObjectCreator.CreateJobHeader();

					consolCost.ApportionmentCharges[0].JR_JH = job1.PK;
					consolCost.ApportionmentCharges[0].JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
					consolCost.ApportionmentCharges[0].JR_JH_InternalJob = job2.PK;
					consolCost.ApportionmentCharges[0].JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
					consolCost.ApportionmentCharges[0].JR_GB = GlbBranch.CurrentBranch.PK;
					consolCost.ApportionmentCharges[0].JR_GB_InternalBranch = consolCost.ApportionmentCharges[0].JR_GB;
					consolCost.ApportionmentCharges[0].JR_GE = GlbDepartment.CurrentDepartment.PK;
					consolCost.ApportionmentCharges[0].JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
					consolCost.ApportionmentCharges[0].Validation.ValidateJR_JH_InternalJob();
					Assert(consolCost.ApportionmentCharges[0].ShouldCreateCostJRJ);
					AssertNoErrors(consolCost.ApportionmentCharges[0].JR_JH_InternalJobInfo);

					consolCost.ApportionmentCharges[1].JR_JH = job2.PK;
					consolCost.ApportionmentCharges[1].JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
					consolCost.ApportionmentCharges[1].JR_JH_InternalJob = job2.PK;
					consolCost.ApportionmentCharges[1].JR_GE_InternalDept = ZGuid.Empty;
					consolCost.ApportionmentCharges[1].JR_OH_CostAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
					consolCost.ApportionmentCharges[1].JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
					Assert(!consolCost.ApportionmentCharges[1].ShouldCreateCostJRJ);
					Assert(!consolCost.ApportionmentCharges[1].ShouldCreateSellJRJ);

					consolCost.ApportionmentCharges[1].Validation.ValidateJR_JH_InternalJob();
					AssertHasError(consolCost.ApportionmentCharges[1].JR_JH_InternalJobInfo, "Please ensure the Internal Job/Branch/Department is different to Charge Job/Branch/Department.");

					consolCost.ApportionmentCharges[1].JR_JH_InternalJob = job1.PK;
					consolCost.ApportionmentCharges[1].Validation.ValidateJR_JH_InternalJob();
					AssertNoErrors(consolCost.ApportionmentCharges[1].JR_JH_InternalJobInfo);
				}
				finally
				{
					apps.ReleaseMutexes();
				}
			}
		}

		public void TestJR_InternalFieldsReadonly()
		{
			var newOrgProxy = TestObjectCreator.CreateOrgHeader("TSTORG", false, false);
			var newCompany = TestObjectCreator.CreateNewCompany("ZZZ");
			var newBranch = TestObjectCreator.CreateBranch("ZZZ", "Branch ZZZ", newCompany, newOrgProxy);
			Factory.Save();

			using (newBranch.SetAsTemporaryContext())
			{
				AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

				var charge1 = (BaseCharge)GetNewBusinessObject();
				var charge2 = (BaseCharge)GetNewBusinessObject();
				var job = charge1.Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				charge1.JR_JH = job.PK;
				charge2.JR_JH = job.PK;

				AssertEquals("Default value for JR_GB_InternalBranch should be empty", ZGuid.Empty, charge1.JR_GB_InternalBranch);
				AssertEquals("Default value for JR_GE_InternalDept should be empty", ZGuid.Empty, charge1.JR_GE_InternalDept);
				AssertEquals("Default value for JR_JH_InternalJob should be empty", ZGuid.Empty, charge1.JR_JH_InternalJob);

				AssertEquals("JR_GB_InternalBranch is readonly by default", true, charge1.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals("JR_GE_InternalDept is readonly by default", true, charge1.JR_GE_InternalDept_ReadOnly_ForTestOnly);
				AssertEquals("JR_JH_InternalJob is readonly by default", true, charge1.JR_JH_InternalJob_ReadOnly_ForTestOnly);

				charge1.JR_OH_CostAccount = newBranch.OrgProxy.PK;
				AssertEquals("JR_GB_InternalBranch should be set based on the org proxy", newBranch.PK, charge1.JR_GB_InternalBranch);
				AssertEquals("JR_GE_InternalDept should be set", job.JH_GE, charge1.JR_GE_InternalDept);
				AssertEquals("JR_JH_InternalJob should be set", job.PK, charge1.JR_JH_InternalJob);

				AssertEquals("JR_GB_InternalBranch should NOT be readonly", false, charge1.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals("JR_GE_InternalDept should NOT be readonly", false, charge1.JR_GE_InternalDept_ReadOnly_ForTestOnly);
				AssertEquals("JR_JH_InternalJob should NOT be readonly", false, charge1.JR_JH_InternalJob_ReadOnly_ForTestOnly);

				charge1.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;
				AssertEquals("Precondition: IsRevenuePosted", true, charge1.IsRevenuePosted);
				AssertEquals("Precondition: IsCostPosted", false, charge1.IsCostPosted);
				AssertEquals("JR_GB_InternalBranch should be set based on the org proxy", newBranch.PK, charge1.JR_GB_InternalBranch);
				AssertEquals("JR_GE_InternalDept should should be set", job.JH_GE, charge1.JR_GE_InternalDept);
				AssertEquals("JR_JH_InternalJob should should be set", job.PK, charge1.JR_JH_InternalJob);

				AssertEquals("JR_GB_InternalBranch should NOT be readonly when cost account is proxy and cost not posted", false, charge1.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals("JR_GE_InternalDept should NOT be readonly when cost account is proxy and cost not posted", false, charge1.JR_GE_InternalDept_ReadOnly_ForTestOnly);
				AssertEquals("JR_JH_InternalJob should NOT be readonly when cost account is proxy and cost not posted", false, charge1.JR_JH_InternalJob_ReadOnly_ForTestOnly);

				charge2.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;
				charge2.JR_OH_SellAccount = newBranch.OrgProxy.PK;

				AssertEquals("Precondition: IsRevenuePosted", false, charge2.IsRevenuePosted);
				AssertEquals("Precondition: IsCostPosted", true, charge2.IsCostPosted);
				AssertEquals("JR_GB_InternalBranch should be set based on the org proxy", newBranch.PK, charge2.JR_GB_InternalBranch);
				AssertEquals("JR_GE_InternalDept should should be set", job.JH_GE, charge2.JR_GE_InternalDept);
				AssertEquals("JR_JH_InternalJob should should be set", job.PK, charge2.JR_JH_InternalJob);

				AssertEquals("JR_GB_InternalBranch should NOT be readonly when sell account is proxy and revenue not posted", false, charge2.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals("JR_GE_InternalDept should NOT be readonly when sell account is proxy and revenue not posted", false, charge2.JR_GE_InternalDept_ReadOnly_ForTestOnly);
				AssertEquals("JR_JH_InternalJob should NOT be readonly when sell account is proxy and revenue not posted", false, charge2.JR_JH_InternalJob_ReadOnly_ForTestOnly);

				charge2.JR_OH_CostAccount = newBranch.OrgProxy.PK;
				AssertEquals("JR_GB_InternalBranch should be readonly when both account is proxy", true, charge2.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals("JR_GE_InternalDept should be readonly when both account is proxy", true, charge2.JR_GE_InternalDept_ReadOnly_ForTestOnly);
				AssertEquals("JR_JH_InternalJob should be readonly when both account is proxy", true, charge2.JR_JH_InternalJob_ReadOnly_ForTestOnly);
			}
		}

		public void TestJR_InternalFieldsReadonlyWhenConsumerTypeShouldNotCreateCostOrSellJRJ()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var spotQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			var jobForSpotQuote = TestObjectCreator.CreateJob(spotQuote, false);
			var chargeForSpotQuote = TestObjectCreator.CreateCharge(jobForSpotQuote, TestObjectCreator.CC1, "Spot Quote Job Charge", TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 10M, null);

			Assert(!jobForSpotQuote.IsConsumerTypeShouldCreateCostJRJ);
			Assert(!jobForSpotQuote.IsConsumerTypeShouldCreateSellJRJ);

			AssertEquals("Default value for JR_GB_InternalBranch should be empty", ZGuid.Empty, chargeForSpotQuote.JR_GB_InternalBranch);
			AssertEquals("Default value for JR_GE_InternalDept should be empty", ZGuid.Empty, chargeForSpotQuote.JR_GE_InternalDept);
			AssertEquals("Default value for JR_JH_InternalJob should be empty", ZGuid.Empty, chargeForSpotQuote.JR_JH_InternalJob);

			AssertEquals("JR_GB_InternalBranch should be readonly", true, chargeForSpotQuote.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
			AssertEquals("JR_GE_InternalDept should be readonly", true, chargeForSpotQuote.JR_GE_InternalDept_ReadOnly_ForTestOnly);
			AssertEquals("JR_JH_InternalJob should be readonly", true, chargeForSpotQuote.JR_JH_InternalJob_ReadOnly_ForTestOnly);
		}

		public void Test_ReadOnlyWhenGatewaySellPostedWithJobRevenueJournal()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);
			var setup = creator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = creator.FRT.PK;
				charge.JR_OH_SellAccount = setup.senAg.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				charge.JR_JH_InternalJob = gatewayJob.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Factory.Save();

				Assert(charge.IsRevenuePostedWithAutoJobRevenueJournal);

				Assert(charge.JR_JH_InternalJob_ReadOnly_ForTestOnly);
				Assert(charge.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				Assert(charge.JR_GE_InternalDept_ReadOnly_ForTestOnly);
				Assert(charge.JR_SellGovtChargeCode_ReadOnly_ForTestOnly);
				Assert(charge.JR_ACInfo.ReadOnly);
				Assert(charge.JR_AT_SellGSTRate_ReadOnly_ForTestOnly);
				Assert(charge.JR_OSSellAmtInfo.ReadOnly);
				Assert(charge.JR_OH_SellAccount_ReadOnly_ForTestOnly);
				Assert(charge.JR_InvoiceTypeInfo.ReadOnly);
				Assert(charge.JR_DisplaySequenceInfo.ReadOnly);
			}
		}

		public void TestSetSellCostCurrencyWithFallbackToLocalCurrency()
		{
			var org1 = TestObjectCreator.AALSHI;
			var org2 = TestObjectCreator.ABIGAS;

			TestCharge.JR_OH_CostAccount = org1.PK;
			TestCharge.JR_OH_SellAccount = org2.PK;
			AssertEquals(Core.Constants.CurrencyCodes.Australia, TestCharge.JR_RX_NKCostCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, TestCharge.JR_RX_NKSellCurrency);

			org1.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
			org1.CompanyData.OB_RX_NKAPDefltCurrency = "THB";
			org2.CompanyData.OB_RX_NKARDDefltCurrency = "KRW";
			org2.CompanyData.OB_RX_NKAPDefltCurrency = "EUR";
			TestCharge.JR_OH_CostAccount = org2.PK;
			TestCharge.JR_OH_SellAccount = org1.PK;
			AssertEquals("AP Default currency", Core.Constants.CurrencyCodes.EuropeanUnion, TestCharge.JR_RX_NKCostCurrency);
			AssertEquals("AR Default currency", Core.Constants.CurrencyCodes.UnitedStates, TestCharge.JR_RX_NKSellCurrency);

			//Mimick autorating
			TestCharge.JR_CostRated = true;
			TestCharge.JR_SellRated = true;
			TestCharge.JR_OH_CostAccount = org1.PK;
			TestCharge.JR_OH_SellAccount = org2.PK;
			AssertEquals("Currency does not change because it is auto rated", Core.Constants.CurrencyCodes.EuropeanUnion, TestCharge.JR_RX_NKCostCurrency);
			AssertEquals("Currency does not change because it is auto rated", Core.Constants.CurrencyCodes.UnitedStates, TestCharge.JR_RX_NKSellCurrency);
		}

		public void TestSetRevenueCalculationDescriptionDoesNotRepeat()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.JR_JH = job.PK;
			TestCharge.JR_Desc = @"
Estimated Duties and Fees
  Duty                                      235.00
  Merchandise Processing Fee                 25.00
";

			AutoRateInfo info = new AutoRateInfo(Factory);
			info.AdditionalInvoiceLineDescription = @"
  Duty                                      235.00
  Merchandise Processing Fee                 25.00
";

			AssertEquals(@"
Estimated Duties and Fees
  Duty                                      235.00
  Merchandise Processing Fee                 25.00
", TestCharge.GetCombinedDescription(info));
		}

		public void TestIsAllowedToOverrideSellTaxMessage()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			AssertNull(TestCharge.InvoicingJob);
			AssertEquals(false, TestCharge.IsAllowedToOverrideSellTaxMessage);

			TestCharge.JR_JH = job.PK;
			AssertNotNull(TestCharge.InvoicingJob);

			var shipmentAllowOverrideSellTaxMsg = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxMsg);
			shipmentAllowOverrideSellTaxMsg.IsAllowed = true;
			AssertEquals("You should be able to override the sell tax msg. ", true, TestCharge.IsAllowedToOverrideSellTaxMessage);
			shipmentAllowOverrideSellTaxMsg.IsAllowed = false;
			AssertEquals("You should not be able to override the sell tax msg. ", false, TestCharge.IsAllowedToOverrideSellTaxMessage);
		}

		public void TestIsAllowedToOverrideCostTaxId()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.JR_JH = job.PK;
			if (TestCharge.JR_OH_CostAccount.IsEmpty)
			{
				TestCharge.JR_OH_CostAccount = testObjectCreator.ABIGAS.PK;
			}
			TestCharge.CostAccount.CompanyData.SetAPTaxApplicable(true);
			TestCharge.Company.GC_IsGSTRegistered = true;
			AssertNotNull(TestCharge.InvoicingJob);

			var invoicingAllowOverrideCostTaxIdCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxId);

			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				invoicingAllowOverrideCostTaxIdCheckPoint.IsAllowed = true;
				if (TestCharge.JR_IsPosted || TestCharge.JR_IsApportioned)
				{
					AssertEquals("Not allow to override when charge is apportioned or posted.", true, TestCharge.IsCostGSTFieldReadOnly);
				}
				else
				{
					AssertEquals("Allow to override only when registry and security right are configured.", false, TestCharge.IsCostGSTFieldReadOnly);
				}
				invoicingAllowOverrideCostTaxIdCheckPoint.IsAllowed = false;
				AssertEquals("Not allow to override.", true, TestCharge.IsCostGSTFieldReadOnly);
			}

			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				invoicingAllowOverrideCostTaxIdCheckPoint.IsAllowed = true;
				AssertEquals("Not allow to override.", true, TestCharge.IsCostGSTFieldReadOnly);
				invoicingAllowOverrideCostTaxIdCheckPoint.IsAllowed = false;
				AssertEquals("Not allow to override.", true, TestCharge.IsCostGSTFieldReadOnly);
			}
		}

		public void TestIsAllowedToOverrideSellTaxId()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.JR_JH = job.PK;
			if (TestCharge.JR_OH_SellAccount.IsEmpty)
			{
				TestCharge.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			}
			TestCharge.SellAccount.CompanyData.SetARTaxApplicable(true);
			TestCharge.Company.GC_IsGSTRegistered = true;
			AssertNotNull(TestCharge.InvoicingJob);

			var invoicingAllowOverrideSellTaxIdCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxId);

			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				invoicingAllowOverrideSellTaxIdCheckPoint.IsAllowed = true;
				if (TestCharge.JR_IsPosted || TestCharge.JR_IsApportioned)
				{
					AssertEquals("Not allow to override when charge is apportioned or posted.", true, TestCharge.IsSellGSTFieldReadOnly_ForTestOnly);
				}
				else
				{
					AssertEquals("Allow to override only when registry and security right are configured.", false, TestCharge.IsSellGSTFieldReadOnly_ForTestOnly);
				}
				invoicingAllowOverrideSellTaxIdCheckPoint.IsAllowed = false;
				AssertEquals("Not allow to override.", true, TestCharge.IsSellGSTFieldReadOnly_ForTestOnly);
			}

			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				invoicingAllowOverrideSellTaxIdCheckPoint.IsAllowed = true;
				AssertEquals("Not allow to override.", true, TestCharge.IsSellGSTFieldReadOnly_ForTestOnly);
				invoicingAllowOverrideSellTaxIdCheckPoint.IsAllowed = false;
				AssertEquals("Not allow to override.", true, TestCharge.IsSellGSTFieldReadOnly_ForTestOnly);
			}
		}

		public void TestIsCommentChargeCode()
		{
			TestCharge.JR_AC = ZGuid.Empty;
			Assert(!TestCharge.IsCommentChargeCode);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestCharge.JR_AC = chargeCode.PK;
			Assert(TestCharge.IsCommentChargeCode);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(!TestCharge.IsCommentChargeCode);
		}

		public void TestIsAllowedToOverrideCostTaxMessage()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			AssertNull(TestCharge.InvoicingJob);
			AssertEquals(false, TestCharge.IsAllowedToOverrideCostTaxMessage);

			TestCharge.JR_JH = job.PK;
			AssertNotNull(TestCharge.InvoicingJob);

			var shipmentAllowOverrideCostTaxMsg = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxMsg);
			shipmentAllowOverrideCostTaxMsg.IsAllowed = true;
			AssertEquals("You should be able to override the sell tax msg. ", true, TestCharge.IsAllowedToOverrideCostTaxMessage);
			shipmentAllowOverrideCostTaxMsg.IsAllowed = false;
			AssertEquals("You should not be able to override the sell tax msg. ", false, TestCharge.IsAllowedToOverrideCostTaxMessage);
		}

		public void TestSetSellAccount_Updates_JR_OA_SellInvoiceAddress()
		{
			TestCharge.JR_OH_SellAccount = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, TestCharge.JR_OA_SellInvoiceAddress);
			AssertEquals(ZGuid.Empty, TestCharge.DisplaySellInvoiceAddress);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();

			TestCharge.JR_OH_SellAccount = org1.PK;
			AssertEquals("expect JR_OA_SellInvoiceAddress empty because it's default address", ZGuid.Empty, TestCharge.JR_OA_SellInvoiceAddress);
			AssertEquals(org1.MainAddress.PK, TestCharge.DisplaySellInvoiceAddress);

			TestCharge.JR_OH_SellAccount = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, TestCharge.JR_OA_SellInvoiceAddress);
			AssertEquals(ZGuid.Empty, TestCharge.DisplaySellInvoiceAddress);
		}

		public void TestAPInvoiceDateOnAppChargeSetToDifferentValueThanOnPostedParentConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "test001");
			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			var cost = TestObjectCreator.CreateConsolCost(consol, chargeCode);
			var charge = cost.ApportionmentCharges.AddNew();
			cost.E6_InvoiceDate = ZDateTime.Now;
			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TestInvoice", TestObjectCreator.AUD, 1.0m, 200m, 0m, 0m, 500m, 0m, 0m, TestObjectCreator.Creditor1);
			cost.E6_AH_APInvoice = transaction.PK;

			AssertEquals(charge.JR_APInvoiceDate, cost.E6_InvoiceDate);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);

			charge.JR_APInvoiceDate = ZDateTime.Now.AddMinutes(-5);

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("APInvoiceDateOnAppChargeSetToDifferentValueThanOnPostedParentConsolCost", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestDisplaySellInvoiceAddress()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			BaseCharge charge1 = Factory.NewWithValidTestData<BaseCharge>();
			charge1.JR_OH_SellAccount = org1.PK;
			charge1.JR_OA_SellInvoiceAddress = ZGuid.Empty;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseCharge chargeReloaded = newFactory.Load<BaseCharge>(charge1.PK);
			AssertEquals("charge will use DefaultSellAddress if sell address is empty", org1.MainAddress.PK, chargeReloaded.DisplaySellInvoiceAddress);

			OrgAddress newAddress = Factory.NewWithValidTestData<OrgAddress>();
			newAddress.OA_OH = org1.PK;
			newAddress.OA_Code = "ABC";
			charge1.DisplaySellInvoiceAddress = newAddress.PK;
			Factory.Save();

			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			BaseCharge chargeReloaded2 = newFactory2.Load<BaseCharge>(charge1.PK);
			AssertEquals("charge will use override address if override address is not empty", newAddress.PK, chargeReloaded2.DisplaySellInvoiceAddress);
		}

		public void TestDisplaySellInvoiceAddressDefaulting()
		{
			var localClient = TestObjectCreator.LocalClient;
			var localClientAddress = TestObjectCreator.CreateAddress(localClient);
			var agent = TestObjectCreator.Agent;
			var agentAddress = TestObjectCreator.CreateAddress(agent);
			var debtor = TestObjectCreator.Debtor;
			var job = TestObjectCreator.CreateJob("1", localClient, 0, agent, 0);

			var chargeLocalClient = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			chargeLocalClient.JR_OH_SellAccount = localClient.PK;
			var chargeAgent = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			chargeAgent.JR_OH_SellAccount = agent.PK;
			var chargeOtherDebtor = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			chargeOtherDebtor.JR_OH_SellAccount = debtor.PK;

			AssertEquals("Precondition: job Local Client Address", localClient.MainAddress.PK, job.JH_OA_LocalChargesAddr);
			AssertEquals("Precondition: job Agent Address", agent.MainAddress.PK, job.JH_OA_AgentCollectAddr);
			AssertEquals("Precondition: Display Local Client charge Sell Address", localClient.MainAddress.PK, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: Display Agent charge Sell Address", agent.MainAddress.PK, chargeAgent.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: Display Other debtor charge Sell Address", debtor.AddressForSendingARDocuments.PK, chargeOtherDebtor.DisplaySellInvoiceAddress);
			AssertEquals("Local Client charge Sell Address should not be set", ZGuid.Empty, chargeLocalClient.JR_OA_SellInvoiceAddress);
			AssertEquals("Agent charge Sell Address should not be set", ZGuid.Empty, chargeAgent.JR_OA_SellInvoiceAddress);
			AssertEquals("Other debtor charge Sell Address should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OA_SellInvoiceAddress);

			job.JH_OA_LocalChargesAddr = localClientAddress.PK;
			AssertEquals("Display Local Client charge Sell Address should be updated", localClientAddress.PK, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Display Agent charge Sell Address should not be updated", agent.MainAddress.PK, chargeAgent.DisplaySellInvoiceAddress);
			AssertEquals("Display Other debtor charge Sell Address should not be updated", debtor.AddressForSendingARDocuments.PK, chargeOtherDebtor.DisplaySellInvoiceAddress);
			AssertEquals("Local Client charge Sell Address should not be set", ZGuid.Empty, chargeLocalClient.JR_OA_SellInvoiceAddress);
			AssertEquals("Agent charge Sell Address should not be set", ZGuid.Empty, chargeAgent.JR_OA_SellInvoiceAddress);
			AssertEquals("Other debtor charge Sell Address should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OA_SellInvoiceAddress);

			job.JH_OA_AgentCollectAddr = agentAddress.PK;
			AssertEquals("Display Local Client charge Sell Address should not be updated", localClientAddress.PK, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Display Agent charge Sell Address should be updated", agentAddress.PK, chargeAgent.DisplaySellInvoiceAddress);
			AssertEquals("Display Other debtor charge Sell Address should not be updated", debtor.AddressForSendingARDocuments.PK, chargeOtherDebtor.DisplaySellInvoiceAddress);
			AssertEquals("Local Client charge Sell Address should not be set", ZGuid.Empty, chargeLocalClient.JR_OA_SellInvoiceAddress);
			AssertEquals("Agent charge Sell Address should not be set", ZGuid.Empty, chargeAgent.JR_OA_SellInvoiceAddress);
			AssertEquals("Other debtor charge Sell Address should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OA_SellInvoiceAddress);

			var expectedAddress = localClientAddress.PK;
			chargeLocalClient.DisplaySellInvoiceAddress = expectedAddress;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			AssertNotEquals("Precontition: JH_OA_LocalChargesAddr should be changed", expectedAddress, job.JH_OA_LocalChargesAddr);
			AssertEquals("Display Local Client charge Sell Address should not be changed once user changed it manually", expectedAddress, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Local Client charge Sell Address should be set", expectedAddress, chargeLocalClient.JR_OA_SellInvoiceAddress);

			expectedAddress = localClient.MainAddress.PK;
			chargeLocalClient.DisplaySellInvoiceAddress = expectedAddress;
			job.JH_OA_LocalChargesAddr = localClientAddress.PK;
			AssertNotEquals("Precontition: JH_OA_LocalChargesAddr should be changed", expectedAddress, job.JH_OA_LocalChargesAddr);
			AssertEquals("Display Local Client charge Sell Address should not be changed once user changed it manually", expectedAddress, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Local Client charge Sell Address should be set", expectedAddress, chargeLocalClient.JR_OA_SellInvoiceAddress);

			chargeLocalClient.DisplaySellInvoiceAddress = ZGuid.Empty;
			AssertEquals("Display Local Client charge Sell Address should have default value once user reset it", localClientAddress.PK, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Local Client charge Sell Address should be reset", ZGuid.Empty, chargeLocalClient.JR_OA_SellInvoiceAddress);
		}

		public void TestDisplaySellInvoiceContactDefaulting()
		{
			var localClient = TestObjectCreator.LocalClient;
			var localClientContact1 = TestObjectCreator.CreateContact(localClient, "Ben");
			var localClientContact2 = TestObjectCreator.CreateContact(localClient, "Nick");
			var agent = TestObjectCreator.Agent;
			var debtor = TestObjectCreator.Debtor;
			var job = TestObjectCreator.CreateJob("1", localClient, 0, agent, 0);

			var chargeLocalClient = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			chargeLocalClient.JR_OH_SellAccount = localClient.PK;
			var chargeAgent = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			chargeAgent.JR_OH_SellAccount = agent.PK;
			var chargeOtherDebtor = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			chargeOtherDebtor.JR_OH_SellAccount = debtor.PK;

			AssertEquals("Precondition: job Local Client Contact", ZGuid.Empty, job.JH_OC_LocalBillingContact);
			AssertEquals("Precondition: Display Local Client charge Sell Contact", ZGuid.Empty, chargeLocalClient.DisplaySellInvoiceContact);
			AssertEquals("Precondition: Display Agent charge Sell Contact", ZGuid.Empty, chargeAgent.DisplaySellInvoiceContact);
			AssertEquals("Precondition: Display Other debtor charge Sell Contact", ZGuid.Empty, chargeOtherDebtor.DisplaySellInvoiceContact);
			AssertEquals("Local Client charge Sell Contact should not be set", ZGuid.Empty, chargeLocalClient.JR_OC_SellInvoiceContact);
			AssertEquals("Agent charge Sell Contact should not be set", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
			AssertEquals("Other debtor charge Sell Contact should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OC_SellInvoiceContact);

			job.JH_OC_LocalBillingContact = localClientContact1.PK;
			AssertEquals("Display Local Client charge Sell Contact should be updated", localClientContact1.PK, chargeLocalClient.DisplaySellInvoiceContact);
			AssertEquals("Display Agent charge Sell Contact", ZGuid.Empty, chargeAgent.DisplaySellInvoiceContact);
			AssertEquals("Display Other debtor charge Sell Contact", ZGuid.Empty, chargeOtherDebtor.DisplaySellInvoiceContact);
			AssertEquals("Local Client charge Sell Contact should not be set", ZGuid.Empty, chargeLocalClient.JR_OC_SellInvoiceContact);
			AssertEquals("Agent charge Sell Contact should not be set", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
			AssertEquals("Other debtor charge Sell Contact should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OC_SellInvoiceContact);

			var expectedContact = localClientContact2.PK;
			chargeLocalClient.DisplaySellInvoiceContact = expectedContact;
			job.JH_OC_LocalBillingContact = localClientContact1.PK;
			AssertNotEquals("Precontition: JH_OC_LocalBillingContact should be changed", expectedContact, job.JH_OC_LocalBillingContact);
			AssertEquals("Display Local Client charge Sell Contact should not be changed once user changed it manually", expectedContact, chargeLocalClient.DisplaySellInvoiceContact);
			AssertEquals("Display Agent charge Sell Contact", ZGuid.Empty, chargeAgent.DisplaySellInvoiceContact);
			AssertEquals("Display Other debtor charge Sell Contact", ZGuid.Empty, chargeOtherDebtor.DisplaySellInvoiceContact);
			AssertEquals("Local Client charge Sell Contact should be set", expectedContact, chargeLocalClient.JR_OC_SellInvoiceContact);
			AssertEquals("Agent charge Sell Contact should not be set", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
			AssertEquals("Other debtor charge Sell Contact should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OC_SellInvoiceContact);

			chargeLocalClient.DisplaySellInvoiceContact = ZGuid.Empty;
			AssertEquals("Display Local Client charge Sell Contact should have default value once user reset it", localClientContact1.PK, chargeLocalClient.DisplaySellInvoiceContact);
			AssertEquals("Local Client charge Sell Contact should be reset", ZGuid.Empty, chargeLocalClient.JR_OC_SellInvoiceContact);

			job.JH_OC_LocalBillingContact = localClientContact2.PK;
			AssertEquals("Display Local Client charge Sell Contact should be updated", localClientContact2.PK, chargeLocalClient.DisplaySellInvoiceContact);
			AssertEquals("Display Agent charge Sell Contact", ZGuid.Empty, chargeAgent.DisplaySellInvoiceContact);
			AssertEquals("Display Other debtor charge Sell Contact", ZGuid.Empty, chargeOtherDebtor.DisplaySellInvoiceContact);
			AssertEquals("Local Client charge Sell Contact should not be set", ZGuid.Empty, chargeLocalClient.JR_OC_SellInvoiceContact);
			AssertEquals("Agent charge Sell Contact should not be set", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
			AssertEquals("Other debtor charge Sell Contact should not be set", ZGuid.Empty, chargeOtherDebtor.JR_OC_SellInvoiceContact);

			TestObjectCreator.CreateRevenueLine(chargeLocalClient, Factory.New<ARInvoice>().PK);
			Assert("Precondition: chargeLocalClient.IsRevenuePosted", chargeLocalClient.IsRevenuePosted);

			job.JH_OC_LocalBillingContact = localClientContact1.PK;
			AssertEquals("Display Local Client charge Sell Contact should not be defaulted once it posted", ZGuid.Empty, chargeLocalClient.DisplaySellInvoiceContact);
			AssertEquals("Local Client charge Sell Contact should not be set", ZGuid.Empty, chargeLocalClient.JR_OC_SellInvoiceContact);
		}

		public void TestDisplaySellInvoiceAddresses()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_OH_SellAccount = orgHeader.PK;
			Factory.Save();

			OrgAddressDependentCollection collection = charge.DisplaySellInvoiceAddresses;
			int addressCountBefore = collection.Count;

			OrgAddress address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Code = "Code1";
			OrgAddress address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Code = "Code2";
			OrgAddress address3 = Factory.NewWithValidTestData<OrgAddress>();
			address3.OA_Code = "Code3";
			OrgAddress address4 = Factory.NewWithValidTestData<OrgAddress>();
			address4.OA_Code = "Code4";
			address4.OA_IsActive = false;

			address1.OA_OH = orgHeader.PK;
			address2.OA_OH = orgHeader.PK;
			address3.OA_OH = orgHeader.PK;
			address4.OA_OH = orgHeader.PK;

			Factory.Save();

			collection = charge.DisplaySellInvoiceAddresses;
			AssertEquals("Expected 3 more addresses in collection", addressCountBefore + 3, collection.Count);
			Assert("First address should be in collection", collection.Contains(address1));
			Assert("Second address should be in collection", collection.Contains(address2));
			Assert("Third address should be in collection", collection.Contains(address3));
		}

		public void TestDisplaySellInvoiceContacts()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_OH_SellAccount = orgHeader.PK;
			Factory.Save();

			OrgContactDependentCollection collection = charge.DisplaySellInvoiceContacts;
			int contactCountBefore = collection.Count;

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Code1";
			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Code2";
			OrgContact contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Code3";
			OrgContact contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "Code4";
			contact4.OC_IsActive = false;

			contact1.OC_OH = orgHeader.PK;
			contact2.OC_OH = orgHeader.PK;
			contact3.OC_OH = orgHeader.PK;
			contact4.OC_OH = orgHeader.PK;

			Factory.Save();

			collection = charge.DisplaySellInvoiceContacts;
			AssertEquals("Expected 3 more contacts in collection", contactCountBefore + 3, collection.Count);
			Assert("First contact should be in collection", collection.Contains(contact1));
			Assert("Second contact should be in collection", collection.Contains(contact2));
			Assert("Third contact should be in collection", collection.Contains(contact3));
		}

		public void TestDisplaySellInvoiceAddress_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.JR_JH = job.PK;
			TestCharge.JR_OH_SellAccount = ZGuid.Empty;
			AssertEquals("must be readonly if Sell Account is missing", true, TestCharge.DisplaySellInvoiceAddress_ReadOnly);

			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsActive, true);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			TestCharge.JR_OH_SellAccount = orgHeader.PK;
			AssertEquals("must NOT be readonly if Sell Account is NOT missing", false, TestCharge.DisplaySellInvoiceAddress_ReadOnly);

			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine arInvLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
			arInvLine.AL_OSAmount = 200m;
			TestCharge.JR_AL_ARLine = arInvLine.PK;
			AssertEquals(true, TestCharge.JR_IsRevenuePosted);
			AssertEquals("must be readonly if charge is posted", true, TestCharge.DisplaySellInvoiceAddress_ReadOnly);
		}

		public void TestJR_A9_SellVATClass_ReadOnly()
		{
			var securityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxMsg);
			var cachedModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var registry = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage;
			var cachedValue = securityRight.IsAllowed;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			using (new DisposableAction(() => { }, () => securityRight.IsAllowed = cachedValue))
			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCharge.JR_JH = job.PK;
				TestCharge.JR_OH_SellAccount = testObjectCreator.ActiveOrg.PK;
				securityRight.IsAllowed = true;

				TestCharge.JR_AT_SellGSTRate = ZGuid.Empty;
				AssertEquals("must be readonly if tax id is missing", true, TestCharge.JR_A9_SellVATClass_ReadOnly);

				TestCharge.SellAccount.CompanyData.SetARTaxApplicable(false);
				TestCharge.JR_AT_SellGSTRate = testObjectCreator.GST1.PK;
				TestCharge.JR_AL_ARLine = ZGuid.Empty;
				AssertEquals("Should be readonly when org company is not AR tax applicable", true, TestCharge.JR_A9_SellVATClass_ReadOnly);

				TestCharge.SellAccount.CompanyData.SetARTaxApplicable(true);
				AssertEquals("Editable when all condition met", false, TestCharge.JR_A9_SellVATClass_ReadOnly);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				var oldStatus = job.JH_Status;
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedModifyRight))
				{
					AssertEquals("Should be readonly when related job is 'JFC' and didn't has security right", true, TestCharge.JR_A9_SellVATClass_ReadOnly);
				}

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedModifyRight))
				{
					AssertEquals("Should be readonly when related job is 'JFC' and security right", false, TestCharge.JR_A9_SellVATClass_ReadOnly);
				}

				job.JH_Status = oldStatus;
				Factory.Save();

				using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Should be readonly when registry is disabled", true, TestCharge.JR_A9_SellVATClass_ReadOnly);
				}

				securityRight.IsAllowed = false;
				AssertEquals("must be readonly if no security right", true, TestCharge.JR_A9_SellVATClass_ReadOnly);
				securityRight.IsAllowed = true;

				ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				ARInvoiceLine arInvLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine.AL_OSAmount = 200m;
				TestCharge.JR_AL_ARLine = arInvLine.PK;
				AssertEquals(true, TestCharge.JR_IsRevenuePosted);
				AssertEquals("must be readonly if charge is posted", true, TestCharge.JR_A9_SellVATClass_ReadOnly);
			}
		}

		public void TestDisplaySellInvoiceAddress_ZAddress()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			TestCharge.JR_OH_SellAccount = org1.PK;
			var address = TestCharge.DisplaySellInvoiceAddress_ZAddress;

			AssertEquals(org1.PK, address.OrgPK);
			AssertEquals(AddressType.ARM, address.DefaultAddressType);
			AssertEquals(org1.Address_List.ARMAddressOrFallback, address.AddressFK);
		}

		public void TestJR_A9_SellVATClass_ReadOnlyWhenGatewaySellPostedWithJobRevenueJournal()
		{
			var securityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.GatewayConsolJobInvoicing, SecurityCore.AllowOverrideSellTaxMsg);
			var registry = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage;
			AssertEquals("Pre-conditoin", true, securityRight.IsAllowed);

			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var job = TestObjectCreator.SetupGatewayLegacyJobAndEnableJRJ())
			{
				var accTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
				accTransactionHeader.AH_TransactionType = TransactionTypes.JobRevenueJournal;
				var accTransactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				accTransactionLine.AL_AH = accTransactionHeader.PK;

				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();
				var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
				GlbCompany.CurrentCompany.OrgProxy.CompanyData.SetAPTaxApplicable(true);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				charge1.JR_E6_GatewaySellHeader = consolCost.PK;
				charge1.JR_JH_InternalJob = job.PK;
				charge1.JR_GE_InternalDept = charge1.JR_GE;
				charge1.JR_GB_InternalBranch = charge1.JR_GB;
				charge1.JR_JH_InternalJob = job.PK;

				var charge2 = job.Charges.AddNew();
				charge2.JR_AL_ARLine = accTransactionLine.PK;
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_E6_GatewaySellHeader = consolCost.PK;

				Assert(charge1.JR_A9_SellVATClass_ReadOnly);
			}
		}

		public void TestJR_A9_CostVATClass_ReadOnly()
		{
			var securityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxMsg);
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var registry = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage;
			var cachedValue = securityRight.IsAllowed;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			using (new DisposableAction(() => { }, () => securityRight.IsAllowed = cachedValue))
			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestCharge.JR_JH = job.PK;
				TestCharge.JR_OH_CostAccount = testObjectCreator.ActiveOrg.PK;
				TestCharge.CostAccount.CompanyData.SetAPTaxApplicable(false);
				securityRight.IsAllowed = true;

				TestCharge.JR_AT_CostGSTRate = ZGuid.Empty;
				AssertEquals("must be readonly if tax id is missing", true, TestCharge.JR_A9_CostVATClass_ReadOnly);

				TestCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
				TestCharge.JR_AL_APLine = ZGuid.Empty;
				AssertEquals("Should be readonly when org company is not AP tax applicable", true, TestCharge.JR_A9_CostVATClass_ReadOnly);

				TestCharge.CostAccount.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);
				AssertEquals("Editable when all condition met", false, TestCharge.JR_A9_CostVATClass_ReadOnly);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				var oldStatus = job.JH_Status;
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					AssertEquals("Should be readonly when related job is 'JFC' and didn't has security right", true, TestCharge.JR_A9_CostVATClass_ReadOnly);
				}

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					AssertEquals("Should be readonly when related job is 'JFC' and security right", false, TestCharge.JR_A9_CostVATClass_ReadOnly);
				}

				job.JH_Status = oldStatus;
				Factory.Save();

				using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Should be readonly when registry is disabled", true, TestCharge.JR_A9_CostVATClass_ReadOnly);
				}

				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				TestCharge.JR_E6 = cost.PK;
				AssertEquals("readonly is true for apportioned line", true, TestCharge.JR_A9_CostVATClass_ReadOnly);
				TestCharge.JR_E6 = ZGuid.Empty;
				AssertEquals("readonly is false for unapportioned line", false, TestCharge.JR_A9_CostVATClass_ReadOnly);

				securityRight.IsAllowed = false;
				AssertEquals("must be readonly if no security right", true, TestCharge.JR_A9_CostVATClass_ReadOnly);
				securityRight.IsAllowed = true;

				var apInvoice = Factory.NewWithValidTestData<APInvoice>();
				var apInvLine = (APInvoiceLine)apInvoice.Lines.AddNew();
				apInvLine.AL_OSAmount = 200m;
				TestCharge.JR_AL_APLine = apInvLine.PK;
				AssertEquals(true, TestCharge.JR_IsCostPosted);
				AssertEquals("must be readonly if charge is posted", true, TestCharge.JR_A9_CostVATClass_ReadOnly);
			}
		}

		public void TestJR_GB_ReadOnly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = testObjectCreator.ActiveOrg.PK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsCostPosted);
			Assert("Pre-condition", !charge.IsRevenuePosted);
			Assert("Pre-condition", !charge.JR_IsApportioned);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForRevenuePosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_GB should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.JR_GB_ReadOnly_ForTestOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_GB should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.JR_GB_ReadOnly_ForTestOnly);
			}
		}

		public void TestJR_GE_ReadOnly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = testObjectCreator.ActiveOrg.PK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsCostPosted);
			Assert("Pre-condition", !charge.IsRevenuePosted);
			Assert("Pre-condition", !charge.JR_IsApportioned);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForRevenuePosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_GE should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.JR_GE_ReadOnly_ForTestOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_GE should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.JR_GE_ReadOnly_ForTestOnly);
			}
		}

		public void TestIsInternalJobInfoDisabled()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = orgPK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", charge.CostOrSellAccountIsOrgProxy);
			Assert("Pre-condition", !charge.CostAndSellAccountAreBothOrgProxies);
			Assert("Pre-condition", !(charge.InvoicingJob != null && !charge.InvoicingJob.IsConsumerTypeShouldCreateCostJRJ && !charge.InvoicingJob.IsConsumerTypeShouldCreateSellJRJ));
			Assert("Pre-condition", !(charge.CostAccountIsOrgProxy && (charge.IsCostPosted || charge.IsInDatabaseAndReadyForCostPosting)));
			Assert("Pre-condition", !(charge.SellAccountIsOrgProxy && (charge.IsRevenuePosted || charge.IsInDatabaseAndReadyForRevenuePosting)));

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("IsInternalJobInfoDisabled should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsInternalJobInfoDisabled);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("IsInternalJobInfoDisabled should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsInternalJobInfoDisabled);
			}
		}

		public void TestIsMainAllFieldsReadonly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = orgPK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsRevenuePosted);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForRevenuePosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("IsMainAllFieldsReadonly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsMainAllFieldsReadonly_ForTestOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("IsMainAllFieldsReadonly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsMainAllFieldsReadonly_ForTestOnly);
			}
		}

		public void TestJR_CostRatingOverride_ReadOnly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = orgPK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_CostRatingOverride_ReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.JR_CostRatingOverride_ReadOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_CostRatingOverride_ReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.JR_CostRatingOverride_ReadOnly);
			}
		}

		public void TestJR_SellRatingOverride_ReadOnly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = orgPK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForRevenuePosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_SellRatingOverride_ReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.JR_SellRatingOverride_ReadOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_SellRatingOverride_ReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.JR_SellRatingOverride_ReadOnly);
			}
		}

		public void TestIsAllCostFieldsReadonly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = orgPK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsRevenueCharge);
			Assert("Pre-condition", !charge.IsCostPosted);
			Assert("Pre-condition", !charge.JR_IsApportioned);
			Assert("Pre-condition", !charge.IsRevenuePostedWithManualJobRevenueJournal);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("IsAllCostFieldsReadonly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsAllCostFieldsReadonly_ForTestOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("IsAllCostFieldsReadonly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsAllCostFieldsReadonly_ForTestOnly);
			}
		}

		public void TestIsCostGSTFieldReadOnly()
		{
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

				var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment);
				job.PlugInData = shipment;
				Factory.Save();

				var charge = Factory.NewWithValidTestData<BaseCharge>();
				charge.JR_JH = job.PK;
				charge.JR_OH_CostAccount = orgPK;
				charge.CostAccount.CompanyData.SetAPTaxApplicable(true);
				charge.JR_E6 = ZGuid.Empty;
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				Assert("Pre-condition", !charge.JR_IsApportioned);
				Assert("Pre-condition", charge.IsCostGSTApplicable);
				Assert("Pre-condition", AccountingUtils.IsUserCanChangeGST(LedgerTypes.AccountsPayable));
				Assert("Pre-condition", charge.InvoicingJob?.InvoicingAllowOverrideCostTaxId ?? false);
				Assert("Pre-condition", !charge.IsCostPosted);
				Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsCostGSTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsCostGSTFieldReadOnly);
				}

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsCostGSTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsCostGSTFieldReadOnly);
				}
			}
		}

		public void TestIsCostWHTFieldReadOnly()
		{
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
				var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

				var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment);
				job.PlugInData = shipment;
				Factory.Save();

				var charge = Factory.NewWithValidTestData<BaseCharge>();
				charge.JR_JH = job.PK;
				charge.JR_OH_CostAccount = orgPK;
				charge.CostAccount.CompanyData.SetAPTaxApplicable(true);
				charge.CostAccount.CompanyData.OB_APWHTApplicable = true;
				charge.JR_E6 = ZGuid.Empty;
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				Assert("Pre-condition", !charge.JR_IsApportioned);
				Assert("Pre-condition", charge.IsCostWHTApplicable);
				Assert("Pre-condition", AccountingUtils.IsUserCanChangeWHT(LedgerTypes.AccountsPayable));
				Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);
				Assert("Pre-condition", !charge.IsCostPosted);

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsCostWHTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsCostWHTFieldReadOnly);
				}

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsCostWHTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsCostWHTFieldReadOnly);
				}
			}
		}

		public void TestIsSellGSTFieldReadOnly()
		{
			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

				var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment);
				job.PlugInData = shipment;
				Factory.Save();

				var charge = Factory.NewWithValidTestData<BaseCharge>();
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = orgPK;
				charge.SellAccount.CompanyData.SetARTaxApplicable(true);
				charge.JR_E6 = ZGuid.Empty;
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				Assert("Pre-condition", charge.IsSellGSTApplicable);
				Assert("Pre-condition", AccountingUtils.IsUserCanChangeGST(LedgerTypes.AccountsReceivable));
				Assert("Pre-condition", (charge.InvoicingJob?.InvoicingAllowOverrideSellTaxId ?? false));
				Assert("Pre-condition", !charge.IsRevenuePosted);
				Assert("Pre-condition", !charge.IsInDatabaseAndReadyForRevenuePosting);

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsSellGSTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsSellGSTFieldReadOnly_ForTestOnly);
				}

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsSellGSTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsSellGSTFieldReadOnly_ForTestOnly);
				}
			}
		}

		public void TestIsSellWHTFieldReadOnly()
		{
			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyWHTId.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
				var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

				var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment);
				job.PlugInData = shipment;
				Factory.Save();

				var charge = Factory.NewWithValidTestData<BaseCharge>();
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = orgPK;
				charge.SellAccount.CompanyData.OB_ARWHTApplicable = true;
				charge.JR_E6 = ZGuid.Empty;
				job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Factory.Save();

				Assert("Pre-condition", charge.IsSellWHTApplicable);
				Assert("Pre-condition", AccountingUtils.IsUserCanChangeWHT(LedgerTypes.AccountsReceivable));
				Assert("Pre-condition", !charge.IsRevenuePosted);
				Assert("Pre-condition", !charge.IsInDatabaseAndReadyForRevenuePosting);

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsSellWHTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.IsSellWHTFieldReadOnly_ForTestOnly);
				}

				using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
				{
					Assert("IsSellWHTFieldReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.IsSellWHTFieldReadOnly_ForTestOnly);
				}
			}
		}

		public void TestSetCostSellTaxRateDefaultTaxMessage()
		{
			AccInvMsg taxRateMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxRateMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccInvMsg overrideMessage = Factory.NewWithValidTestData<AccInvMsg>();
			overrideMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccTaxRate rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_Code = "TAX1";
			rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rate.AT_A9_DefaultVatClass = taxRateMessage.PK;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CCODE1";
			chargeCode.AC_AT_GSTRate = rate.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_ParentID = chargeCode.PK;
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = rate.PK;
			taxOverride.AO_A9_DefaultVATClass = overrideMessage.PK;

			Factory.Save();

			CommonShipment shipment = CommonShipment.New(Factory);
			TestJob.PlugInData = shipment;
			JobCharge charge = TestJob.Charges.AddNew();
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_CostGSTRate = rate.PK;
			AssertEquals("Message should come from the charge override if there's one", overrideMessage.PK, charge.JR_A9_CostVATClass);

			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_SellGSTRate = rate.PK;
			AssertEquals("Message should come from the charge override if there's one", overrideMessage.PK, charge.JR_A9_SellVATClass);

			chargeCode.TaxOverrides.DeleteAll();
			Factory.Save();
			chargeCode.ClearGSTRateCacheForTesting();

			charge = TestJob.Charges.AddNew();
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_CostGSTRate = rate.PK;
			AssertEquals("Message should come from the tax rate when no charge override exists", taxRateMessage.PK, charge.JR_A9_CostVATClass);

			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_SellGSTRate = rate.PK;
			AssertEquals("Message should come from the tax rate when no charge override exists", taxRateMessage.PK, charge.JR_A9_SellVATClass);
		}

		public void TestSetSameCostSellTaxRateRedefaultTaxMessage()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var msg1 = Factory.NewWithValidTestData<AccInvMsg>();
				msg1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				var msg2 = Factory.NewWithValidTestData<AccInvMsg>();
				msg2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				var rate = Factory.NewWithValidTestData<AccTaxRate>();
				rate.AT_Code = "TAX1";
				rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = "CCODE1";
				chargeCode.AC_AT_GSTRate = rate.PK;
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

				var taxOverride1 = chargeCode.TaxOverrides.AddNew();
				taxOverride1.AO_CostSellAll = "ALL";
				taxOverride1.AO_Direction = "ALL";
				taxOverride1.AO_IncoTerm = "ALL";
				taxOverride1.AO_JobType = "ALL";
				taxOverride1.AO_Origin = "ALL";
				taxOverride1.AO_Destination = "ALL";
				taxOverride1.AO_TaxRegCntryOrGroup = "ALL";
				taxOverride1.AO_GB = GlbBranch.CurrentBranch.PK;
				taxOverride1.AO_AT = rate.PK;
				taxOverride1.AO_A9_DefaultVATClass = msg1.PK;

				var taxOverride2 = chargeCode.TaxOverrides.AddNew();
				taxOverride2.AO_CostSellAll = "ALL";
				taxOverride2.AO_Direction = "ALL";
				taxOverride2.AO_IncoTerm = "ALL";
				taxOverride2.AO_JobType = "ALL";
				taxOverride2.AO_Origin = "ALL";
				taxOverride2.AO_Destination = "ALL";
				taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
				taxOverride2.AO_GB = TestObjectCreator.NonCurrentBranch.PK;
				taxOverride2.AO_AT = rate.PK;
				taxOverride2.AO_A9_DefaultVATClass = msg2.PK;

				Factory.Save();

				var shipment = CommonShipment.New(Factory);
				TestJob.PlugInData = shipment;
				var charge = TestJob.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;

				AssertEquals(rate.PK, charge.JR_AT_CostGSTRate);
				AssertEquals("Cost tax message should use msg1", msg1.PK, charge.JR_A9_CostVATClass);
				AssertEquals(rate.PK, charge.JR_AT_SellGSTRate);
				AssertEquals("Sell tax message should use msg1", msg1.PK, charge.JR_A9_SellVATClass);

				charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals("Cost tax rate isn't changed", rate.PK, charge.JR_AT_CostGSTRate);
				AssertEquals("Cost tax message should redefault to msg2 even tax rate isn't changed", msg2.PK, charge.JR_A9_CostVATClass);
				AssertEquals("Cost tax rate isn't changed", rate.PK, charge.JR_AT_SellGSTRate);
				AssertEquals("Sell tax message should redefault to msg2 even tax rate isn't changed", msg2.PK, charge.JR_A9_SellVATClass);
			}
		}

		public void TestExceptionResetSellGSTTaxDafault()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

				Charge charge = Factory.New<Charge>();
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsActive, true);
				OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
				charge.JR_OH_SellAccount = orgHeader.PK;
				charge.SellAccount.CompanyData.SetARTaxApplicable(true);
				charge.JR_GB = Guid.Empty;

				AssertNoExceptionThrown("SetSellGSTTaxDafault should no exception thrown.", charge.ResetSellGSTTaxDefault);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestJR_AT_CostGSTRate_ReadOnly_IfChargeAttachedToConsol()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var shipmentWithoutConsole = TestObjectCreator.CreateShipment("S0001998", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipmentWithoutConsole, false);
			var shipmentCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 12m, null);

			AssertEquals("Charge not apportioned", false, shipmentCharge.JR_IsApportioned);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var invoicingAllowOverrideCostTaxIdCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxId);
			var securityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxId);
			var cachedSecurityValue = securityRight.IsAllowed;

			using (new DisposableAction(() => { }, () => securityRight.IsAllowed = cachedSecurityValue))
			{
				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					securityRight.IsAllowed = false;
					AssertEquals("JR_AT_CostGSTRate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_AT_CostGSTRateInfo.ReadOnly);
					securityRight.IsAllowed = true;
					AssertEquals("JR_AT_CostGSTRate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_AT_CostGSTRateInfo.ReadOnly);
				}

				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					securityRight.IsAllowed = false;
					AssertEquals("JR_AT_CostGSTRate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_AT_CostGSTRateInfo.ReadOnly);
					securityRight.IsAllowed = true;
					AssertEquals("JR_AT_CostGSTRate is not readonly as charge not related to consol but operator can modify TestObjectCreator.GST1", false, shipmentCharge.JR_AT_CostGSTRateInfo.ReadOnly);

					var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001990");
					var shipmentWithConsol = TestObjectCreator.CreateShipment("S0001999", consol);
					var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 10M, true);

					Factory.Save();

					AssertEquals("One Apportionment charge should be created", 1, ((Job)shipmentWithConsol.Job).Charges.Count);
					var charge = ((Job)shipmentWithConsol.Job).Charges[0];
					AssertEquals("Charge is apportioned", true, charge.JR_IsApportioned);
					AssertEquals("JR_AT_CostGSTRate is readonly as attached to a consol even though operator can modify TestObjectCreator.GST1", true, charge.JR_AT_CostGSTRateInfo.ReadOnly);
				}
			}
		}

		public void TestJR_AW_CostWHTRate_ReadOnly_IfChargeAttachedToConsol()
		{
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			var shipmentWithoutConsol = TestObjectCreator.CreateShipment("S0001998", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipmentWithoutConsol, false);
			TestObjectCreator.CC1.AC_AW_WithholdingTaxRate = TestObjectCreator.WHT1.PK;
			var shipmentCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 12m, null);

			AssertEquals("Charge not apportioned", false, shipmentCharge.JR_IsApportioned);

			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("JR_AW_CostWHTRate is readonly as charge not related to consol and operator can not modify TestObjectCreator.WHT1", true, shipmentCharge.JR_AW_CostWHTRateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("JR_AW_CostWHTRate is not readonly as charge not related to consol but operator can modify TestObjectCreator.WHT1", false, shipmentCharge.JR_AW_CostWHTRateInfo.ReadOnly);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001990");
			var shipmentWithConsol = TestObjectCreator.CreateShipment("S0001999", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 10M, true);

			Factory.Save();

			AssertEquals("One Apportionment charge should be created", 1, ((Job)shipmentWithConsol.Job).Charges.Count);
			var charge = ((Job)shipmentWithConsol.Job).Charges[0];
			AssertEquals("Charge is apportioned", true, charge.JR_IsApportioned);
			AssertEquals("JR_AW_CostWHTRate is readonly as attached to a consol even though operator can modify TestObjectCreator.WHT1", true, charge.JR_AW_CostWHTRateInfo.ReadOnly);
		}

		public void TestIsManualJobAccrualCharge()
		{
			TestCharge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("This is not MJA charge", false, TestCharge.IsManualJobAccrualCharge);

			TestCharge.JR_AC = TestObjectCreator.ManualJobAccrualChargeCode.PK;
			AssertEquals("This is MJA charge", true, TestCharge.IsManualJobAccrualCharge);
		}

		public void TestIsAllowedToModifyChargesWIthGU_SecurityItemIsAllowedSetToTrue()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", testCompany);

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ForwardingShipment testShipment = TestObjectCreator.CreateShipment("S00001234");
				Job testJob = TestObjectCreator.CreateJob(testShipment);
				Charge testCharge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, 100m, 100m);
				Factory.Save();

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();

				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				SecurityCore security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				GlbSecurity loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				AssertEquals("User is allowed to modify changes", true, testCharge.IsAllowedToModifyThisCharge);
				AssertEquals("User is allowed to delete changes", true, testCharge.CanDelete);
				AssertEquals("Reason for not able to delete", "", testCharge.ReasonForNotAbleToDelete);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				AssertEquals("User is allowed to modify changes", true, testCharge.IsAllowedToModifyThisCharge);
				AssertEquals("User is allowed to delete changes", true, testCharge.CanDelete);
				AssertEquals("Reason for not able to delete", "", testCharge.ReasonForNotAbleToDelete);

				testCharge.JR_GB = testBranch.PK;
				AssertEquals("User is not allowed to modify charges", false, testCharge.IsAllowedToModifyThisCharge);
			}
		}

		public void TestIsAllowedToModifyChargesWIthGU_SecurityItemIsAllowedSetToFalse()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ForwardingShipment testShipment = TestObjectCreator.CreateShipment("S00001234");
				Job testJob = TestObjectCreator.CreateJob(testShipment);
				Charge testCharge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, 100m, 100m);
				Factory.Save();

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();

				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;

				loginSecurity.GU_SecurityItemIsAllowed = false;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				securityFactory.Save();

				AssertEquals("User is allowed to modify charges", true, testCharge.IsAllowedToModifyThisCharge);
				AssertEquals("User is allowed to delete charges", true, testCharge.CanDelete);
				AssertEquals("Reason for not able to delete", "", testCharge.ReasonForNotAbleToDelete);

				loginSecurity.GU_SecurityItemIsAllowed = false;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				using (Env.SetTemporaryUserContext(testUser.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("User is not allowed to modify charges", false, testCharge.IsAllowedToModifyThisCharge);
					AssertEquals("User is not allowed to delete charges", false, testCharge.CanDelete);
					AssertEquals("Reason for not able to delete", "You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission.", testCharge.ReasonForNotAbleToDelete);
				}
			}
		}

		public void TestJR_ChargeableUnit_ChargeableUnitForRevenueApportionment()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0001");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			BaseCharge charge = job.Charges.AddNew();
			charge.ChargeableUnitForRevenueApportionment = Constants.Weight.Kilograms;

			AssertEquals(Constants.Weight.Kilograms, charge.JR_ChargeableUnit);
		}

		public void TestJR_ChargeableUnit()
		{
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ActualChargeable = 100.0m;
			charge.SetShipmentInfo(shipment);

			AssertEquals("JR_ChargeableUnit", Core.Constants.Weight.Kilograms, charge.JR_ChargeableUnit);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_ActualChargeable = 200.0m;
			shipment1.JS_ActualWeight = 500m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1.JS_ActualVolume = 0m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				ApportionSplitCharge shipment1Charge = cost.ApportionmentCharges.FindChargeForJob(shipment1);

				AssertEquals("JR_ChargeableUnit", Core.Constants.Volume.CubicMetres, shipment1Charge.JR_ChargeableUnit);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIApportionedCharge_Chargeable()
		{
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualChargeable = 100.0m;
			charge.SetShipmentInfo(shipment);

			AssertEquals("JR_Chargeable", 100.0m, charge.JR_Chargeable);
			AssertEquals("ChargeableUnits", 100.0m, ((IApportionedCharge)charge).ChargeableUnits);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_ActualChargeable = 200.0m;
			shipment2.JS_ActualChargeable = 300.0m;
			shipment1.JS_ActualWeight = 500m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1.JS_ActualVolume = 0m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment2.JS_ActualWeight = 400m;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment2.JS_ActualVolume = 0m;
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();

				cost.E6_AC_ChargeCode = TestObjectCreator.FRT.PK;
				cost.E6_OSCostAmount = 1000m;
				ApportionSplitCharge shipment1Charge = cost.ApportionmentCharges.FindChargeForJob(shipment1);
				ApportionSplitCharge shipment2Charge = cost.ApportionmentCharges.FindChargeForJob(shipment2);

				AssertEquals("Consol.JK_ConsolChargeableUnit  ", Core.Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
				AssertNotNull("ShipmentInfo", shipment1Charge.ShipmentInfo);
				AssertEquals("JR_Chargeable", 500.0m, shipment1Charge.JR_Chargeable);
				AssertEquals("ChargeableUnits", 500.0m, ((IApportionedCharge)shipment1Charge).ChargeableUnits);
				AssertNotNull("ShipmentInfo", shipment2Charge.ShipmentInfo);
				AssertEquals("JR_Chargeable", 181.437m, shipment2Charge.JR_Chargeable);
				AssertEquals("ChargeableUnits", 181.437m, ((IApportionedCharge)shipment2Charge).ChargeableUnits);
				AssertEquals("shipment1Charge.JR_OSCostAmt", 733.74m, shipment1Charge.JR_OSCostAmt);
				AssertEquals("shipment2Charge.JR_OSCostAmt", 266.26m, shipment2Charge.JR_OSCostAmt);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestJR_ActualWeightUnit()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_ActualChargeable = 200.0m;
			shipment1.JS_ActualWeight = 500m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.LongTons;
			shipment1.JS_ActualVolume = 0m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				ApportionSplitCharge shipment1Charge = cost.ApportionmentCharges.FindChargeForJob(shipment1);

				AssertEquals("JR_ActualWeightUnit", Core.Constants.Weight.LongTons, shipment1Charge.JR_ActualWeightUnit);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestIApportionedCharge_ActualWeight()
		{
			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = 100.0m;
			charge.SetShipmentInfo(shipment);

			AssertEquals("JR_ActualWeight", 100.0m, charge.JR_ActualWeight);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 200.0m;
			shipment2.JS_ActualWeight = 300.0m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1.JS_ActualVolume = 0m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment2.JS_ActualVolume = 0m;
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();

				cost.E6_OSCostAmount = 1000m;
				ApportionSplitCharge shipment1Charge = cost.ApportionmentCharges.FindChargeForJob(shipment1);
				ApportionSplitCharge shipment2Charge = cost.ApportionmentCharges.FindChargeForJob(shipment2);

				AssertEquals("Consol.JK_ConsolChargeableUnit ", Core.Constants.Weight.Kilograms, consol.JK_ConsolChargeableUnit);
				AssertNotNull("ShipmentInfo", shipment1Charge.ShipmentInfo);
				AssertEquals("JR_ActualWeight", 200.0m, shipment1Charge.JR_ActualWeight);
				AssertNotNull("ShipmentInfo", shipment2Charge.ShipmentInfo);
				AssertEquals("JR_ActualWeight", 300.0m, shipment2Charge.JR_ActualWeight);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public override void TestJR_GBConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;
			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);

			GlbBranch newBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch1.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbBranch newBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch2.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_GB, newBranch1.PK, newBranch2.PK);
		}

		public void TestJR_OSCostGSTAmtForNZEntryFee()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var gSTRate = Factory.NewWithValidTestData<AccTaxRate>();
				gSTRate.AT_RN_NKCountry = "NZ";
				gSTRate.SetRateNumerator_ForTestOnly(15);

				var entryFeeChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				entryFeeChargeCode.AC_Code = "ENTRYFEE";
				entryFeeChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				entryFeeChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
				entryFeeChargeCode.AC_AT_GSTRate = gSTRate.PK;

				var chargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var typeAndCode1 = chargeTypesAndCodes.AddNew();
				typeAndCode1.ChargeType = NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee;
				typeAndCode1.AC_ChargeCode = entryFeeChargeCode.PK;
				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeTypesAndCodes);
				var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
				Factory.Save();

				var creator = new TestObjectCreator(Factory);
				var shipment = Factory.New<ForwardingShipment>();
				var shipmentJob = TestObjectCreator.CreateJob(shipment, false);
				shipmentJob.LocalChargesPK = creator.AALSHI.PK;
				shipmentJob.AgentCollectPK = creator.ABIGAS.PK;
				shipmentJob.PlugInData = shipment;
				shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;
				shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
				var charge = shipmentJob.Charges.AddNew();
				charge.JR_AC = entryFeeChargeCode.PK;
				charge.JR_RX_NKCostCurrency = currency.RX_Code;
				charge.JR_OSCostAmt = NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly;
				charge.JR_AT_CostGSTRate = gSTRate.PK;
				AssertEquals("Tax amount should be 6.43", 6.43m, charge.JR_OSCostGSTAmt_Calc);

				using (charge.SetNZEntryFeeChargeTaxAmountSuspender.GetSuspender())
				using (charge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender())
				{
					charge.JR_IsCostTaxAmountOverridden = true;
					charge.JR_OSCostGSTAmt_Calc = 6.42;
				}
				AssertEquals("Tax amount should be 6.42 as it is overriden using suspender", 6.42m, charge.JR_OSCostGSTAmt_Calc);

				var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
				var chargeWithCost = shipmentJob.Charges.AddNew();
				chargeWithCost.JR_E6 = consolCost.PK;
				charge.JR_AC = entryFeeChargeCode.PK;
				charge.JR_RX_NKCostCurrency = currency.RX_Code;
				charge.JR_OSCostAmt = NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly;
				charge.JR_AT_CostGSTRate = gSTRate.PK;
				AssertEquals("Tax amount should be 6.42", 6.42m, charge.JR_OSCostGSTAmt_Calc);
			}
		}
		public override void TestJR_GEConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;
			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);

			GlbDepartment dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_GE, dept1.PK, dept2.PK);
		}

		public override void TestJR_ACConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;
			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);

			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			code1.AC_GC = GlbCompany.CurrentCompany.PK;
			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_AC, code1.PK, code2.PK);
		}

		public override void TestJR_OH_CostAccountConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;
			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_OH_CostAccount, org1.PK, org2.PK);
		}

		public override void TestJR_OH_SellAccountConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;
			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_OH_SellAccount, org1.PK, org2.PK);
		}

		public void TestJR_E6Concurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;
			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);

			JobConsolCost cost1 = Factory.NewWithValidTestData<JobConsolCost>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_OH_SellAccount, ZGuid.Empty, cost1.PK);
		}

		public override void TestJR_RX_NKCostCurrencyConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;

			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);
			AssertEquals("JR_RX_NKCostCurrency", creator.AUD.RX_Code, charge.JR_RX_NKCostCurrency);

			ZString otherCurrency = creator.USD.RX_Code;
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_RX_NKCostCurrency, ZString.Empty, otherCurrency);
		}

		public override void TestJR_RX_NKSellCurrencyConcurrency()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job;
			BaseCharge charge;

			SetupJobAndBaseChargeForConcurrencyTests(creator, out job, out charge);
			AssertEquals("JR_RX_NKSellCurrency", creator.AUD.RX_Code, charge.JR_RX_NKSellCurrency);

			ZString otherCurrency = creator.USD.RX_Code;
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_RX_NKSellCurrency, ZString.Empty, otherCurrency);
		}

		/// <summary>
		/// This method reloads the charge in 2 separate factories (that have data refresh turned off), sets 'value1' and 'value2' on each copy respectively. 
		/// The first factory is saved and then the second, which will cause a concurrency issue.
		/// The method then asserts that the exception handler reported that the error was critical and couldn't be merged.
		/// </summary>
		/// <param name="charge">This is the charge created for testing. It will get reloaded in other factories in this method so that the concurrency can be tested</param>
		/// <param name="column">This is the column that should be set by value1 and value2</param>
		/// <param name="value1">This is the value that gets set on the first reloaded copy of the charge.</param>
		/// <param name="value2">This is the value that gets set on the second reloaded copy of the reloaded charge, which when saved will cause a concurrency exception</param>
		void AssertConcurrency(BaseCharge charge, SchemaColumn column, IZType value1, IZType value2)
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			BaseCharge chargeInFactory1 = factory1.Load<BaseCharge>(charge.PK);
			BaseCharge chargeInFactory2 = factory2.Load<BaseCharge>(charge.PK);

			chargeInFactory1[column] = value1;
			chargeInFactory2[column] = value2;

			factory1.Save();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory2.Save();
			}
			catch (Exception e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}

			AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void SetupJobAndBaseChargeForConcurrencyTests(TestObjectCreator creator, out Job job, out BaseCharge charge)
		{
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.Consignee).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.Consignor).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.IsDirectShipment).Returns(false);
			mockSupporter.Setup(m => m.ActualChargeable).Returns(100m);
			mockSupporter.Setup(m => m.ActualChargeableUnit).Returns((ZString)Core.Constants.Weight.Kilograms);
			mockSupporter.Setup(m => m.ConsolType).Returns((ZString)Constants.AgentType.Agent);
			mockSupporter.Setup(m => m.ConsolRateCurrency).Returns(creator.USD);
			mockSupporter.Setup(m => m.ConsolExchangeRate).Returns(0.9m);
			mockSupporter.Setup(m => m.Origin).Returns(RefUNLOCO.LoadFromIATA(Factory, "AUSYD"));
			mockSupporter.Setup(m => m.Destination).Returns(RefUNLOCO.LoadFromIATA(Factory, "USLAX"));
			mockSupporter.Setup(m => m.TransportMode).Returns((ZString)Constants.TransportModes.Air);
			mockSupporter.Setup(m => m.ContainerMode).Returns((ZString)Constants.ContainerModes.Loose);
			mockSupporter.Setup(m => m.IsImport).Returns(false);
			mockSupporter.Setup(m => m.IsExport).Returns(true);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.IsCrossTrade).Returns(false);
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			mockSupporter.Setup(m => m.MasterBillNumber).Returns((ZString)"MASTERBILL01");
			mockSupporter.Setup(m => m.HouseBillNumber).Returns((ZString)"HOUSEBILL01");
			mockSupporter.Setup(m => m.ATA).Returns(ZDateTime.Empty);
			mockSupporter.Setup(m => m.ETA).Returns(ZDateTime.Now.AddDays(1));
			mockSupporter.Setup(m => m.ATD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ETD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ActualWeight).Returns((ZDecimal)100m);
			mockSupporter.Setup(m => m.ActualWeightUnit).Returns((ZString)Core.Constants.Weight.Pounds);
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			mockSupporter.Setup(m => m.EditSecurityLock).Returns(false);
			mockSupporter.Setup(m => m.ServiceLevel).Returns("STD");

			var mockPlugIn = new Mock<IJobInvoicingPlugIn>();
			mockPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockPlugIn.Setup(m => m.Factory).Returns(creator.Factory);
			mockPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockPlugIn.Setup(m => m.JobNumber).Returns("S00001001");
			mockPlugIn.Setup(m => m.IsDeleted).Returns(false);
			mockPlugIn.Setup(m => m.IsInDatabase).Returns(true);

			job = new Job.Loader(mockPlugIn.Object).TryCreateWithoutMutexForTestOnly();
			creator = new TestObjectCreator(job.Factory);
			job.JH_GB = creator.NonCurrentBranch.PK;
			job.JH_GE = creator.NonCurrentDepartment.PK;
			charge = job.Factory.New<BaseCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = creator.CC1.PK;
			charge.JR_OH_SellAccount = job.LocalChargesPK;
			charge.JR_GE = creator.NonCurrentDepartment.PK;
			charge.JR_GB = creator.NonCurrentBranch.PK;

			job.Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestGrossWeightInKilos()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.Consignee).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.Consignor).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.IsDirectShipment).Returns(false);
			mockSupporter.Setup(m => m.ActualChargeable).Returns(new ZDecimal(100));
			mockSupporter.Setup(m => m.ActualChargeableUnit).Returns(Core.Constants.Weight.Kilograms);
			mockSupporter.Setup(m => m.ConsolType).Returns(new ZString(Constants.AgentType.Agent));
			mockSupporter.Setup(m => m.ConsolRateCurrency).Returns(creator.USD);
			mockSupporter.Setup(m => m.ConsolExchangeRate).Returns(new ZDecimal(0.9m));
			mockSupporter.Setup(m => m.Origin).Returns(RefUNLOCO.LoadFromIATA(Factory, "AUSYD"));
			mockSupporter.Setup(m => m.Destination).Returns(RefUNLOCO.LoadFromIATA(Factory, "USLAX"));
			mockSupporter.Setup(m => m.TransportMode).Returns(new ZString(Constants.TransportModes.Air));
			mockSupporter.Setup(m => m.ContainerMode).Returns(new ZString(Constants.ContainerModes.Loose));
			mockSupporter.Setup(m => m.IsImport).Returns(false);
			mockSupporter.Setup(m => m.IsExport).Returns(true);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			mockSupporter.Setup(m => m.MasterBillNumber).Returns(new ZString("MASTERBILL01"));
			mockSupporter.Setup(m => m.HouseBillNumber).Returns(new ZString("HOUSEBILL01"));
			mockSupporter.Setup(m => m.ATA).Returns(ZDateTime.Empty);
			mockSupporter.Setup(m => m.ETA).Returns(ZDateTime.Now.AddDays(1));
			mockSupporter.Setup(m => m.ATD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ETD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ActualWeight).Returns(new ZDecimal(100m));
			mockSupporter.Setup(m => m.ActualWeightUnit).Returns(new ZString(Core.Constants.Weight.Pounds));
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);

			var mockPlugIn = new Mock<IJobInvoicingPlugIn>();
			mockPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockPlugIn.Setup(m => m.Factory).Returns(new BusinessObjectFactory());
			mockPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockPlugIn.Setup(m => m.IsInDatabase).Returns(true);

			Job job = new Job.Loader(mockPlugIn.Object).TryCreateWithoutMutexForTestOnly();
			Charge charge = job.Charges.AddNew();
			AssertEquals(Core.Constants.Weight.Convert(100, Core.Constants.Weight.Pounds, Core.Constants.Weight.Kilograms), charge.GrossWeight);

			mockSupporter.Setup(m => m.ActualWeight).Returns(new ZDecimal(200m));
			mockSupporter.Setup(m => m.ActualWeightUnit).Returns(new ZString(Core.Constants.Weight.LongTons));
			AssertEquals(Core.Constants.Weight.Convert(200, Core.Constants.Weight.LongTons, Core.Constants.Weight.Kilograms), charge.GrossWeight);

			mockSupporter.Setup(m => m.ActualWeightUnit).Returns(new ZString("Invalid Unit"));
			AssertEquals(0m, charge.GrossWeight);
		}

		[ExpectNoExceptions]
		public void TestGrossVolumeInCubicMeters()
		{
			var creator = new TestObjectCreator(Factory);
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.Consignee).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.Consignor).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.ActualChargeable).Returns(new ZDecimal(1000));
			mockSupporter.Setup(m => m.ActualChargeableUnit).Returns(Core.Constants.Weight.Kilograms);
			mockSupporter.Setup(m => m.ConsolType).Returns(new ZString(Constants.AgentType.Agent));
			mockSupporter.Setup(m => m.ConsolRateCurrency).Returns(creator.USD);
			mockSupporter.Setup(m => m.ConsolExchangeRate).Returns(new ZDecimal(0.9m));
			mockSupporter.Setup(m => m.Origin).Returns(RefUNLOCO.LoadFromIATA(Factory, "AUSYD"));
			mockSupporter.Setup(m => m.Destination).Returns(RefUNLOCO.LoadFromIATA(Factory, "NZAKL"));
			mockSupporter.Setup(m => m.TransportMode).Returns(new ZString(Constants.TransportModes.Air));
			mockSupporter.Setup(m => m.ContainerMode).Returns(new ZString(Constants.ContainerModes.Loose));
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			mockSupporter.Setup(m => m.MasterBillNumber).Returns(new ZString("MASTERBILL01"));
			mockSupporter.Setup(m => m.HouseBillNumber).Returns(new ZString("HOUSEBILL01"));
			mockSupporter.Setup(m => m.ETD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ETA).Returns(ZDateTime.Now.AddDays(1));
			mockSupporter.Setup(m => m.ActualVolume).Returns(new ZDecimal(15m));
			mockSupporter.Setup(m => m.ActualVolumeUnit).Returns(new ZString(Core.Constants.Volume.CubicFeet));
			mockSupporter.Setup(m => m.IsDirectShipment).Returns(false);
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.IsImport).Returns(false);
			mockSupporter.Setup(m => m.IsExport).Returns(true);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);

			var mockPlugIn = new Mock<IJobInvoicingPlugIn>();
			mockPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockPlugIn.Setup(m => m.Factory).Returns(new BusinessObjectFactory());
			mockPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockPlugIn.Setup(m => m.IsInDatabase).Returns(true);

			var job = new Job.Loader(mockPlugIn.Object).TryCreateWithoutMutexForTestOnly();
			var charge = job.Charges.AddNew();
			AssertEquals(Core.Constants.Volume.Convert(15, Core.Constants.Volume.CubicFeet, Core.Constants.Volume.CubicMetres), charge.GrossVolume);

			mockSupporter.Setup(m => m.ActualVolume).Returns(new ZDecimal(500m));
			mockSupporter.Setup(m => m.ActualVolumeUnit).Returns(new ZString(Core.Constants.Volume.CubicCentimeters));
			AssertEquals(Core.Constants.Volume.Convert(500, Core.Constants.Volume.CubicCentimeters, Core.Constants.Volume.CubicMetres), charge.GrossVolume);

			mockSupporter.Setup(m => m.ActualVolumeUnit).Returns(new ZString("Invalid Unit"));
			AssertEquals(0m, charge.GrossVolume);
		}

		public void TestResetChargeDebtor()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			AssertEquals("charge1's Debtor", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);
			charge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			AssertEquals("charge1's Debtor", TestObjectCreator.ABIGAS.PK, charge1.JR_OH_SellAccount);
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to its default value of AAALSHI", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);
		}

		public void TestResetChargeDebtorWhenOrgIsNotDebtor()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C0001");
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0001", consol);
			Job job = TestObjectCreator.CreateJob(shipment);
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;
			job.AgentCollectPK = ZGuid.Empty;
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, null);
			cost.E6_OSCostAmount = 100M;
			Factory.Save();

			AssertEquals("Precondition: org is not a debtor.", false, TestObjectCreator.AALSHI.OH_IsDebtor);
			AssertEquals("Precondition: WIPMustHaveDebtorCode registry item is set to false.", false, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value);
			AssertType("Precondition: job consol cost was created from a consol.", typeof(JobConsolCost.ConsolCostCalculationStrategy), cost.CalculationStrategy);

			ApportionSplitCharge apportionSplitCharge = cost.ApportionmentCharges[0];
			AssertEquals("Apportion split charge should not have a debtor.", ZGuid.Empty, apportionSplitCharge.JR_OH_SellAccount);

			Charge charge = Factory.Load<Charge>(apportionSplitCharge.PK);
			AssertEquals("Job charge should not have a debtor.", ZGuid.Empty, charge.JR_OH_SellAccount);
		}

		public void TestResetChargeDebtorWhenCassIsInContext()
		{
			Job job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			AssertEquals("charge1's Debtor", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

			testObjectCreator.AALSHI.OH_IsActive = false;

			AssertEquals("Factory is not in context", false, charge1.Factory.HasContext(BusinessContext.CASS));
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to default because CASS is not in context", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

			charge1.Factory.SetContext(BusinessContext.CASS);
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to null because of CASS in context", ZGuid.Empty, charge1.JR_OH_SellAccount);
		}

		public void TestResetChargeDebtorWhenInvoicingPlugInGUIIsInContext()
		{
			Job job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			AssertEquals("charge1's Debtor", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

			testObjectCreator.AALSHI.OH_IsDebtor = false;

			AssertEquals("Factory is not in context", false, charge1.Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to default because InvoicingPlugInGUI is not in context", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

			charge1.Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to empty because of InvoicingPlugInGUI in context", ZGuid.Empty, charge1.JR_OH_SellAccount);
		}

		public void TestIsValidDebtorForDefaultingWhenAJRJIsEnabledAndCreditorIsAlreadyAnOrgProxy()
		{
			var orgProxy1 = GlbBranch.CurrentBranch.OrgProxy;
			orgProxy1.OH_IsDebtor = true;
			orgProxy1.OH_IsCreditor = true;

			var orgProxy2 = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy2.OH_IsDebtor = true;
			orgProxy2.OH_IsCreditor = true;

			var anotherBranchQuery = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK) { OrderBy = GlbBranchSchema.GB_Code.Name };
			anotherBranchQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			var anotherBranch = Factory.LoadTop1<GlbBranch>(anotherBranchQuery);
			anotherBranch.GB_OH_OrgProxy = orgProxy2.PK;

			var nonOrgProxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			nonOrgProxyOrg.OH_IsDebtor = true;
			nonOrgProxyOrg.OH_IsCreditor = true;

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				AssertEquals("Pre-condition: is valid for debtor defaulting", true, charge.IsValidDebtorForDefaulting(orgProxy1.PK));
				AssertEquals(true, charge.IsValidDebtorForDefaulting(orgProxy2.PK));

				charge.JR_OH_CostAccount = orgProxy1.PK;

				AssertEquals("Should not be able to default another org proxy as the debot as the creditor is already an org proxy and AJRJ is enabled", false, charge.IsValidDebtorForDefaulting(orgProxy2.PK));
				AssertEquals(false, charge.IsValidDebtorForDefaulting(orgProxy1.PK));
				AssertEquals("If debtor is not an org proxy then it's not a problem", true, charge.IsValidDebtorForDefaulting(nonOrgProxyOrg.PK));

				charge.JR_OH_CostAccount = nonOrgProxyOrg.PK;

				AssertEquals(true, charge.IsValidDebtorForDefaulting(orgProxy1.PK));
				AssertEquals(true, charge.IsValidDebtorForDefaulting(orgProxy2.PK));

				charge.JR_OH_CostAccount = orgProxy2.PK;

				AssertEquals("Should not be able to default another org proxy as the debot as the creditor is already an org proxy and AJRJ is enabled", false, charge.IsValidDebtorForDefaulting(orgProxy2.PK));
				AssertEquals(false, charge.IsValidDebtorForDefaulting(orgProxy1.PK));
				AssertEquals(true, charge.IsValidDebtorForDefaulting(nonOrgProxyOrg.PK));
			}

			using (AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				charge.JR_OH_CostAccount = orgProxy1.PK;

				AssertEquals("Doesnt matter as AJRJ is not enabled in the registry", true, charge.IsValidDebtorForDefaulting(orgProxy1.PK));
				AssertEquals(true, charge.IsValidDebtorForDefaulting(orgProxy2.PK));
			}
		}

		public void TestDoNotDefaultNonDebtorWithAPInvoiceBusinessContext()
		{
			AssertDoNotDefaultNonDebtorWithBusinessContext(BusinessContext.APInvoiceForm);
			AssertDoNotDefaultNonDebtorWithBusinessContext(BusinessContext.APBulkInvoicePoster);
			AssertDoNotDefaultNonDebtorWithBusinessContext(BusinessContext.APCreditNoteForm);
		}

		void AssertDoNotDefaultNonDebtorWithBusinessContext(BusinessContext businessContext)
		{
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			AssertEquals("charge1's Debtor", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

			testObjectCreator.AALSHI.OH_IsDebtor = false;

			AssertEquals("Factory is not in context", false, charge1.Factory.HasContext(businessContext));
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to default", TestObjectCreator.AALSHI.PK, charge1.JR_OH_SellAccount);

			charge1.Factory.SetContext(businessContext);
			charge1.ResetChargeDebtor();
			AssertEquals("Debtor should be reset to empty", ZGuid.Empty, charge1.JR_OH_SellAccount);
			charge1.Factory.RemoveContext(businessContext);
		}

		public void TestGetChargeDebtor()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000", "NZAKL", "AUSYD");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;
			job.AgentCollectPK = TestObjectCreator.ABIGAS.PK;
			AssertEquals("job.IsImport", true, job.IsImport);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			AssertEquals("charge1.GetChargeDebtor()", TestObjectCreator.AALSHI.PK, job.GetDebtorPK(charge1));

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			AssertEquals("job.IsExport", true, job.IsExport);
			AssertEquals("charge1.GetChargeDebtor()", TestObjectCreator.ABIGAS.PK, job.GetDebtorPK(charge1));
		}

		public void TestClearPaymentDetails()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			JobConsolCost cost1 = Factory.NewWithValidTestData<JobConsolCost>();

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_E6 = cost1.PK;

			cost1.E6_PaymentType = charge1.JR_PaymentType = "BLA";
			cost1.E6_AB_BankAccount = charge1.JR_AB = ZGuid.NewZGuid();
			cost1.E6_AK_ChequeBook = charge1.JR_AK = ZGuid.NewZGuid();
			cost1.E6_ChequeOrReference = charge1.JR_ChequeNo = "12345";

			charge1.ClearPaymentDetails();

			Assert("E6_PaymentType", cost1.E6_PaymentType.IsEmpty);
			Assert("JR_PaymentType", charge1.JR_PaymentType.IsEmpty);
			Assert("JR_AB", charge1.JR_AB.IsEmpty);
			Assert("E6_AB_BankAccount", cost1.E6_AB_BankAccount.IsEmpty);
			Assert("JR_AK", charge1.JR_AK.IsEmpty);
			Assert("E6_AK_ChequeBook", cost1.E6_AK_ChequeBook.IsEmpty);
			Assert("JR_ChequeNo", charge1.JR_ChequeNo.IsEmpty);
			Assert("E6_ChequeOrReference", cost1.E6_ChequeOrReference.IsEmpty);
		}

		public void TestChargeIsAlwaysValidatedWhenAmountsChange()
		{
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			IJobInvoicingPlugIn shipment = (IJobInvoicingPlugIn)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = creator.NonCurrentDepartment.PK;
			Charge charge1 = job.Charges.AddNew();
			creator.CC1.AC_MarginPercentage = 0m;
			charge1.JR_AC = creator.CC1.PK;
			charge1.JR_OH_CostAccount = ZGuid.Empty;
			charge1.JR_OH_SellAccount = ZGuid.Empty;
			charge1.RunPreSaveValidation();
			AssertNoErrors(charge1);
			Factory.Save();
			charge1.RunPreSaveValidation();
			AssertNoErrors(charge1);
			Assert(charge1.ShouldValidateOnSave);
			charge1.JR_LocalCostAmt = 10m;
			AssertEquals(0m, charge1.JR_LocalSellAmt);
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_OH_CostAccountInfo);
			charge1.JR_OH_CostAccount = creator.Creditor1.PK;
			charge1.RunPreSaveValidation();
			AssertNoErrors(charge1);

			charge1.JR_LocalSellAmt = 10m;
			charge1.RunPreSaveValidation();
			AssertHasErrors(charge1.JR_OH_SellAccountInfo);
			charge1.JR_OH_SellAccount = creator.LocalClient.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.RunPreSaveValidation();
			AssertNoErrors(charge1);
			Factory.Save();
		}

		public void TestInvoiceTypeForOneOffQuotation()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var orgHeader = testObjectCreator.CreateOrgHeader("TSTORG", false, true);

			var orgHeaderWithPeriodicInvoiceSetting = testObjectCreator.CreateOrgHeader("TSTORG1", false, true);
			OrgInvoiceType invoiceType = orgHeaderWithPeriodicInvoiceSetting.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.OneOffQuotation.Code;
			invoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;

			var quote = Factory.NewWithValidTestData<OneOffQuoteHost>();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.PlugInData = quote;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "test", testObjectCreator.AUD, 100M, null, testObjectCreator.AUD, 100M, orgHeader);
			AssertEquals("Invoice type should be FIN since debtor is not configured for periodic invoicing.", InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);

			var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "test periofic invoice", testObjectCreator.AUD, 100M, null, testObjectCreator.AUD, 100M, orgHeaderWithPeriodicInvoiceSetting);
			AssertEquals("Invoice type should be FID since debtor is configured for periodic invoicing.", InvoiceTypesList.Codes.FinalInvoice_Batching, charge1.JR_InvoiceType);
		}

		public void TestInternalJobs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.PlugInData = shipment;
			var charge = job.Charges.AddNew();
			charge.JR_Calc_RelatedJobNumber = "S0001001";
			Assert(charge.InternalJobs is JobCollection);
			AssertNotNull(charge.InternalJobs.FilterBusinessObjectDefaults);
			var defaultList = charge.InternalJobs.FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>();
			AssertEquals(1, defaultList.Count);
			AssertEquals("Gateway Jobs for Shipment #", defaultList[0].FilterName);
			AssertEquals("Property", defaultList[0].PropertyName);
			AssertEquals(charge.JR_Calc_RelatedJobNumber, defaultList[0].Value);
		}

		public void TestDebtors()
		{
			ForwardingShipment ship = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.PlugInData = ship;
			Charge charge = job.Charges.AddNew();
			Assert(charge.Debtors is DebtorCollection);

			OneOffQuoteHost quote = Factory.NewWithValidTestData<OneOffQuoteHost>();
			job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.PlugInData = quote;
			charge = job.Charges.AddNew();
			Assert(!(charge.Debtors is DebtorCollection));
		}

		public void TestAgentDeclaredSellAmount()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			AssertAgentDeclaredSellAmount(true);
			AssertAgentDeclaredSellAmount(false);
			
			void AssertAgentDeclaredSellAmount(bool isIncludedInProfitShare)
			{
				var charge = job.Charges.AddNew();

				charge.JR_IsIncludedInProfitShare = isIncludedInProfitShare;

				charge.JR_OSSellAmt = 1000m;
				AssertEquals(1000m, charge.JR_AgentDeclaredSellAmt);

				charge.JR_OSSellAmt = 2000m;
				AssertEquals(2000m, charge.JR_AgentDeclaredSellAmt);

				charge.JR_LocalSellAmt = 800m;
				AssertEquals(800m, charge.JR_AgentDeclaredSellAmt);

				charge.JR_AgentDeclaredSellAmt = 900m;
				charge.JR_LocalSellAmt = 1800m;
				AssertEquals(900m, charge.JR_AgentDeclaredSellAmt);

				charge.JR_AgentDeclaredSellAmt = 1800m;
				charge.JR_IsIncludedInProfitShare = !isIncludedInProfitShare;
				charge.JR_LocalSellAmt = 3000m;
				AssertEquals(3000m, charge.JR_AgentDeclaredSellAmt);
			}
		}

		public void TestAgentDeclaredCostAmount()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			AssertAgentDeclaredCostAmount(true);
			AssertAgentDeclaredCostAmount(false);

			void AssertAgentDeclaredCostAmount(bool isIncludedInProfitShare)
			{
				var charge = job.Charges.AddNew();

				charge.JR_IsIncludedInProfitShare = isIncludedInProfitShare;

				charge.JR_OSCostAmt = 1000m;
				AssertEquals(1000m, charge.JR_AgentDeclaredCostAmt);

				charge.JR_OSCostAmt = 2000m;
				AssertEquals(2000m, charge.JR_AgentDeclaredCostAmt);

				charge.JR_LocalCostAmt = 800m;
				AssertEquals(800m, charge.JR_AgentDeclaredCostAmt);

				charge.JR_AgentDeclaredCostAmt = 900m;
				charge.JR_LocalCostAmt = 1800m;
				AssertEquals(900m, charge.JR_AgentDeclaredCostAmt);

				charge.JR_AgentDeclaredCostAmt = 1800m;
				charge.JR_IsIncludedInProfitShare = !isIncludedInProfitShare;
				charge.JR_LocalCostAmt = 3000m;
				AssertEquals(3000m, charge.JR_AgentDeclaredCostAmt);
			}
		}

		public void TestGetRevenueAmountBasedOnCost()
		{
			AccChargeCode marginChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			marginChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			marginChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			marginChargeCode.AC_MarginPercentage = 80;
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_AC = marginChargeCode.PK;
			charge1.JR_OSCostAmt = 100m;
			AssertEquals("Expect revenue amount equal to 125 if charge is MRG type", 125m, charge1.GetRevenueAmountBasedOnCost());
			marginChargeCode.AC_MarginPercentage = 0m;
			job1.ClearChargeTypeCache_ForTestOnly();
			AssertEquals("Expect revenue amount equal to 0 if margin percentage is 0", 0m, charge1.GetRevenueAmountBasedOnCost());

			using (TransactionLineJobChargeTransformer.SetNewChargeCostIsGoingToBePostedInTransformerContext(Factory))
			{
				APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
				APInvoiceLine aPInvLine = (APInvoiceLine)apInvoice.Lines.AddNew();
				aPInvLine.AL_OSAmount = 200m;
				charge1.JR_AL_APLine = aPInvLine.PK;
				charge1.JR_OSCostAmt = 200m;
				charge1.JR_OSSellAmt = 0m;
				((InvoicingBase)charge1.APTransactionHeader_ForTestOnly).SubmittedFromInvoicingForm = true;
				AssertEquals("Expect revenue amount equal to 200 if margin percentage is 0 during AP invoice posting", 200m, charge1.GetRevenueAmountBasedOnCost());

				AccChargeCode dSBChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				dSBChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				dSBChargeCode.AC_MarginPercentage = 80;
				Charge charge2 = job1.Charges.AddNew();
				charge2.JR_GB = GlbBranch.CurrentBranch.PK;
				charge2.JR_AC = dSBChargeCode.PK;
				charge2.JR_OSCostAmt = 100m;
				AssertEquals("Expect revenue amount equal to 100 if charge is DSB type", 100m, charge2.GetRevenueAmountBasedOnCost());

				AccChargeCode revChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				revChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				revChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
				revChargeCode.AC_MarginPercentage = 80;
				Charge charge3 = job1.Charges.AddNew();
				charge3.JR_GB = GlbBranch.CurrentBranch.PK;
				charge3.JR_AC = revChargeCode.PK;
				charge3.JR_OSCostAmt = 100m;
				AssertEquals("Expect revenue amount equal to 0 if charge is REV type", 0m, charge3.GetRevenueAmountBasedOnCost());

				AccChargeCode mJAChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				mJAChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				mJAChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
				Charge charge4 = job1.Charges.AddNew();
				charge4.JR_GB = GlbBranch.CurrentBranch.PK;
				charge4.JR_AC = mJAChargeCode.PK;
				charge4.JR_OSCostAmt = 100m;
				AssertEquals("Expect revenue amount equal to 0 if charge is MJA type", 0m, charge4.GetRevenueAmountBasedOnCost());
			}
		}

		public void TestGetLocalRevenueAmountBasedOnCost()
		{
			AccChargeCode marginChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			marginChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			marginChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			marginChargeCode.AC_MarginPercentage = 80;
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_AC = marginChargeCode.PK;
			charge1.JR_RX_NKCostCurrency = "USD";
			//Charge1.JR_OSCostExRate = 1.5m;
			charge1.CostExchangeRate.SetBuyRate_ForTestOnly(1.5m);
			charge1.JR_OSCostAmt = 100m;
			AssertEquals("Expect revenue amount equal to 125 if charge is MRG type", 83.34m, charge1.GetLocalRevenueAmountBasedOnCost());
			marginChargeCode.AC_MarginPercentage = 0m;
			job1.ClearChargeTypeCache_ForTestOnly();
			AssertEquals("Expect revenue amount equal to 0 if margin percentage is 0", 0m, charge1.GetLocalRevenueAmountBasedOnCost());

			using (TransactionLineJobChargeTransformer.SetNewChargeCostIsGoingToBePostedInTransformerContext(Factory))
			{
				APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
				APInvoiceLine aPInvLine = (APInvoiceLine)apInvoice.Lines.AddNew();
				aPInvLine.AL_OSAmount = 200m;
				charge1.JR_AL_APLine = aPInvLine.PK;
				charge1.JR_OSCostAmt = 200m;
				charge1.JR_OSSellAmt = 0m;
				((InvoicingBase)charge1.APTransactionHeader_ForTestOnly).SubmittedFromInvoicingForm = true;
				AssertEquals("Expect revenue amount equal to 200 if margin percentage is 0 during AP invoice posting", 133.33m, charge1.GetLocalRevenueAmountBasedOnCost());

				AccChargeCode dSBChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				dSBChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				dSBChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
				dSBChargeCode.AC_MarginPercentage = 80;
				Charge charge2 = job1.Charges.AddNew();
				charge2.JR_GB = GlbBranch.CurrentBranch.PK;
				charge2.JR_AC = dSBChargeCode.PK;
				charge2.JR_RX_NKCostCurrency = "USD";
				charge2.JR_OSCostExRate = 1.5m;
				charge2.JR_OSCostAmt = 100m;
				AssertEquals("Expect revenue amount equal to 100 if charge is DSB type", 66.67m, charge2.GetLocalRevenueAmountBasedOnCost());

				AccChargeCode revChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				revChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				revChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
				revChargeCode.AC_MarginPercentage = 80;
				Charge charge3 = job1.Charges.AddNew();
				charge3.JR_GB = GlbBranch.CurrentBranch.PK;
				charge3.JR_AC = revChargeCode.PK;
				charge3.JR_RX_NKCostCurrency = "USD";
				charge3.JR_OSCostExRate = 1.5m;
				charge3.JR_OSCostAmt = 100m;
				AssertEquals("Expect revenue amount equal to 0 if charge is REV type", 0m, charge3.GetLocalRevenueAmountBasedOnCost());

				AccChargeCode mJAChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				mJAChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				mJAChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
				Charge charge4 = job1.Charges.AddNew();
				charge4.JR_GB = GlbBranch.CurrentBranch.PK;
				charge4.JR_AC = mJAChargeCode.PK;
				charge4.JR_RX_NKCostCurrency = "USD";
				charge4.JR_OSCostExRate = 1.5m;
				charge4.JR_OSCostAmt = 100m;
				AssertEquals("Expect revenue amount equal to 0 if charge is MJA type", 0m, charge4.GetLocalRevenueAmountBasedOnCost());
			}
		}

		public void TestJobReference()
		{
			ForwardingShipment ship = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "Job1";
			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_LocalSellAmt = 100m;

			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = ship.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job2.JH_JobNum = "Job2";
			ship.JS_UniqueConsignRef = job2.JH_JobNum;
			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_JH = job2.PK;
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_LocalSellAmt = 100m;

			Charge charge3 = Factory.NewWithValidTestData<Charge>();
			charge3.JR_AC = Env.Registry.FreightChargeCode;
			charge3.JR_LocalSellAmt = 100m;

			Factory.Save();

			job1.Charges.IncludeChargesFromJobs(job2.PK);
			job1.Charges.Load();

			AssertEquals("Job1", charge1.JobReference);
			AssertEquals("Job2", charge2.JobReference);
			AssertEquals(charge3.Job.JH_JobNum, charge3.JobReference);

			AssertEquals("Job1", ((Charge)job1.Charges.FindByPK(charge1.PK)).JobReference);
			AssertEquals("Job2", ((Charge)job1.Charges.FindByPK(charge2.PK)).JobReference);
		}

		#region Charge Code Collection Filter

		public void TestChargeCodeCollection()
		{
			TestCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			AssertNoDefault(TestCharge.Lookups.ChargeCodes, "Charge Group", "Property");

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeCode4.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			chargeCode5.AC_ChargeType = "ABC";

			var chargeCodes = new AccChargeCodeCollection(Factory, TestCharge.Lookups.ChargeCodes.CompleteFilter);
			chargeCodes.Load();

			AssertEquals("Collection should contain MRG charge code", true, chargeCodes.Contains(chargeCode1));
			AssertEquals("Collection should contain DSB charge code", true, chargeCodes.Contains(chargeCode2));
			AssertEquals("Collection should contain REV charge code", true, chargeCodes.Contains(chargeCode3));
			AssertEquals("Collection should contain MJA charge code", true, chargeCodes.Contains(chargeCode4));
			AssertEquals("Collection should not contain CMT charge code", false, chargeCodes.Contains(chargeCode5));

			OneOffQuoteHost quote = Factory.NewWithValidTestData<OneOffQuoteHost>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.PlugInData = quote;
			Charge charge1 = job.Charges.AddNew();
			AssertHasDefault(charge1.Lookups.ChargeCodes, "Dept Filter", "Property", new ZString("BRN"));
			AssertNoDefault(charge1.Lookups.ChargeCodes, "Charge Group", "Property");

			((OneOffQuoteHostInvoicingSupporter)quote.InvoicingSupporter).fDefaultChargeGroup = "SDS";
			Charge charge2 = job.Charges.AddNew();
			AssertHasDefault(charge2.Lookups.ChargeCodes, "Dept Filter", "Property", new ZString("BRN"));
			AssertHasDefault(charge2.Lookups.ChargeCodes, "Charge Group", "Property", new ZString("SDS"));
		}

		#endregion

		#region Validation

		public virtual void TestValidateJR_OSCostExRate()
		{
			TestCharge.JR_AC = TestObjectCreator.MRG100.PK;
			TestCharge.JR_RX_NKCostCurrency = "USD";
			TestCharge.JR_OSCostExRate = 0;
			Assert("Cost ExRate must be set unless the charge code is a Revenue Charge Code", TestCharge.JR_OSCostExRateInfo.HasErrors());

			TestCharge.JR_OSCostExRate = 10;
			Assert("Cost ExRate must be set unless the charge code is a Revenue Charge Code", !TestCharge.JR_OSCostExRateInfo.HasErrors());

			TestCharge.JR_OSCostExRate = -10;
			Assert("Cost ExRate must be > 0 unless the charge code is a Revenue Charge Code", TestCharge.JR_OSCostExRateInfo.HasErrors());

			if (AllowsRevenueChargeCode)
			{
				TestCharge.JR_AC = TestObjectCreator.RevenueChargeCode.PK;
				TestCharge.JR_OSCostExRate = 0;
				TestCharge.Validation.ValidateJR_OSCostExRate();
				Assert("Cost ExRate must be set unless the charge code is a Revenue Charge Code", !TestCharge.JR_OSCostExRateInfo.HasErrors());
			}
		}

		protected virtual bool AllowsRevenueChargeCode
		{
			get { return true; }
		}

		public virtual void TestValidateJR_AB()
		{
			TestCharge.JR_APInvoiceNum = "0001";
			TestCharge.JR_RX_NKCostCurrency = "AUD";
			TestCharge.JR_AB = ZGuid.NewZGuid();
			AssertEquals("Invalid value", true, TestCharge.JR_ABInfo.HasErrors());

			AccBankAccount invalidAcct = Factory.New<AccBankAccount>();
			invalidAcct.AB_RX_NKAccountCurrency = "USD";
			TestCharge.Validation.ValidateJR_AB();
			TestCharge.JR_AB = invalidAcct.PK;
			AssertEquals("All currency Bank Accounts is valid now.", false, TestCharge.JR_ABInfo.HasErrors());

			AccBankAccount validAcct = Factory.New<AccBankAccount>();
			validAcct.AB_RX_NKAccountCurrency = TestCharge.JR_RX_NKCostCurrency;
			TestCharge.JR_AB = validAcct.PK;
			AssertEquals("Valid Account", false, TestCharge.JR_ABInfo.HasErrors());
		}

		public virtual void TestValdiateJR_AK()
		{
			TestCharge.JR_RX_NKCostCurrency = "AUD";
			AccBankAccount bankAcct = Factory.New<AccBankAccount>();
			bankAcct.AB_RX_NKAccountCurrency = TestCharge.JR_RX_NKCostCurrency;
			TestCharge.JR_AB = bankAcct.PK;

			TestCharge.JR_AK = ZGuid.NewZGuid();
			AssertEquals("Invalid value", true, TestCharge.JR_AKInfo.HasErrors());

			AccChequeBook invalidChq = Factory.New<AccChequeBook>();
			invalidChq.AK_AB = ZGuid.NewZGuid();
			TestCharge.Validation.ValidateJR_AK();
			TestCharge.JR_AK = invalidChq.PK;
			AssertEquals("ChqBook does not belong to Account", true, TestCharge.JR_AKInfo.HasErrors());

			AccChequeBook validChq = Factory.New<AccChequeBook>();
			validChq.AK_AB = bankAcct.PK;
			TestCharge.JR_AK = validChq.PK;
			AssertEquals("Valid Chq Book", false, TestCharge.JR_AKInfo.HasErrors());
		}

		public virtual void TestValidateJR_GB()
		{
			TestCharge.JR_GB = ZGuid.NewZGuid();
			TestCharge.Validation.ValidateJR_GB();
			AssertHasErrors(TestCharge.JR_GBInfo);

			TestCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			TestCharge.Validation.ValidateJR_GB();
			AssertNoErrors(TestCharge.JR_GBInfo);

			TestCharge.JR_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			TestCharge.Validation.ValidateJR_GB();
			AssertHasErrors(TestCharge.JR_GBInfo);

			TestCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			TestCharge.Validation.ValidateJR_GB();
			AssertNoErrors(TestCharge.JR_GBInfo);
		}

		#endregion

		#region Currency is USD

		public void TestIsSellUSD()
		{
			RefCurrency uSCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			RefCurrency otherCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "CAD"));
			TestCharge.JR_RX_NKSellCurrency = otherCurrency.RX_Code;
			Assert("Should not be US sell currency", !TestCharge.IsSellUSD);
			TestCharge.JR_RX_NKSellCurrency = uSCurrency.RX_Code;
			Assert("Should be US sell currency", TestCharge.IsSellUSD);
		}

		#endregion

		#region TestChargeCodeSetsDebtor

		public void TestChargeCodeSetsDebtor()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var cne = newFactory.NewWithValidTestData<OrgHeader>();
			var cnr = newFactory.NewWithValidTestData<OrgHeader>();
			var overseasAgent = newFactory.NewWithValidTestData<OrgHeader>();
			var overseasAgent2 = newFactory.NewWithValidTestData<OrgHeader>();

			var originChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			originChargeCode.AC_ChargeGroup = "ORG";

			var freightChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			freightChargeCode.AC_ChargeGroup = "FRT";

			var destChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			destChargeCode.AC_ChargeGroup = "DST";

			newFactory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			ForwardingShipment exportShipment = creator.CreateShipment("S00001234", "AUSYD", "USLAX");
			exportShipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			exportShipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			exportShipment.JS_OH_DeliveryAgent = overseasAgent.PK;

			ForwardingShipment importShipment = creator.CreateShipment("S00001235", "USLAX", "AUSYD");
			importShipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			importShipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			importShipment.JS_OH_DeliveryAgent = overseasAgent.PK;

			ForwardingShipment crossTradeShipment = creator.CreateShipment("S00001236", "USLAX", "INBOM");
			crossTradeShipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			crossTradeShipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			crossTradeShipment.JS_OH_DeliveryAgent = overseasAgent.PK;

			//CrossTrade
			using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Job testJob = Accounting.Business.JobInvoicing.Job.CreateWithMutex(newFactory, crossTradeShipment))
			{
				testJob.PlugInData = crossTradeShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", originChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", freightChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", destChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "FOB", originChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "FOB", freightChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "FOB", destChargeCode, cne.PK);

				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "DDP", originChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "DDP", freightChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "DDP", destChargeCode, overseasAgent.PK);

				IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, destChargeCode.PK.ToString());
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "DDP", destChargeCode, cne.PK);
				IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);

				testJob.AgentCollectPK = overseasAgent2.PK;
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, Core.Constants.IncoTerms.CostInsuranceAndFreight, freightChargeCode, overseasAgent2.PK);
			}

			// Export
			using (Job testJob = Accounting.Business.JobInvoicing.Job.CreateWithMutex(newFactory, exportShipment))
			{
				testJob.PlugInData = exportShipment;
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "EXW", originChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "EXW", freightChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "EXW", destChargeCode, overseasAgent.PK);

				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "FOB", originChargeCode, cnr.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "FOB", freightChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "FOB", destChargeCode, overseasAgent.PK);

				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "DDP", originChargeCode, cnr.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "DDP", freightChargeCode, cnr.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "DDP", destChargeCode, cnr.PK);

				IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, destChargeCode.PK.ToString());
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "EXW", destChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "FOB", destChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(exportShipment, testJob, "DDP", destChargeCode, overseasAgent.PK);
				IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			}

			// Import
			using (Job testJob = Accounting.Business.JobInvoicing.Job.CreateWithMutex(newFactory, importShipment))
			{
				testJob.PlugInData = importShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "EXW", originChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "EXW", freightChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "EXW", destChargeCode, cne.PK);

				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "FOB", originChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "FOB", freightChargeCode, cne.PK);
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "FOB", destChargeCode, cne.PK);

				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "DDP", originChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "DDP", freightChargeCode, overseasAgent.PK);
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "DDP", destChargeCode, overseasAgent.PK);

				IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, destChargeCode.PK.ToString());
				AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, "DDP", destChargeCode, cne.PK);
				IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty);
			}
		}

		public void TestChargeCodeSetsDebtor_DefaultDebtorFromInvoiceJob()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var org1 = newFactory.NewWithValidTestData<OrgHeader>();
			var org2 = newFactory.NewWithValidTestData<OrgHeader>();

			var originChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			originChargeCode.AC_ChargeGroup = "ORG";

			newFactory.Save();

			var crossTradeShipment = Factory.NewWithValidTestData<OneOffQuoteHost>();
			crossTradeShipment.JS_TransportMode = "SEA";
			crossTradeShipment.JS_PackingMode = "LCL";
			crossTradeShipment.JS_INCO = "FOB";
			crossTradeShipment.JS_UniqueConsignRef = "S00001235";
			crossTradeShipment.JS_HouseBill = "UVWXYZ";
			crossTradeShipment.JS_RL_NKOrigin = "USLAX";
			crossTradeShipment.JS_RL_NKDestination = "INBOM";
			crossTradeShipment.JS_ActualChargeable = 100M;
			crossTradeShipment.ConsigneeDocumentaryAddress.OrganisationPK = org1.PK;

			using (Job testJob = Accounting.Business.JobInvoicing.Job.CreateWithMutex(newFactory, crossTradeShipment))
			{
				testJob.PlugInData = crossTradeShipment;
				testJob.AgentCollectPK = org1.PK;
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", originChargeCode, org1.PK);

				((OneOffQuoteHostInvoicingSupporter)crossTradeShipment.InvoicingSupporter).fDefaultDebtor = org2;
				AssertDebtorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", originChargeCode, org2.PK);
			}
		}

		OrgHeader LocalClientForSetDebtor;
		OrgHeader OverseasAgentForSetDebtor;

		public void TestChargeCodeSetsCorrectDebtor()
		{
			var helper = new TestHelper(Factory);
			var creator = new TestObjectCreator(Factory);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			#region Setup Charges

			var calculatorCode = UnitCalculator.Code;
			var oBR_ChargeCode = helper.ChargeCodes.New("OBR1", "OBR", calculatorCode, ChargeCodeGroupList.Codes.OriginBrokerage);
			var oBO_ChargeCode = helper.ChargeCodes.New("OBO1", "OBO", calculatorCode, ChargeCodeGroupList.Codes.OriginBrokerageOnly);
			var cDS_ChargeCode = helper.ChargeCodes.New("CDS1", "CDS", calculatorCode, ChargeCodeGroupList.Codes.CustomsDuty);
			var bON_ChargeCode = helper.ChargeCodes.New("BON1", "BON", calculatorCode, ChargeCodeGroupList.Codes.BrokerageOnly);
			var bRK_ChargeCode = helper.ChargeCodes.New("BRK1", "BRK", calculatorCode, ChargeCodeGroupList.Codes.Brokerage);
			var cLL_ChargeCode = helper.ChargeCodes.New("CLL1", "CLL", calculatorCode, ChargeCodeGroupList.Codes.CFSLoadList);
			var cSH_ChargeCode = helper.ChargeCodes.New("CSH1", "CSH", calculatorCode, ChargeCodeGroupList.Codes.CFSShipment);
			var cST_ChargeCode = helper.ChargeCodes.New("CST1", "CST", calculatorCode, ChargeCodeGroupList.Codes.ContainerStorage);
			var lOD_ChargeCode = helper.ChargeCodes.New("LOD1", "LOD", calculatorCode, ChargeCodeGroupList.Codes.Loading);
			var oRG_ChargeCode = helper.ChargeCodes.New("ORG1", "ORG", calculatorCode, ChargeCodeGroupList.Codes.Origin);
			var fRT_ChargeCode = helper.ChargeCodes.New("FRT1", "FRT", calculatorCode, ChargeCodeGroupList.Codes.Freight);
			var iNS_ChargeCode = helper.ChargeCodes.New("INS1", "INS", calculatorCode, ChargeCodeGroupList.Codes.Insurance);
			var dST_ChargeCode = helper.ChargeCodes.New("DST1", "DST", calculatorCode, ChargeCodeGroupList.Codes.Destination);
			var uNL_ChargeCode = helper.ChargeCodes.New("UNL1", "UNL", calculatorCode, ChargeCodeGroupList.Codes.Unloading);
			var sDS_ChargeCode = helper.ChargeCodes.New("SDS1", "SDS", calculatorCode, ChargeCodeGroupList.Codes.ShippingDisbursements);

			#endregion

			#region Setup Organizations

			var localClient = helper.NewOrgHeader();
			var overseasAgent = helper.NewOrgHeader();
			var consignor = helper.NewOrgHeader();
			var consignee = helper.NewOrgHeader();

			localClient.OH_Code = "LOCALCLIENT";
			overseasAgent.OH_Code = "OVERSEAS";
			consignor.OH_Code = "CONSIGNOR";
			consignee.OH_Code = "CONSIGNEE";

			LocalClientForSetDebtor = localClient;
			OverseasAgentForSetDebtor = overseasAgent;

			var cneIFT = helper.NewOrgHeader();
			var cnrIFT = helper.NewOrgHeader();

			cneIFT.OH_Code = "CNEIFT";
			cnrIFT.OH_Code = "CNRIFT";

			consignee.SetRelatedParty(cneIFT, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			consignor.SetRelatedParty(cnrIFT, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			#endregion

			Factory.Save();

			#region Forwarding

			#region Export

			var exportShipment = creator.CreateShipment("EXPORT_SHP");
			exportShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			exportShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			exportShipment.JS_RL_NKOrigin = "AUSYD";
			exportShipment.JS_RL_NKDestination = "USLAX";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, exportShipment))
			{
				testJob.PlugInData = exportShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oBO_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, oBO_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, cDS_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, cDS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, bRK_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, bRK_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, lOD_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, oRG_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, dST_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, uNL_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(exportShipment, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, overseasAgent);
				});
			}

			#endregion

			#region Import

			var importShipment = creator.CreateShipment("IMPORT_SHP");
			importShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			importShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			importShipment.JS_RL_NKOrigin = "USLAX";
			importShipment.JS_RL_NKDestination = "AUSYD";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, importShipment))
			{
				testJob.PlugInData = importShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oBO_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, oBO_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, cDS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, cDS_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, bRK_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, bRK_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, dST_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, uNL_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Shipment_RegistryCheck(importShipment, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, localClient);
				});
			}

			#endregion

			#region Cross Trade

			var crossTradeShipment = creator.CreateShipment("CROSSTRADE_SHP");
			crossTradeShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			crossTradeShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			crossTradeShipment.JS_RL_NKOrigin = "USLAX";
			crossTradeShipment.JS_RL_NKDestination = "INBOM";

			using (AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, crossTradeShipment))
			{
				testJob.PlugInData = crossTradeShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oBO_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, oBO_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, cDS_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, cDS_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, bRK_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, bRK_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, lOD_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, oRG_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, dST_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, cneIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, uNL_ChargeCode, cnrIFT);
					AssertDebtorCorrect_Shipment_RegistryCheck(crossTradeShipment, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, cneIFT);
				});
			}

			#endregion

			#region Domestic

			var domesticShipment = creator.CreateShipment("DOMESTIC_SHP");
			domesticShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			domesticShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			domesticShipment.JS_RL_NKOrigin = "AUSYD";
			domesticShipment.JS_RL_NKDestination = "AUMEL";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, domesticShipment))
			{
				testJob.PlugInData = domesticShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Collect, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Collect, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, dST_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, uNL_ChargeCode, localClient);
					AssertDebtorCorrect_Shipment_RegistryCheck(domesticShipment, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, localClient);
				});
			}

			#endregion

			#endregion

			#region Liner and Agency

			#region Export

			var exportBOL = Factory.NewWithValidTestData<BillOfLading>();
			exportBOL.JS_TransportMode = Constants.TransportModes.Sea;
			exportBOL.JS_PackingMode = Constants.ContainerModes.FCL;
			exportBOL.JS_UniqueConsignRef = "EXPORT_BOL";
			exportBOL.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			exportBOL.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			exportBOL.JS_RL_NKOrigin = "AUSYD";
			exportBOL.JS_RL_NKDestination = "USLAX";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, exportBOL))
			{
				testJob.PlugInData = exportBOL;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Collect, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, sDS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(exportBOL, testJob, Constants.DomesticPaymentTerms.Collect, sDS_ChargeCode, localClient);
				});
			}

			#endregion

			#region Import

			var importBOL = Factory.NewWithValidTestData<BillOfLading>();
			importBOL.JS_TransportMode = Constants.TransportModes.Sea;
			importBOL.JS_PackingMode = Constants.ContainerModes.FCL;
			importBOL.JS_UniqueConsignRef = "IMPORT_BOL";
			importBOL.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			importBOL.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			importBOL.JS_RL_NKOrigin = "USLAX";
			importBOL.JS_RL_NKDestination = "AUSYD";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, importBOL))
			{
				testJob.PlugInData = importBOL;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Collect, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, sDS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(importBOL, testJob, Constants.DomesticPaymentTerms.Collect, sDS_ChargeCode, localClient);
				});
			}

			#endregion

			#region Cross Trade

			var crossTradeBOL = Factory.NewWithValidTestData<BillOfLading>();
			crossTradeBOL.JS_TransportMode = Constants.TransportModes.Sea;
			crossTradeBOL.JS_PackingMode = Constants.ContainerModes.FCL;
			crossTradeBOL.JS_UniqueConsignRef = "CROSSTRADE_BOL";
			crossTradeBOL.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			crossTradeBOL.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			crossTradeBOL.JS_RL_NKOrigin = "USLAX";
			crossTradeBOL.JS_RL_NKDestination = "INBOM";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, crossTradeBOL))
			{
				testJob.PlugInData = crossTradeBOL;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Collect, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, consignor);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, consignee);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, sDS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(crossTradeBOL, testJob, Constants.DomesticPaymentTerms.Collect, sDS_ChargeCode, localClient);
				});
			}

			#endregion

			#region Domestic

			var domesticBOL = Factory.NewWithValidTestData<BillOfLading>();
			domesticBOL.JS_TransportMode = Constants.TransportModes.Sea;
			domesticBOL.JS_PackingMode = Constants.ContainerModes.FCL;
			domesticBOL.JS_UniqueConsignRef = "DOMESTIC_BOL";
			domesticBOL.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			domesticBOL.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			domesticBOL.JS_RL_NKOrigin = "AUSYD";
			domesticBOL.JS_RL_NKDestination = "AUMEL";

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, domesticBOL))
			{
				testJob.PlugInData = domesticBOL;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Collect, cST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Prepaid, sDS_ChargeCode, localClient);
					AssertDebtorCorrect_AgencyShipment(domesticBOL, testJob, Constants.DomesticPaymentTerms.Collect, sDS_ChargeCode, localClient);
				});
			}

			#endregion

			#endregion

			#region Customs

			#region Export

			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			exportDeclaration.JE_TransportMode = Constants.TransportModes.Air;
			exportDeclaration.JE_ContainerMode = Constants.ContainerModes.Loose;
			exportDeclaration.JE_DeclarationReference = "EXPORT_DECL";
			exportDeclaration.JE_OH_Importer = consignee.PK;
			exportDeclaration.JE_OH_Supplier = consignor.PK;
			exportDeclaration.JE_RL_NKOrigin = "AUSYD";
			exportDeclaration.JE_RL_NKFinalDestination = "USLAX";
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			exportDeclaration.JE_GB = Env.CurrentBranch.PK;

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, exportDeclaration))
			{
				testJob.PlugInData = exportDeclaration;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, oBR_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, oBR_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, bON_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, bON_ChargeCode, overseasAgent);

					exportDeclaration.JE_JS = exportShipment.PK;

					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, oBO_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, oBO_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, cDS_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, cDS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, bRK_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, bRK_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, lOD_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, oRG_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, dST_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, uNL_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(exportDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, overseasAgent);
				});
			}

			#endregion

			#region Import

			var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			importDeclaration.JE_TransportMode = Constants.TransportModes.Air;
			importDeclaration.JE_ContainerMode = Constants.ContainerModes.Loose;
			importDeclaration.JE_DeclarationReference = "IMPORT_DECL";
			importDeclaration.JE_OH_Importer = consignee.PK;
			importDeclaration.JE_OH_Supplier = consignor.PK;
			importDeclaration.JE_RL_NKOrigin = "USLAX";
			importDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_GB = Env.CurrentBranch.PK;

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, importDeclaration))
			{
				testJob.PlugInData = importDeclaration;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, oBR_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, oBR_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, bON_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, bON_ChargeCode, localClient);

					importDeclaration.JE_JS = importShipment.PK;

					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, oBO_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, oBO_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, cDS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, cDS_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, bRK_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, bRK_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, lOD_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, lOD_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, oRG_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, oRG_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, fRT_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, iNS_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, iNS_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, dST_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, dST_ChargeCode, localClient);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Prepaid, uNL_ChargeCode, overseasAgent);
					AssertDebtorCorrect_Declaration_RegistryCheck(importDeclaration, testJob, Constants.DomesticPaymentTerms.Collect, uNL_ChargeCode, localClient);
				});
			}

			#endregion

			#endregion

			#region CFS

			var loadList = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Freight.Integration.CFS.ICFSLoadListConsol)));
			loadList.JK_UniqueConsignRef = "LOADLIST1";
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.JK_ConsolMode = Constants.ContainerModes.FCL;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "USLAX";

			var cfsShipment = loadList.Shipments.AddNew();
			cfsShipment.JS_UniqueConsignRef = "CFS";
			cfsShipment.JS_TransportMode = Constants.TransportModes.Sea;
			cfsShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			cfsShipment.JS_ShipmentType = "STD";
			cfsShipment.JS_INCO = "CLT";
			cfsShipment.JS_RL_NKOrigin = "AUSYD";
			cfsShipment.JS_RL_NKDestination = "USLAX";
			cfsShipment.ConsigneeDeliveryAddress.E2_OA_Address = consignee.MainAddress.PK;
			cfsShipment.ConsignorPickupAddress.E2_OA_Address = consignor.MainAddress.PK;

			using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, cfsShipment))
			{
				testJob.PlugInData = cfsShipment;
				testJob.AgentCollectPK = overseasAgent.PK;
				testJob.LocalChargesPK = localClient.PK;

				CombineAssertions(() =>
				{
					AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, cLL_ChargeCode, localClient.PK);
					AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, cSH_ChargeCode, localClient.PK);
					AssertDebtorCorrectWhenSettingCharge(importShipment, testJob, Constants.DomesticPaymentTerms.Prepaid, cST_ChargeCode, localClient.PK);
				});
			}

			#endregion

			#region Transport

			//To be tested in Transport solution

			#endregion

			#region Warehouse

			//To be tested in Warehouse solution

			#endregion
		}

		public void TestChargeCodeSetsCorrectDebtor_ForAgencyBookingAndBillOfLading()
		{
			var helper = new TestHelper(Factory);

			Func<ZString, ZBool, OrgHeader> setupTestOrg = (orgCode, isDebtor) =>
			{
				var org = helper.NewOrgHeader();
				org.OH_Code = orgCode;
				org.OH_IsDebtor = isDebtor;

				return org;
			};

			var bookingOrg = setupTestOrg("BOOKINGORG", true);
			var localClientAsValidDebtor = setupTestOrg("VALIDORG", true);
			var localClientAsInvalidDebtor = setupTestOrg("INVALIDORG", false);

			Factory.Save();

			Action<AgencyShipment> assertDebtor = shipment =>
			{
				using (var testJob = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
				{
					var fRT_ChargeCode = helper.ChargeCodes.New("FRT1", "FRT", ChargeCodeGroupList.Codes.Freight);

					testJob.PlugInData = shipment;
					Factory.SetContext(BusinessContext.InvoicingPlugInGUI);

					testJob.LocalChargesPK = bookingOrg.PK;
					AssertDebtorCorrect_AgencyShipment(shipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, bookingOrg);

					testJob.LocalChargesPK = localClientAsValidDebtor.PK;
					AssertDebtorCorrect_AgencyShipment(shipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, localClientAsValidDebtor);

					testJob.LocalChargesPK = localClientAsInvalidDebtor.PK;
					AssertDebtorCorrect_AgencyShipment(shipment, testJob, Constants.DomesticPaymentTerms.Prepaid, fRT_ChargeCode, null);
				}
			};

			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			assertDebtor(booking);

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			assertDebtor(billOfLading);
		}

		void AssertDebtorCorrectWhenSettingCharge(CommonShipment shipment, Job job, ZString incoTerm, AccChargeCode chargeCode, ZGuid expectedDebtor)
		{
			shipment.JS_INCO = incoTerm;

			BaseCharge charge = job.Charges.AddNew();
			Assert(charge.JR_OH_SellAccount.IsEmpty);

			charge.JR_AC = chargeCode.PK;
			var sellAccountOrg = Factory.Load<OrgHeader>(charge.JR_OH_SellAccount);
			var expectedOrg = Factory.Load<OrgHeader>(expectedDebtor);
			var testParameters = $"Job: {shipment.JS_UniqueConsignRef}, Payment Term: {incoTerm}, Charge Group: {chargeCode.AC_ChargeGroup}";

			AssertEquals(testParameters, expectedOrg?.OH_Code, sellAccountOrg?.OH_Code);
		}

		void AssertDebtorCorrect_Shipment_RegistryCheck(CommonShipment shipment, Job job, string prepaidOrCollect, AccChargeCode chargeCode, OrgHeader expectedDebtor)
		{
			var incoTerm = prepaidOrCollect == Constants.DomesticPaymentTerms.Collect
												? Constants.IncoTerms.ExWorks
												: chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.CustomsDuty
													? Constants.IncoTerms.DeliveredDutyPaid
													: Constants.IncoTerms.DeliveredAtFrontier;

			var expectedDebtorPK = expectedDebtor == null ? ZGuid.Empty : expectedDebtor.PK;
			AssertDebtorCorrectWhenSettingCharge(shipment, job, incoTerm, chargeCode, expectedDebtorPK);

			using (IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToString()))
			{
				AssertDebtorCorrectWhenSettingCharge(shipment, job, incoTerm, chargeCode, LocalClientForSetDebtor.PK);
			}

			using (IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToString()))
			{
				AssertDebtorCorrectWhenSettingCharge(shipment, job, incoTerm, chargeCode, OverseasAgentForSetDebtor.PK);
			}
		}

		void AssertDebtorCorrect_AgencyShipment(AgencyShipment shipment, Job job, string incoTerm, AccChargeCode chargeCode, OrgHeader expectedDebtor)
		{
			var expectedDebtorPK = expectedDebtor == null ? ZGuid.Empty : expectedDebtor.PK;
			AssertDebtorCorrectWhenSettingCharge(shipment, job, incoTerm, chargeCode, expectedDebtorPK);
		}

		void AssertDebtorCorrectWhenSettingChargeDeclaration(BaseJobDeclaration declaration, Job job, string prepaidOrCollect, AccChargeCode chargeCode, ZGuid expectedDebtor)
		{
			declaration.JE_ShipmentIncoTerm = prepaidOrCollect == Constants.DomesticPaymentTerms.Collect
												? Constants.IncoTerms.ExWorks
												: chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.CustomsDuty
													? Constants.IncoTerms.DeliveredDutyPaid
													: Constants.IncoTerms.DeliveredAtFrontier;

			var charge = job.Charges.AddNew();
			Assert(charge.JR_OH_SellAccount.IsEmpty);

			charge.JR_AC = chargeCode.PK;
			var sellAccountOrg = Factory.Load<OrgHeader>(charge.JR_OH_SellAccount);
			var expectedOrg = Factory.Load<OrgHeader>(expectedDebtor);
			var testParameters = $"Job: {declaration.JE_DeclarationReference}, Payment Term: {declaration.JE_ShipmentIncoTerm}, Charge Group: {chargeCode.AC_ChargeGroup}";

			AssertEquals(testParameters, expectedOrg?.OH_Code, sellAccountOrg?.OH_Code);
		}

		void AssertDebtorCorrect_Declaration_RegistryCheck(BaseJobDeclaration declaration, Job job, string prepaidOrCollect, AccChargeCode chargeCode, OrgHeader expectedDebtor)
		{
			var expectedDebtorPK = expectedDebtor == null ? ZGuid.Empty : expectedDebtor.PK;
			AssertDebtorCorrectWhenSettingChargeDeclaration(declaration, job, prepaidOrCollect, chargeCode, expectedDebtorPK);

			using (IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToString()))
			{
				AssertDebtorCorrectWhenSettingChargeDeclaration(declaration, job, prepaidOrCollect, chargeCode, LocalClientForSetDebtor.PK);
			}

			using (IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToString()))
			{
				AssertDebtorCorrectWhenSettingChargeDeclaration(declaration, job, prepaidOrCollect, chargeCode, OverseasAgentForSetDebtor.PK);
			}
		}

		#endregion

		#region TestChargeCodeSetsProfitShareIncludedFlag

		public void TestChargeCodeSetsProfitShareIncludedFlag()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();

			var chargeCode = orgFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			orgFactory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Assert("Precondition: Shipment is an export", shipment.IsExport());

			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;

			BaseCharge charge = testJob.Charges.AddNew();
			AssertEquals("Pre-condition: JR_IsIncludedInProfitShare is false", false, charge.JR_IsIncludedInProfitShare);

			charge.JR_AC = chargeCode.PK;
			AssertEquals("JR_IsIncludedInProfitShare now should be set", true, charge.JR_IsIncludedInProfitShare);

			BaseCharge charge2 = testJob.Charges.AddNew();
			AssertEquals("Pre-condition: JR_IsIncludedInProfitShare is false", false, charge2.JR_IsIncludedInProfitShare);

			using (charge2.GetSuspenderForConsolCostImporter())
			{
				charge2.JR_AC = chargeCode.PK;
			}

			AssertEquals("JR_IsIncludedInProfitShare should not be set because it is used by ConsolCostImporter", false, charge2.JR_IsIncludedInProfitShare);

			profitShareAgreement.O4_AgreementType = "CLF";
			profitShareFactory.Save();
			shipment.JS_INCO = "CFR";
			BaseCharge charge3 = testJob.Charges.AddNew();
			charge3.JR_AC = chargeCode.PK;
			AssertEquals("JR_IsIncludedInProfitShare now should not be set", false, charge3.JR_IsIncludedInProfitShare);
		}

		public void TestSetProfitShareIncludedFlag()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();

			var chargeCode = orgFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			orgFactory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Assert("Precondition: Shipment is an export", shipment.IsExport());

			Job testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BaseCharge charge = testJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			BaseCharge charge2 = testJob.Charges.AddNew();
			using (charge2.GetSuspenderForConsolCostImporter())
			{
				charge2.JR_AC = chargeCode.PK;
			}
			BaseCharge charge3 = testJob.Charges.AddNew();

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;

			AssertEquals("Pre-condition: JR_IsIncludedInProfitShare is false", false, charge.JR_IsIncludedInProfitShare);
			charge.SetProfitShareIncludedFlag();
			AssertEquals("JR_IsIncludedInProfitShare now should be set", true, charge.JR_IsIncludedInProfitShare);

			AssertEquals("Pre-condition: JR_IsIncludedInProfitShare is false", false, charge2.JR_IsIncludedInProfitShare);
			charge2.SetProfitShareIncludedFlag();
			AssertEquals("JR_IsIncludedInProfitShare should not be set because it is used by ConsolCostImporter", true, charge2.JR_IsIncludedInProfitShare);
		}

		#endregion

		#region TestChargeCodeSetsDefaultCreditor

		public void TestChargeCodeSetsDefaultCreditor()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(newFactory);

			OrgHeader org1 = newFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = newFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = newFactory.NewWithValidTestData<OrgHeader>();

			AccChargeCode chargeCode1 = newFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Margin;

			AccChargeCode chargeCode2 = newFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

			AccChargeCode chargeCode3 = newFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			AccChargeCode chargeCode4 = newFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode4.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			newFactory.Save();

			CommonCartage cartage = creator.CreateCartage();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();

			CommonWorkSheet workSheet = newFactory.New<CommonWorkSheet>();
			workSheet.EY_OH_TransportCo = org1.PK;
			leg2.JU_EY_RunSheet = workSheet.PK;

			using (Job testJob = JobInvoicing.Job.CreateWithMutex(newFactory, cartage))
			{
				testJob.PlugInData = cartage;

				BaseCharge charge1 = testJob.Charges.AddNew();
				Assert("No charge code", charge1.JR_OH_CostAccount.IsEmpty);
				charge1.JR_AC = chargeCode1.PK;
				AssertEquals("Creditor should be org1", org1.PK, charge1.JR_OH_CostAccount);

				BaseCharge charge2 = testJob.Charges.AddNew();
				workSheet.EY_OH_TransportCo = org2.PK;
				charge2.JR_AC = chargeCode2.PK;
				AssertEquals("Creditor should now be org2", org2.PK, charge2.JR_OH_CostAccount);

				BaseCharge charge3 = testJob.Charges.AddNew();
				charge3.JR_AC = chargeCode3.PK;
				AssertEquals("Charge code does not have a creditor", ZGuid.Empty, charge3.JR_OH_CostAccount);

				BaseCharge charge4 = testJob.Charges.AddNew();
				workSheet.EY_OH_TransportCo = ZGuid.Empty;
				charge4.JR_AC = chargeCode2.PK;
				AssertEquals("No Default Creditor on cartage", ZGuid.Empty, charge4.JR_OH_CostAccount);

				BaseCharge charge5 = testJob.Charges.AddNew();
				workSheet.EY_OH_TransportCo = org3.PK;
				charge5.JR_AC = chargeCode4.PK;
				AssertEquals("Charge should now be  does not have a creditor", org3.PK, charge5.JR_OH_CostAccount);
			}
		}

		public void TestChargeCodeSetsCreditor()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var creditor = newFactory.NewWithValidTestData<OrgHeader>();
			var debtor = newFactory.NewWithValidTestData<OrgHeader>();

			OrgInvoiceRollupOrGroup group = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = "ALL";
			group.PG_InvoicePostingStyle = "FIO";

			var originChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			originChargeCode.AC_ChargeGroup = "ORG";
			originChargeCode.AC_ChargeType = Constants.ChargeType.Margin;

			newFactory.Save();

			var crossTradeShipment = Factory.NewWithValidTestData<OneOffQuoteHost>();
			crossTradeShipment.JS_TransportMode = "SEA";
			crossTradeShipment.JS_PackingMode = "LCL";
			crossTradeShipment.JS_INCO = "FOB";
			crossTradeShipment.JS_UniqueConsignRef = "S00001235";
			crossTradeShipment.JS_HouseBill = "UVWXYZ";
			crossTradeShipment.JS_RL_NKOrigin = "USLAX";
			crossTradeShipment.JS_RL_NKDestination = "INBOM";
			crossTradeShipment.JS_ActualChargeable = 100M;
			((OneOffQuoteHostInvoicingSupporter)crossTradeShipment.InvoicingSupporter).fDefaultDebtor = debtor;

			using (Job testJob = Job.CreateWithMutex(newFactory, crossTradeShipment))
			{
				testJob.PlugInData = crossTradeShipment;
				AssertCreditorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", originChargeCode, ZGuid.Empty);
				var moqAccessor = new Mock<OneOffQuoteHostInvoicingSupporter.DefaultOrgAccessor>(MockBehavior.Strict);

				moqAccessor.Setup(m => m(It.Is<DefaultCreditorSetting>(cs => cs.ChargeCode == originChargeCode && cs.InvoiceType == InvoiceTypesList.Codes.FinalInvoice)))
					.Returns(creditor);

				((OneOffQuoteHostInvoicingSupporter)crossTradeShipment.InvoicingSupporter).DefaultCreditor = moqAccessor.Object;
				AssertCreditorCorrectWhenSettingCharge(crossTradeShipment, testJob, "EXW", originChargeCode, creditor.PK);
			}
		}

		public void TestChargeCodeDoesNotDefaultCreditorIfInGatewayBillingTabAndCreditorDoesNotHavePayablesEnabled()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(newFactory);

			OrgHeader org1 = newFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsCreditor = false;

			AccChargeCode chargeCode1 = newFactory.NewWithValidTestData<AccChargeCode>();

			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Margin;

			newFactory.Save();

			CommonCartage cartage = creator.CreateCartage();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();

			CommonWorkSheet workSheet = newFactory.New<CommonWorkSheet>();
			workSheet.EY_OH_TransportCo = org1.PK;
			leg.JU_EY_RunSheet = workSheet.PK;

			using (Job testJob = JobInvoicing.Job.CreateWithMutex(newFactory, cartage))
			{
				testJob.PlugInData = cartage;

				BaseCharge charge1 = testJob.Charges.AddNew();
				Assert("No charge code", charge1.JR_OH_CostAccount.IsEmpty);

				charge1.Factory.SetContext(BusinessContext.InvoicingPlugInGUI);

				charge1.JR_AC = chargeCode1.PK;
				AssertEquals("Charge should not have a creditor", ZGuid.Empty, charge1.JR_OH_CostAccount);
			}
		}

		public void TestChargeCodeDoesDefaultCreditorIfNotInGatewayBillingTabAndCreditorDoesNotHavePayablesEnabled()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(newFactory);

			OrgHeader org1 = newFactory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsCreditor = false;

			AccChargeCode chargeCode1 = newFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Margin;

			newFactory.Save();

			CommonCartage cartage = creator.CreateCartage();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			CommonCartageLeg leg = move.CartageLegs.AddNew();

			CommonWorkSheet workSheet = newFactory.New<CommonWorkSheet>();
			workSheet.EY_OH_TransportCo = org1.PK;
			leg.JU_EY_RunSheet = workSheet.PK;

			using (Job testJob = JobInvoicing.Job.CreateWithMutex(newFactory, cartage))
			{
				testJob.PlugInData = cartage;

				BaseCharge charge1 = testJob.Charges.AddNew();
				Assert("No charge code", charge1.JR_OH_CostAccount.IsEmpty);

				charge1.JR_AC = chargeCode1.PK;
				AssertEquals("Charge should have org1 as defaulted creditor", org1.PK, charge1.JR_OH_CostAccount);
			}
		}

		void AssertCreditorCorrectWhenSettingCharge(ForwardingShipment shipment, Job job, ZString incoTerm, AccChargeCode chargeCode, ZGuid expectedCreditor)
		{
			shipment.JS_INCO = incoTerm;

			BaseCharge charge = job.Charges.AddNew();
			Assert(charge.JR_OH_CostAccount.IsEmpty);

			charge.JR_AC = chargeCode.PK;
			AssertEquals(expectedCreditor, charge.JR_OH_CostAccount);
		}

		#endregion

		public void TestChargeTypeMarginPercentSetOnRevenuePosting()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(newFactory);

			AccChargeCode testChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_Code = "TESTZUB";
			testChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			testChargeCode.AC_MarginPercentage = 10.5m;

			ForwardingShipment shipment = creator.CreateShipment("S00001234", "AUSYD", "USLAX");
			Job testJob = creator.CreateJob(shipment);
			BaseCharge charge = testJob.Charges.AddNew();
			charge.JR_AC = testChargeCode.PK;
			newFactory.Save();

			AssertEquals(false, charge.IsRevenuePosted);
			AssertEquals("Empty", ZString.Empty, charge.JR_ChargeType);
			AssertEquals("Empty", 0m, charge.JR_MarginPercentage);

			AccTransactionLines transactionLine = newFactory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			transactionLine.AL_JH = testJob.PK;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			transactionLine.AL_GB = GlbBranch.CurrentBranch.PK;
			transactionLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			transactionLine.AL_AG = creator.GLHeader1.PK;
			transactionLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			charge.JR_AL_APLine = transactionLine.PK;
			newFactory.Save();

			AssertEquals("Revenue not posted, cost is posted", false, charge.IsRevenuePosted);
			AssertEquals("Empty", ZString.Empty, charge.JR_ChargeType);
			AssertEquals("Empty", 0m, charge.JR_MarginPercentage);
			AssertEquals("CURRENT charge type reflected correctly", Core.Constants.ChargeType.Margin, charge.ChargeType);
			AssertEquals("CURRENT margin percentage reflected correctly", 10.5m, charge.MarginPercentage);

			transactionLine = newFactory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			transactionLine.AL_JH = testJob.PK;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			transactionLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			charge.JR_AL_ARLine = transactionLine.PK;
			newFactory.Save();

			AssertEquals(true, charge.IsRevenuePosted);
			AssertEquals("Correct charge type", Core.Constants.ChargeType.Margin, charge.JR_ChargeType);
			AssertEquals("Correct charge type", Core.Constants.ChargeType.Margin, charge.ChargeType);
			AssertEquals("Correct margin percentage", 10.5m, charge.JR_MarginPercentage);
			AssertEquals("Correct margin percentage", 10.5m, charge.MarginPercentage);
		}

		public void TestARInvoiceNumber()
		{
			ZString testNumber = "XXX";
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_TransactionNum = testNumber;

			AccTransactionLines line = aRInvoice.Lines.AddNew();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestCharge.JR_AL_ARLine = line.PK;

			AssertEquals(testNumber, TestCharge.JR_ARInvoiceNumber);
		}

		public void TestJobInvoiceNumber()
		{
			ZString testNumber = "XXX";
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_ConsolidatedInvoiceRef = testNumber;

			AccTransactionLines line = aRInvoice.Lines.AddNew();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			TestCharge.JR_AL_ARLine = line.PK;

			AssertEquals(testNumber, TestCharge.JR_JobInvoiceNumber);
		}

		public override void TestIsRevenuePosted()
		{
			TestCharge.JR_AL_ARLine = ZGuid.Empty;
			AssertEquals(false, TestCharge.IsRevenuePosted);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			TestCharge.JR_AL_ARLine = transactionLine.PK;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals(false, TestCharge.IsRevenuePosted);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(false, TestCharge.IsRevenuePosted);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals(false, TestCharge.IsRevenuePosted);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals(true, TestCharge.IsRevenuePosted);

			var header = Factory.New<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transactionLine.AL_AH = header.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(true, TestCharge.IsRevenuePosted);// Automatic Job Revenue Journals have AL_LineType = CST and can be linked to the AR Line of a charge
		}

		public override void TestIsApproved()
		{
			TestCharge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, TestCharge.IsApproved);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			TestCharge.JR_AL_APLine = transactionLine.PK;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals(false, TestCharge.IsApproved);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(false, TestCharge.IsApproved);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals(false, TestCharge.IsApproved);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals(true, TestCharge.IsApproved);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.UnapprovedCost;
			AssertEquals(false, TestCharge.IsApproved);
		}

		public void TestAccural()
		{
			TestCharge.JR_AL_APLine = ZGuid.Empty;
			AssertNull(TestCharge.Accrual);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			TestCharge.JR_AL_APLine = transactionLine.PK;
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertNull(TestCharge.Accrual);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertNull(TestCharge.Accrual);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertNull(TestCharge.Accrual);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(transactionLine.PK, TestCharge.Accrual.PK);
		}

		public void TestCreateAccrualResetsHasReversedAccrualFlag()
		{
			AssertEquals("Pre-condition: HasReversedAccrual is not set", false, TestCharge.HasReversedAccrual_ForTestOnly);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestCharge.JR_JH = job.PK;
			TestCharge.CreateAccrualCore_ForTestOnly();
			AssertNotNull("Must have an Accrual", TestCharge.Accrual);
			AssertEquals("HasReversedAccrual", false, TestCharge.HasReversedAccrual_ForTestOnly);

			TestCharge.ReverseAccrual(ZDateTime.Now);
			AssertEquals("HasReversedAccrual should be set after an Accrual reversal", true, TestCharge.HasReversedAccrual_ForTestOnly);

			TestCharge.CreateAccrualCore_ForTestOnly();
			AssertEquals("HasReversedAccrual must be reset because of new Accrual created", false, TestCharge.HasReversedAccrual_ForTestOnly);
		}

		public void TestCreateWIPResetsHasReversedWIPFlag()
		{
			AssertEquals("Pre-condition: HasReversedWIP is not set", false, TestCharge.HasReversedWIP_ForTestOnly);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestCharge.JR_JH = job.PK;
			TestCharge.CreateWIPCore_ForTestOnly();
			AssertNotNull("Must have a WIP", TestCharge.WIP);
			AssertEquals("HasReversedWIP", false, TestCharge.HasReversedWIP_ForTestOnly);

			TestCharge.ReverseWIP(ZDateTime.Now);
			AssertEquals("HasReversedWIP should be set after a WIP reversal", true, TestCharge.HasReversedWIP_ForTestOnly);

			TestCharge.CreateWIPCore_ForTestOnly();
			AssertEquals("HasReversedWIP must be reset because of new WIP created", false, TestCharge.HasReversedWIP_ForTestOnly);
		}

		#region JR_OSCostAmtWithGSTAmt

		public void TestJR_OSCostAmtWithGSTAmt()
		{
			Job job1 = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0, null, 0);
			Charge charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge 1_01", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor1, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge2 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC2, "Charge 1_02", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor1, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge3 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC3, "Charge 1_03", TestObjectCreator.AUD, 3000M, TestObjectCreator.Creditor1, "1001", TestObjectCreator.AUD, 0M, null);
			AssertEquals("Cost with TestObjectCreator.GST1 should be 1100", charge1.JR_OSCostAmt + charge1.JR_OSCostGSTAmt_Calc, charge1.JR_OSCostAmtWithGSTAmt);
			AssertEquals("Cost with TestObjectCreator.GST1 should be 2000", charge2.JR_OSCostAmt + charge2.JR_OSCostGSTAmt_Calc, charge2.JR_OSCostAmtWithGSTAmt);
			AssertEquals("Cost with TestObjectCreator.GST1 should be 3300", charge3.JR_OSCostAmt + charge3.JR_OSCostGSTAmt_Calc, charge3.JR_OSCostAmtWithGSTAmt);
		}

		#endregion

		#region Totals for the AP Invoices

		public void TestTotalTaxOnInvForJob()
		{
			Job job1 = TestObjectCreator.CreateJob(TestObjectCreator.Creditor4, 0, null, 0);
			Charge charge1_01 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_01", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor4, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_02 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC11, "Charge 1_02", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor4, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_03 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_03", TestObjectCreator.AUD, 3000M, TestObjectCreator.Creditor4, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_04 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_04", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor5, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_05 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC11, "Charge 1_05", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor5, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_06 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_06", TestObjectCreator.AUD, 1000M, null, "", TestObjectCreator.AUD, 0M, null);
			Charge charge1_07 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_07", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor6, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_08 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC11, "Charge 1_08", TestObjectCreator.AUD, 2000M, null, "", TestObjectCreator.AUD, 0M, null);
			Charge charge1_09 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_09", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor6, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_10 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_10", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor6, "1001", TestObjectCreator.AUD, 0M, null);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 100M, charge1_01.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 300M, charge1_02.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 300M, charge1_03.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 0M, charge1_04.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 0M, charge1_05.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_06", 0M, charge1_06.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_07", 200M, charge1_07.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_08", 0M, charge1_08.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_09", 400M, charge1_09.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_19", 400M, charge1_10.TotalTaxOnInvForJob);

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_OSTaxAmount = 150m;
			APInvoiceLine line = (APInvoiceLine)apInvoice.Lines.AddNew();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Charge charge1_11 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_11", TestObjectCreator.AUD, 100M, apInvoice.Header, apInvoice.InvoiceNumber, TestObjectCreator.AUD, 0M, null);
			charge1_11.JR_AL_APLine = line.PK;
			AssertEquals("Total Invoice Amount Inc GST - Charge1_11", 150M, charge1_11.TotalTaxOnInvForJob);
		}

		public void TestTotalAmountOnInvForJob()
		{
			Job job1 = TestObjectCreator.CreateJob(TestObjectCreator.Creditor4, 0, null, 0);
			Charge charge1_01 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_01", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor4, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_02 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC11, "Charge 1_02", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor4, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_03 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_03", TestObjectCreator.AUD, 3000M, TestObjectCreator.Creditor4, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_04 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_04", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor5, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_05 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC11, "Charge 1_05", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor5, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_06 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_06", TestObjectCreator.AUD, 1000M, TestObjectCreator.Creditor5, "", TestObjectCreator.AUD, 0M, null);
			Charge charge1_07 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_07", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor6, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_08 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC11, "Charge 1_08", TestObjectCreator.AUD, 2000M, null, "", TestObjectCreator.AUD, 0M, null);
			Charge charge1_09 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_09", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor6, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_10 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_10", TestObjectCreator.AUD, 2000M, TestObjectCreator.Creditor6, "1001", TestObjectCreator.AUD, 0M, null);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 1100M, charge1_01.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 5300M, charge1_02.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 5300M, charge1_03.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 2000M, charge1_04.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 2000M, charge1_05.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_06", 0M, charge1_06.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_07", 2200M, charge1_07.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_08", 0M, charge1_08.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_09", 4400M, charge1_09.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_10", 4400M, charge1_10.TotalAmountOnInvForJob);

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_OSTotal = -1500m;
			APInvoiceLine line = (APInvoiceLine)apInvoice.Lines.AddNew();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Charge charge1_11 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_11", TestObjectCreator.AUD, 100M, apInvoice.Header, apInvoice.InvoiceNumber, TestObjectCreator.AUD, 0M, null);
			charge1_11.JR_AL_APLine = line.PK;
			AssertEquals("Total Invoice Amount Inc TestObjectCreator.GST1 - Charge1_11", 1500M, charge1_11.TotalAmountOnInvForJob);
		}

		public void TestTotalsOnInvForJobWithDifferentCurrencies()
		{
			Job job1 = TestObjectCreator.CreateJob(TestObjectCreator.Creditor4, 0, null, 0);

			//TestObjectCreator.CreateExchangeRate(Job1, TestObjectCreator.USD, 0.5m, 0.5m);
			//TestObjectCreator.CreateExchangeRate(Job1, TestObjectCreator.GBP, 2.0m, 2.0m);

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			TestObjectCreator.Creditor4.MiscServ.OM_APWHTApplicable = true;
			TestObjectCreator.Creditor4.Factory.Save();
			TestObjectCreator.Creditor6.MiscServ.OM_APWHTApplicable = true;
			TestObjectCreator.Creditor6.Factory.Save();

			Charge charge1_01 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_01", TestObjectCreator.GBP, 1000M, TestObjectCreator.Creditor1, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_02 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_02", TestObjectCreator.USD, 2000M, TestObjectCreator.Creditor1, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_03 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_03", TestObjectCreator.USD, 3000M, TestObjectCreator.Creditor1, "1000", TestObjectCreator.AUD, 0M, null);
			Charge charge1_04 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_04", TestObjectCreator.USD, 1000M, TestObjectCreator.Creditor6, "1001", TestObjectCreator.AUD, 0M, null);
			Charge charge1_05 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC6, "Charge 1_05", TestObjectCreator.USD, 1000M, TestObjectCreator.Creditor6, "1001", TestObjectCreator.AUD, 0M, null);

			foreach (var rate in job1.ExchangeRates.Cast<IExchangeRate>().Where(r => r.CurrencyCode == TestObjectCreator.USD.RX_Code))
			{
				rate.SetBuyRate_ForTestOnly(0.5m);
			}

			foreach (var rate in job1.ExchangeRates.Cast<IExchangeRate>().Where(r => r.CurrencyCode == TestObjectCreator.GBP.RX_Code))
			{
				rate.SetBuyRate_ForTestOnly(2.0m);
			}

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 1050M, charge1_01.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 1050M, charge1_02.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 1050M, charge1_03.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 200M, charge1_04.TotalTaxOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 200M, charge1_05.TotalTaxOnInvForJob);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 11550M, charge1_01.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 11550M, charge1_02.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 11550M, charge1_03.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 2200M, charge1_04.TotalAmountOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 2200M, charge1_05.TotalAmountOnInvForJob);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", "AUD", charge1_01.CurrencyCodeOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", "AUD", charge1_02.CurrencyCodeOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", "AUD", charge1_03.CurrencyCodeOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", "USD", charge1_04.CurrencyCodeOnInvForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", "USD", charge1_05.CurrencyCodeOnInvForJob);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 550M, charge1_01.LineTotalAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 4400M, charge1_02.LineTotalAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 6600M, charge1_03.LineTotalAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 1100M, charge1_04.LineTotalAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 1100M, charge1_05.LineTotalAmountOnInvoiceForJob);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 50M, charge1_01.LineGSTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 400M, charge1_02.LineGSTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 600M, charge1_03.LineGSTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 100M, charge1_04.LineGSTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 100M, charge1_05.LineGSTAmountOnInvoiceForJob);

			AssertEquals("Total Invoice Amount Inc GST - Charge1_01", 25M, charge1_01.LineWHTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_02", 200M, charge1_02.LineWHTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_03", 300M, charge1_03.LineWHTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_04", 50M, charge1_04.LineWHTAmountOnInvoiceForJob);
			AssertEquals("Total Invoice Amount Inc GST - Charge1_05", 50M, charge1_05.LineWHTAmountOnInvoiceForJob);

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			APInvoiceLine line = (APInvoiceLine)apInvoice.Lines.AddNew();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			Charge charge1_11 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC10, "Charge 1_11", TestObjectCreator.AUD, 100M, apInvoice.Header, apInvoice.InvoiceNumber, TestObjectCreator.AUD, 0M, null);
			charge1_11.JR_AL_APLine = line.PK;
			AssertEquals("Total Invoice Amount Inc GST - Charge1_11", TestObjectCreator.USD.RX_Code, charge1_11.CurrencyCodeOnInvForJob);
		}

		#endregion

		public void TestSettingJR_AL_ARLine()
		{
			AccTransactionLines rEVline = Factory.New<AccTransactionLines>();
			rEVline.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;

			WIP wIP1 = Factory.New<WIP>();
			WIP wIP2 = Factory.New<WIP>();
			TestCharge.JR_AL_ARLine = rEVline.PK;
			TestCharge.JR_AL_ARLine = wIP1.PK;
			AssertEquals("old link still there", rEVline.PK, TestCharge.JR_AL_ARLine);
			TestCharge.JR_AL_ARLine = ZGuid.Empty;
			AssertEquals("old link still there", rEVline.PK, TestCharge.JR_AL_ARLine);

			fTestCharge = null;
			TestCharge.JR_AL_ARLine = wIP1.PK;
			wIP1.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_ARLine = wIP2.PK;
			AssertEquals("WIP can override another WIP", wIP2.PK, TestCharge.JR_AL_ARLine);

			wIP2.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_ARLine = ZGuid.Empty;
			AssertEquals("clearing WIP is OK", ZGuid.Empty, TestCharge.JR_AL_ARLine);
		}

		public void TestSettingJR_AL_APLine()
		{
			AccTransactionLines costline = Factory.New<AccTransactionLines>();
			costline.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

			Accrual aCR1 = Factory.New<Accrual>();
			Accrual aCR2 = Factory.New<Accrual>();

			TestCharge.JR_AL_APLine = costline.PK;
			TestCharge.JR_AL_APLine = aCR1.PK;
			AssertEquals("old link still there", costline.PK, TestCharge.JR_AL_APLine);
			TestCharge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals("old link still there", costline.PK, TestCharge.JR_AL_APLine);

			fTestCharge = null;
			TestCharge.JR_AL_APLine = aCR1.PK;
			aCR1.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_APLine = aCR2.PK;
			AssertEquals("WIP can override another WIP", aCR2.PK, TestCharge.JR_AL_APLine);

			aCR2.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals("clearing WIP is OK", ZGuid.Empty, TestCharge.JR_AL_APLine);
		}

		public void TestCostAndSellSuspendersReportError()
		{
			var charge = TestJob.Charges.AddNew();
			charge[JobChargeSchema.Constants.JR_OSCostAmt] = 111.11M;
			AssertEquals(111.11M, charge.JR_OSCostAmt);

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_RX_NKCostCurrency = "JPY";
				Assert(charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}

			AssertErrorMessageThrown(JobChargeSchema.Constants.JR_OSCostAmt);

			charge[JobChargeSchema.Constants.JR_OSSellAmt] = 111.11M;
			AssertEquals(111.11M, charge.JR_OSSellAmt);

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_RX_NKSellCurrency = "JPY";
				Assert(charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}

			AssertErrorMessageThrown(JobChargeSchema.Constants.JR_OSSellAmt);
		}

		[ExpectNoExceptions]
		public void TestCostAndSellSuspendersNotReportError()
		{
			var charge = TestJob.Charges.AddNew();
			charge[JobChargeSchema.Constants.JR_OSCostAmt] = 111.11M;
			AssertEquals(111.11M, charge.JR_OSCostAmt);

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_RX_NKCostCurrency = "JPY";
				charge[JobChargeSchema.Constants.JR_OSCostAmt] = 111M;
				Assert(charge.CostRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}

			charge[JobChargeSchema.Constants.JR_OSSellAmt] = 111.11M;
			AssertEquals(111.11M, charge.JR_OSSellAmt);

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_RX_NKSellCurrency = "JPY";
				charge[JobChargeSchema.Constants.JR_OSSellAmt] = 111M;
				Assert(charge.SellRoundingErrorReproterFunctionalitySuspender.IsSuspended);
			}
		}

		void AssertErrorMessageThrown(string propertyName)
		{
			AssertMultilineASCIIEquals(
@"In Enterprise.Accounting.Business.JobInvoicing.Charge -
Properties requiring to be rounded on currency change have more decimal places than allowed, this may be due to setter not propagating values due to has changes check.
Incorrectly rounded values are:
" + propertyName + @": 111.11
Currency AUD was changed to JPY
Only 0 decimals are allowed."
, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestClearRevenueLink()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_Code = "TESTZUB";
			testChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			testChargeCode.AC_MarginPercentage = 10.5m;

			BaseCharge charge = TestJob.Charges.AddNew();
			charge.JR_AC = testChargeCode.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			AccTransactionLines line = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			AssertEquals("Precondition: Revenue NOT Posted", false, charge.IsRevenuePosted);
			AssertEquals("Precondition: Revenue line should not be set", ZGuid.Empty, charge.JR_AL_ARLine);
			AssertEquals("Precondition: Charge Type empty", "", charge.JR_ChargeType);
			AssertEquals("Precondition: Charge margin percent empty", 0m, charge.JR_MarginPercentage);

			charge.JR_AL_ARLine = line.PK;

			AssertEquals("Precondition: Revenue is now Posted", true, charge.IsRevenuePosted);
			AssertEquals("Precondition: Revenue line IS now set", line.PK, charge.JR_AL_ARLine);
			AssertEquals("Precondition: Charge Type empty", "", charge.JR_ChargeType);
			AssertEquals("Precondition: Charge margin percent empty", 0m, charge.JR_MarginPercentage);

			Factory.Save();

			AssertEquals("Revenue Posted", true, charge.IsRevenuePosted);
			AssertEquals("Once posted, shouldn't reset", line.PK, charge.JR_AL_ARLine);
			AssertEquals("Charge Type stored", Core.Constants.ChargeType.Margin, charge.JR_ChargeType);
			AssertEquals("Charge margin percent stored", 10.5m, charge.JR_MarginPercentage);

			charge.ClearRevenueLink();

			AssertEquals("Revenue not Posted", false, charge.IsRevenuePosted);
			AssertEquals("Should be cleared", ZGuid.Empty, charge.JR_AL_ARLine);
			AssertEquals("Charge Type cleared", "", charge.JR_ChargeType);
			AssertEquals("Charge margin percent cleared", 0m, charge.JR_MarginPercentage);
		}

		public void TestClearRevenueLinkForARLineInDatabase()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.USD, 6.22M, 100M, 0M, 622M, 0M);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			job.ExchangeRates.AddRate(TestObjectCreator.USD, 6.22M, TestObjectCreator.AALSHI.PK, ExchangeRateOrgTypeEnum.Debtor);

			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge",
				TestObjectCreator.AUD, 0M, null, "charge",
				TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI);

			jobCharge.JR_OSSellExRate = 6M;

			AssertEquals(6M, jobCharge.JR_OSSellExRate);
			AssertEquals(6.22M, job.ExchangeRates[0].JF_BaseRate);

			jobCharge.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;

			AssertEquals(true, jobCharge.IsRevenuePosted);
			AssertEquals(false, jobCharge.ARLine.IsInDatabase);

			jobCharge.ClearRevenueLink();

			AssertEquals("Should be cleared", ZGuid.Empty, jobCharge.JR_AL_ARLine);
			AssertEquals("Because ARLine.IsInDatabase is false, should not run UpdateRevenueExchangeRate()", 6M, jobCharge.JR_OSSellExRate);

			jobCharge.JR_AL_ARLine = invoice.Lines[0].PK;

			AssertEquals(true, jobCharge.IsRevenuePosted);
			AssertEquals(true, jobCharge.ARLine.IsInDatabase);

			jobCharge.ClearRevenueLink();

			AssertEquals("Should be cleared", ZGuid.Empty, jobCharge.JR_AL_ARLine);
			AssertEquals("Because ARLine.IsInDatabase is true, should run UpdateRevenueExchangeRate()", 6.22M, jobCharge.JR_OSSellExRate);
		}

		public void TestClearCostLink()
		{
			AccTransactionLines line = Factory.New<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

			TestCharge.JR_AL_APLine = line.PK;

			TestCharge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals("Once posted, shouldn't reset", line.PK, TestCharge.JR_AL_APLine);

			TestCharge.ClearCostLink();
			AssertEquals("Should be cleared", ZGuid.Empty, TestCharge.JR_AL_APLine);
		}

		public void TestClearCostLinkForAPLineInDatabase()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.USD, 6.22M, 100M, 0M, 622M, 0M);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(8));
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			job.ExchangeRates.AddRate(TestObjectCreator.USD, 6.22M, TestObjectCreator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);

			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge",
				TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "charge",
				TestObjectCreator.AUD, 0M);

			jobCharge.JR_OSCostExRate = 6M;

			AssertEquals(6M, jobCharge.JR_OSCostExRate);
			AssertEquals(6.22M, job.ExchangeRates[0].JF_BaseRate);

			jobCharge.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;

			AssertEquals(true, jobCharge.IsCostPosted);
			AssertEquals(false, jobCharge.APLine.IsInDatabase);

			jobCharge.ClearCostLink();

			AssertEquals("Should be cleared", ZGuid.Empty, jobCharge.JR_AL_APLine);
			AssertEquals("Because APLine.IsInDatabase is false, should not run UpdateCostExchangeRate()", 6M, jobCharge.JR_OSCostExRate);

			jobCharge.JR_AL_APLine = invoice.Lines[0].PK;

			AssertEquals(true, jobCharge.IsCostPosted);
			AssertEquals(true, jobCharge.APLine.IsInDatabase);

			jobCharge.ClearCostLink();

			AssertEquals("Should be cleared", ZGuid.Empty, jobCharge.JR_AL_APLine);
			AssertEquals("Because APLine.IsInDatabase is true, should run UpdateCostExchangeRate()", 6.22M, jobCharge.JR_OSCostExRate);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, TestCharge.JR_GC);
			AssertEquals(GlbBranch.CurrentBranch.PK, TestCharge.JR_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, TestCharge.JR_GE);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestCharge.JR_RX_NKSellCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestCharge.JR_RX_NKCostCurrency);
		}

		public void TestIsDebtorInSameCountry()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();

				OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
				testOrg.OH_IsActive = true;
				testOrg.OH_IsDebtor = true;
				testOrg.OH_RL_NKClosestPort = "AUSYD";

				TestCharge.JR_OH_SellAccount = testOrg.PK;
				AssertEquals("Debtor should be in same country", true, TestCharge.IsDebtorInSameCountry);

				testOrg.OH_RL_NKClosestPort = "NZAKL";
				AssertEquals("Debtor should NOT be in same country", false, TestCharge.IsDebtorInSameCountry);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestDefaultChargeDescription()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(
				Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_LocalLanguageDescription = "Local";
			testChargeCode.AC_Desc = "Standard";

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsActive = true;
			testOrg.OH_IsDebtor = true;
			testOrg.OH_RL_NKClosestPort = "AUSYD";

			using (Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345")))
			{
				job.LocalChargesPK = testOrg.PK;

				TestCharge.JR_JH = job.PK;
				TestCharge.JR_AC = testChargeCode.PK;
				AssertEquals("Description should be the local language description", "Local", TestCharge.JR_Desc);

				testOrg.OH_RL_NKClosestPort = "NZAKL";
				fTestCharge = null;
				TestCharge.JR_JH = job.PK;
				TestCharge.JR_AC = testChargeCode.PK;
				AssertEquals("Description should be the standard description", "Standard", TestCharge.JR_Desc);
			}
		}

		public void TestJR_LocalCurrencyCode()
		{
			AssertNotNull("Precondition", TestCharge.Company);
			AssertEquals("Precondition", GlbCompany.CurrentCompany.PK, TestCharge.JR_GC);
			AssertEquals("Local currency code should match", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestCharge.JR_LocalCurrencyCode);

			TestCharge.JR_GC = ZGuid.Empty;
			AssertNull("No Company", TestCharge.Company);
			AssertEquals("Local currency code falls back to CurrentCompany", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestCharge.JR_LocalCurrencyCode);

			var otherCurrencyCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RX_NKLocalCurrency, SQLComparisonOperator.NotEqual, TestCharge.JR_LocalCurrencyCode));
			TestCharge.JR_GC = otherCurrencyCompany.PK;
			AssertNotNull("Charge should have Company", TestCharge.Company);
			AssertEquals("Local currency code is taken from Charge Company", otherCurrencyCompany.GC_RX_NKLocalCurrency, TestCharge.JR_LocalCurrencyCode);
		}

		public void TestChequeBooksCollection()
		{
			AssertEquals("ChequeBooks collection should be of valid type", typeof(ActiveChequeBookCollection), TestCharge.ChequeBooks.GetType());
		}

		protected virtual string[] ZDecimalPropertiesNotRequiringRoundingByOSCurrency()
		{
			return new[] {
					nameof(BaseCharge.JR_LineCFX),
					nameof(BaseCharge.JR_MarginPercentage),
					nameof(BaseCharge.JR_OSSellExRate),
					nameof(BaseCharge.JR_ProductQuantity),
					nameof(BaseCharge.JR_OSCostExRate),
					nameof(BaseCharge.JR_AgentDeclaredCostAmt),
					nameof(BaseCharge.JR_AgentDeclaredSellAmt),
					nameof(BaseCharge.JR_DeclaredOSCostAmt),
					nameof(BaseCharge.JR_EstimatedCost),
					nameof(BaseCharge.JR_EstimatedRevenue),
					nameof(BaseCharge.JR_LocalCostAmt),
					nameof(BaseCharge.JR_LocalSellAmt),
					nameof(BaseCharge.JR_LocalSellInvoiceAmt),
					nameof(BaseCharge.JR_AgentDeclaredSellAmtLocal),
					nameof(BaseCharge.JR_AgentDeclaredCostAmtLocal),
				};
		}

		public void TestGetPropertiesRequiringRounding()
		{
			var bizo = (BaseCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			var foundProperties = ZDecimalPropertiesNotRequiringRoundingByOSCurrency().Append(bizo.CostOSPropertiesRequiringRounding().ToArray()).Append(bizo.SellOSPropertiesRequiringRounding().ToArray());
			var allDecimalProperties = GetExpectedBusinessObjectType()
				.GetProperties()
				.Where(p => p.PropertyType == typeof(ZDecimal) && p.CanWrite)
				.Where(p => !p.Name.EndsWith("_ForTestOnly"))
				.Select(p => p.Name);

			AssertContainsExactElementsInAnyOrder(string.Format(
@"There is a decimal property in Charge which may need rounding, please add it to {0} (or the one for sell) or {1} in the relevant class.
Please consider whether or not your property will be affected by other rounding/setting logic in other properties.
For example, if a setter is called due to currecy changing, but detects no change to its own value, it may not bother to propagate the rounding change to the other properties it needs to.
If you are including a check whether property has changes or not please use method {2} in {3}.
", nameof(BaseCharge.CostOSPropertiesRequiringRounding), nameof(ZDecimalPropertiesNotRequiringRoundingByOSCurrency), nameof(AccountingValuesRoundingHelper.PropertyHasChanges), nameof(AccountingValuesRoundingHelper)), allDecimalProperties, foundProperties);
		}

		public void TestSetJR_GB_DoesNotSetTaxRateIfUsedByConsolCostImporter()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccTaxRate rate1 = factory.NewWithValidTestData<AccTaxRate>();
			rate1.AT_Code = "TAX1";
			rate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccTaxRate rate2 = factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_Code = "TAX2";
			rate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccChargeCode chargeCode = factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CCODE1";
			chargeCode.AC_AT_GSTRate = rate2.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			factory.Save();

			BaseCharge charge = TestJob.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AT_CostGSTRate = rate1.PK;

			using (charge.GetSuspenderForConsolCostImporter())
			{
				charge.JR_GB = testObjectCreator.NonCurrentBranch.PK;
				AssertEquals("expect tax rate does not change due to using SuspenderForConsolCostImporter", rate1.PK, charge.JR_AT_CostGSTRate);
			}
		}

		#region Charge Code GST Tax IDs

		public void TestChargeCodeGSTTaxIDs()
		{
			BusinessObjectFactory accFactory = new BusinessObjectFactory();
			AccTaxRate rate1 = accFactory.NewWithValidTestData<AccTaxRate>();
			rate1.AT_Code = "TAX1";
			rate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate rate2 = accFactory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_Code = "TAX2";
			rate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccChargeCode chargeCode = accFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CCODE1";
			chargeCode.AC_AT_GSTRate = rate1.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "REV";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "FOB";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = rate2.PK;

			accFactory.Save();

			CommonShipment shipment = CommonShipment.New(Factory);

			TestJob.PlugInData = shipment;
			JobCharge charge = TestJob.Charges.AddNew();
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Charge should have the charge code's default GST Tax ID", rate1.PK, charge.JR_AT_SellGSTRate);

			shipment.JS_INCO = "FOB";
			TestJob.PlugInData = shipment;
			charge = TestJob.Charges.AddNew();
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Charge should have the OVERRIDEN FOB GST Tax ID", rate2.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("Charge should have the default GST Tax ID for cost", rate1.PK, charge.JR_AT_CostGSTRate);

			taxOverride.AO_CostSellAll = "COS";
			accFactory.Save();
			charge = TestJob.Charges.AddNew();
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.CostAccount.OH_Code = "TTT1"; // To eliminate caching on the ChargeCode level we need to change one of the cache key components
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Charge should have the default GST Tax ID for Revenue", rate1.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("Charge should have the overriden GST Tax ID for cost", rate2.PK, charge.JR_AT_CostGSTRate);

			taxOverride.AO_CostSellAll = "ALL";
			accFactory.Save();
			charge = TestJob.Charges.AddNew();
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.CostAccount.OH_Code = "TTT2"; // To eliminate caching on the ChargeCode level we need to change one of the cache key components
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Charge should have the overriden GST Tax ID for Revenue", rate2.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("Charge should have the overriden GST Tax ID for cost", rate2.PK, charge.JR_AT_CostGSTRate);
		}

		public void TestChargeCodeGSTTaxIDsWithCustomsStatus()
		{
			BusinessObjectFactory accFactory = new BusinessObjectFactory();
			AccTaxRate rate1 = accFactory.NewWithValidTestData<AccTaxRate>();
			rate1.AT_Code = "TAX1";
			rate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate rate2 = accFactory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_Code = "TAX2";
			rate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccChargeCode chargeCode = accFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CCODE1";
			chargeCode.AC_AT_GSTRate = rate1.PK;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "FOB";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = rate2.PK;
			taxOverride.AO_CustomsStatus = "T1";

			accFactory.Save();

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_INCO = "FOB";

			TestJob.PlugInData = shipment;
			accFactory.Save();

			Charge charge = TestJob.Charges.AddNew();
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Charge should have the charge code's default GST Tax ID", rate1.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("Charge should have the charge code's default GST Tax ID", rate1.PK, charge.JR_AT_CostGSTRate);

			charge = TestJob.Charges.AddNew();
			((CommonShipment)charge.InvoicingJob.PlugInData).CustomsEntryNumberType = "T1";
			charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Charge should have the overriden GST Tax ID for Revenue", rate2.PK, charge.JR_AT_SellGSTRate);
			AssertEquals("Charge should have the overriden GST Tax ID for Cost", rate2.PK, charge.JR_AT_CostGSTRate);
		}

		// Currently Singapore Customs Declarations do not have Job Invoicing support.
		// Therefore stand-alone declarations are done through the shipment system.
		// This causes the tax overrides to not work correctly, as the shipment tax override is chosen
		// instead of the brokerage tax override.
		//
		// To solve this in the short-term, the below code manually uses the brokerage tax override
		// based on the department specified on the JobHeader. If the department is a customs department,
		// this logic is used.
		//
		// The below test tests this short-term logic.
		//
		public void TestChargeCodeGSTTaxIDsSingaporeStandAloneCustomsDecWorkAround()
		{
			ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				BusinessObjectFactory accFactory = new BusinessObjectFactory();
				GlbDepartment customsDepartment = Factory.New<GlbDepartment>();
				customsDepartment.GE_Code = "ZUB";
				customsDepartment.GE_CustomsBrokerage = true;

				CommonShipment shipment = CommonShipment.New(Factory);

				AccTaxRate rateShp = accFactory.NewWithValidTestData<AccTaxRate>();
				rateShp.AT_Code = "TAX1";
				rateShp.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				AccTaxRate rateBrk = accFactory.NewWithValidTestData<AccTaxRate>();
				rateBrk.AT_Code = "TAX2";
				rateBrk.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				AccChargeCode chargeCode = accFactory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_AT_GSTRate = rateShp.PK;
				chargeCode.AC_Code = "CCODE1";
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				accFactory.Save();

				AccChargeTaxOverride shpTaxOverride = chargeCode.TaxOverrides.AddNew();
				shpTaxOverride.AO_CostSellAll = "ALL";
				shpTaxOverride.AO_Direction = "ALL";
				shpTaxOverride.AO_IncoTerm = "ALL";
				shpTaxOverride.AO_JobType = "SHP";
				shpTaxOverride.AO_Origin = "ALL";
				shpTaxOverride.AO_Destination = "ALL";
				shpTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				shpTaxOverride.AO_AT = rateShp.PK;

				AccChargeTaxOverride customsTaxOverride = chargeCode.TaxOverrides.AddNew();
				customsTaxOverride.AO_CostSellAll = "ALL";
				customsTaxOverride.AO_Direction = "ALL";
				customsTaxOverride.AO_IncoTerm = "ALL";
				customsTaxOverride.AO_JobType = "BRK";
				customsTaxOverride.AO_Origin = "ALL";
				customsTaxOverride.AO_Destination = "ALL";
				customsTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				customsTaxOverride.AO_AT = rateBrk.PK;

				accFactory.Save();

				TestJob.PlugInData = shipment;
				TestJob.JH_GE = customsDepartment.PK;

				// Not Singapore
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				JobCharge charge = TestJob.Charges.AddNew();
				charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
				charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
				charge.JR_AC = chargeCode.PK;

				AssertEquals("Charge should have the shipment GST Tax ID for Revenue, as the job is a shipment", rateShp.PK, charge.JR_AT_SellGSTRate);
				AssertEquals("Charge should have the shipment GST Tax ID for Cost, as the job is a shipment", rateShp.PK, charge.JR_AT_CostGSTRate);

				// Singapore
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);

				charge = TestJob.Charges.AddNew();
				charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
				charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
				charge.JR_AC = chargeCode.PK;

				AssertEquals("Charge should have the brokerage GST Tax ID for Revenue, as the job is a shipment but has a customs department, and is in Singapore", rateBrk.PK, charge.JR_AT_SellGSTRate);
				AssertEquals("Charge should have the brokerage GST Tax ID for Cost, as the job is a shipment but has a customs department, and is in Singapore", rateBrk.PK, charge.JR_AT_CostGSTRate);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
			}
		}

		#endregion

		#region Cost/Sell Taxes

		public void TestOSCostTaxAmounts()
		{
			AccTaxRate gSTANDQSTRate = Factory.NewWithValidTestData<AccTaxRate>();
			gSTANDQSTRate.AT_Code = "GSTANDQST";
			gSTANDQSTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDQSTRate.SetRateNumerator_ForTestOnly(5);
			gSTANDQSTRate.SetExtraRate_ForTestOnly(75, 10);
			gSTANDQSTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			AccTaxRate gSTANDEDURate = Factory.NewWithValidTestData<AccTaxRate>();
			gSTANDEDURate.AT_Code = "GSTANDEDU";
			gSTANDEDURate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDEDURate.SetRateNumerator_ForTestOnly(10);
			gSTANDEDURate.SetExtraRate_ForTestOnly(3, 1);
			gSTANDEDURate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDEDURate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			AccTaxRate rETRate = Factory.NewWithValidTestData<AccTaxRate>();
			rETRate.AT_Code = "RET";
			rETRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rETRate.SetRateNumerator_ForTestOnly(16);
			rETRate.SetExtraRate_ForTestOnly(4, 1);
			rETRate.AT_Type = AccTaxRate.Types.Rated;
			rETRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			AccTaxRate gSTAndQSTBasedOnQCTRate = Factory.NewWithValidTestData<AccTaxRate>();
			gSTAndQSTBasedOnQCTRate.AT_Code = "QCT";
			gSTAndQSTBasedOnQCTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTAndQSTBasedOnQCTRate.SetRateNumerator_ForTestOnly(5);
			gSTAndQSTBasedOnQCTRate.SetExtraRate_ForTestOnly(9975, 1000);
			gSTAndQSTBasedOnQCTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTAndQSTBasedOnQCTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			AccTaxRate oTO6Rate = Factory.NewWithValidTestData<AccTaxRate>();
			oTO6Rate.AT_Code = "OTO6";
			oTO6Rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			oTO6Rate.SetRateNumerator_ForTestOnly(0);
			oTO6Rate.SetExtraRate_ForTestOnly(6, 1);
			oTO6Rate.AT_Type = AccTaxRate.Types.Rated;
			oTO6Rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;

			CommonShipment shipment = CommonShipment.New(Factory);
			TestJob.PlugInData = shipment;
			Factory.Save();
			BaseCharge gSTcharge = TestJob.Charges.AddNew();
			gSTcharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTcharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			gSTcharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			gSTcharge.JR_AC = TestObjectCreator.CC1.PK;
			gSTcharge.JR_OSCostAmt = 100.0m;
			gSTcharge.JR_APInvoiceNum = "INV111";

			AssertEquals("Before saving: JR_OSCostGSTAmt_Calc", 10.0m, gSTcharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSCostAmtWithGST", 110.0m, gSTcharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSCostGSTAmt", 10.0m, gSTcharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSCostExtraTaxAmt", 0m, gSTcharge.JR_Calc_OSCostExtraTaxAmt);

			Factory.Save();
			gSTcharge = new BusinessObjectFactory().Load<BaseCharge>(gSTcharge.PK);

			AssertEquals("On load: JR_OSCostGSTAmt_Calc", 10.0m, gSTcharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSCostAmtWithGST", 110.0m, gSTcharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("On load: JR_Calc_OSCostGSTAmt", 10.0m, gSTcharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("On load: JR_Calc_OSCostExtraTaxAmt", 0m, gSTcharge.JR_Calc_OSCostExtraTaxAmt);

			AccChargeCode gSTANDQSTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDQSTChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			gSTANDQSTChargeCode.AC_AT_GSTRate = gSTANDQSTRate.PK;
			gSTANDQSTChargeCode.AC_Code = "QSTCC";
			gSTANDQSTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			gSTANDQSTChargeCode.FillWithValidTestData();

			BaseCharge gSTANDQSTCharge = TestJob.Charges.AddNew();
			gSTANDQSTCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTANDQSTCharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			gSTANDQSTCharge.JR_AT_CostGSTRate = gSTANDQSTRate.PK; //Based on TestObjectCreator.GST1 rate
			gSTANDQSTCharge.JR_AC = gSTANDQSTChargeCode.PK;
			gSTANDQSTCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			gSTANDQSTCharge.JR_OSCostAmt = 100.0m;
			gSTANDQSTCharge.JR_APInvoiceNum = "INV111";

			AssertNotNull(gSTANDQSTCharge.CostExchangeRate);

			gSTANDQSTCharge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			AssertEquals("Before saving: JR_OSCostGSTAmt_Calc", 12.88m, gSTANDQSTCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSCostAmtWithGST", 112.88m, gSTANDQSTCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSCostGSTAmt", 5.0m, gSTANDQSTCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSCostExtraTaxAmt", 7.88m, gSTANDQSTCharge.JR_Calc_OSCostExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalCostQSTAmt", 15.76m, gSTANDQSTCharge.JR_Calc_LocalCostExtraTaxAmt);
			AssertEquals("Before saving: LineExtraTaxAmountOnInvoiceForJob", 15.76m, gSTANDQSTCharge.LineExtraTaxAmountOnInvoiceForJob);

			foreach (ExchangeRate rate in TestJob.ExchangeRates) //to allow to save
			{
				if (rate.JF_BaseRate == 0)
				{
					rate.JF_BaseRate = 1m;
				}
			}

			Factory.Save();
			gSTANDQSTCharge = new BusinessObjectFactory().Load<BaseCharge>(gSTANDQSTCharge.PK);

			AssertEquals("On load: JR_OSCostGSTAmt_Calc", 12.88m, gSTANDQSTCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSCostAmtWithGST", 112.88m, gSTANDQSTCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("On load: JR_Calc_OSCostGSTAmt", 5.0m, gSTANDQSTCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("On load: JR_Calc_OSCostExtraTaxAmt", 7.88m, gSTANDQSTCharge.JR_Calc_OSCostExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalCostQSTAmt", 15.76m, gSTANDQSTCharge.JR_Calc_LocalCostExtraTaxAmt);
			AssertEquals("On load: LineExtraTaxAmountOnInvoiceForJob", 15.76m, gSTANDQSTCharge.LineExtraTaxAmountOnInvoiceForJob);

			gSTcharge = gSTANDQSTCharge.Factory.Load<BaseCharge>(gSTcharge.PK);
			gSTcharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("On load: LineExtraTaxAmountOnInvoiceForJob", 7.88m, gSTANDQSTCharge.LineExtraTaxAmountOnInvoiceForJob);

			AccChargeCode gSTANDEDUChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDEDUChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			gSTANDEDUChargeCode.AC_AT_GSTRate = gSTANDEDURate.PK;
			gSTANDEDUChargeCode.AC_Code = "EDUCC";
			gSTANDEDUChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			gSTANDEDUChargeCode.FillWithValidTestData();

			BaseCharge gSTANDEDUCharge = TestJob.Charges.AddNew();
			gSTANDEDUCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTANDEDUCharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			gSTANDEDUCharge.JR_AT_CostGSTRate = gSTANDEDURate.PK; //Based on TestObjectCreator.GST1 rate
			gSTANDEDUCharge.JR_AC = gSTANDEDUChargeCode.PK;
			gSTANDEDUCharge.JR_OSCostAmt = 100.0m;

			AssertEquals("Before saving: JR_OSCostGSTAmt_Calc", 10.3m, gSTANDEDUCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSCostAmtWithGST", 110.3m, gSTANDEDUCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSCostGSTAmt", 10m, gSTANDEDUCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSCostExtraTaxAmt", 0.3m, gSTANDEDUCharge.JR_Calc_OSCostExtraTaxAmt);

			Factory.Save();
			gSTANDEDUCharge = new BusinessObjectFactory().Load<BaseCharge>(gSTANDEDUCharge.PK);

			AssertEquals("On load: JR_OSCostGSTAmt_Calc", 10.3m, gSTANDEDUCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSCostAmtWithGST", 110.3m, gSTANDEDUCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("On load: JR_Calc_OSCostGSTAmt", 10m, gSTANDEDUCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("On load: JR_Calc_OSCostExtraTaxAmt", 0.3m, gSTANDEDUCharge.JR_Calc_OSCostExtraTaxAmt);

			AccChargeCode rETChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			rETChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			rETChargeCode.AC_AT_GSTRate = rETRate.PK;
			rETChargeCode.AC_Code = "RETCC";
			rETChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			rETChargeCode.FillWithValidTestData();

			BaseCharge rETCharge = TestJob.Charges.AddNew();
			rETCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			rETCharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			rETCharge.JR_AT_CostGSTRate = rETRate.PK; //Based on TestObjectCreator.GST1 rate
			rETCharge.JR_AC = rETChargeCode.PK;
			rETCharge.JR_OSCostAmt = 100.0m;

			AssertEquals("Before saving: JR_OSCostGSTAmt_Calc", 12m, rETCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSCostAmtWithGST", 112m, rETCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSCostGSTAmt", 16m, rETCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSCostExtraTaxAmt", -4m, rETCharge.JR_Calc_OSCostExtraTaxAmt);

			Factory.Save();
			rETCharge = new BusinessObjectFactory().Load<BaseCharge>(rETCharge.PK);

			AssertEquals("On load: JR_OSCostGSTAmt_Calc", 12m, rETCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSCostAmtWithGST", 112m, rETCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("On load: JR_Calc_OSCostGSTAmt", 16m, rETCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("On load: JR_Calc_OSCostExtraTaxAmt", -4m, rETCharge.JR_Calc_OSCostExtraTaxAmt);

			AccChargeCode gSTAndQSTBasedOnQCTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTAndQSTBasedOnQCTChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			gSTAndQSTBasedOnQCTChargeCode.AC_AT_GSTRate = gSTAndQSTBasedOnQCTRate.PK;
			gSTAndQSTBasedOnQCTChargeCode.AC_Code = "QCTCC";
			gSTAndQSTBasedOnQCTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			gSTAndQSTBasedOnQCTChargeCode.FillWithValidTestData();

			BaseCharge gSTAndQSTBasedOnQCTCharge = TestJob.Charges.AddNew();
			gSTAndQSTBasedOnQCTCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTAndQSTBasedOnQCTCharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			gSTAndQSTBasedOnQCTCharge.JR_AT_CostGSTRate = gSTAndQSTBasedOnQCTRate.PK; //Based on TestObjectCreator.GST1 rate
			gSTAndQSTBasedOnQCTCharge.JR_AC = gSTAndQSTBasedOnQCTChargeCode.PK;
			gSTAndQSTBasedOnQCTCharge.JR_OSCostAmt = 100.0m;

			AssertEquals("Before saving: JR_OSCostGSTAmt_Calc", 14.98m, gSTAndQSTBasedOnQCTCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSCostAmtWithGST", 114.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSCostGSTAmt", 5m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSCostExtraTaxAmt", 9.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSCostExtraTaxAmt);

			Factory.Save();
			gSTAndQSTBasedOnQCTCharge = new BusinessObjectFactory().Load<BaseCharge>(gSTAndQSTBasedOnQCTCharge.PK);

			AssertEquals("On load: JR_OSCostGSTAmt_Calc", 14.98m, gSTAndQSTBasedOnQCTCharge.JR_OSCostGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSCostAmtWithGST", 114.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("On load: JR_Calc_OSCostGSTAmt", 5m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSCostGSTAmt);
			AssertEquals("On load: JR_Calc_OSCostExtraTaxAmt", 9.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSCostExtraTaxAmt);

			AccChargeCode oTO6ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			oTO6ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			oTO6ChargeCode.AC_AT_GSTRate = oTO6Rate.PK;
			oTO6ChargeCode.AC_Code = "OTOCC";
			oTO6ChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			oTO6ChargeCode.FillWithValidTestData();

			BaseCharge oTO6Charge = TestJob.Charges.AddNew();
			oTO6Charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			oTO6Charge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			oTO6Charge.JR_AT_CostGSTRate = oTO6Rate.PK; //Based on TestObjectCreator.GST1 rate
			oTO6Charge.JR_AC = oTO6ChargeCode.PK;
			oTO6Charge.JR_OSCostAmt = 100.0m;

			AssertEquals("Before saving: JR_OSCostGSTAmt_Calc", 6m, oTO6Charge.JR_OSCostGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSCostAmtWithGST", 106m, oTO6Charge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSCostGSTAmt", 0m, oTO6Charge.JR_Calc_OSCostGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSCostExtraTaxAmt", 6m, oTO6Charge.JR_Calc_OSCostExtraTaxAmt);

			Factory.Save();
			oTO6Charge = new BusinessObjectFactory().Load<BaseCharge>(oTO6Charge.PK);

			AssertEquals("On load: JR_OSCostGSTAmt_Calc", 6m, oTO6Charge.JR_OSCostGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSCostAmtWithGST", 106m, oTO6Charge.JR_Calc_OSCostAmtWithGST);
			AssertEquals("On load: JR_Calc_OSCostGSTAmt", 0m, oTO6Charge.JR_Calc_OSCostGSTAmt);
			AssertEquals("On load: JR_Calc_OSCostExtraTaxAmt", 6m, oTO6Charge.JR_Calc_OSCostExtraTaxAmt);
		}

		public void TestJR_Calc_LocalSellExtraTaxAmt_UsesTaxDate()
		{
			var tax = TestObjectCreator.GSTANDQST1WithDates;
			TestCharge.JR_AT_SellGSTRate = tax.PK;
			TestCharge.JR_OSSellAmt = 100;
			AssertEquals(9.98m, TestCharge.JR_Calc_LocalSellExtraTaxAmt);

			TestCharge.JR_SellTaxDate = ZDate.Today.AddMonths(-3);
			AssertEquals(9.18m, TestCharge.JR_Calc_LocalSellExtraTaxAmt);
		}

		public void TestOSSellTaxAmounts()
		{
			AccTaxRate gSTANDQSTRate = Factory.NewWithValidTestData<AccTaxRate>();
			gSTANDQSTRate.AT_Code = "GSTANDQST";
			gSTANDQSTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDQSTRate.SetRateNumerator_ForTestOnly(5);
			gSTANDQSTRate.SetExtraRate_ForTestOnly(75, 10);
			gSTANDQSTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			AccTaxRate gSTANDEDURate = Factory.NewWithValidTestData<AccTaxRate>();
			gSTANDEDURate.AT_Code = "GSTANDEDU";
			gSTANDEDURate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTANDEDURate.SetRateNumerator_ForTestOnly(10);
			gSTANDEDURate.SetExtraRate_ForTestOnly(3, 1);
			gSTANDEDURate.AT_Type = AccTaxRate.Types.Rated;
			gSTANDEDURate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			AccTaxRate rETRate = Factory.NewWithValidTestData<AccTaxRate>();
			rETRate.AT_Code = "RET";
			rETRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rETRate.SetRateNumerator_ForTestOnly(16);
			rETRate.SetExtraRate_ForTestOnly(4, 1);
			rETRate.AT_Type = AccTaxRate.Types.Rated;
			rETRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			AccTaxRate gSTAndQSTBasedOnQCTRate = Factory.NewWithValidTestData<AccTaxRate>();
			gSTAndQSTBasedOnQCTRate.AT_Code = "QCT";
			gSTAndQSTBasedOnQCTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gSTAndQSTBasedOnQCTRate.SetRateNumerator_ForTestOnly(5);
			gSTAndQSTBasedOnQCTRate.SetExtraRate_ForTestOnly(9975, 1000);
			gSTAndQSTBasedOnQCTRate.AT_Type = AccTaxRate.Types.Rated;
			gSTAndQSTBasedOnQCTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			AccTaxRate oTO6Rate = Factory.NewWithValidTestData<AccTaxRate>();
			oTO6Rate.AT_Code = "OTO6";
			oTO6Rate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			oTO6Rate.SetRateNumerator_ForTestOnly(0);
			oTO6Rate.SetExtraRate_ForTestOnly(6, 1);
			oTO6Rate.AT_Type = AccTaxRate.Types.Rated;
			oTO6Rate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;

			CommonShipment shipment = CommonShipment.New(Factory);
			TestJob.PlugInData = shipment;

			Factory.Save();

			BaseCharge gSTcharge = TestJob.Charges.AddNew();
			gSTcharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTcharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			gSTcharge.JR_AC = TestObjectCreator.CC1.PK;
			gSTcharge.JR_OSSellAmt = 100.0m;

			AssertEquals("Before saving: JR_OSSellGSTAmt", 10.0m, gSTcharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSSellAmtWithGST", 110.0m, gSTcharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSSellGSTAmt", 10.0m, gSTcharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSSellExtraTaxAmt", 0m, gSTcharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalSellExtraTaxAmt", 0m, gSTcharge.JR_Calc_LocalSellExtraTaxAmt);

			Factory.Save();
			gSTcharge = new BusinessObjectFactory().Load<BaseCharge>(gSTcharge.PK);
			AssertEquals("On load: JR_OSSellGSTAmt", 10.0m, gSTcharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSSellAmtWithGST", 110.0m, gSTcharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("On load: JR_Calc_OSSellGSTAmt", 10.0m, gSTcharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("On load: JR_Calc_OSSellExtraTaxAmt", 0m, gSTcharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalSellExtraTaxAmt", 0m, gSTcharge.JR_Calc_LocalSellExtraTaxAmt);

			AccChargeCode gSTANDQSTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDQSTChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			gSTANDQSTChargeCode.AC_AT_GSTRate = gSTANDQSTRate.PK;
			gSTANDQSTChargeCode.AC_Code = "QSTCC";
			gSTANDQSTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			gSTANDQSTChargeCode.FillWithValidTestData();

			BaseCharge gSTANDQSTCharge = TestJob.Charges.AddNew();
			gSTANDQSTCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTANDQSTCharge.JR_AT_SellGSTRate = gSTANDQSTRate.PK; //Based on TestObjectCreator.GST1 rate
			gSTANDQSTCharge.JR_AC = gSTANDQSTChargeCode.PK;
			gSTANDQSTCharge.JR_OSSellAmt = 100.0m;

			//Calculating before saving
			AssertEquals("Before saving: JR_OSSellGSTAmt", 12.88m, gSTANDQSTCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSSellAmtWithGST", 112.88m, gSTANDQSTCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSSellGSTAmt", 5.0m, gSTANDQSTCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSSellExtraTaxAmt", 7.88m, gSTANDQSTCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalSellExtraTaxAmt", 7.88m, gSTANDQSTCharge.JR_Calc_LocalSellExtraTaxAmt);

			//Calculating on load (Charge is already saved) ???
			Factory.Save();

			gSTANDQSTCharge = new BusinessObjectFactory().Load<BaseCharge>(gSTANDQSTCharge.PK);
			AssertEquals("On load: JR_OSSellGSTAmt", 12.88m, gSTANDQSTCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSSellAmtWithGST", 112.88m, gSTANDQSTCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("On load: JR_Calc_OSSellGSTAmt", 5.0m, gSTANDQSTCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("On load: JR_Calc_OSSellExtraTaxAmt", 7.88m, gSTANDQSTCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalSellExtraTaxAmt", 7.88m, gSTANDQSTCharge.JR_Calc_LocalSellExtraTaxAmt);

			AccChargeCode gSTANDEDUChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTANDEDUChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			gSTANDEDUChargeCode.AC_AT_GSTRate = gSTANDEDURate.PK;
			gSTANDEDUChargeCode.AC_Code = "EDUCC";
			gSTANDEDUChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			gSTANDEDUChargeCode.FillWithValidTestData();

			BaseCharge gSTANDEDUCharge = TestJob.Charges.AddNew();
			gSTANDEDUCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTANDEDUCharge.JR_AT_SellGSTRate = gSTANDEDURate.PK; //Based on TestObjectCreator.GST1 rate
			gSTANDEDUCharge.JR_AC = gSTANDEDUChargeCode.PK;
			gSTANDEDUCharge.JR_OSSellAmt = 100.0m;

			//Calculating before saving
			AssertEquals("Before saving: JR_OSSellGSTAmt", 10.3m, gSTANDEDUCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSSellAmtWithGST", 110.3m, gSTANDEDUCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSSellGSTAmt", 10m, gSTANDEDUCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSSellExtraTaxAmt", 0.3m, gSTANDEDUCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalSellExtraTaxAmt", 0.3m, gSTANDEDUCharge.JR_Calc_LocalSellExtraTaxAmt);

			//Calculating on load (Charge is already saved) ???
			Factory.Save();

			gSTANDEDUCharge = new BusinessObjectFactory().Load<BaseCharge>(gSTANDEDUCharge.PK);
			AssertEquals("On load: JR_OSSellGSTAmt", 10.3m, gSTANDEDUCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSSellAmtWithGST", 110.3m, gSTANDEDUCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("On load: JR_Calc_OSSellGSTAmt", 10m, gSTANDEDUCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("On load: JR_Calc_OSSellExtraTaxAmt", 0.3m, gSTANDEDUCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalSellExtraTaxAmt", 0.3m, gSTANDEDUCharge.JR_Calc_LocalSellExtraTaxAmt);

			AccChargeCode rETChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			rETChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			rETChargeCode.AC_AT_GSTRate = rETRate.PK;
			rETChargeCode.AC_Code = "RETCC";
			rETChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			rETChargeCode.FillWithValidTestData();

			BaseCharge rETCharge = TestJob.Charges.AddNew();
			rETCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			rETCharge.JR_AT_SellGSTRate = rETRate.PK; //Based on TestObjectCreator.GST1 rate
			rETCharge.JR_AC = rETChargeCode.PK;
			rETCharge.JR_OSSellAmt = 100.0m;

			//Calculating before saving
			AssertEquals("Before saving: JR_OSSellGSTAmt", 12m, rETCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSSellAmtWithGST", 112m, rETCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSSellGSTAmt", 16m, rETCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSSellExtraTaxAmt", -4m, rETCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalSellExtraTaxAmt", -4m, rETCharge.JR_Calc_LocalSellExtraTaxAmt);

			//Calculating on load (Charge is already saved) ???
			Factory.Save();

			rETCharge = new BusinessObjectFactory().Load<BaseCharge>(rETCharge.PK);
			AssertEquals("On load: JR_OSSellGSTAmt", 12m, rETCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSSellAmtWithGST", 112m, rETCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("On load: JR_Calc_OSSellGSTAmt", 16m, rETCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("On load: JR_Calc_OSSellExtraTaxAmt", -4m, rETCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalSellExtraTaxAmt", -4m, rETCharge.JR_Calc_LocalSellExtraTaxAmt);

			AccChargeCode gSTAndQSTBasedOnQCTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			gSTAndQSTBasedOnQCTChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			gSTAndQSTBasedOnQCTChargeCode.AC_AT_GSTRate = gSTAndQSTBasedOnQCTRate.PK;
			gSTAndQSTBasedOnQCTChargeCode.AC_Code = "QCTCC";
			gSTAndQSTBasedOnQCTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			gSTAndQSTBasedOnQCTChargeCode.FillWithValidTestData();

			BaseCharge gSTAndQSTBasedOnQCTCharge = TestJob.Charges.AddNew();
			gSTAndQSTBasedOnQCTCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			gSTAndQSTBasedOnQCTCharge.JR_AT_SellGSTRate = gSTAndQSTBasedOnQCTRate.PK; //Based on TestObjectCreator.GST1 rate
			gSTAndQSTBasedOnQCTCharge.JR_AC = gSTAndQSTBasedOnQCTChargeCode.PK;
			gSTAndQSTBasedOnQCTCharge.JR_OSSellAmt = 100.0m;

			//Calculating before saving
			AssertEquals("Before saving: JR_OSSellGSTAmt", 14.98m, gSTAndQSTBasedOnQCTCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSSellAmtWithGST", 114.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSSellGSTAmt", 5m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSSellExtraTaxAmt", 9.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalSellExtraTaxAmt", 9.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_LocalSellExtraTaxAmt);

			//Calculating on load (Charge is already saved) ???
			Factory.Save();

			gSTAndQSTBasedOnQCTCharge = new BusinessObjectFactory().Load<BaseCharge>(gSTAndQSTBasedOnQCTCharge.PK);
			AssertEquals("On load: JR_OSSellGSTAmt", 14.98m, gSTAndQSTBasedOnQCTCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSSellAmtWithGST", 114.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("On load: JR_Calc_OSSellGSTAmt", 5m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSSellGSTAmt);
			AssertEquals("On load: JR_Calc_OSSellExtraTaxAmt", 9.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalSellExtraTaxAmt", 9.98m, gSTAndQSTBasedOnQCTCharge.JR_Calc_LocalSellExtraTaxAmt);

			AccChargeCode oTO6ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			oTO6ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			oTO6ChargeCode.AC_AT_GSTRate = oTO6Rate.PK;
			oTO6ChargeCode.AC_Code = "OTOCC";
			oTO6ChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			oTO6ChargeCode.FillWithValidTestData();

			BaseCharge oTO6Charge = TestJob.Charges.AddNew();
			oTO6Charge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			oTO6Charge.JR_AT_SellGSTRate = oTO6Rate.PK; //Based on TestObjectCreator.GST1 rate
			oTO6Charge.JR_AC = oTO6ChargeCode.PK;
			oTO6Charge.JR_OSSellAmt = 100.0m;

			//Calculating before saving
			AssertEquals("Before saving: JR_OSSellGSTAmt", 6m, oTO6Charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Before saving: JR_Calc_OSSellAmtWithGST", 106m, oTO6Charge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("Before saving: JR_Calc_OSSellGSTAmt", 0m, oTO6Charge.JR_Calc_OSSellGSTAmt);
			AssertEquals("Before saving: JR_Calc_OSSellExtraTaxAmt", 6m, oTO6Charge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("Before saving: JR_Calc_LocalSellExtraTaxAmt", 6m, oTO6Charge.JR_Calc_LocalSellExtraTaxAmt);

			//Calculating on load (Charge is already saved) ???
			Factory.Save();

			oTO6Charge = new BusinessObjectFactory().Load<BaseCharge>(oTO6Charge.PK);
			AssertEquals("On load: JR_OSSellGSTAmt", 6m, oTO6Charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("On load: JR_Calc_OSSellAmtWithGST", 106m, oTO6Charge.JR_Calc_OSSellAmtWithGST);
			AssertEquals("On load: JR_Calc_OSSellGSTAmt", 0m, oTO6Charge.JR_Calc_OSSellGSTAmt);
			AssertEquals("On load: JR_Calc_OSSellExtraTaxAmt", 6m, oTO6Charge.JR_Calc_OSSellExtraTaxAmt);
			AssertEquals("On load: JR_Calc_LocalSellExtraTaxAmt", 6m, oTO6Charge.JR_Calc_LocalSellExtraTaxAmt);
		}

		#endregion

		#region Department Mapping - Customs Charges

		public void TestSettingCustomsChargeCodeSetsDepartment()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			GlbDepartment cEADept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));

			Job testJob = Factory.NewJobForTesting<Job>();
			CommonShipment shipment = CommonShipment.New(Factory);
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = shipment.PK;

			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.Department.GE_Code = "FEA";

			BaseCharge newCharge = testJob.Charges.AddNew();

			newCharge.JR_AC = testChargeCode.PK;
			AssertEquals("Charge department is current dept, as charge code is not a customs charge", testJob.Department.PK, newCharge.JR_GE);

			newCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			newCharge.JR_AC = ZGuid.Empty;
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			newCharge.JR_AC = testChargeCode.PK;
			AssertEquals("Charge department is now the equivalent customs dept", cEADept, newCharge.Department);

			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsCFSRegistered = false;

			newCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			newCharge.JR_AC = ZGuid.Empty;
			AssertEquals("Charge department is reverted back to current dept, as no charge code specified", testJob.Department.PK, newCharge.JR_GE);

			newCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			shipment.JS_IsCFSRegistered = true; // make it both CFS AND FREIGHT
			newCharge.JR_AC = testChargeCode.PK;
			AssertEquals("Charge department is now the equivalent customs dept", cEADept, newCharge.Department);

			newCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			newCharge.JR_AC = ZGuid.Empty;
			AssertEquals("Charge department is reverted back to current dept, as no charge code specified", testJob.Department.PK, newCharge.JR_GE);

			newCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			shipment.JS_IsForwardRegistered = false; // make it both CFS AND FREIGHT
			newCharge.JR_AC = testChargeCode.PK;
			AssertEquals("Charge department is now the current department because shipment is not a FREIGHT shipment",
						testJob.Department.PK, newCharge.Department.PK);
		}

		public void TestSettingCustomsChargesToCancelledDepartment()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			GlbDepartment cEADept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));
			cEADept.GE_IsActive = false;

			Job testJob = Factory.NewJobForTesting<Job>();
			CommonShipment shipment = CommonShipment.New(Factory);
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = shipment.PK;

			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.Department.GE_Code = "FEA";

			BaseCharge newCharge = testJob.Charges.AddNew();

			newCharge.JR_AC = testChargeCode.PK;
			AssertEquals("Charge department is current dept, as charge code is not a customs charge", testJob.Department.PK, TestCharge.JR_GE);

			newCharge.JR_AC = ZGuid.Empty;
			testChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			newCharge.JR_AC = testChargeCode.PK;
			AssertEquals("Charge department is STILL current department as customs equiv dept is inactive", testJob.Department.PK, newCharge.Department.PK);
		}

		#endregion

		#region Autorating Notes

		public void TestAutoratingNotes()
		{
			AssertEquals("No autorating notes on new charge", ZBlob.Empty, TestCharge.RevenueCalculationDescription);
			AssertEquals("No autorating notes on new charge", ZBlob.Empty, TestCharge.CostCalculationDescription);

			TestCharge.JR_AC = Env.Registry.FreightChargeCode;
			TestCharge.RevenueCalculationDescription = ZBlob.FromAscii("AAA");
			TestCharge.CostCalculationDescription = ZBlob.FromAscii("BBB");

			AssertEquals("AAA", TestCharge.RevenueCalculationDescription.ToAscii());
			AssertEquals("BBB", TestCharge.CostCalculationDescription.ToAscii());
		}

		[TestDate(2012, 5, 8)]
		public void TestAutoratingNotes_OldDescriptionIncludingChargeCode()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 100m;

			var sellNote1 = CreateNote(charge.Notes, "Some old sell text 1", "AUTORATE_SELL_HISTORICAL_1");
			var costNote1 = CreateNote(charge.Notes, "Some old cost text 1", "AUTORATE_COST_HISTORICAL_1");
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);
			var sellNote2 = CreateNote(charge.Notes, "Some old sell text 2", "AUTORATE_SELL_HISTORICAL_2");
			var costNote2 = CreateNote(charge.Notes, "Some old cost text 2", "AUTORATE_COST_HISTORICAL_2");
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);
			var sellNote3 = CreateNote(charge.Notes, "Some old sell text 3", "AUTORATE_SELL_HISTORICAL_3");
			var costNote3 = CreateNote(charge.Notes, "Some old cost text 3", "AUTORATE_COST_HISTORICAL_3");
			Factory.Save();

			AssertEquals("latest sell note found", "Some old sell text 3", charge.RevenueCalculationDescription.ToAscii());
			AssertEquals("latest cost note found", "Some old cost text 3", charge.CostCalculationDescription.ToAscii());

			charge.RevenueCalculationDescription = ZBlob.FromAscii("New style sell note");
			charge.CostCalculationDescription = ZBlob.FromAscii("New style cost note");

			Assert("extra older notes deleted", sellNote1.IsDeleted);
			Assert("extra older notes deleted", sellNote2.IsDeleted);
			Assert("extra older notes deleted", costNote1.IsDeleted);
			Assert("extra older notes deleted", costNote2.IsDeleted);
			AssertEquals("Description updated", "AUTORATE_SELL", sellNote3.ST_Description);
			AssertEquals("Description updated", "AUTORATE_COST", costNote3.ST_Description);
			AssertEquals("NoteData updated", "New style sell note", sellNote3.ST_NoteDataAsText);
			AssertEquals("NoteData updated", "New style cost note", costNote3.ST_NoteDataAsText);

			charge.RevenueCalculationDescription = ZBlob.FromAscii("Updated sell note");
			charge.CostCalculationDescription = ZBlob.FromAscii("Updated cost note");
			AssertEquals("NoteData updated", "Updated sell note", sellNote3.ST_NoteDataAsText);
			AssertEquals("NoteData updated", "Updated cost note", costNote3.ST_NoteDataAsText);
		}

		public void TestUpdateNoteWhenItWasDeletedSomewhereElse_SellNote_NoneExtra()
		{
			TestUpdateNoteWhenItWasDeletedSomewhereElse_Sell(0);
		}

		public void TestUpdateNoteWhenItWasDeletedSomewhereElse_SellNote_OneExtra()
		{
			TestUpdateNoteWhenItWasDeletedSomewhereElse_Sell(1);
		}

		public void TestUpdateNoteWhenItWasDeletedSomewhereElse_SellNote_MultipleExtra()
		{
			TestUpdateNoteWhenItWasDeletedSomewhereElse_Sell(2);
		}

		public void TestUpdateNoteWhenItWasDeletedSomewhereElse_CostNote_NoneExtra()
		{
			TestUpdateNoteWhenItWasDeletedSomewhereElse_Cost(0);
		}

		public void TestUpdateNoteWhenItWasDeletedSomewhereElse_CostNote_OneExtra()
		{
			TestUpdateNoteWhenItWasDeletedSomewhereElse_Cost(1);
		}

		public void TestUpdateNoteWhenItWasDeletedSomewhereElse_CostNote_MultipleExtra()
		{
			TestUpdateNoteWhenItWasDeletedSomewhereElse_Cost(2);
		}

		void TestUpdateNoteWhenItWasDeletedSomewhereElse_Sell(int numExtraNotes)
		{
			var charge = GetCharge();
			TestUpdateNoteWhenItWasDeletedSomewhereElse(
				charge,
				blob => charge.RevenueCalculationDescription = blob,
				charge.GetSellCalculationNote,
				"AUTORATE_SELL",
				numExtraNotes
			);
		}

		void TestUpdateNoteWhenItWasDeletedSomewhereElse_Cost(int numExtraNotes)
		{
			var charge = GetCharge();
			TestUpdateNoteWhenItWasDeletedSomewhereElse(
				charge,
				blob => charge.CostCalculationDescription = blob,
				charge.GetCostCalculationNote,
				"AUTORATE_COST",
				numExtraNotes
			);
		}

		Charge GetCharge()
		{
			var newFactory = Factory.CreateNewFactory();
			var job = newFactory.NewJobWithValidTestDataForTesting<Job>();
			var charge = job.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 100m;
			return charge;
		}

		void TestUpdateNoteWhenItWasDeletedSomewhereElse(Charge charge, Action<ZBlob> setDescription, Func<StmNote> getNote, string noteDescription, int numExtraNotes)
		{
			var factory = charge.Factory;

			setDescription.Invoke(ZBlob.FromAscii("New note"));
			factory.Save();

			var noteToBeDeleted = getNote.Invoke();

			for (var i = 0; i < numExtraNotes; i++)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(5);
				CreateNote(charge.Notes, $"Extra Note {i}", $"{noteDescription}_{i}");
				factory.Save();
			}

			noteToBeDeleted.Delete();

			var newDescription = ZBlob.FromAscii("Set data to note after it is deleted.");
			StmNote lastNote = null;
			AssertNoExceptionThrown("Should not throw DeletedRowInaccessibleException and update latest note or create a new one", () =>
			{
				setDescription.Invoke(newDescription);
				lastNote = getNote.Invoke();
			});

			AssertNotNull(lastNote);
			AssertEquals("The note should have the description", newDescription, lastNote.ST_NoteData);
		}

		StmNote CreateNote(Notes notes, string noteText, string description)
		{
			StmNote note = notes.AddNew();
			note.ST_IsCustomDescription = true;
			note.ST_Description = description;
			note.ST_NoteData = ZBlob.FromAscii(noteText);
			note.ReadOnly = true;

			return note;
		}

		#endregion

		#region Override Rating

		public void TestSuppressAutoRatingOverrideOnAJob()
		{
			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();

			using (job.SuppressAutoRatingOverride())
			{
				charge.JR_OSSellAmt = 150m;
			}

			Assert("OverrideRating is not set when AutoRating is being run", !charge.JR_SellRatingOverride);

			charge.JR_OSSellAmt = 300m;
			charge.JR_SellRated = true;
			Assert("OverrideRating IS set when not autorating and SellRated = true", charge.JR_SellRatingOverride);

			charge.JR_SellRated = false;
			charge.JR_OSSellAmt = 400m;
			Assert("If SellRated flag is reset, override Rating remains", charge.JR_SellRatingOverride);
		}

		public void TestJR_OSSellAmtSettingOverrideRating()
		{
			var charge1 = (BaseCharge)GetNewBusinessObject();

			charge1.JR_SellRated = false;
			charge1.JR_OSSellAmt = 100m;
			Assert("OverrideRating is used if SellRated = false", charge1.JR_SellRatingOverride);

			charge1.JR_SellRated = false;
			charge1.JR_SellRatingOverride = false;

			using (charge1.SuppressAutoRatingOverride())
			{
				charge1.JR_OSSellAmt = 150m;
			}

			Assert("OverrideRating is not set when charge1.JR_SellRatingOverride and SellRated = false", !charge1.JR_SellRatingOverride);

			charge1.JR_SellRated = true;
			using (charge1.SuppressAutoRatingOverride())
			{
				charge1.JR_OSSellAmt = 200m;
			}

			Assert("OverrideRating is not set when AutoRating is being run and SellRated = true", !charge1.JR_SellRatingOverride);

			charge1.JR_OSSellAmt = 300m;
			Assert("OverrideRating IS set when not autorating", charge1.JR_SellRatingOverride);

			charge1.JR_SellRated = false;
			charge1.JR_OSSellAmt = 400m;
			Assert("If SellRated flag is reset, override Rating remains", charge1.JR_SellRatingOverride);
		}

		public void TestJR_OSCostAmtSettingOverrideRating()
		{
			var charge1 = (BaseCharge)GetNewBusinessObject();

			charge1.JR_CostRated = false;
			charge1.JR_OSCostAmt = 100m;
			Assert("OverrideRating is used if CostRated = false", charge1.JR_CostRatingOverride);

			charge1.JR_CostRated = false;
			charge1.JR_CostRatingOverride = false;

			using (charge1.SuppressAutoRatingOverride())
			{
				charge1.JR_OSCostAmt = 150m;
			}

			Assert("OverrideRating is not set when AutoRating is being run and CostRated = false", !charge1.JR_CostRatingOverride);

			charge1.JR_CostRated = true;
			using (charge1.SuppressAutoRatingOverride())
			{
				charge1.JR_OSCostAmt = 200m;
			}

			Assert("OverrideRating is not set when AutoRating is being run and CostRated = true", !charge1.JR_CostRatingOverride);

			charge1.JR_OSCostAmt = 300m;
			Assert("OverrideRating IS set when not autorating", charge1.JR_CostRatingOverride);

			charge1.JR_CostRated = false;
			charge1.JR_OSCostAmt = 400m;
			Assert("If CostRated flag is reset, override Rating remains", charge1.JR_CostRatingOverride);
		}

		#endregion

		#region GST

		public void TestGSTCollection()
		{
			AccTaxRate activeTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			activeTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AccTaxRate otherCountryActiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			otherCountryActiveTaxRate.AT_RN_NKCountry = "GB";
			AccTaxRate inactiveTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			inactiveTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			inactiveTaxRate.AT_IsActive = false;

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.GSTCollection.Load();
			AssertEquals("Should contain active tax rate", true, charge.GSTCollection.Contains(activeTaxRate));
			AssertEquals("Should not contain other company tax rate", false, charge.GSTCollection.Contains(otherCountryActiveTaxRate));
			AssertEquals("Should not contain inactive tax rate", false, charge.GSTCollection.Contains(inactiveTaxRate));
		}

		public void TestGSTTaxRateCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.GSTCollection.Load();
			Assert("Collection should present only the VAT Tax System", charge.GSTCollection.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		#endregion

		#region Recognize Profit on WIPs and Accruals

		public void TestApplyRevenueRecognitionDateIsNotCalledIfSuspended()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = "SHP";
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObjectFactory periodFactory = new BusinessObjectFactory();
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(periodFactory);
			helper.SetupPeriods();

			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.Value;
			AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00012345");
				shipment.JS_E_ARV = helper.PreviousSubLedgerClosedPeriod.AM_StartDate;
				Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, shipment);
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				AssertEquals("Precondition", ZDateTime.Empty, job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)));

				BaseCharge charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_JH = job.PK;
				charge.JR_LocalCostAmt = 10M;
				charge.JR_LocalSellAmt = 10M;
				using (ServiceContainerSuspenderHelper.GetApplyRevenueRecognitionDateSuspender(Factory))
				{
					Factory.Save();
				}
				AssertEquals("Job Revenue Recognition Date should not be set because it is suspended", ZDateTime.Empty, job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegistryValue);
			}
		}

		public void TestRecognizeProfitInNextOpenPeriod()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = "SHP";
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObjectFactory periodFactory = new BusinessObjectFactory();
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(periodFactory);
			helper.SetupPeriods();

			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.Value;
			AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00012345");
				shipment.JS_E_ARV = helper.PreviousSubLedgerClosedPeriod.AM_StartDate;
				Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, shipment);
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				AssertEquals("Precondition", ZDateTime.Empty, job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)));

				BaseCharge charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_JH = job.PK;
				charge.JR_LocalCostAmt = 10M;
				charge.JR_LocalSellAmt = 10M;
				Factory.Save();

				charge.Reload();
				AssertNotNull("WIP should be created", charge.WIP);
				AssertNotNull("Accrual should be created", charge.Accrual);

				AssertEquals("Job Revenue Recognition Date should be ETA", shipment.JS_E_ARV, job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)));

				AssertEquals("WIP Post Date should be end date of next open period", helper.PreviousOpenPeriod.AM_EndDate, charge.WIP.AL_PostDate);
				AssertEquals("Accrual Post Date should be end date of next open period", helper.PreviousOpenPeriod.AM_EndDate, charge.WIP.AL_PostDate);

				AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegistryValue);

				BaseCharge newCharge = job.Charges.AddNew();
				newCharge.JR_AC = TestObjectCreator.CC1.PK;
				newCharge.JR_JH = job.PK;
				newCharge.JR_LocalCostAmt = 100M;
				newCharge.JR_LocalSellAmt = 100M;
				Factory.Save();

				newCharge.Reload();

				AssertNotNull("WIP should be created", newCharge.WIP);
				AssertNotNull("Accrual should be created", newCharge.Accrual);

				AssertEquals("Job Revenue Recognition Date should be ETA", shipment.JS_E_ARV, job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)));

				AssertEquals("WIP Post Date should be today's date", ZDateTime.Now.Date, newCharge.WIP.AL_PostDate.Date);
				AssertEquals("Accrual Post Date should be today's date", ZDateTime.Now.Date, newCharge.WIP.AL_PostDate.Date);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegistryValue);
			}
		}

		public void TestCreateWipAndAccrualOnSaving()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = "SHP";
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345"));
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseCharge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10M;
			charge.JR_LocalSellAmt = 10M;
			Factory.Save();

			AssertEquals("Precondition", ZDateTime.Empty, job.GetRevenueRecognitionDate(charge.CostRecognition));
			AssertEquals("Precondition", ZDateTime.Empty, job.GetRevenueRecognitionDate(charge.SellRecognition));

			charge.Reload();
			AssertNull("WIP should not be created.", charge.WIP);
			AssertNull("Accrual should not be created.", charge.Accrual);

			((ForwardingShipment)job.PlugInData).JS_E_ARV = ZDateTime.BrettsBirthday;
			TestObjectCreator.CreateTestPeriods(ZDateTime.BrettsBirthday);
			Factory.Save();

			charge.Reload();
			AssertNotNull("WIP should be created.", charge.WIP);
			AssertNotNull("Accrual should be created.", charge.Accrual);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012346"));
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10M;
			charge.JR_LocalSellAmt = 10M;
			Factory.Save();

			AssertNotNull("WIP should be created.", charge.WIP);
			AssertNotNull("Accrual should be created.", charge.Accrual);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012347"));
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = job.PK;
			charge.JR_LocalCostAmt = 10M;
			charge.JR_LocalSellAmt = 10M;
			Factory.Save();

			AssertNotNull("WIP should be created.", charge.WIP);
			AssertNotNull("Accrual should be created.", charge.Accrual);
		}

		public virtual void TestNewWIPIsCreatedOnSave_WhenPreviousWIPAmountEqualsNewAmountPlusCFX()
		{
			TestObjectCreator.AALSHI.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			charge.JR_LineCFX = 2M;
			charge.JR_OSSellAmt = 150.0M;

			var exchangeRate = charge.InvoicingJob.ExchangeRates[0] ?? charge.InvoicingJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_CFXPercent = 2M;

			Factory.Save();

			AssertNotNull("Precondition: a WIP is created on save", charge.WIP);
			AssertEquals("Precondition: WIP amount is 150 * 1.02", -153m, charge.WIP.AL_LineAmount);
			var originalWIPPk = charge.WIP.PK;

			charge.JR_OSSellAmt = 153m;

			Factory.Save();

			AssertNotNull("A WIP is created on save", charge.WIP);
			AssertNotEquals("A new WIP is created on save", originalWIPPk, charge.WIP.PK);
			AssertEquals("WIP amount is 153 * 1.02", -156.06m, charge.WIP.AL_LineAmount);
		}

		#endregion

		public override void TestExchangeRateDecimalPlaces()
		{
			var origIsReciprocal = TestObjectCreator.SetCurrentCompanyReciprocal(true);
			try
			{
				AssertEquals("Number of ex rate decimal places = 6", 6, TestCharge.ExchangeRateDecimalPlaces);
				TestObjectCreator.SetCurrentCompanyReciprocal(false);
				AssertEquals("Number of ex rate decimal places = 6", 6, TestCharge.ExchangeRateDecimalPlaces);
			}
			finally
			{
				TestObjectCreator.SetCurrentCompanyReciprocal(origIsReciprocal);
			}
		}

		public void TestCostAccount()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			AssertNull("CostAccount", charge.CostAccount);
			charge.JR_OH_CostAccount = Factory.New<OrgHeader>().PK;
			AssertNotNull("CostAccount", charge.CostAccount);
			AssertEquals("CostAccount should be read-only", true, charge.CostAccount.ReadOnly);
		}

		public void TestSellAccount()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			AssertNull("SellAccount", charge.SellAccount);
			charge.JR_OH_SellAccount = Factory.New<OrgHeader>().PK;
			AssertNotNull("SellAccount", charge.SellAccount);
			AssertEquals("SellAccount should be read-only", true, charge.SellAccount.ReadOnly);
		}

		#region Concurrency

		public override void TestConcurrencyPolicyDoesNotAllowToChangeJR_AL_APLineInOtherSession()
		{
			Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345"));
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseCharge charge = (BaseCharge)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10M;
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 10M;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			TestObjectCreator newFactoryUtils = new TestObjectCreator(newFactory);

			APInvoice newFactoryAPInvoice = newFactoryUtils.CreateAPInvoice<APInvoice>("I001", TestObjectCreator.AUD, 1M, 10M, 0, 0, 10M, 0, 0);

			APInvoiceLine line = (APInvoiceLine)newFactoryAPInvoice.Lines.AddNew();
			line.AL_AG = newFactoryUtils.GLHeader1.PK;
			BaseCharge chargeReload = newFactory.Load<BaseCharge>(charge.PK);
			if (chargeReload.APLine != null)
			{
				chargeReload.ReverseAccrual(ZDateTime.Now);
			}

			chargeReload.JR_AL_APLine = line.PK;
			line.AL_OSAmount = line.AL_LineAmount = -chargeReload.JR_OSCostAmt;

			newFactoryAPInvoice.SubmittedFromInvoicingForm = true;
			newFactory.Save();

			ErrorReporter.Clear();
			try
			{
				APInvoice otherAPInvoice = this.TestObjectCreator.CreateAPInvoice<APInvoice>("I002", TestObjectCreator.AUD, 1M, 10M, 0, 0, 10M, 0, 0);
				APInvoiceLine otherLine = (APInvoiceLine)newFactoryAPInvoice.Lines.AddNew();
				if (charge.APLine != null)
				{
					charge.ReverseAccrual(ZDateTime.Now);
				}

				charge.JR_AL_APLine = otherLine.PK;

				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
		}

		public override void TestConcurrencyPolicyDoesNotAllowToChangeJR_AL_ARLineInOtherSession()
		{
			Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345"));
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseCharge charge = (BaseCharge)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge.JR_AC = TestObjectCreator.MRG100.PK;
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10M;
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 10M;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			TestObjectCreator newFactoryUtils = new TestObjectCreator(newFactory);

			ARInvoice newFactoryARInvoice = newFactoryUtils.CreateARInvoice<ARInvoice>("I001", newFactoryUtils.AUD, 1M, newFactoryUtils.AALSHI);

			ARInvoiceLine line = (ARInvoiceLine)newFactoryARInvoice.Lines.AddNew();
			line.AL_AG = newFactoryUtils.GLHeader1.PK;
			BaseCharge chargeReload = newFactory.Load<BaseCharge>(charge.PK);
			if (chargeReload.ARLine != null)
			{
				chargeReload.ReverseWIP(ZDateTime.Now);
			}

			chargeReload.JR_AL_ARLine = line.PK;
			line.AL_OSAmount = line.AL_LineAmount = chargeReload.JR_OSSellAmt;

			newFactoryARInvoice.SubmittedFromInvoicingForm = true;
			newFactory.Save();

			ErrorReporter.Clear();
			try
			{
				ARInvoice otherARInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("I002", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				ARInvoiceLine otherLine = (ARInvoiceLine)newFactoryARInvoice.Lines.AddNew();
				if (charge.ARLine != null)
				{
					charge.ReverseWIP(ZDateTime.Now);
				}

				charge.JR_AL_ARLine = otherLine.PK;

				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
		}

		public void TestConcurrencyPolicyDoesNotAllowToDeleteChargePostedInOtherSession()
		{
			Job job = Enterprise.Accounting.Business.JobInvoicing.Job.CreateWithMutex(Factory, TestObjectCreator.CreateShipment("S00012345"));
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseCharge charge = (BaseCharge)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10M;
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 10M;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			TestObjectCreator newFactoryUtils = new TestObjectCreator(newFactory);

			APInvoice aPInvoice = newFactoryUtils.CreateAPInvoice<APInvoice>("I001", TestObjectCreator.AUD, 1M, 10M, 0, 0, 10M, 0, 0);

			APInvoiceLine line = (APInvoiceLine)aPInvoice.Lines.AddNew();
			line.AL_AG = newFactoryUtils.GLHeader1.PK;
			BaseCharge chargeReload = newFactory.Load<BaseCharge>(charge.PK);
			if (chargeReload.APLine != null)
			{
				chargeReload.APLine.AL_ReverseDate = ZDateTime.Now;
			}

			chargeReload.JR_AL_APLine = line.PK;
			line.AL_OSAmount = line.AL_LineAmount = -chargeReload.JR_OSCostAmt;

			aPInvoice.SubmittedFromInvoicingForm = true;
			newFactory.Save();

			ErrorReporter.Clear();
			try
			{
				charge.Delete();
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));
		}

		#endregion

		public void TestReverseWIPAccrualClearCost()
		{
			Accrual accrual = Factory.New<Accrual>();
			WIP wip = Factory.New<WIP>();
			TestCharge.JR_AL_APLine = accrual.PK;
			TestCharge.JR_AL_ARLine = wip.PK;

			AssertNotNull("Precondition: Accrual must be created.", TestCharge.Accrual);
			AssertNotNull("Precondition: WIP must be created.", TestCharge.WIP);
			AssertNotNull("Precondition: RelatedJobCharge must be set", accrual.RelatedJobCharge);
			AssertNotNull("Precondition: RelatedJobCharge must be set", wip.RelatedJobCharge);

			TestCharge.ReverseAccrual(ZDateTime.Now);
			AssertEquals("JR_AL_APLine must be empty after reversing.", ZGuid.Empty, TestCharge.JR_AL_APLine);

			TestCharge.ReverseWIP(ZDateTime.Now);
			AssertEquals("JR_AL_ARLine must be empty after reversing.", ZGuid.Empty, TestCharge.JR_AL_ARLine);
		}

		public void TestUpdateWIPAccrualOnClearingCostAndRevenueLinks()
		{
			Accrual accrual = Factory.New<Accrual>();
			WIP wip = Factory.New<WIP>();
			TestCharge.JR_AL_APLine = accrual.PK;
			TestCharge.JR_AL_ARLine = wip.PK;

			AssertNotNull("Precondition: Accrual must be created.", TestCharge.Accrual);
			AssertNotNull("Precondition: WIP must be created.", TestCharge.WIP);
			AssertNotNull("Precondition: RelatedJobCharge must be set", accrual.RelatedJobCharge);
			AssertNotNull("Precondition: RelatedJobCharge must be set", wip.RelatedJobCharge);

			TestCharge.ClearCostLink();
			AssertEquals("PostCondition: JR_AL_APLine must be empty after reversing.", ZGuid.Empty, TestCharge.JR_AL_APLine);
			AssertNull("RelatedJobCharge for previous Accrual must be null.", accrual.RelatedJobCharge);

			TestCharge.ClearRevenueLink();
			AssertEquals("PostCondition: JR_AL_ARLine must be empty after reversing.", ZGuid.Empty, TestCharge.JR_AL_ARLine);
			AssertNull("RelatedJobCharge for previous WIP must be null.", wip.RelatedJobCharge);
		}

		public void TestReverseAccrual()
		{
			ZDateTime arrivalDate = ZDateTime.Today;
			TestObjectCreator.CreateTestPeriods(arrivalDate);
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			shipment.JS_E_ARV = arrivalDate;
			Factory.Save();
			Job job = TestObjectCreator.CreateJob(shipment, false);

			TestCharge.JR_AC = TestObjectCreator.CC1.PK;
			TestCharge.JR_JH = job.PK;
			TestCharge.JR_LocalCostAmt = 100M;
			job.ApplyRevenueRecognitionDate(TestCharge);
			AssertEquals("Precondition: revenue recognition date should be set", arrivalDate, job.GetRevenueRecognitionDate(setting.RecognitionDateOptionCode));

			Accrual accrual = TestObjectCreator.CreateAccrual(TestCharge);
			AssertEquals("Precondition: charge should have correct accrual.", accrual, TestCharge.Accrual);
			AssertEquals(setting.RecognitionDateOptionCode, accrual.AL_RevRecognitionType);
			AssertEquals(ZDateTime.Empty, accrual.AL_ReverseDate);

			DateTime originalReverseDate = new DateTime(2003, 4, 3);

			TestCharge.ReverseAccrual(originalReverseDate);
			AssertEquals("Reverse Date Should Be Arrival Date because use recogintion date", arrivalDate, accrual.AL_ReverseDate);
			AssertNull("Charge Accrual Should Be Null After Reverse", TestCharge.Accrual);

			accrual = TestObjectCreator.CreateAccrual(TestCharge);
			AssertEquals(accrual, TestCharge.Accrual);
			AssertEquals(setting.RecognitionDateOptionCode, accrual.AL_RevRecognitionType);
			AssertEquals(ZDateTime.Empty, accrual.AL_ReverseDate);

			TestCharge.ReverseAccrual(originalReverseDate, true);
			AssertEquals("Reverse Date should Be Original Date because not use recognition date", originalReverseDate, accrual.AL_ReverseDate);
			AssertNull("Charge Accrual Should Be Null After Reverse", TestCharge.Accrual);
		}

		public void TestReverseWIP()
		{
			ZDateTime arrivalDate = ZDateTime.Today;
			TestObjectCreator.CreateTestPeriods(arrivalDate);
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			shipment.JS_E_ARV = arrivalDate;
			Factory.Save();
			Job job = TestObjectCreator.CreateJob(shipment, false);

			TestCharge.JR_AC = TestObjectCreator.CC1.PK;
			TestCharge.JR_JH = job.PK;
			TestCharge.JR_LocalCostAmt = 100M;
			job.ApplyRevenueRecognitionDate(TestCharge);
			AssertEquals("Precondition: revenue recognition date should be set", arrivalDate, job.GetRevenueRecognitionDate(setting.RecognitionDateOptionCode));

			WIP wip = TestObjectCreator.CreateWIP(TestCharge);
			AssertEquals("Precondition: charge should have correct WIP.", wip, TestCharge.WIP);
			AssertEquals(setting.RecognitionDateOptionCode, wip.AL_RevRecognitionType);
			AssertEquals(ZDateTime.Empty, wip.AL_ReverseDate);

			DateTime originalReverseDate = new DateTime(2003, 4, 3);

			TestCharge.ReverseWIP(originalReverseDate);
			AssertEquals("Reverse Date Should Be Arrival Date because use recogintion date", arrivalDate, wip.AL_ReverseDate);
			AssertNull("Charge WIP Should Be Null After Reverse", TestCharge.WIP);

			wip = TestObjectCreator.CreateWIP(TestCharge);
			AssertEquals(wip, TestCharge.WIP);
			AssertEquals(setting.RecognitionDateOptionCode, wip.AL_RevRecognitionType);
			AssertEquals(ZDateTime.Empty, wip.AL_ReverseDate);

			TestCharge.ReverseWIP(originalReverseDate, true);
			AssertEquals("Reverse Date should Be Original Date because not use recognition date", originalReverseDate, wip.AL_ReverseDate);
			AssertNull("Charge WIP Should Be Null After Reverse", TestCharge.WIP);
		}

		public void TestUpdatingWIPAcrualRelatedJobChargeOnChangingChargeLineValue()
		{
			AssertEquals("Precondition: Accrual should be empty.", ZGuid.Empty, TestCharge.JR_AL_APLine);

			Accrual accrual1 = Factory.New<Accrual>();
			AssertNull(accrual1.RelatedJobCharge);

			TestCharge.JR_AL_APLine = accrual1.PK;
			AssertEquals("RelatedJobCharge must not load another cope of charge as BaseCharge object.", TestCharge.GetType(), accrual1.RelatedJobCharge.GetType());
			AssertEquals("RelatedJobCharge must have correct charge value.", TestCharge.PK, accrual1.RelatedJobCharge.PK);

			Accrual accrual2 = Factory.New<Accrual>();
			AssertNull(accrual2.RelatedJobCharge);

			TestCharge.Accrual.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_APLine = accrual2.PK;
			AssertNull("RelatedJobCharge for previous Accrual must be null.", accrual1.RelatedJobCharge);
			AssertEquals("RelatedJobCharge must not load another cope of charge as BaseCharge object.", TestCharge.GetType(), accrual2.RelatedJobCharge.GetType());
			AssertEquals("RelatedJobCharge must have correct charge value.", TestCharge.PK, accrual2.RelatedJobCharge.PK);

			AssertEquals("Precondition: WIP should be empty.", ZGuid.Empty, TestCharge.JR_AL_ARLine);

			WIP wip1 = Factory.New<WIP>();
			AssertNull(wip1.RelatedJobCharge);

			TestCharge.JR_AL_ARLine = wip1.PK;
			AssertEquals("RelatedJobCharge must not load another cope of charge as BaseCharge object.", TestCharge.GetType(), wip1.RelatedJobCharge.GetType());
			AssertEquals("RelatedJobCharge must have correct charge value.", TestCharge.PK, wip1.RelatedJobCharge.PK);

			WIP wip2 = Factory.New<WIP>();
			AssertNull(wip2.RelatedJobCharge);

			TestCharge.WIP.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_ARLine = wip2.PK;
			AssertNull("RelatedJobCharge for previous WIP must be null.", wip1.RelatedJobCharge);
			AssertEquals("RelatedJobCharge must not load another cope of charge as BaseCharge object.", TestCharge.GetType(), wip2.RelatedJobCharge.GetType());
			AssertEquals("RelatedJobCharge must have correct charge value.", TestCharge.PK, wip2.RelatedJobCharge.PK);
		}

		public void TestUpdatingWIPAcrualRelatedJobChargeInAccrualAndWIPGetters()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Accrual accrual = Factory.NewWithValidTestData<Accrual>();
			accrual.AL_OSExTaxAmount = 100M;
			WIP wip = Factory.NewWithValidTestData<WIP>();
			wip.AL_OSExTaxAmount = 100M;
			TestCharge.JR_AL_APLine = accrual.PK;
			TestCharge.JR_AL_ARLine = wip.PK;
			TestCharge.JR_AC = TestObjectCreator.CC1.PK;
			TestCharge.JR_JH = job.PK;
			TestCharge.JR_LocalCostAmt = 100M;
			TestCharge.JR_LocalSellAmt = 100M;
			TestCharge.JR_OSCostAmt = 100M;
			TestCharge.JR_OSSellAmt = 100M;
			accrual.AL_JH = TestCharge.JR_JH;
			wip.AL_JH = TestCharge.JR_JH;
			accrual.AL_AG = TestObjectCreator.GLHeader1.PK;
			wip.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseCharge loadedCharge = (BaseCharge)newFactory.Load(TestCharge.GetType(), TestCharge.PK);
			accrual = loadedCharge.Accrual;
			wip = loadedCharge.WIP;

			AssertEquals("RelatedJobCharge must not load another cope of charge as BaseCharge object.", TestCharge.GetType(), accrual.RelatedJobCharge.GetType());
			AssertEquals("RelatedJobCharge must have correct charge value.", TestCharge.PK, accrual.RelatedJobCharge.PK);
			AssertEquals("RelatedJobCharge must not load another cope of charge as BaseCharge object.", TestCharge.GetType(), wip.RelatedJobCharge.GetType());
			AssertEquals("RelatedJobCharge must have correct charge value.", TestCharge.PK, wip.RelatedJobCharge.PK);
		}

		public void TestBillInLocalCurrency()
		{
			Job job1 = TestObjectCreator.CreateJob("Z00001002", TestObjectCreator.ABIGAS, 0M, TestObjectCreator.ZECTRA, 0M);
			ExchangeRate rate1 = TestObjectCreator.CreateExchangeRate(job1, TestObjectCreator.GBP, .4M);
			BaseCharge charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Charge 1_1", TestObjectCreator.GBP, 250M, TestObjectCreator.AALSHI, TestObjectCreator.GBP, 400M, TestObjectCreator.ABIGAS);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Bill In Local Currency", true, charge.BillInLocalCurrency);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("Bill In Local Currency", false, charge.BillInLocalCurrency);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertEquals("Bill In Local Currency", false, charge.BillInLocalCurrency);

			charge.JR_OH_SellAccount = TestObjectCreator.ZECTRA.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Bill In Local Currency", true, charge.BillInLocalCurrency);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("Bill In Local Currency", false, charge.BillInLocalCurrency);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			AssertEquals("Bill In Local Currency", false, charge.BillInLocalCurrency);

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Bill In Local Currency", true, charge.BillInLocalCurrency);

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
			AssertEquals("Bill In Local Currency", true, charge.BillInLocalCurrency);

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			AssertEquals("Bill In Local Currency", false, charge.BillInLocalCurrency);

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect;
			AssertEquals("Bill In Local Currency", true, charge.BillInLocalCurrency);

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AssertEquals("Bill In Local Currency", false, charge.BillInLocalCurrency);
		}

		#region CFX

		[ExpectNoExceptions()]
		public void TestCalculatingCFXAmtNotThrowingException()
		{
			var exchangeRateFactory = new BusinessObjectFactory();
			var currency = exchangeRateFactory.Load<RefCurrency>(TestObjectCreator.USD.PK);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var charge = (BaseCharge)Factory.New(GetExpectedBusinessObjectType());
			SetupChargeForCFXTest(currency, charge, 50m, 0.5m, 100m, 2m);
			AssertNull(charge.Job);
			charge.JR_RX_NKSellCurrency = "ABC"; // invalid currency entered
			charge.JR_LineCFX = 2m;
			var expectedCfxAmt = (charge is ApportionSplitCharge && !charge.IsInDatabase) ? 0m : 50m; //we keep value of exchange rate on the charge after breaking link to ex rate

			if (charge is Charge)
			{
				AssertNoExceptionThrown("Calculating CFX Amount should not throw exception if there is no exchange rate (job == null)", () => ((Charge)charge).CalculateCFXAmt());
			}
			AssertEquals("CFX Amount should be zero if there is no exchange rate (job == null)", expectedCfxAmt, charge.JR_CFXAmt);
		}

		public void TestCFXAmtCalculation()
		{
			TestObjectCreator.Debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			ABaseCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ABaseCharge.InvoicingJob.LocalChargesPK = TestObjectCreator.Debtor.PK;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert("System should now be in a state where it will apply CFX", ABaseCharge.IsApplyCFX);

			AssertEquals("Line Currency should be USD", TestObjectCreator.USD.RX_Code, ABaseCharge.JR_RX_NKSellCurrency);

			ABaseCharge.JR_OSSellAmt = 200.00m;
			ABaseCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			ABaseCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			var exRate = ABaseCharge.ExchangeRateProvider_ForTestOnly.GetExchangeRate(TestObjectCreator.USD.RX_Code, TestObjectCreator.Debtor.PK, ExchangeRateValidLedgerEnum.AR);
			exRate.SetBuyRate_ForTestOnly(GlbCompany.CurrentCompany.GC_IsReciprocal ? 1 / 0.65m : 0.65m);
			ABaseCharge.InvoicingJob.ExchangeRates[0].JF_CFXPercent = 5.00m;

			ZDecimal expectedSellExRate = GlbCompany.CurrentCompany.GC_IsReciprocal ? 0.65m / 0.95m : 0.65m * 0.95m;
			AssertEquals("Buy Rate calculated", 0.65m, exRate.Rate);
			AssertEquals("Sell Rate calculated", expectedSellExRate, exRate.SellRate);
			AssertEquals("Sell Rate on line", expectedSellExRate, ABaseCharge.JR_OSSellExRate);

			ZDecimal expectedLocalSellAtSellRate = Env.CurrentCompany.ExchangeRate.ForeignToLocal(200.00m, expectedSellExRate);
			ZDecimal expectedLocalSellAtBuyRate = Env.CurrentCompany.ExchangeRate.ForeignToLocal(200.00m, 0.65m);

			AssertEquals("Local Sell Amount should be 200/(0.65m*0.95m)", expectedLocalSellAtSellRate, ABaseCharge.JR_LocalSellAmt);

			AssertEquals("CFX value calculated", expectedLocalSellAtSellRate - expectedLocalSellAtBuyRate, ABaseCharge.JR_CFXAmt);

			ABaseCharge.InvoicingJob.ExchangeRates[0].JF_CFXPercent = 0.00m;

			AssertEquals("Line CFX should be reset", 0m, ABaseCharge.JR_LineCFX);
			AssertEquals("CFX value should now be zero due to zero CFX uplift", 0m, ABaseCharge.JR_CFXAmt);
		}

		public void TestIsApplyBillToCFX()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestJob.LocalChargesPK = TestObjectCreator.Debtor.PK;
			TestJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			ABaseCharge.JR_OH_SellAccount = TestJob.LocalChargesPK;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			TestObjectCreator.MRG100.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ABaseCharge.JR_AC = TestObjectCreator.MRG100.PK;

			ABaseCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert(!ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("Apply CFX should still be false - no dependency on Cost Currency", !ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("Sell currency is foreign but bill in local is false", !ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert(ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("Sell currency is local", !ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Assert("Sell currency is foreign and bill in local is true", ABaseCharge.IsApplyCFX);

			AccTransactionLines transactionLine = Factory.NewWithValidTestData<ARInvoice>().Lines.AddNew();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			transactionLine.AL_GB = GlbBranch.CurrentBranch.PK;
			transactionLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			transactionLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			transactionLine.AL_AT = ABaseCharge.JR_AT_SellGSTRate;
			ABaseCharge.JR_AL_ARLine = transactionLine.PK;

			Assert("Should not apply because revenue exists, even if it's not saved", ABaseCharge.IsApplyCFX);

			ABaseCharge.CostExchangeRate?.SetBuyRate_ForTestOnly(1m);
			ABaseCharge.RevenueExchangeRate?.SetBuyRate_ForTestOnly(1m);
			ABaseCharge.SellInvoiceExchangeRate?.SetBuyRate_ForTestOnly(1m);

			Factory.Save();

			Assert("Should not apply because revenue is saved", !ABaseCharge.IsApplyCFX);
		}

		public void TestIsApplyAgentCFX()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestJob.AgentCollectPK = ZGuid.NewZGuid();

			ABaseCharge.JR_OH_SellAccount = TestJob.AgentCollectPK;
			ABaseCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert(!ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_RX_NKSellCurrency = "USD";
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert(!ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = "GBP";
			ABaseCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = "GBP";
			ABaseCharge.JR_RX_NKSellCurrency = "USD";
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert(!ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = "USD";
			ABaseCharge.JR_RX_NKSellCurrency = "GBP";
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert(ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_RX_NKSellCurrency = "USD";
			ABaseCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Assert(ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = "USD";
			ABaseCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!ABaseCharge.IsApplyCFX);

			ABaseCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ABaseCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!ABaseCharge.IsApplyCFX);
		}

		public void TestLocalGSTId()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Singapore;
			Charge charge = Factory.New<Charge>();
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsActive, true);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			charge.JR_OH_SellAccount = orgHeader.PK;
			charge.SellAccount.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			charge.JR_AC = testObjectCreator.CC1.PK;
			Job job = Factory.NewJobForTesting<Job>();
			charge.JR_JH = job.PK;
			AssertNotNull("SellAcount", charge.SellAccount);
			AssertNotNull("SellAcount.MiscServ", charge.SellAccount.MiscServ);
			AssertEquals("SellAccount.CompanyData.IsARTaxApplicable", true, charge.SellAccount.CompanyData.IsARTaxApplicable);
			AssertNotNull("InvoicingJob", charge.InvoicingJob);
			AssertEquals("GlbCompany.CurrentCompany.GC_RN_NKCountryCode", Constants.CountryCodes.Singapore, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			ZGuid sellInvTaxMsg;
			Assert("LocalGSTId", charge.Job.GetGSTID(charge, CostSell.Revenue, out sellInvTaxMsg) != ZGuid.Empty);
		}

		protected void SetupChargeForCFXTest(RefCurrency currency, BaseCharge charge,
			ZDecimal osSellAmt, ZDecimal oSSellRate, ZDecimal localSellAmt, ZDecimal lineCFX)
		{
			charge.JR_RX_NKSellCurrency = currency.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_LineCFX = lineCFX;
			charge.JR_OSSellAmt = osSellAmt;
			//Charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(OSSellRate);
			charge.JR_OSSellExRate = oSSellRate;
			charge.JR_LocalSellAmt = localSellAmt;
		}

		#endregion

		public void TestHaveConstructorStackTrace()
		{
			var hasTrace = BusinessObject as IHaveConstructorStackTrace;
			AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);

			AssertNull("Should be no ConstructorStackTrace by default", hasTrace.ConstructorStackTrace);

			StackTrace trace = new StackTrace();
			hasTrace.ConstructorStackTrace = trace;

			AssertEquals("Should be assigned StackTrace", trace, hasTrace.ConstructorStackTrace);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				hasTrace = GetNewBusinessObject() as IHaveConstructorStackTrace;
				AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);
				AssertNotNull("Should have ConstructorStackTrace", hasTrace.ConstructorStackTrace);
				AssertContains("Trace should be as expected", trace.ToString(), hasTrace.ConstructorStackTrace.ToString());
			}
		}

		public void TestAPARLineValueHistoryIsSameForAllBizOaroundRow()
		{
			var allBizOs = new List<BaseCharge>();

			// Add non-abstract subclasses if necessary
			foreach (Type type in new Type[] { typeof(BaseCharge), typeof(Charge), typeof(ApportionSplitCharge) })
			{
				var bizO = (BaseCharge)Factory.Load(type, TestCharge.PK);
				AssertHasSameAPARValueHistory(bizO, TestCharge.GetAPLineValueHistory(), TestCharge.GetARLineValueHistory());
				allBizOs.Add(bizO);
			}
			Assert("Shoul,ld be different instances", allBizOs.Any(x => x != TestCharge));

			var acr1 = Factory.NewWithValidTestData<Accrual>();
			var wip1 = Factory.NewWithValidTestData<WIP>();
			TestCharge.JR_AL_APLine = acr1.PK;
			TestCharge.JR_AL_ARLine = wip1.PK;

			AssertEquals("ACR1 PK is not in history because it is current value", false, TestCharge.GetAPLineValueHistory().Contains(acr1.PK));
			AssertEquals("WIP1 PK is not in history because it is current value", false, TestCharge.GetARLineValueHistory().Contains(wip1.PK));

			foreach (var charge in allBizOs)
			{
				AssertHasSameAPARValueHistory(charge, TestCharge.GetAPLineValueHistory(), TestCharge.GetARLineValueHistory());
			}

			var acr2 = Factory.NewWithValidTestData<Accrual>();
			var wip2 = Factory.NewWithValidTestData<WIP>();
			TestCharge.ReverseAccrual(ZDateTime.Now);
			TestCharge.ReverseWIP(ZDateTime.Now);
			TestCharge.JR_AL_APLine = acr2.PK;
			TestCharge.JR_AL_ARLine = wip2.PK;

			AssertEquals("ACR2 PK is not in history because it is current value", false, TestCharge.GetAPLineValueHistory().Contains(acr2.PK));
			AssertEquals("WIP2 PK is not in history because it is current value", false, TestCharge.GetARLineValueHistory().Contains(wip2.PK));
			AssertEquals("ACR1 PK should be in history", true, TestCharge.GetAPLineValueHistory().Contains(acr1.PK));
			AssertEquals("WIP1 PK should be in history", true, TestCharge.GetARLineValueHistory().Contains(wip1.PK));

			foreach (var charge in allBizOs)
			{
				AssertHasSameAPARValueHistory(charge, TestCharge.GetAPLineValueHistory(), TestCharge.GetARLineValueHistory());
			}

			foreach (var charge in allBizOs)
			{
				charge.Delete();
			}

			AssertEquals("ACR2 PK should be in history because BizO was deleted", true, TestCharge.GetAPLineValueHistory().Contains(acr2.PK));
			AssertEquals("WIP2 PK should be in history because BizO was deleted", true, TestCharge.GetARLineValueHistory().Contains(wip2.PK));
			AssertEquals("ACR1 PK should be in history", true, TestCharge.GetAPLineValueHistory().Contains(acr1.PK));
			AssertEquals("WIP1 PK should be in history", true, TestCharge.GetARLineValueHistory().Contains(wip1.PK));

			foreach (var charge in allBizOs)
			{
				AssertHasSameAPARValueHistory(charge, TestCharge.GetAPLineValueHistory(), TestCharge.GetARLineValueHistory());
			}
		}

		void AssertHasSameAPARValueHistory(BaseCharge charge, ZGuid[] apHistory, ZGuid[] arHistory)
		{
			AssertArrayEqualsByElements("JR_AL_APLine history", apHistory, charge.GetAPLineValueHistory());
			AssertArrayEqualsByElements("JR_AL_ARLine history", arHistory, charge.GetARLineValueHistory());
		}

		public void TestRatingOverride()
		{
			TestCharge.JR_AL_APLine = ZGuid.Empty;

			AssertEquals("JR_CostRatingOverride should be false", false, TestCharge.JR_CostRatingOverride);
			AssertEquals("JR_CostRatingOverrideComment should be Empty", ZString.Empty, TestCharge.JR_CostRatingOverrideComment);
			AssertEquals("JR_CostRatingOverrideComment should be ReadOnly", true, TestCharge.JR_CostRatingOverrideComment_ReadOnly_ForTestOnly);
			AssertNoNotifications("Should be NO Warnings", TestCharge.JR_CostRatingOverrideCommentInfo);

			TestCharge.JR_SellRatingOverrideComment = "Some Reason";
			AssertNoNotifications("Should be NO Warnings", TestCharge.JR_SellRatingOverrideCommentInfo);

			TestCharge.JR_SellRatingOverride = false;
			AssertEquals("JR_SellRatingOverride should be false", false, TestCharge.JR_SellRatingOverride);
			AssertEquals("JR_SellRatingOverrideComment should be Empty", ZString.Empty, TestCharge.JR_SellRatingOverrideComment);
			AssertEquals("JR_SellRatingOverrideComment should be ReadOnly", true, TestCharge.JR_SellRatingOverrideComment_ReadOnly_ForTestOnly);
			AssertNoNotifications("Should be NO Warnings", TestCharge.JR_SellRatingOverrideCommentInfo);
		}

		public virtual void TestCostRatingOverrideComment()
		{
			TestCharge.JR_CostRatingOverride = true;
			AssertEquals("JR_CostRatingOverrideComment should be NOT ReadOnly", false, TestCharge.JR_CostRatingOverrideComment_ReadOnly_ForTestOnly);
			AssertHasWarning("Should be Warning", TestCharge.JR_CostRatingOverrideCommentInfo, "You have not entered an Override Comment.");
		}

		public void TestSetJR_OH_SellAccount_ReSetDisplaySellInvoiceAddress_ZAddress()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TEST1", false, true);
			var address1 = TestObjectCreator.CreateAddress(orgHeader1, "test111");
			address1.AddAddressType(OrgAddressType.Receivables);

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TEST2", false, true);
			var address2 = TestObjectCreator.CreateAddress(orgHeader2, "test222");
			address2.AddAddressType(OrgAddressType.Receivables);

			TestCharge.JR_OH_SellAccount = orgHeader1.PK;
			AssertCollectionContains(address1.PK, TestCharge.DisplaySellInvoiceAddress_ZAddress.OrgAddress_List.List.Cast<ICodeDescription>().Select(l => l.PK));
			AssertCollectionNotContains(address2.PK, TestCharge.DisplaySellInvoiceAddress_ZAddress.OrgAddress_List.List.Cast<ICodeDescription>().Select(l => l.PK));

			TestCharge.JR_OH_SellAccount = orgHeader2.PK;
			AssertCollectionContains(address2.PK, TestCharge.DisplaySellInvoiceAddress_ZAddress.OrgAddress_List.List.Cast<ICodeDescription>().Select(l => l.PK));
			AssertCollectionNotContains(address1.PK, TestCharge.DisplaySellInvoiceAddress_ZAddress.OrgAddress_List.List.Cast<ICodeDescription>().Select(l => l.PK));
		}

		public void TestJR_OH_SellAccountReadOnly()
		{
			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("JR_OH_SellAccountInfo.ReadOnly", false, TestCharge.JR_OH_SellAccountInfo.ReadOnly);

			TestCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			AssertEquals("JR_OH_SellAccountInfo.ReadOnly", false, TestCharge.JR_OH_SellAccountInfo.ReadOnly);

			TestCharge.JR_SellRated = true;
			AssertEquals("JR_OH_SellAccountInfo.ReadOnly", true, TestCharge.JR_OH_SellAccountInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestCharge.JR_SellRated = false;
			AssertEquals("JR_OH_SellAccountInfo.ReadOnly", false, TestCharge.JR_OH_SellAccountInfo.ReadOnly);

			TestCharge.JR_AL_ARLine = TestObjectCreator.CreateInvoiceLine(TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M),
				TestObjectCreator.AUD, 1M, 100M).PK;
			AssertEquals("Precondition: IsRevenuePosted", true, TestCharge.IsRevenuePosted);
			AssertEquals("JR_OH_SellAccountInfo.ReadOnly", true, TestCharge.JR_OH_SellAccountInfo.ReadOnly);
		}

		public virtual void TestJR_SellGovtChargeCode_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.JR_JH = job.PK;

			var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellGovtCrgCode);
			securityCheckPoint.IsAllowed = false;
			AssertEquals("JR_SellGovtChargeCode_ReadOnly = true", true, TestCharge.JR_SellGovtChargeCode_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = true;
			AssertEquals("JR_SellGovtChargeCode_ReadOnly = false", false, TestCharge.JR_SellGovtChargeCode_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = false;
			AssertEquals("JR_SellGovtChargeCode_ReadOnly = true", true, TestCharge.JR_SellGovtChargeCode_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = true;
			AssertEquals("JR_SellGovtChargeCode_ReadOnly = false", false, TestCharge.JR_SellGovtChargeCode_ReadOnly_ForTestOnly);

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var arInvLine = (ARInvoiceLine)arInvoice.Lines.AddNew();
			arInvLine.AL_OSAmount = 200m;
			TestCharge.JR_AL_ARLine = arInvLine.PK;
			AssertEquals(true, TestCharge.JR_IsRevenuePosted);
			AssertEquals("must be readonly if charge is posted", true, TestCharge.JR_SellGovtChargeCode_ReadOnly_ForTestOnly);
		}

		public virtual void TestJR_CostGovtChargeCode_ReadOnly()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.JR_JH = job.PK;

			var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostGovtCrgCode);
			securityCheckPoint.IsAllowed = false;
			AssertEquals("JR_CostGovtChargeCode_ReadOnly = true", true, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = true;
			AssertEquals("JR_CostGovtChargeCode_ReadOnly = false", false, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = false;
			AssertEquals("JR_CostGovtChargeCode_ReadOnly = true", true, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = true;
			AssertEquals("JR_CostGovtChargeCode_ReadOnly = false", false, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apInvLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			apInvLine.AL_OSAmount = 200m;
			TestCharge.JR_AL_APLine = apInvLine.PK;
			AssertEquals("Cost is posted", true, TestCharge.JR_IsCostPosted);
			AssertEquals("must be readonly if charge is posted", true, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);

			apInvLine.AL_LineType = TransactionLineTypes.Accrual;
			AssertEquals("Cost is not posted", false, TestCharge.JR_IsCostPosted);
			AssertEquals("JR_CostGovtChargeCode_ReadOnly = false", false, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);

			TestCharge.JR_E6 = Factory.NewWithValidTestData<JobConsolCost>().PK;
			AssertEquals(true, TestCharge.JR_IsApportioned);
			AssertEquals("must be readonly if charge is apportioned", true, TestCharge.JR_CostGovtChargeCode_ReadOnly_ForTestOnly);
		}

		public void TestGovtChargeCodeGetUupdatedWhenChargeCodeIsChanged()
		{
			TestObjectCreator.CC2.AC_GovtChargeCode = string.Empty;
			var shipment = TestObjectCreator.CreateShipment("S00012345");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			TestCharge.JR_JH = job.PK;

			foreach (var enableGovtChargeCode in new[] { false, true })
			{
				TestCharge.JR_AC = TestObjectCreator.CC2.PK;
				TestObjectCreator.CC1.GovtChargeCodeOverrides.RemoveAndDeleteAll();
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode);
				AssertEquals("Before: JR_CostGovtChargeCode", string.Empty, TestCharge.JR_CostGovtChargeCode);
				AssertEquals("Before: JR_SellGovtChargeCode", string.Empty, TestCharge.JR_SellGovtChargeCode);

				Factory.Save();

				TestObjectCreator.CC1.AC_GovtChargeCode = "GVTCC1";
				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(TestObjectCreator.CC1.GovtChargeCodeOverrides.IsNullOrEmpty());
				AssertEquals("After: JR_CostGovtChargeCode", enableGovtChargeCode ? "GVTCC1" : string.Empty, TestCharge.JR_CostGovtChargeCode);
				AssertEquals("After: JR_SellGovtChargeCode", enableGovtChargeCode ? "GVTCC1" : string.Empty, TestCharge.JR_SellGovtChargeCode);

				TestCharge.JR_AC = TestObjectCreator.CC2.PK;
				AssertEquals("After: JR_CostGovtChargeCode", string.Empty, TestCharge.JR_CostGovtChargeCode);
				AssertEquals("After: JR_SellGovtChargeCode", string.Empty, TestCharge.JR_SellGovtChargeCode);

				var govtOverride = TestObjectCreator.CC1.GovtChargeCodeOverrides.AddNew();
				govtOverride.ACG_JobType = "ALL";
				govtOverride.ACG_TransportMode = "ALL";
				govtOverride.ACG_Direction = "ALL";
				govtOverride.ACG_GovtChargeCode = "GVTCC1_FallBack";
				Factory.Save();

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				AssertEquals("After: JR_CostGovtChargeCode", enableGovtChargeCode ? "GVTCC1_FallBack" : string.Empty, TestCharge.JR_CostGovtChargeCode);
				AssertEquals("After: JR_SellGovtChargeCode", enableGovtChargeCode ? "GVTCC1_FallBack" : string.Empty, TestCharge.JR_SellGovtChargeCode);
			}
		}

		public void TestGovernmentChargeCodeLogAdded()
		{
			var govtChargeCode = "GVTCC1";
			var newGovtChargeCode1 = "New GVTCC1";
			var newGovtChargeCode2 = "New GVTCC2";

			var costGovtChargeCodeChangedFromDefaultLogReference = string.Format("Cost Government Charge Code changed from Default value: '{0}'. New Value: '{1}'", govtChargeCode, newGovtChargeCode1);
			var sellGovtChargeCodeChangedFromDefaultLogReference = string.Format("Sell Government Charge Code changed from Default value: '{0}'. New Value: '{1}'", govtChargeCode, newGovtChargeCode1);

			var costGovtChargeCodeChangedLogReference = string.Format("Cost Government Charge Code Edited. New Value: '{0}', Old Value: '{1}'", newGovtChargeCode2, newGovtChargeCode1);
			var sellGovtChargeCodeChangedLogReference = string.Format("Sell Government Charge Code Edited. New Value: '{0}', Old Value: '{1}'", newGovtChargeCode2, newGovtChargeCode1);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GovtChargeCode = govtChargeCode;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, "test job charge", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, null, 0M, null);

			AssertEquals("JR_CostGovtChargeCode", govtChargeCode, charge.JR_CostGovtChargeCode);
			AssertEquals("JR_SellGovtChargeCode", govtChargeCode, charge.JR_SellGovtChargeCode);

			charge.JR_CostGovtChargeCode = newGovtChargeCode1;
			charge.JR_SellGovtChargeCode = newGovtChargeCode1;

			Factory.Save();

			var logs = job.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, costGovtChargeCodeChangedFromDefaultLogReference));
			AssertEquals("1 log added", 1, logs.Length);

			logs = job.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, sellGovtChargeCodeChangedFromDefaultLogReference));
			AssertEquals("1 log added", 1, logs.Length);

			charge.JR_CostGovtChargeCode = newGovtChargeCode2;
			charge.JR_SellGovtChargeCode = newGovtChargeCode2;

			Factory.Save();

			logs = job.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, costGovtChargeCodeChangedLogReference));
			AssertEquals("1 log added", 1, logs.Length);

			logs = job.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, sellGovtChargeCodeChangedLogReference));
			AssertEquals("1 log added", 1, logs.Length);
		}

		public void TestPropertiesReadOnlyWhenJobReadyToPostCost()
		{
			var originalGC_IsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			AssertEquals("Pre-condition", true, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxMsg).IsAllowed);

			using (new DisposableAction(() => { }, () => GlbCompany.CurrentCompany.GC_IsWHTRegistered = originalGC_IsWHTRegistered))
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetUpTestChargeToMakeAllPropertiesWritable();

				AssertEquals("Job.IsReadyForCostPosting", false, TestCharge.Job.IsReadyForCostPosting);

				AssertReadOnlyOnCostRelatedProperties(false);

				TestCharge.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
				AssertEquals("Job.IsReadyForCostPosting", true, TestCharge.Job.IsReadyForCostPosting);

				AssertReadOnlyOnCostRelatedProperties(true);
			}
		}

		protected virtual void AssertReadOnlyOnCostRelatedProperties(bool expectedReadOnly)
		{
			AssertEquals("JR_A9_CostVATClass", expectedReadOnly, TestCharge.JR_A9_CostVATClassInfo.ReadOnly);
			AssertEquals("JR_AT_CostGSTRate", expectedReadOnly, TestCharge.JR_AT_CostGSTRateInfo.ReadOnly);
			AssertEquals("JR_AW_CostWHTRate", expectedReadOnly, TestCharge.JR_AW_CostWHTRateInfo.ReadOnly);
			AssertEquals("JR_CostRatingOverride", expectedReadOnly, TestCharge.JR_CostRatingOverrideInfo.ReadOnly);
		}

		public void TestPropertiesReadOnlyWhenJobReadyToPostRevenue()
		{
			var originalGC_IsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			AssertEquals("Pre-condition", true, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxMsg).IsAllowed);

			using (new DisposableAction(() => { }, () => GlbCompany.CurrentCompany.GC_IsWHTRegistered = originalGC_IsWHTRegistered))
			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetUpTestChargeToMakeAllPropertiesWritable();

				AssertEquals("Job.IsReadyForRevenuePosting", false, TestCharge.Job.IsReadyForRevenuePosting);

				AssertReadOnlyOnRevenueRelatedProperties(false);

				TestCharge.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				AssertEquals("Job.IsReadyForRevenuePosting", true, TestCharge.Job.IsReadyForRevenuePosting);

				AssertReadOnlyOnRevenueRelatedProperties(true);
			}
		}

		protected virtual void AssertReadOnlyOnRevenueRelatedProperties(bool expectedReadOnly)
		{
			AssertEquals("JR_A9_SellVATClass", expectedReadOnly, TestCharge.JR_A9_SellVATClassInfo.ReadOnly);
			AssertEquals("JR_OH_SellAccount", expectedReadOnly, TestCharge.JR_OH_SellAccountInfo.ReadOnly);
			AssertEquals("JR_SellRatingOverride", expectedReadOnly, TestCharge.JR_SellRatingOverrideInfo.ReadOnly);
		}

		public void TestShouldNotChangeCostCurrencyUnderChargeProcessingBusinessContext()
		{
			//SET UP A SHIPMENT AND A JOB
			var shipment = TestObjectCreator.CreateShipment("S00012345");
			var job = TestObjectCreator.CreateJob(shipment, false, false);

			//CREATE JOBCHARGE
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC3.PK;
			charge.JR_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

			//CREATE ORGANIZATION HEADER WITH DEFAULT CURRENCY SET TO FOREIGN CURRENCY
			var newCompanyOrgHeader = TestObjectCreator.ABIGAS;
			newCompanyOrgHeader.CompanyData.OB_RX_NKAPDefltCurrency = "HKD";

			//ASSIGN THIS NEW ORGANIZATION HEADER AS A COSTACCOUNT
			Factory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
			charge.JR_OH_CostAccount = newCompanyOrgHeader.PK;
			Factory.RemoveContext(BusinessContext.ChargeProcessingForAPTransactionPosting);

			//CURRENCY STILL SHOULD STAY THE SAME IF WE ARE USING THE CHARGEPROCESSING BUSINESSCONTEXT
			AssertEquals("JobCharge CostCurrency remains 'AUD' since setting the CostAccount under BusinessContext ChargeProcessing should not change this field.", "AUD", charge.JR_RX_NKCostCurrency);
		}

		public void TestPropertiesReadOnlyWhenJobReadyToPostRevenueAndCost()
		{
			var originalGC_IsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			AssertEquals("Pre-condition", true, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxMsg).IsAllowed);
			AssertEquals(true, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxMsg).IsAllowed);

			using (new DisposableAction(() => { }, () => GlbCompany.CurrentCompany.GC_IsWHTRegistered = originalGC_IsWHTRegistered))
			using (AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetUpTestChargeToMakeAllPropertiesWritable();

				AssertEquals("Job.IsReadyForRevenuePosting", false, TestCharge.Job.IsReadyForRevenuePosting);

				AssertReadOnlyOnRevenueRelatedProperties(false);
				AssertReadOnlyOnCostRelatedProperties(false);
				AssertReadOnlyOnRevenueAndCostRelatedProperties(false);

				TestCharge.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
				AssertEquals("Job.IsReadyForRevenuePosting", true, TestCharge.Job.IsReadyForRevenuePosting);
				AssertEquals("Job.IsReadyForCostPosting", true, TestCharge.Job.IsReadyForCostPosting);

				AssertReadOnlyOnRevenueRelatedProperties(true);
				AssertReadOnlyOnCostRelatedProperties(true);
				AssertReadOnlyOnRevenueAndCostRelatedProperties(true);
			}
		}

		protected virtual void AssertReadOnlyOnRevenueAndCostRelatedProperties(bool expectedReadOnly)
		{
		}

		void SetUpTestChargeToMakeAllPropertiesWritable()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			TestCharge.FillWithValidTestData();
			TestCharge.JR_JH = job.PK;

			// Cost Set Up
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipmentAllowOverrideCostTaxMsg = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxMsg);
			shipmentAllowOverrideCostTaxMsg.IsAllowed = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			OrgHeader orgHeader = TestObjectCreator.Creditor1;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			TestCharge.JR_OH_CostAccount = orgHeader.PK;
			TestCharge.JR_AT_CostGSTRate = testObjectCreator.GST1.PK;
			TestCharge.JR_AL_APLine = ZGuid.Empty;
			TestCharge.CostAccount.CompanyData.SetAPTaxApplicable(true);

			TestCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestCharge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			TestCharge.JR_AK = TestObjectCreator.AUDChequeBook.PK;

			// Sell Set Up
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyWHTId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipmentAllowOverrideSellTaxMsg = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideSellTaxMsg);
			shipmentAllowOverrideSellTaxMsg.IsAllowed = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			orgHeader = TestObjectCreator.Debtor;
			orgHeader.CompanyData.SetARTaxApplicable(true);
			TestCharge.JR_OH_SellAccount = orgHeader.PK;
			TestCharge.JR_AT_SellGSTRate = testObjectCreator.GST1.PK;
			TestCharge.JR_AL_ARLine = ZGuid.Empty;
			TestCharge.SellAccount.CompanyData.SetARTaxApplicable(true);
			TestCharge.SellAccount.MiscServ.OM_ARWHTApplicable = true;

			Factory.Save();
		}

		public void TestCanRecognizeProfitOnWIPsAndAccruals()
		{
			AssertNull("Precondition: job should be null", TestCharge.InvoicingJob);
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			TestCharge.JR_JH = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"), false).PK;
			TestCharge.JR_OSCostAmt = 0M;
			TestCharge.JR_OSSellAmt = 0M;
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestCharge.JR_LocalCostAmt = -10M;
			TestCharge.JR_LocalSellAmt = 0M;
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);
			TestCharge.JR_LocalCostAmt = 0M;
			TestCharge.JR_LocalSellAmt = -10M;
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestCharge.JR_LocalCostAmt = -10M;
			TestCharge.JR_LocalSellAmt = 0M;
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);
			TestCharge.JR_LocalCostAmt = 0M;
			TestCharge.JR_LocalSellAmt = -10M;
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			TestCharge.JR_LocalCostAmt = 10M;
			TestCharge.JR_LocalSellAmt = 0M;
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			TestCharge.JR_LocalCostAmt = 0M;
			TestCharge.JR_LocalSellAmt = 10M;
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			TestCharge.JR_LocalCostAmt = 10M;
			TestCharge.JR_LocalSellAmt = 10M;
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(false, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(true, TestCharge.CanRecognizeProfitOnWIPsAndAccruals);
		}

		public void TestShouldReverseAccrual()
		{
			TestCharge.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;
			AssertEquals(false, TestCharge.ShouldReverseAccrual);

			TestCharge.ClearCostLink();
			TestCharge.JR_AL_APLine = Factory.New<Accrual>().PK;
			TestCharge.Accrual.AL_AC = TestObjectCreator.CC1.PK;
			TestCharge.Accrual.AL_LocalExTaxAmount = 10M;
			TestCharge.Accrual.AL_OH = TestObjectCreator.AALSHI.PK;
			TestCharge.Accrual.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestCharge.Accrual.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			TestCharge.JR_AC = TestCharge.Accrual.AL_AC;
			TestCharge.JR_LocalCostAmt = TestCharge.Accrual.AL_LocalExTaxAmount;
			TestCharge.JR_OH_CostAccount = TestCharge.Accrual.AL_OH;
			TestCharge.JR_GE = TestCharge.Accrual.AL_GE;
			TestCharge.JR_GB = TestCharge.Accrual.AL_GB;
			AssertEquals(false, TestCharge.ShouldReverseAccrual);

			TestCharge.JR_AC = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseAccrual);

			TestCharge.JR_AC = TestCharge.Accrual.AL_AC;
			TestCharge.JR_LocalCostAmt = 0M;
			AssertEquals(true, TestCharge.ShouldReverseAccrual);

			TestCharge.JR_LocalCostAmt = TestCharge.Accrual.AL_LocalExTaxAmount;
			TestCharge.JR_OH_CostAccount = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseAccrual);

			TestCharge.JR_OH_CostAccount = TestCharge.Accrual.AL_OH;
			TestCharge.JR_GE = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseAccrual);

			TestCharge.JR_GE = TestCharge.Accrual.AL_GE;
			TestCharge.JR_GB = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseAccrual);

			TestCharge.JR_GB = TestCharge.Accrual.AL_GB;
			AssertEquals(false, TestCharge.ShouldReverseAccrual);

			TestCharge.Accrual.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, TestCharge.ShouldReverseAccrual);
		}

		public void TestShouldReverseWIP()
		{
			TestCharge.JR_AL_ARLine = Factory.New<ARInvoiceLine>().PK;
			AssertEquals(false, TestCharge.ShouldReverseWIP);

			TestCharge.ClearRevenueLink();
			TestCharge.JR_AL_ARLine = Factory.New<WIP>().PK;
			TestCharge.JR_AL_CFXLine = Factory.New<ARInvoiceLine>().PK;
			TestCharge.CFXLine.AL_LineAmount = -3M;
			TestCharge.WIP.AL_AC = TestObjectCreator.CC1.PK;
			TestCharge.WIP.AL_LocalExTaxAmount = 10M;
			TestCharge.WIP.AL_OH = TestObjectCreator.ABIGAS.PK;
			TestCharge.WIP.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestCharge.WIP.AL_GB = TestObjectCreator.NonCurrentBranch.PK;
			TestCharge.JR_AC = TestCharge.WIP.AL_AC;
			TestCharge.JR_LocalSellAmt = TestCharge.WIP.AL_LocalExTaxAmount - TestCharge.CFXLine.AL_LineAmount;
			TestCharge.JR_OH_SellAccount = TestCharge.WIP.AL_OH;
			TestCharge.JR_GE = TestCharge.WIP.AL_GE;
			TestCharge.JR_GB = TestCharge.WIP.AL_GB;
			AssertEquals(false, TestCharge.ShouldReverseWIP);

			TestCharge.JR_AC = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseWIP);

			TestCharge.JR_AC = TestCharge.WIP.AL_AC;
			TestCharge.JR_LocalSellAmt = TestCharge.WIP.AL_LocalExTaxAmount;
			AssertEquals(true, TestCharge.ShouldReverseWIP);

			TestCharge.JR_LocalSellAmt = TestCharge.WIP.AL_LocalExTaxAmount - TestCharge.CFXLine.AL_LineAmount;
			TestCharge.JR_OH_SellAccount = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseWIP);

			TestCharge.JR_OH_SellAccount = TestCharge.WIP.AL_OH;
			TestCharge.JR_GE = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseWIP);

			TestCharge.JR_GE = TestCharge.WIP.AL_GE;
			TestCharge.JR_GB = ZGuid.Empty;
			AssertEquals(true, TestCharge.ShouldReverseWIP);

			TestCharge.JR_GB = TestCharge.WIP.AL_GB;
			AssertEquals(false, TestCharge.ShouldReverseWIP);

			TestCharge.WIP.AL_ReverseDate = ZDateTime.Now;  // Must Reverse before detaching, otherwise will be a developer notification
			TestCharge.JR_AL_ARLine = ZGuid.Empty;
			AssertEquals(false, TestCharge.ShouldReverseWIP);
		}

		public virtual void TestShouldReverseWIP_WithBillInInvoiceCurrencyWithLocalSellCurrency_AndExchangeRateCalculation()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_LineCFX = 5m;
			charge.JR_LocalSellAmt = 20m;
			charge.JR_RX_NKSellInvoiceCurrency = testObjectCreator.USD.RX_Code;
			Assert("Precondition: JR_LocalSellInvoiceAmt only does ExRate calculation when true", charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			charge.JR_AL_ARLine = Factory.New<WIP>().PK;
			charge.WIP.AL_AC = charge.JR_AC;
			charge.WIP.AL_OH = charge.JR_OH_SellAccount;
			charge.WIP.AL_LocalExTaxAmount = charge.JR_LocalSellInvoiceAmt;
			AssertEquals("When WIP AL_LocalExTaxAmount == JR_LocalSellInvoiceAmt, WIP should not be reversed", false, charge.ShouldReverseWIP);

			var exRate = Factory.New<ExchangeRate>();
			exRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exRate.JF_CFXMinimum = 10m;
			exRate.JF_CFXPercent = 5m;
			charge.SellInvoiceExchangeRate = new ExchangeRateWrapper(exRate, null);
			AssertEquals("Precondition: JR_LineCFX", 5m, charge.JR_LineCFX);
			AssertEquals("Precondition: JR_LocalSellInvoiceAmt adds JF_CFXMinimum", 30m, charge.JR_LocalSellInvoiceAmt);

			charge.WIP.AL_LocalExTaxAmount = charge.JR_LocalSellInvoiceAmt;
			AssertEquals("When WIP AL_LocalExTaxAmount == JR_LocalSellInvoiceAmt, WIP should not be reversed", false, charge.ShouldReverseWIP);

			charge.JR_LocalSellAmt = 30m;
			AssertEquals(30m, charge.JR_LocalSellAmt);
			AssertEquals(40m, charge.JR_LocalSellInvoiceAmt);
			AssertEquals("When WIP AL_LocalExTaxAmount != JR_LocalSellInvoiceAmt, WIP should be reversed", true, charge.ShouldReverseWIP);

			charge.JR_LocalSellAmt = 2000m;
			AssertEquals("Precondition: JR_LocalSellInvoiceAmt adds JF_CFXPercent", 2100m, charge.JR_LocalSellInvoiceAmt);
			charge.WIP.AL_LocalExTaxAmount = charge.JR_LocalSellInvoiceAmt;
			AssertEquals("When WIP AL_LocalExTaxAmount == JR_LocalSellInvoiceAmt, WIP should not be reversed", false, charge.ShouldReverseWIP);

			charge.JR_LocalSellAmt = 2100m;
			AssertEquals(2100m, charge.JR_LocalSellAmt);
			AssertEquals(2205m, charge.JR_LocalSellInvoiceAmt);
			AssertEquals("When WIP AL_LocalExTaxAmount != JR_LocalSellInvoiceAmt, WIP should be reversed", true, charge.ShouldReverseWIP);

			charge.WIP.AL_LocalExTaxAmount = charge.JR_LocalSellInvoiceAmt;
			AssertEquals("When WIP AL_LocalExTaxAmount == JR_LocalSellInvoiceAmt, WIP should not be reversed", false, charge.ShouldReverseWIP);
		}

		public void TestShouldCreateAccrualWhenCreditorIsAnOrgProxy()
		{
			TestCharge.JR_JH = Factory.NewJobForTesting<Job>().PK;
			TestCharge.InvoicingJob.Charges.Load();
			TestCharge.JR_GE_InternalDept = TestObjectCreator.FISDepartment.PK;
			TestCharge.JR_LocalCostAmt = 10M;
			TestCharge.JR_OSCostAmt = 10m;
			AssertEquals(true, TestCharge.ShouldCreateAccrual);

			TestCharge.JR_OH_CostAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			AssertEquals(true, TestCharge.ShouldCreateAccrual);

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			TestCharge.JR_GB_InternalBranch = ZGuid.Empty;
			TestCharge.JR_GE_InternalDept = ZGuid.Empty;
			TestCharge.JR_JH_InternalJob = ZGuid.Empty;
			Assert(!TestCharge.ShouldCreateCostJRJ);
			Assert(TestCharge.ShouldCreateAccrual);

			TestCharge.JR_GB_InternalBranch = TestCharge.JR_GB;
			TestCharge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
			TestCharge.JR_JH_InternalJob = TestCharge.JR_JH;
			Assert(TestCharge.ShouldCreateCostJRJ);
			Assert(!TestCharge.ShouldCreateAccrual);
		}

		public void TestShouldCreateAccrual()
		{
			AssertNull("Precondition: job should be null", TestCharge.InvoicingJob);
			AssertEquals(false, TestCharge.ShouldCreateAccrual);

			TestCharge.JR_JH = Factory.NewJobForTesting<Job>().PK;
			TestCharge.InvoicingJob.Charges.Load();
			TestCharge.JR_LocalCostAmt = 0M;
			AssertEquals(false, TestCharge.ShouldCreateAccrual);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestCharge.JR_LocalCostAmt = -10M;
			AssertEquals(true, TestCharge.ShouldCreateAccrual);
			TestCharge.JR_LocalCostAmt = 0M;
			AssertEquals(false, TestCharge.ShouldCreateAccrual);
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestCharge.JR_LocalCostAmt = -10M;
			AssertEquals(false, TestCharge.ShouldCreateAccrual);
			TestCharge.JR_LocalCostAmt = 0M;
			AssertEquals(false, TestCharge.ShouldCreateAccrual);

			TestCharge.JR_LocalCostAmt = 10M;
			AssertEquals(true, TestCharge.ShouldCreateAccrual);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestCharge.ShouldCreateAccrual);

			TestObjectCreator.CreateAccrual(TestCharge);
			AssertEquals(true, TestCharge.ShouldCreateAccrual);

			TestCharge.InvoicingJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			AssertEquals("Precondition: no Accruals should be created for the consumer type", false, fTestCharge.InvoicingJob.ConsumerTypeShouldCreateAccrual(TestCharge.JR_InvoiceType));
			AssertEquals(false, TestCharge.ShouldCreateAccrual);
		}

		public void TestShouldCreateWIPWhenCreditorIsAnOrgProxy()
		{
			TestCharge.JR_JH = Factory.NewJobForTesting<Job>().PK;
			TestCharge.InvoicingJob.Charges.Load();
			TestCharge.JR_GE = TestObjectCreator.FISDepartment.PK;
			TestCharge.JR_LocalSellAmt = 10M;
			TestCharge.JR_OSSellAmt = 10m;
			AssertEquals(true, TestCharge.ShouldCreateWIP);

			TestCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			AssertEquals(true, TestCharge.ShouldCreateWIP);

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			TestCharge.JR_GB_InternalBranch = ZGuid.Empty;
			TestCharge.JR_GE_InternalDept = ZGuid.Empty;
			TestCharge.JR_JH_InternalJob = ZGuid.Empty;
			Assert(!TestCharge.ShouldCreateSellJRJ);
			Assert(TestCharge.ShouldCreateWIP);

			TestCharge.JR_GB_InternalBranch = TestCharge.JR_GB;
			TestCharge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
			TestCharge.JR_JH_InternalJob = TestCharge.JR_JH;
			Assert(TestCharge.ShouldCreateSellJRJ);
			Assert(!TestCharge.ShouldCreateWIP);
		}

		public void TestShouldCreateWIP()
		{
			AssertNull("Precondition: job should be null", TestCharge.InvoicingJob);
			AssertEquals(false, TestCharge.ShouldCreateWIP);

			TestCharge.JR_JH = Factory.NewJobForTesting<Job>().PK;
			TestCharge.InvoicingJob.Charges.Load();
			TestCharge.JR_LocalSellAmt = 0M;
			AssertEquals(false, TestCharge.ShouldCreateWIP);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestCharge.JR_LocalSellAmt = 0M;
			AssertEquals(false, TestCharge.ShouldCreateWIP);
			TestCharge.JR_LocalSellAmt = -10M;
			AssertEquals(true, TestCharge.ShouldCreateWIP);
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestCharge.JR_LocalSellAmt = 0M;
			AssertEquals(false, TestCharge.ShouldCreateWIP);
			TestCharge.JR_LocalSellAmt = -10M;
			AssertEquals(false, TestCharge.ShouldCreateWIP);

			TestCharge.JR_LocalSellAmt = 10M;
			AssertEquals(true, TestCharge.ShouldCreateWIP);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TestCharge.ShouldCreateWIP);

			TestObjectCreator.CreateWIP(TestCharge);
			AssertEquals(true, TestCharge.ShouldCreateWIP);

			TestCharge.InvoicingJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			AssertEquals("Precondition: no WIPs should be created for the consumer type", false, TestCharge.InvoicingJob.ConsumerTypeShouldCreateWIP(TestCharge.JR_InvoiceType));
			AssertEquals(false, TestCharge.ShouldCreateWIP);
		}

		public void TestCostRecognition()
		{
			AssertNull("Precondition:", TestCharge.InvoicingJob);
			AssertEquals("CostRecognition for empty Job", "", TestCharge.CostRecognition);

			TestCharge.JR_JH = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"), false).PK;

			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, TestCharge.CostRecognition);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure, TestCharge.CostRecognition);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, TestCharge.CostRecognition);

			TestObjectCreator.CreateAccrual(TestCharge);
			TestCharge.Accrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, TestCharge.CostRecognition);

			TestCharge.Accrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, TestCharge.CostRecognition);
		}

		public void TestChargeGroupAndSubGroup()
		{
			AssertNull("Precondition:", TestCharge.InvoicingJob);
			AssertEquals("ChargeGroup for empty Job should be empty.", string.Empty, TestCharge.ChargeGroup);
			AssertEquals("ChargeCodeSubGroup for empty Job should be empty.", string.Empty, TestCharge.ChargeCodeSubGroup);

			var chargeCode1 = TestObjectCreator.CC1;
			var chargeCode2 = TestObjectCreator.CC2;

			TestCharge.JR_AC = chargeCode1.PK;
			AssertEquals(chargeCode1.AC_ChargeGroup, TestCharge.ChargeGroup);
			AssertEquals(chargeCode1.AC_ChargeSubGroup, TestCharge.ChargeCodeSubGroup);

			TestCharge.JR_AC = chargeCode2.PK;
			AssertEquals(chargeCode2.AC_ChargeGroup, TestCharge.ChargeGroup);
			AssertEquals(chargeCode2.AC_ChargeSubGroup, TestCharge.ChargeCodeSubGroup);
		}

		[TestDate(2018, 06, 06)]
		[ExpectNoExceptions()]
		public void TestReSettingAL_ReverseDateForWIPACR()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			RevenueRecognitionCollection revenueRecognitionSetup = new RevenueRecognitionCollection();
			RevenueRecognition setting = revenueRecognitionSetup.AddNew();
			setting.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, revenueRecognitionSetup);

			var shipment = TestObjectCreator.CreateShipment("S0001002", true);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "CC3 charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			var wip = TestObjectCreator.CreateWIP(job, TestObjectCreator.CC3, 1M, "CC3 charge", 100M, jobCharge);
			wip.AL_OH = TestObjectCreator.ABIGAS.PK;

			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Now.AddDays(-7));
			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals("Precondition: an Invoice should be posted.", 1, postManager.Poster.PostedInvoices.Count);

			//the following try-finally block is to forcefully set WIP reverse date to any date other than today
			//this is required to trigger developer exception AttemptToResetReverseDateWhenNotExpected later when the job status is set to closed				
			try
			{
				Factory.SetContext(BusinessContext.WipAccrualReversing);
				wip.AL_ReverseDate = ZDateTime.Now.AddDays(-1);
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.WipAccrualReversing);
			}

			Factory.Save();

			var query = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Revenue);
			var revLine = Factory.LoadTop1<TransactionLine>(query);

			AssertEquals("Reversed date not set on REV line", ZDateTime.Empty, revLine.AL_ReverseDate);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			AssertEquals("Reversed date set on REV line", ZDateTime.Today, revLine.AL_ReverseDate);
		}

		public void TestSellRecognition()
		{
			AssertNull("Precondition:", TestCharge.InvoicingJob);
			AssertEquals("SellRecognition for empty Job", "", TestCharge.SellRecognition);

			TestCharge.JR_JH = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"), false).PK;

			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, TestCharge.SellRecognition);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure, TestCharge.SellRecognition);

			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			TestCharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, TestCharge.SellRecognition);

			TestObjectCreator.CreateWIP(TestCharge);
			TestCharge.WIP.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, TestCharge.SellRecognition);

			TestCharge.WIP.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, TestCharge.SellRecognition);
		}

		public void TestIsCostRecognized()
		{
			TestCharge.JR_JH = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"), false).PK;
			AssertEquals("Precondition: ShouldCreateAccrual", false, TestCharge.ShouldCreateAccrual);
			AssertEquals(true, TestCharge.IsCostRecognized);

			TestCharge.JR_LocalCostAmt = 10M;
			AssertEquals("Precondition: ShouldCreateAccrual", true, TestCharge.ShouldCreateAccrual);
			AssertEquals(false, TestCharge.IsCostRecognized);

			TestCharge.JR_LocalCostAmt = 0M;
			var accrual = TestObjectCreator.CreateAccrual(TestCharge);
			AssertEquals(true, TestCharge.IsCostRecognized);

			GlbBranch branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;

			TestCharge.ReverseAccrual(ZDateTime.BrettsBirthday);

			TestCharge.JR_JH_InternalJob = TestCharge.JR_JH;
			TestCharge.JR_GB_InternalBranch = TestCharge.JR_GB;
			TestCharge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;

			TestCharge.JR_LocalCostAmt = 30M;
			TestCharge.JR_OSCostAmt = 30m;
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals("Precondition: ShouldCreateCostJRJ", true, TestCharge.ShouldCreateCostJRJ);
			AssertEquals(false, TestCharge.IsCostRecognized);

			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			TestCharge.JR_LocalCostAmt = 20M;
			AssertEquals(false, TestCharge.IsCostRecognized);

			var apLine = TestObjectCreator.CreateCostLine(TestCharge, Factory.New<APInvoice>().PK);
			apLine.AL_RevRecognitionType = "";
			apLine.AL_ReverseDate = ZDateTime.BrettsBirthday;
			AssertEquals("Precondition: IsCostPosted", true, TestCharge.IsCostPosted);
			AssertEquals(true, TestCharge.IsCostRecognized);

			apLine.AL_ReverseDate = ZDateTime.Empty;
			AssertEquals(false, TestCharge.IsCostRecognized);
		}

		public void TestIsSellRecognized()
		{
			TestCharge.JR_JH = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"), false).PK;
			AssertEquals("Precondition: ShouldCreateAccrual", false, TestCharge.ShouldCreateWIP);
			AssertEquals(true, TestCharge.IsSellRecognized);

			TestCharge.JR_LocalSellAmt = 10M;
			AssertEquals("Precondition: ShouldCreateAccrual", true, TestCharge.ShouldCreateWIP);
			AssertEquals(false, TestCharge.IsSellRecognized);

			TestCharge.JR_LocalSellAmt = 0M;
			var wip = TestObjectCreator.CreateWIP(TestCharge);
			AssertEquals(true, TestCharge.IsSellRecognized);

			GlbBranch branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Debtor.PK;
			TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			TestCharge.ReverseWIP(ZDateTime.BrettsBirthday);
			TestCharge.JR_JH_InternalJob = TestCharge.JR_JH;
			TestCharge.JR_GB_InternalBranch = TestCharge.JR_GB;
			TestCharge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;

			TestCharge.JR_LocalSellAmt = 30M;
			TestCharge.JR_OSSellAmt = 30m;
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals("Precondition: ShouldCreateSellJRJ", true, TestCharge.ShouldCreateSellJRJ);
			AssertEquals(false, TestCharge.IsSellRecognized);

			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			TestCharge.JR_LocalSellAmt = 20M;
			AssertEquals(false, TestCharge.IsSellRecognized);

			var arLine = TestObjectCreator.CreateRevenueLine(TestCharge, Factory.New<ARInvoice>().PK);
			arLine.AL_RevRecognitionType = "";
			arLine.AL_ReverseDate = ZDateTime.BrettsBirthday;
			AssertEquals("Precondition: IsRevenuePosted", true, TestCharge.IsRevenuePosted);
			AssertEquals(true, TestCharge.IsSellRecognized);

			arLine.AL_ReverseDate = ZDateTime.Empty;
			AssertEquals(false, TestCharge.IsSellRecognized);
		}

		public void TestSetBranch()
		{
			var organisation = TestObjectCreator.Creditor1;
			var branch1 = TestObjectCreator.CreateBranch("BR1", "Test Branch 1", GlbCompany.CurrentCompany);
			branch1.GB_OH_OrgProxy = organisation.PK;
			var branch2 = TestObjectCreator.CreateBranch("BR2", "Test Branch 2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("BR3", "Test Branch 3", GlbCompany.CurrentCompany);

			var chargeCode = TestObjectCreator.CC1;
			var anotherChargeCode = TestObjectCreator.CC2;
			var chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.All;
			chargeBranchOverride.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			chargeBranchOverride.YA_GB_SpecificBranch = branch2.PK;

			chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.Import;
			chargeBranchOverride.YA_TransportMode = Constants.TransportModes.Air;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker;

			var shipment = TestObjectCreator.CreateShipment("S001", "NZAKL", "AUSYD");
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OH_ImportBroker = organisation.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			TestCharge.JR_GB = branch3.PK;
			AssertEquals("Precondition: JR_GB", branch3.PK, TestCharge.JR_GB);
			AssertEquals("Precondition: JR_AC.IsEmpty", true, TestCharge.JR_AC.IsEmpty);
			AssertEquals("Precondition: JR_JH.IsEmpty", true, TestCharge.JR_JH.IsEmpty);

			TestCharge.JR_AC = anotherChargeCode.PK;
			AssertEquals("Branch should not be changed as job is not set", branch3.PK, TestCharge.JR_GB);

			TestCharge.JR_JH = job.PK;
			AssertEquals("Branch should be changed as the charge code doesn't have branch override setup.", branch3.PK, TestCharge.JR_GB);

			TestCharge.JR_AC = chargeCode.PK;
			AssertEquals("Branch should be set to branch defined in the charge code branch override setup.", branch1.PK, TestCharge.JR_GB);

			TestCharge.JR_GB = branch3.PK;
			TestCharge.JR_JH = ZGuid.Empty;
			AssertEquals("Precondition: JR_GB", branch3.PK, TestCharge.JR_GB);
			TestCharge.JR_JH = job.PK;
			AssertEquals("Branch should be set to branch defined in the charge code branch override setup.", branch1.PK, TestCharge.JR_GB);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			TestCharge.JR_AC = ZGuid.Empty;
			TestCharge.JR_AC = chargeCode.PK;
			AssertEquals("Branch should be set to branch defined in the charge code branch override setup with ALL transport mode.", branch2.PK, TestCharge.JR_GB);
		}

		public void TestOnFactorySaveIsNotCalledForIncompleteInvoiceChange()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Job job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 0M, null);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();

			JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_OSCostAmount = 75m;
			cost.ApportionmentCharges[0].JR_OSCostAmt = 75m;
			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals("invoice: Count of lines", 1, invoice.Lines.Count);

			invoice.SaveAsIncomplete();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var chargeInNewFactory = newFactory.Load<Charge>(charge.PK);
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			testObjectCreatorInNewFactory.CreateJobChargeRevRecognition(job, setting1.RecognitionDateOptionCode, ZDateTime.Today);
			InvoicingBase invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
			invoiceInNewFactory.RestoreSavedData();
			var apportionmentCharge = invoiceInNewFactory.ConsolCosting.ConsolCosts[0].ApportionmentCharges[0];
			var apportionmentChargeAsCharge = newFactory.Load<Charge>(apportionmentCharge.PK);
			AssertEquals("Precondition: Apportionment charge JR_AL_APLine", ZGuid.Empty, apportionmentCharge.JR_AL_APLine);
			AssertEquals("Precondition: JR_AL_APLine", ZGuid.Empty, charge.JR_AL_APLine);
			AssertEquals("Precondition: ShouldCreateAccrual", true, apportionmentChargeAsCharge.ShouldCreateAccrual);
			AssertEquals("Precondition: IsSavedByFactory", false, apportionmentChargeAsCharge.IsSavedByFactory);

			newFactory.Save();
			AssertEquals("Incomplete invoice charge JR_AL_APLine", ZGuid.Empty, apportionmentCharge.JR_AL_APLine);
			AssertEquals("Incomplete invoice charge shouldn't be saved", false, apportionmentCharge.IsInDatabase);
			AssertNull("Job charge Accrual should not be created as factory in still contains Incomplete invoice and charges should not be saved", chargeInNewFactory.Accrual);
			AssertEquals("Accrual should not be created", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines), new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK)));
		}

		public void TestChinaVATInDifferentComapnies()
		{
			var chineseCoGSTRegistered = TestObjectCreator.CreateNewCompany("CN1", Core.Constants.CountryCodes.China);
			chineseCoGSTRegistered.GC_IsGSTRegistered = true;
			var chineseCoNotGSTRegistered = TestObjectCreator.CreateNewCompany("CN2", Core.Constants.CountryCodes.China);
			chineseCoNotGSTRegistered.GC_IsGSTRegistered = false;
			var beijingBranch = TestObjectCreator.CreateNewBranch(chineseCoGSTRegistered, "BJ1");
			var beijingBranch2 = TestObjectCreator.CreateNewBranch(chineseCoGSTRegistered, "BJ2");
			var shanghaiBranch = TestObjectCreator.CreateNewBranch(chineseCoNotGSTRegistered, "SH1");
			var shanghaiBranch2 = TestObjectCreator.CreateNewBranch(chineseCoNotGSTRegistered, "SH2");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, beijingBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var bstReportTaxRate = TestObjectCreator.CreateTaxRateWithoutZZ("BSTREPORT", "Biz Tax", 0);
				bstReportTaxRate.AT_Type = AccTaxRate.Types.ReportableUnderBusinessTax;
				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S1");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var debtor = TestObjectCreator.AALSHI;
				debtor.CompanyData.SetARTaxApplicable(true);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, null, "1", TestObjectCreator.AUD, 100m, debtor);

				AssertNotEquals("Tax ID should not be empty", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals("Tax ID should be defaulted to charge code's tax id", TestObjectCreator.CC1.AC_AT_GSTRate, charge.JR_AT_SellGSTRate);
				charge.JR_AC = TestObjectCreator.CC2.PK;
				AssertNotEquals("Tax ID should not be empty after changing a charge code", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals("Tax ID should be defaulted to charge code's tax id", TestObjectCreator.CC2.AC_AT_GSTRate, charge.JR_AT_SellGSTRate);
				charge.JR_GB = beijingBranch2.PK;
				AssertNotEquals("Tax ID should not be empty after changing a branch", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals("Tax ID should be defaulted to charge code's tax id", TestObjectCreator.CC2.AC_AT_GSTRate, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Tax ID should be empty after removing a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = debtor.PK;
				AssertNotEquals("Tax ID should not be empty after re-entering a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals("Tax ID should be defaulted to charge code's tax id after re-entering a debtor", TestObjectCreator.CC2.AC_AT_GSTRate, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Tax ID should be empty after removing a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);

				debtor.CompanyData.SetARTaxApplicable(false);
				charge.JR_OH_SellAccount = debtor.PK;
				AssertEquals("Tax ID should be empty", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_AC = TestObjectCreator.CC1.PK;
				AssertEquals("Tax ID should be empty after changing a charge code", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_GB = beijingBranch2.PK;
				AssertEquals("Tax ID should be empty after changing a branch", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Tax ID should be empty after removing a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = debtor.PK;
				AssertEquals("Tax ID should be empty after re-entering a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, shanghaiBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var shipment = TestObjectCreator.CreateShipment("S1");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var debtor = TestObjectCreator.ABIGAS;
				debtor.CompanyData.SetARTaxApplicable(true);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, "desc", TestObjectCreator.AUD, 100m, null, "1", TestObjectCreator.AUD, 100m, debtor);

				AssertEquals("Tax ID should be empty", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_AC = TestObjectCreator.CC4.PK;
				AssertEquals("Tax ID should be empty after changing a charge code", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_GB = shanghaiBranch2.PK;
				AssertEquals("Tax ID should be empty after changing a branch", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Tax ID should be empty after removing a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = debtor.PK;
				AssertEquals("Tax ID should be empty after re-entering a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);

				debtor.CompanyData.SetARTaxApplicable(false);
				charge.JR_OH_SellAccount = debtor.PK;
				AssertEquals("Tax ID should be empty", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_AC = TestObjectCreator.CC3.PK;
				AssertEquals("Tax ID should be empty after changing a charge code", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_GB = beijingBranch2.PK;
				AssertEquals("Tax ID should be empty after changing a branch", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Tax ID should be empty after removing a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				charge.JR_OH_SellAccount = debtor.PK;
				AssertEquals("Tax ID should be empty after re-entering a debtor", ZGuid.Empty, charge.JR_AT_SellGSTRate);
			}
		}

		public void TestProgressiveChinaVATRolloutSystem()
		{
			TestProgressiveChinaVATRolloutSystemCore(false, false);
		}

		public void TestProgressiveChinaVATRolloutSystemWithNoBSTSetup()
		{
			TestProgressiveChinaVATRolloutSystemCore(false, true);
		}

		public void TestProgressiveChinaVATRolloutSystemUsingTaxOverrides()
		{
			TestProgressiveChinaVATRolloutSystemCore(true, false);
		}

		public void TestUpdateOsSellGSTExtraTaxWHTAmountWithLargeNumber()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AT_SellGSTRate = ZGuid.NewZGuid();
			charge.JR_AW_SellWHTRate = ZGuid.NewZGuid();
			var taxRate = Factory.NewWithPrimaryKey<AccTaxRate>(charge.JR_AT_SellGSTRate.ToGuid());
			var accWithholding = Factory.NewWithPrimaryKey<AccWithholding>(charge.JR_AW_SellWHTRate.ToGuid());
			taxRate.SetRateNumerator_ForTestOnly(100);
			accWithholding.AW_Rate = 100;
			charge.JR_OSSellAmt = decimal.MaxValue / 10;
			charge.SellGSTRate.SetRateNumerator_ForTestOnly(100);
			AssertNoExceptionThrown(() => charge.UpdateOsSellWHTAmount());

			charge.JR_AT_CostGSTRate = ZGuid.NewZGuid();
			charge.JR_AW_CostWHTRate = ZGuid.NewZGuid();
			var costRate = Factory.NewWithPrimaryKey<AccTaxRate>(charge.JR_AT_CostGSTRate.ToGuid());
			var costWithholding = Factory.NewWithPrimaryKey<AccWithholding>(charge.JR_AW_CostWHTRate.ToGuid());
			costRate.SetRateNumerator_ForTestOnly(100);
			costWithholding.AW_Rate = 100;
			charge.JR_OSCostAmt = decimal.MaxValue / 10;
			charge.CostWHTRate.AW_Rate = 100;
			AssertNoExceptionThrown(() => charge.UpdateOsCostGSTAmount());
		}

		public void TestCostTaxDateInUpdateCostGSTWHTDetails()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("Test", false, false);
			var consol = TestObjectCreator.CreateConsol();
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			consolCost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;

			TestCharge.PreventReadOnlyFromChangingValues_ForTestOnly = true;
			TestCharge.JR_E6 = consolCost.PK;
			TestCharge.JR_OH_CostAccount = orgHeader1.PK;
			AssertEquals(consolCost.E6_TaxDate, TestCharge.JR_CostTaxDate);

			consolCost.SetTaxDateSafe(ZDate.BrettsBirthday);
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("Test2", false, false);
			TestCharge.JR_OH_CostAccount = orgHeader2.PK;
			AssertEquals(consolCost.E6_TaxDate, TestCharge.JR_CostTaxDate);
		}

		Charge CreateChargeForTestProgressiveChinaVATRollout(ZGuid chargeCodePK)
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			TestObjectCreator creator2 = new TestObjectCreator(factory2);

			var localClient1 = factory2.NewWithValidTestData<OrgHeader>();
			var testJob = creator2.CreateJob(localClient1, 0, TestObjectCreator.Agent, 0);
			Charge charge = testJob.Charges.AddNew();
			var agentCreditor = factory2.NewWithValidTestData<OrgHeader>();
			agentCreditor.OH_IsCreditor = true;
			agentCreditor.CompanyData.SetAPTaxApplicable(true);
			var agentDebtor = factory2.NewWithValidTestData<OrgHeader>();
			agentDebtor.OH_IsDebtor = true;
			agentDebtor.CompanyData.SetARTaxApplicable(true);
			charge.JR_OH_CostAccount = agentCreditor.PK;
			charge.JR_OH_SellAccount = agentDebtor.PK;
			charge.JR_AC = chargeCodePK;
			return charge;
		}

		void TestProgressiveChinaVATRolloutSystemCore(bool useTaxOverrides, bool noBstSetupTest)
		{
			// Arrange
			var chineseCo = TestObjectCreator.CreateNewCompany("CN", Core.Constants.CountryCodes.China);
			chineseCo.GC_IsGSTRegistered = true;
			var beijingBranch = TestObjectCreator.CreateNewBranch(chineseCo, "ZBJ");
			var shanghaiBranch = TestObjectCreator.CreateNewBranch(chineseCo, "ZSH");
			var beijingBranchOrg = TestObjectCreator.CreateOrgHeader("CHINABEJ", false, false);
			var shanghaiBranchOrg = TestObjectCreator.CreateOrgHeader("CHINASHA", false, false);
			var nonChineseCo = TestObjectCreator.CreateNewCompany("AU", Core.Constants.CountryCodes.Australia);
			var nonChineseBranch = TestObjectCreator.CreateNewBranch(nonChineseCo, "ZBR");
			var nonChineseBranchOrg = TestObjectCreator.CreateOrgHeader("AUSBRIS", false, false);
			Factory.Save();
			var taxRate = TestObjectCreator.CreateTaxRate("VAT", "Chinese VAT", 10);
			var chineseChargeCode = TestObjectCreator.CreateChargeCode("XYZ", "XYZ", Constants.ChargeType.Margin, 100, taxRate, null, chineseCo);
			var nonChineseChargeCode = TestObjectCreator.CreateChargeCode("XYZ", "XYZ", Constants.ChargeType.Margin, 100, taxRate, null, nonChineseCo);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, beijingBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var bstReportTaxRate = TestObjectCreator.CreateTaxRateWithoutZZ("BSTREPORT", "Biz Tax", 0);

				if (!noBstSetupTest)
				{
					bstReportTaxRate.AT_Type = AccTaxRate.Types.ReportableUnderBusinessTax;
				}
				else
				{
					bstReportTaxRate.AT_Type = AccTaxRate.Types.Rated;
					bstReportTaxRate.AT_Code = "123";
				}

				beijingBranch.GB_OH_OrgProxy = beijingBranchOrg.PK;
				shanghaiBranch.GB_OH_OrgProxy = shanghaiBranchOrg.PK;
				nonChineseBranch.GB_OH_OrgProxy = nonChineseBranchOrg.PK;

				if (useTaxOverrides)
				{
					var taxOverrideRate = TestObjectCreator.CreateTaxRate("OVR", "Override", "RAT", 10, "", 0, 1);
					TestObjectCreator.CreateTaxOverride(chineseChargeCode, taxOverrideRate.PK);
					TestObjectCreator.CreateTaxOverride(nonChineseChargeCode, taxOverrideRate.PK);
				}

				TestObjectCreator.SetCustomsCodeForOrgHeader(shanghaiBranchOrg, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China, "111");
				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				TestObjectCreator creator2 = new TestObjectCreator(factory2);

				// Charge created in chinese branch
				var charge = CreateChargeForTestProgressiveChinaVATRollout(chineseChargeCode.PK);

				// Asserts
				charge.JR_GB = beijingBranch.PK;
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.SellGSTRate.AT_Code);
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.CostGSTRate.AT_Code);
				charge.JR_GB = shanghaiBranch.PK;
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.SellGSTRate.AT_Code);
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.CostGSTRate.AT_Code);
			}

			// Charge created in non chinese branch
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, nonChineseBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var charge = CreateChargeForTestProgressiveChinaVATRollout(nonChineseChargeCode.PK);

				// Asserts
				charge.JR_GB = beijingBranch.PK;
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.SellGSTRate.AT_Code);
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.CostGSTRate.AT_Code);
				charge.JR_GB = shanghaiBranch.PK;
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.SellGSTRate.AT_Code);
				AssertEquals(useTaxOverrides ? "ZZOVR" : "ZZVAT", charge.CostGSTRate.AT_Code);
			}
		}

		[SuspendCriticalValidation]
		public void TestAttributesAreReloadedOnDeletingCharge()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "KRSEL", "C00001213");

			var shipment = consol.Shipments.AddNew();
			var job = creator.CreateJob(shipment, false, false);
			var apportionments = new ApportionmentListing(Factory, consol);
			var jobConsolCost = apportionments.CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = creator.CC1.PK;
			jobConsolCost.E6_OSCostAmount = 100m;
			AssertEquals("Job-consol-cost must have 1 apportionmentCharge", 1, jobConsolCost.ApportionmentCharges.Count);
			var apportionmentCharge = jobConsolCost.ApportionmentCharges[0];

			job.Charges.Load();
			AssertEquals("Job must have 1 charge", 1, job.Charges.Count);
			var jobCharge = job.Charges[0];
			AssertEquals("apportionmentCharge equals to jobCharge", apportionmentCharge.PK, jobCharge.PK);

			AssertEquals("apportionmentCharge must have 0 attribute", 0, apportionmentCharge.JobChargeAttributes.Count);
			AssertEquals("jobCharge must have 0 attribute", 0, jobCharge.JobChargeAttributes.Count);

			var jobChargeAttribute = jobCharge.JobChargeAttributes.AddNew();
			jobChargeAttribute.EC_Name = "abc";
			jobChargeAttribute.EC_Value = "def";

			AssertEquals("apportionmentCharge must still have 0 attribute", 0, apportionmentCharge.JobChargeAttributes.Count);
			AssertEquals("jobCharge must have 1 attribute", 1, jobCharge.JobChargeAttributes.Count);

			// Previously, deleting apportionmentCharge would not delete JobChargeAttributes created by same datarow charge jobCharge
			jobConsolCost.ApportionmentCharges[0].Delete();

			Factory.Save();
		}

		public void TestGetMarginAmountDownWhenAmountIsTooLarge()
		{
			ZDecimal amountMaxValue = decimal.MaxValue;
			TestCharge.JR_MarginPercentage = 10.5m;
			TestCharge.JR_ChargeType = "1";
			AssertNoExceptionThrown(() => TestCharge.GetMarginAmountDown(amountMaxValue, TestCharge.JR_SellCurrency));
		}

		public void TestJR_Cost_LocalGSTAmountHighPrecision()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.SetCurrentCompanyReciprocal(false);
			TestCharge.JR_RX_NKCostCurrency = "USD";
			TestCharge.JR_OSCostExRate = 17.75;
			using (TestCharge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender())
			{
				TestCharge.JR_IsCostTaxAmountOverridden = true;
				TestCharge.JR_OSCostGSTAmt_Calc = 1328.71m;
			}
			var localGSTAmountHighPrecision = TestCharge.JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly.Round(5);
			AssertEquals(74.8569M, localGSTAmountHighPrecision);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			var rate1 = Factory.NewWithValidTestData<AccTaxRate>();
			rate1.AT_Code = "TAX1";
			rate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			rate1.SetRate_ForTestOnly(1247, 100);

			TestCharge.JR_IsCostTaxAmountOverridden = false;
			TestCharge.JR_RX_NKCostCurrency = "USD";
			TestCharge.JR_OSCostExRate = 17.75;
			TestCharge.JR_LocalCostAmt = 147.67m;
			TestCharge.JR_AT_CostGSTRate = rate1.PK;
			localGSTAmountHighPrecision = TestCharge.JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly.Round(5);
			AssertEquals(18.41445M, localGSTAmountHighPrecision);
		}

		[TestDate(2015, 5, 1)]
		public void TestJR_Calc_ARInvoiceDate()
		{
			TestObjectCreator.AALSHI.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			AssertEquals(ZDateTime.Now.Date, charge.JR_Calc_ARInvoiceDate.Date);
		}

		public void TestGatewayBillingWithGeneratedGatewaySellCanBeSaved()
		{
			using (var job = TestObjectCreator.SetupGatewayLegacyJobAndEnableJRJ())
			{
				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();
				var consol = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001990", receivingGatewayCompany: GlbCompany.CurrentCompany);

				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = "JK";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_ActualChargeable = 500;
				shipment1.JS_ActualWeight = 500;
				shipment1.JS_RL_NKDestination = "SGSIN";
				shipment1.JS_RL_NKOrigin = "AUSYD";

				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_OSSellAmt = 100m;
				charge1.JR_JH = job.PK;
				charge1.JR_JH_InternalJob = job.PK;
				charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				charge1.JR_GB_InternalBranch = charge1.JR_GB;
				charge1.JR_GE_InternalDept = charge1.JR_GE;

				Assert("Precondition", consol.IsGateway());

				GatewaySellToCostSynchroniser.Synchronise(job);
				var appListing = consol.GetApportionments(true);
				AssertEquals("Precondition: One sell apportionment is made", 1, appListing.CostsCollection.Count);
				var consolCost = appListing.CostsCollection[0];
				AssertEquals("One apportionment charge is made", 1, consolCost.ApportionmentCharges.Count);
				var appCharge1 = consolCost.ApportionmentCharges[0];
				AssertEquals("Apportionment charge Os Cost Amt", 100m, appCharge1.JR_OSCostAmt);
				AssertEquals("Apportionment charge Os Sell Amt", 0m, appCharge1.JR_OSSellAmt);

				AssertNoExceptionThrown("Charges are created and attached to the job, saving these should not result in any critical save validation errors", () => Factory.Save());

				var appListingAfter = consol.GetApportionments(true);
				AssertEquals("Postcondition: One sell apportionment remains", 1, appListingAfter.CostsCollection.Count);
				var consolCostAfter = appListingAfter.CostsCollection[0];
				AssertEquals("One apportionment charge remains", 1, consolCostAfter.ApportionmentCharges.Count);
				var appChargeAfter = consolCostAfter.ApportionmentCharges[0];
				AssertEquals("Apportionment charge OS Cost Amt", 100m, appChargeAfter.JR_OSCostAmt);
				AssertEquals("Apportionment charge OS Sell Amt", 100m, appChargeAfter.JR_OSSellAmt);
				Assert("Postcondition: apportioned charge has Cost line", appChargeAfter.IsCostPosted);
				Assert("Postcondition: apportioned charge does not have Revenue line", !appChargeAfter.IsRevenuePosted);
				AssertEquals("Apportionment charge is posted with JRJ", "JRJ", appChargeAfter.APLine.TransactionHeader.AH_TransactionType);

				AssertEquals("One charge after saving as we don't add additional charges to the GTW Billing tab anymore", 1, job.Charges.Count);
				AssertCollectionContains("Contains original charge", job.Charges, x => x.PK == charge1.PK);
				AssertEquals("OS Sell Amt is NOT reset", 100m, charge1.JR_OSSellAmt);
				AssertEquals("Local Sell Amt is NOT reset", 100m, charge1.JR_LocalSellAmt);
				Assert("Original charge is not Cost posted", !charge1.IsCostPosted);
				Assert("Original charge IS Revenue posted", charge1.IsRevenuePosted);
				Assert("Original charge IS Revenue posted with JRJ", charge1.IsRevenuePostedWithAutoJobRevenueJournal);
			}
		}

		public void TestJR_LocalSellInvoiceAmt()
		{
			TestObjectCreator.AALSHI.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellExRate = 0.8m;
			//charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.8m);
			charge.JR_LocalSellAmt = 100m;

			AssertEquals(100m, charge.JR_LocalSellInvoiceAmt);

			charge.JR_RX_NKSellCurrency = testObjectCreator.AUD.RX_Code;
			AssertEquals(100m, charge.JR_LocalSellInvoiceAmt);

			charge.JR_LocalSellInvoiceAmt_ForTestOnly = 200m;
			AssertEquals("No developer error should be reported", 0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_RX_NKSellInvoiceCurrency = testObjectCreator.USD.RX_Code;
			Assert(charge.BillInInvoiceCurrencyWithLocalSellCurrency);
			charge.JR_LineCFX = 5m;
			AssertEquals(210m, charge.JR_LocalSellInvoiceAmt);

			charge.JR_LocalSellInvoiceAmt_ForTestOnly = 220m;
			AssertEquals(210m, charge.JR_LocalSellInvoiceAmt);
			AssertEquals("A developer error should be reported", 1, ExceptionReporterTestListener.Instance.Count);
			AssertStartsWith("Should say about setting JR_LocalSellInvoiceAmt", "Should not be setting JR_LocalSellInvoiceAmt property", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();

			var exRate1 = Factory.New<ExchangeRate>();
			exRate1.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exRate1.JF_BaseRate = 1m;
			exRate1.JF_CFXMinimum = 10m;
			exRate1.JF_CFXPercent = 5m;

			var charge2 = (BaseCharge)GetNewBusinessObject();
			charge2.JR_JH = job.PK;
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge2.JR_OSSellExRate = 1m;
			charge2.JR_LocalSellAmt = -100m;
			charge2.JR_LineCFX = 5m;

			charge2.SellInvoiceExchangeRate = new ExchangeRateWrapper(exRate1, null);
			AssertEquals(-110m, charge2.JR_LocalSellInvoiceAmt);
			AssertEquals("A developer error should not be reported", 0, ExceptionReporterTestListener.Instance.Count);

			var exRate2 = Factory.New<ExchangeRate>();
			exRate2.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exRate2.JF_BaseRate = 1m;
			exRate2.JF_CFXMinimum = 5m;
			exRate2.JF_CFXPercent = 10m;

			var charge3 = (BaseCharge)GetNewBusinessObject();
			charge3.JR_JH = job.PK;
			charge3.JR_AC = TestObjectCreator.CC1.PK;
			charge3.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge3.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge3.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge3.JR_OSSellExRate = 1m;
			charge3.JR_LocalSellAmt = -100m;
			charge3.JR_LineCFX = 10m;

			charge3.SellInvoiceExchangeRate = new ExchangeRateWrapper(exRate2, null);
			AssertEquals(-110m, charge3.JR_LocalSellInvoiceAmt);
			AssertEquals("A developer error should not be reported", 0, ExceptionReporterTestListener.Instance.Count);

			var exRate3 = Factory.New<ExchangeRate>();
			exRate3.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			exRate3.JF_BaseRate = 1m;
			exRate3.JF_CFXMinimum = 5m;
			exRate3.JF_CFXPercent = 0m;

			var charge4 = (BaseCharge)GetNewBusinessObject();
			charge4.JR_JH = job.PK;
			charge4.JR_AC = TestObjectCreator.CC1.PK;
			charge4.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge4.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge4.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			charge4.JR_OSSellExRate = 1m;
			charge4.JR_LocalSellAmt = -100m;
			charge.JR_LineCFX = 0m;

			charge4.SellInvoiceExchangeRate = new ExchangeRateWrapper(exRate3, null);
			AssertEquals(-105m, charge4.JR_LocalSellInvoiceAmt);
			AssertEquals("A developer error should not be reported", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestJR_LocalSellInvoiceAmt_WithDifferentLocalCompanyDecimals()
		{
			TestObjectCreator.AALSHI.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var oneDPCurrency = Factory.NewWithValidTestData<RefCurrency>();
			oneDPCurrency.RX_SubUnitRatio = 10;

			var debtorOrg = TestObjectCreator.CreateOrgHeader("ABCPROXY", true, true);
			var debtorCompany = TestObjectCreator.CreateNewCompany("ABC");
			debtorCompany.GC_RX_NKLocalCurrency = "TWD";
			debtorCompany.GC_Name = "GC_Name";
			debtorCompany.GC_OH_OrgProxy = debtorOrg.PK;
			var debtorBranch = TestObjectCreator.CreateNewBranch(debtorCompany, "BR1");
			AssertEquals("Precondition: local decimals of new company is zero", 0, debtorCompany.LocalCurrency.Decimals);
			AssertEquals("Precondition: local decimals of current company is two", 2, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			AssertEquals("Precondition: decimals is 1 for test currency", 1, oneDPCurrency.Decimals);

			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = debtorOrg.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			charge.JR_LineCFX = 2M;
			charge.JR_OSSellAmt = 196.0M;
			charge.JR_LocalSellAmt = 196.0M;

			var exchangeRate = charge.InvoicingJob.ExchangeRates[0] ?? charge.InvoicingJob.ExchangeRates.AddNew();
			exchangeRate.JF_BaseRate = 1.23555m;
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_CFXPercent = 2M;

			Factory.Save();
			AssertEquals("JR_LocalSellInvoiceAmt: Exchange rate calculation rounds to 2 decimal places due to default company", 199.92m, charge.JR_LocalSellInvoiceAmt);

			SetChargeCompanyAndCurrency(charge, debtorBranch);
			AssertEquals("JR_LocalSellInvoiceAmt: Exchange rate calculation rounds to 0 decimal places due to TWD company", 200m, charge.JR_LocalSellInvoiceAmt);

			debtorCompany.GC_RX_NKLocalCurrency = oneDPCurrency.RX_Code;
			charge.JR_RX_NKSellCurrency = oneDPCurrency.RX_Code;
			AssertEquals("JR_LocalSellInvoiceAmt: Exchange rate calculation rounds to 1 decimal places due to change of currency", 199.9m, charge.JR_LocalSellInvoiceAmt);

			SetChargeCompanyAndCurrency(charge, GlbBranch.CurrentBranch);
			AssertEquals("JR_LocalSellInvoiceAmt: Exchange rate calculation rounds to 2 decimal places due to default company", 199.92m, charge.JR_LocalSellInvoiceAmt);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, debtorBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var chargeLoadedInAnotherCompany = (BaseCharge)newFactory.Load(GetExpectedBusinessObjectType(), charge.PK);

				AssertEquals("Precondition: debtor company currency is 1 decimal place.", 1, debtorCompany.LocalCurrency.Decimals);
				AssertEquals("JR_LocalSellInvoiceAmt: Exchange rate calculation rounds to 2 decimal places due to default company", 199.92m, chargeLoadedInAnotherCompany.JR_LocalSellInvoiceAmt);

				SetChargeCompanyAndCurrencyNull(chargeLoadedInAnotherCompany);
				AssertEquals("JR_LocalSellInvoiceAmt: No exchange rate calculation is performed when Charge.Company is null because IsSellLocal is false", 196.0m, chargeLoadedInAnotherCompany.JR_LocalSellInvoiceAmt);
			}

			void SetChargeCompanyAndCurrency(BaseCharge c, GlbBranch branch)
			{
				c.JR_GB = branch.PK;
				c.JR_GC = branch.Company.PK;
				c.JR_RX_NKSellCurrency = branch.Company.GC_RX_NKLocalCurrency;
				c.JR_RX_NKSellInvoiceCurrency = "USD";
				c.JR_LineCFX = 2M;
			}
			void SetChargeCompanyAndCurrencyNull(BaseCharge c)
			{
				c.JR_GB = ZGuid.Empty;
				c.JR_GC = ZGuid.Empty;
				c.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				c.JR_RX_NKSellInvoiceCurrency = "USD";
				c.JR_LineCFX = 2M;
			}
		}

		#region Performance Testing

		public void TestJR_AW_SellWHTRateDoesNotRecalculteJR_AW_SellWHTAmtIfNewValueIsSameAsCurrentValue()
		{
			BaseCharge charge = CreateBaseCharge();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_OSSellAmt = 200m;
			charge.JR_LocalSellAmt = 200m;
			charge.JR_OSSellExRate = 1m;
			Factory.Save();

			AssertEquals(0m, charge.JR_OSSellWHTAmt);
			charge.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			AssertEquals("JR_AW_SellWHTAmt should be calculated", 10m, charge.JR_OSSellWHTAmt);

			charge.JR_OSSellWHTAmt = 0m;
			charge.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			AssertEquals("JR_AW_SellWHTAmt should not be calculated because current JR_AW_SellWHTRate is same as new value"
				, 0m, charge.JR_OSSellWHTAmt);

			charge.JR_AW_SellWHTRate = TestObjectCreator.CreateOrLoadWithholdingTax("WHT2", "WHT Rate 2", 4m).PK;
			AssertEquals("JR_AW_SellWHTAmt should be calculated because now we are setting a different value",
				8m, charge.JR_OSSellWHTAmt);
		}

		public void TestJR_RX_NKCostCurrencyDoesNotRunValidationIfNewValueIsSameAsCurrentValue()
		{
			var expectedErrorMessage = "You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission. You must reset the value to its previous value AUD";

			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var charge = CreateBaseCharge();
				Factory.Save();

				var securityFactory = new BusinessObjectFactory();

				var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

				var loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;

				var invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;

				loginSecurity.GU_SecurityItemIsAllowed = false;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				using (Env.SetTemporaryUserContext(testUser.GS_LoginName, TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
				{
					charge.JR_RX_NKCostCurrencyInfo.ClearAllNotifications();
					charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
					AssertHasError("JR_RX_NKCostCurrency should be validated", charge.JR_RX_NKCostCurrencyInfo,
						expectedErrorMessage);

					charge.JR_RX_NKCostCurrencyInfo.ClearAllNotifications();
					charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
					AssertNoError(@"JR_RX_NKCostCurrency should not be validated because curent JR_RX_NKCostCurrency
								is same as new value", charge.JR_RX_NKCostCurrencyInfo, expectedErrorMessage);

					charge.JR_RX_NKCostCurrencyInfo.ClearAllNotifications();
					charge.JR_RX_NKCostCurrency = TestObjectCreator.EUR.RX_Code;
					AssertHasError("JR_RX_NKCostCurrency should be validated because now we are setting a different value",
						charge.JR_RX_NKCostCurrencyInfo, expectedErrorMessage);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestLocalWHTId_DBHitsWhenChargeCodeIsNullOrCompanyIsNotWHTRegistered()
		{
			AssertDBHitsForLocalWHTIdOrCostWHTId(x => x.LocalWHTId_ForTestOnly);
		}

		public void TestCostWHTId_DBHitsWhenChargeCodeIsNullOrCompanyIsNotWHTRegistered()
		{
			AssertDBHitsForLocalWHTIdOrCostWHTId(x => x.CostWHTId_ForTestOnly);
		}

		void AssertDBHitsForLocalWHTIdOrCostWHTId(Func<BaseCharge, ZGuid> getLocalWHTIdOrCostWHTId)
		{
			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR";
			creditor.OH_IsCreditor = true;

			var debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTOR";
			debtor.OH_IsCreditor = true;
			var sellAccountMiscServ = debtor.MiscServ; // init Sell Account MiscServ

			var charge = CreateBaseCharge();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_OSSellAmt = 200m;
			charge.JR_LocalSellAmt = 200m;
			charge.JR_OSSellExRate = 1m;
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_OH_SellAccount = debtor.PK;

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var chargeInNewFactory = newFactory.Load<BaseCharge>(charge.PK);

			var dbHitCountBefore = newFactory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);
			var wHTId = getLocalWHTIdOrCostWHTId(chargeInNewFactory);
			var dbHitCountAfter = newFactory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);

			AssertEquals(0, dbHitCountAfter - dbHitCountBefore);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false; // to avoid hitting OrgMisvServ when setting JR_AC
			chargeInNewFactory.JR_AC = ZGuid.Empty;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			dbHitCountBefore = newFactory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);
			wHTId = getLocalWHTIdOrCostWHTId(chargeInNewFactory);
			dbHitCountAfter = newFactory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);

			AssertEquals(0, dbHitCountAfter - dbHitCountBefore);

			chargeInNewFactory.JR_AC = TestObjectCreator.CC1.PK;

			dbHitCountBefore = newFactory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);
			wHTId = getLocalWHTIdOrCostWHTId(chargeInNewFactory);
			dbHitCountAfter = newFactory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);

			AssertEquals(1, dbHitCountAfter - dbHitCountBefore);
		}

		BaseCharge CreateBaseCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();
			return charge;
		}
		#endregion

		public void TestDoNotShowUserContextMessageDuringTransaction()
		{
			using (Globals.SetIsUnitTestingProductionFunctionality())
			{
				UserContext.ForceToNotSkipForTest = true;

				var jobConsolCost = CreateConsolCostForNonCurrentCompany();

				var appBFactory = new BusinessObjectFactory();
				appBFactory.RefreshEnabled = false;
				var user = appBFactory.Load<GlbStaff>(Env.CurrentUser.PK);
				var newName = Env.CurrentUser.LoginName + "1";
				user.GS_LoginName = newName;
				appBFactory.Save();

				var newAppAFactory = new BusinessObjectFactory();
				newAppAFactory.RefreshEnabled = false;
				user = newAppAFactory.Load<GlbStaff>(Env.CurrentUser.PK);
				AssertEquals(newName, user.GS_LoginName);

				var jobConsolCostInNewFactory = newAppAFactory.Load<JobConsolCost>(jobConsolCost.PK);
				var apportionmentCharge = jobConsolCostInNewFactory.ApportionmentCharges[0];
				apportionmentCharge.JR_IsUsedForApportionment = true;
				jobConsolCostInNewFactory.E6_OSCostAmount = 150m;
				jobConsolCostInNewFactory.E6_LocalCostAmount = 150m;
				AssertEquals(jobConsolCostInNewFactory.E6_OSCostAmount, apportionmentCharge.JR_OSCostAmt);

				var ex = AssertExceptionThrown<ZCannotSaveException>(() => newAppAFactory.Save());
				var expectedMsg = string.Format(@$"Could not find login name ""{Env.CurrentUser.LoginName}"" in the database (Perhaps it has been changed or deleted). Some operations may not function properly. Please restart {BrandingFactory.Instance.ProductName} to fix this. If this error persists, please contact your system administrator.");
				AssertEquals(expectedMsg, ex.Message);
			}

			JobConsolCost CreateConsolCostForNonCurrentCompany()
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var consol = TestObjectCreator.CreateConsol("AUSYD", "KRSEL", "C00001213");
					var shipment = consol.Shipments.AddNew();

					var job = TestObjectCreator.CreateJob(shipment, false, false);
					job.JH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;

					var apportionments = new ApportionmentListing(Factory, consol);
					var jobConsolCost = apportionments.CostsCollection.TryAddNew();
					jobConsolCost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
					jobConsolCost.E6_OSCostAmount = 100m;
					AssertEquals(1, jobConsolCost.ApportionmentCharges.Count);
					Factory.Save();

					return jobConsolCost;
				}
			}
		}

		#region Exchange Rates

		public void TestCostExchangeRate()
		{
			TestExchangeRate((c, e) => c.CostExchangeRate = e, (c) => c.JR_AL_APLine = Factory.NewWithValidTestData<APInvoiceLine>().PK, c => c.OnCostExchangeRateChangedCalledCount);
		}

		public void TestRevenueExchangeRate()
		{
			TestExchangeRate((c, e) => c.RevenueExchangeRate = e, (c) => c.JR_AL_ARLine = Factory.NewWithValidTestData<ARInvoiceLine>().PK, c => c.OnRevenueExchangeRateChangedCalledCount);
		}

		public void TestSellInvoiceExchangeRate()
		{
			TestExchangeRate((c, e) => c.SellInvoiceExchangeRate = e, (c) => c.JR_AL_ARLine = Factory.NewWithValidTestData<ARInvoiceLine>().PK, c => c.OnSellInvoiceExchangeRateChangedCalledCount);
		}

		void TestExchangeRate(Action<BaseCharge, IExchangeRateJobBilling> setExchangeRate, Action<BaseCharge> setPostedLine, Func<TestingBaseCharge, int> getCallsCount)
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = Factory.New<TestingBaseCharge>();

			var exRate = Factory.New<ExchangeRate>();
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.JF_JH = job.PK;

			setExchangeRate(charge, new ExchangeRateWrapper(exRate, null));

			AssertEquals(1, getCallsCount(charge));
			exRate.JF_BaseRate = 0.45m;

			AssertEquals(2, getCallsCount(charge));

			setExchangeRate(charge, new ExchangeRateWrapper(exRate, null));

			AssertEquals("Charge should not be notified of exchange rate change if the same rate re-assigned", 2, getCallsCount(charge));

			charge.Delete();

			Assert(exRate.IsDeleted);

			var charge2 = Factory.New<TestingBaseCharge>();

			var exRate2 = Factory.New<ExchangeRate>();
			exRate2.JF_RX_NKRateCurrency = "EUR";
			exRate2.JF_JH = job.PK;

			setExchangeRate(charge2, new ExchangeRateWrapper(exRate2, null));
			AssertEquals("Even action should be called", 1, getCallsCount(charge2));

			setPostedLine(charge2);

			var exRate3 = Factory.New<ExchangeRate>();
			exRate3.JF_RX_NKRateCurrency = "CAD";
			exRate3.JF_JH = job.PK;

			setExchangeRate(charge2, new ExchangeRateWrapper(exRate3, null));
			AssertEquals("Not doing even action as Charge is posted", 1, getCallsCount(charge2));
		}

		#endregion

		public void TestAssigningRevenueLineMakesRevExRatePersistentExceptJRJ()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var exRate = Factory.New<ExchangeRate>();
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.JF_BaseRate = 0.8m;

			charge.RevenueExchangeRate = new ExchangeRateWrapper(exRate, null);
			Assert(!exRate.JF_IsTransformed);
			AssertEquals("Event call count on initialising", 1, charge.OnRevenueExchangeRateChangedCalledCount);

			var line = Factory.New<ARInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.WIP;
			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Event call count not increased after setting WIP Line", 1, charge.OnRevenueExchangeRateChangedCalledCount);

			Assert(!exRate.JF_IsTransformed);

			line.AL_ReverseDate = ZDateTime.Now;

			line = Factory.New<ARInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Event call count not increased as Revenue is posted", 1, charge.OnRevenueExchangeRateChangedCalledCount);
			Assert(exRate.JF_IsTransformed);

			charge.ClearRevenueLinkOnlyTemporary();
			exRate.JF_IsTransformed = false;

			var tranHdr = Factory.New<AccTransactionHeader>();
			tranHdr.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			line = Factory.New<ARInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = tranHdr.PK;
			charge.JR_AL_ARLine = line.PK;
			AssertEquals("Event call count not increased as Revenue is posted", 1, charge.OnRevenueExchangeRateChangedCalledCount);

			Assert("Posting of JRJ should not make revenue exchange rate persistent", !exRate.JF_IsTransformed);
		}

		public void TestAssigningCostLineMakesCostExRatePersistent()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var exRate = Factory.New<ExchangeRate>();
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.JF_BaseRate = 0.8m;

			charge.CostExchangeRate = new ExchangeRateWrapper(exRate, null);
			AssertEquals("Event call count on initialising", 1, charge.OnCostExchangeRateChangedCalledCount);
			Assert(!exRate.JF_IsTransformed);

			var line = Factory.New<APInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Accrual;
			charge.JR_AL_APLine = line.PK;
			AssertEquals("Event call count not increased after setting ACR Line", 1, charge.OnCostExchangeRateChangedCalledCount);
			Assert(!exRate.JF_IsTransformed);

			line.AL_ReverseDate = ZDateTime.Now;

			line = Factory.New<APInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Cost;
			charge.JR_AL_APLine = line.PK;
			AssertEquals("Event call count not increased as Revenue is posted", 1, charge.OnCostExchangeRateChangedCalledCount);
			Assert(exRate.JF_IsTransformed);
		}

		public void TestChangingCostAccountUpdatesExRate()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			charge.JR_JH = job.PK;
			charge.JR_RX_NKCostCurrency = "USD";

			Assert("BaseCharge does not contain functionality to update rates on currency change", !job.ExchangeRates.Any());

			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			Assert(job.ExchangeRates.Any());

			var rate = job.ExchangeRates.Cast<ExchangeRate>().First();

			AssertEquals("USD", rate.JF_RX_NKRateCurrency);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor, rate.OrgType);
			AssertEquals(TestObjectCreator.AALSHI.PK, rate.JF_OH_Org);

			AssertNotNull(charge.CostExchangeRate);
			AssertEquals("USD", charge.CostExchangeRate.CurrencyCode);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor, charge.CostExchangeRate.OrgType);
			AssertEquals(TestObjectCreator.AALSHI.PK, charge.CostExchangeRate.OrgPk);
		}

		public void TestChangingSellAccountUpdatesExRate()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			charge.JR_JH = job.PK;
			charge.JR_RX_NKSellCurrency = "USD";

			Assert("BaseCharge does not contain functionality to update rates on currency change", !job.ExchangeRates.Any());

			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			Assert(job.ExchangeRates.Any());

			var rate = job.ExchangeRates.Cast<ExchangeRate>().First();

			AssertEquals("USD", rate.JF_RX_NKRateCurrency);
			AssertEquals(ExchangeRateOrgTypeEnum.Debtor, rate.OrgType);
			AssertEquals(TestObjectCreator.AALSHI.PK, rate.JF_OH_Org);

			AssertNotNull(charge.RevenueExchangeRate);
			AssertEquals("USD", charge.RevenueExchangeRate.CurrencyCode);
			AssertEquals(ExchangeRateOrgTypeEnum.Debtor, charge.RevenueExchangeRate.OrgType);
			AssertEquals(TestObjectCreator.AALSHI.PK, charge.RevenueExchangeRate.OrgPk);
		}

		public void TestOSCostExRateReadOnly()
		{
			var charge = Factory.New<TestingBaseCharge>();
			Assert(charge.JR_OSCostExRateInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestClearExchangeRates()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			charge.JR_JH = job.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_RX_NKCostCurrency = "EUR";
			charge.JR_RX_NKSellInvoiceCurrency = "RUB";

			charge.UpdateSellInvoiceExchangeRate();
			charge.UpdateRevenueExchangeRate();
			charge.UpdateCostExchangeRate();

			AssertNotNull(charge.CostExchangeRate);
			AssertNotNull(charge.RevenueExchangeRate);
			AssertNotNull(charge.SellInvoiceExchangeRate);

			charge.ClearExchangeRates();

			AssertNull(charge.CostExchangeRate);
			AssertNull(charge.RevenueExchangeRate);
			AssertNull(charge.SellInvoiceExchangeRate);

			charge.Delete();
			charge.ClearExchangeRates();
		}

		public void TestUpdateCostExchangeRate()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			charge.JR_JH = job.PK;
			charge.JR_RX_NKCostCurrency = "EUR";

			Assert("No Exchange Rates in the Factory", !Factory.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());

			var line = Factory.New<APInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Cost;
			charge.JR_AL_APLine = line.PK;

			Assert(charge.IsCostPosted);

			charge.UpdateCostExchangeRate();
			AssertNull("Cost Exchange Rate does not get updated if cost is posted", charge.CostExchangeRate);

			line.AL_ReverseDate = ZDateTime.Now;
			charge.ClearCostLinkOnlyTemporary();
			Assert(!charge.IsCostPosted);

			charge.JR_E6 = ZGuid.NewZGuid();
			Assert(charge.JR_IsApportioned);

			charge.UpdateCostExchangeRate();
			AssertNull("Cost Exchange Rate does not get updated if charge is apportioned", charge.CostExchangeRate);

			charge.JR_E6 = ZGuid.Empty;

			Assert(!charge.IsCostPosted);
			Assert(!charge.JR_IsApportioned);

			charge.UpdateCostExchangeRate();
			AssertNotNull(charge.CostExchangeRate);
			AssertEquals("EUR", charge.CostExchangeRate.CurrencyCode);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor, charge.CostExchangeRate.OrgType);
			AssertEquals(ZGuid.Empty, charge.CostExchangeRate.OrgPk);
		}

		public void TestUpdateRevenueExchangeRate()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			charge.JR_JH = job.PK;
			charge.JR_RX_NKSellCurrency = "EUR";

			Assert("No Exchange Rates in the Factory", !Factory.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());

			charge.SetContext(BusinessContext.AddingDefaultApportionmentCharge);
			charge.UpdateRevenueExchangeRate();
			charge.RemoveContext(BusinessContext.AddingDefaultApportionmentCharge);
			AssertNull("Revenue Exchange Rate does not get updated if AddingDefaultApportionmentCharge context is set on charge", charge.RevenueExchangeRate);

			var line = Factory.New<ARInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;

			Assert(charge.IsRevenuePosted);

			charge.UpdateRevenueExchangeRate();
			AssertNull("Revenue Exchange Rate does not get updated if Revenue is posted", charge.RevenueExchangeRate);

			line.AL_ReverseDate = ZDateTime.Now;
			charge.ClearRevenueLinkOnlyTemporary();
			Assert(!charge.IsRevenuePosted);

			charge.UpdateRevenueExchangeRate();
			AssertNotNull(charge.RevenueExchangeRate);
			AssertEquals("EUR", charge.RevenueExchangeRate.CurrencyCode);
			AssertEquals(ExchangeRateOrgTypeEnum.Debtor, charge.RevenueExchangeRate.OrgType);
			AssertEquals(ZGuid.Empty, charge.RevenueExchangeRate.OrgPk);
		}

		public void TestUpdateSellInvoiceExchangeRate()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);

			charge.JR_JH = job.PK;
			charge.JR_RX_NKSellInvoiceCurrency = "EUR";

			Assert("No Exchange Rates in the Factory", !Factory.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());

			var line = Factory.New<ARInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;

			Assert(charge.IsRevenuePosted);

			charge.UpdateSellInvoiceExchangeRate();
			AssertNull("Revenue Exchange Rate does not get updated if Revenue is posted", charge.SellInvoiceExchangeRate);

			line.AL_ReverseDate = ZDateTime.Now;
			charge.ClearRevenueLinkOnlyTemporary();
			Assert(!charge.IsRevenuePosted);

			charge.UpdateSellInvoiceExchangeRate();
			AssertNotNull(charge.SellInvoiceExchangeRate);
			AssertEquals("EUR", charge.SellInvoiceExchangeRate.CurrencyCode);
			AssertEquals(ExchangeRateOrgTypeEnum.Debtor, charge.SellInvoiceExchangeRate.OrgType);
			AssertEquals(ZGuid.Empty, charge.SellInvoiceExchangeRate.OrgPk);
		}

		public void TestSellInvoiceExchangeRateInitializesDuringPeriodicInvoicing()
		{
			AccountingConfigurationRegistry.Instance.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.SetContext(BusinessContext.PeriodicInvoicePosting);

			var charge = Factory.New<TestingBaseCharge>();
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			charge.JR_JH = job.PK;
			charge.JR_RX_NKSellInvoiceCurrency = "EUR";

			using (AccountingConfigurationRegistry.Instance.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("Should not add rate to shipment, should only retrieve", !job.ExchangeRates.Any());
				AssertNull("Should not add rate to shipment, should only retrieve", charge.SellInvoiceExchangeRate);
			}

			var rate = job.ExchangeRates.AddNew();
			rate.JF_BaseRate = 5;
			rate.JF_RX_NKRateCurrency = "EUR";
			AssertNull("Should not initialise unless context is periodic invoicing and registry item is true", charge.SellInvoiceExchangeRate);

			Factory.RemoveContext(BusinessContext.PeriodicInvoicePosting);
			AccountingConfigurationRegistry.Instance.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNull("Should not initialise unless context is periodic invoicing and registry item is true", charge.SellInvoiceExchangeRate);

			Factory.SetContext(BusinessContext.PeriodicInvoicePosting);
			AssertNotNull("Should initialise when context is periodic invoicing and registry item is true", charge.SellInvoiceExchangeRate);
		}

		public void TestCostTaxOverriddenFlagIsTrueForPostedCharge()
		{
			var job = TestObjectCreator.CreateJob("S0001", TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("T0001", TestObjectCreator.AUD, 1.0M, 250M, 25M, 0M, 250M, 25M, 0M);
			var charge = TestObjectCreator.CreateJobCharge(transaction.Lines[0], job, TestObjectCreator.CC1);
			AssertEquals("IsPosted", true, charge.IsCostPosted);
			AssertEquals("JR_IsCostTaxAmountOverridden", true, charge.JR_IsCostTaxAmountOverridden);
		}

		public void TestResettingJR_IsCostTaxAmountOverriddenResetsCostGSTAmountAsWell()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 250M, true);
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 50.5M;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);

			consolCost.E6_IsTaxAmountOverridden = false;
			AssertEquals("JR_IsCostTaxAmountOverridden", false, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 25M, consolCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
		}

		public void TestChangingTaxRateRecalculatesGSTAmount_WhenOverriddenFlagIsFalse()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", null);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 2500M, 2500M);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals("JR_IsCostTaxAmountOverridden", false, charge.JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 250M, charge.JR_OSCostGSTAmt_Calc);

			charge.JR_AT_CostGSTRate = TestObjectCreator.GST2.PK;
			AssertEquals("JR_IsCostTaxAmountOverridden", false, charge.JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 500M, charge.JR_OSCostGSTAmt_Calc);
		}

		public void TestChangingTaxRateSetGSTAmountToZero_WhenOverriddenFlagIsTrue()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 250M, true);
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 80.5M;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 80.5m, consolCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);

			consolCost.E6_AT_TaxRate = TestObjectCreator.GST2.PK;
			AssertEquals("JR_IsCostTaxAmountOverridden", false, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 50m, consolCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
		}

		public void TestChangingOSCostAmountRecalculatesGSTAmount_WhenWhenOverriddenFlagIsFalse()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001", null);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 2500M, 2500M);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals("JR_IsCostTaxAmountOverridden", false, charge.JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 250M, charge.JR_OSCostGSTAmt_Calc);

			charge.JR_OSCostAmt = 5000M;
			AssertEquals("JR_IsCostTaxAmountOverridden", false, charge.JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 500M, charge.JR_OSCostGSTAmt_Calc);
		}

		public void TestChangingOSCostAmountResetsGSTAmountToZero_WhenWhenOverriddenFlagIsTrue()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 250M, true);
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.E6_OSGSTAmount_Calc = 80.5M;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 80.5M, consolCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);

			consolCost.E6_OSCostAmount = 5000M;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);
			AssertEquals("JR_OSCostGSTAmt_Calc", 500M, consolCost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
		}

		public void TestJR_IsCostTaxAmountOverridden()
		{
			var shipment = TestObjectCreator.CreateShipment("S0003", null);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 2500M, 2500M);
			charge.JR_IsCostTaxAmountOverridden = true;

			AssertEquals("SettingJR_IsCostTaxAmountOverriddenToTrueForUnapportionedCharge_2", ErrorReporter.LastKeyReported);
			AssertEquals(@"Trying to set JR_IsCostTaxAmountOverridden to true for an unapportioned job charge, which is not allowed.
Current Value of JR_IsCostTaxAmountOverridden: N, JR_E6: 00000000-0000-0000-0000-000000000000", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var consol = TestObjectCreator.CreateConsol();
			shipment = TestObjectCreator.CreateShipment("S0001", consol);
			job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 250M, true);
			consolCost.E6_IsTaxAmountOverridden = true;
			consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden = false;
			AssertEquals("ApportionedChargeWithMismatchedJR_IsCostTaxAmountOverriddeValue_2", ErrorReporter.LastKeyReported);
			AssertEquals(@"Trying to set JR_IsCostTaxAmountOverridden to a value which does not match with linked Consol Cost.
Current Value of JR_IsCostTaxAmountOverridden: Y, E6_IsTaxAmountOverridden: Y", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			consol = TestObjectCreator.CreateConsol(consolNum: "C002");
			shipment = TestObjectCreator.CreateShipment("S0002", consol);
			job = TestObjectCreator.CreateJob(shipment, false);
			consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 250M, true);
			consolCost.E6_IsTaxAmountOverridden = true;
			AssertEquals("JR_IsCostTaxAmountOverridden", true, consolCost.ApportionmentCharges[0].JR_IsCostTaxAmountOverridden);

			consol = TestObjectCreator.CreateConsol(consolNum: "C003");
			shipment = TestObjectCreator.CreateShipment("S0003", consol);
			job = TestObjectCreator.CreateJob(shipment, false);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 250M, true);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, 350M, true);
			consolCost1.E6_IsTaxAmountOverridden = true;

			var reloadedAsCharge = Factory.Load<BaseCharge>(consolCost1.ApportionmentCharges[0].PK);
			reloadedAsCharge.JR_E6 = consolCost2.PK;
			consolCost2.Delete();
			reloadedAsCharge.JR_IsCostTaxAmountOverridden = true;
			AssertEquals("SettingJR_IsCostTaxAmountOverriddenToTrueForUnapportionedCharge_2", ErrorReporter.LastKeyReported);
			AssertEquals($@"Trying to set JR_IsCostTaxAmountOverridden to true for an unapportioned job charge, which is not allowed.
Current Value of JR_IsCostTaxAmountOverridden: N, JR_E6: {reloadedAsCharge.JR_E6}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Light Validation Tests

		public void TestLightValidationIsDisabledWhenEnableLightValidationForChargeAndConsolCostRegistryIsFalse()
		{
			AssertLightValidationResultForCharge(false);
		}

		public void TestLightValidationIsEnabledWhenEnableLightValidationForChargeAndConsolCostRegistryIsTrue()
		{
			AssertLightValidationResultForCharge(true);
		}

		public void TestDBHitsForEnableLightValidationForChargeAndConsolCostRegistry()
		{
			var charge = TestCharge;
			var dbHitsBefore = Db.Connection.ExecutedCommandCount;
			var result = charge.EnableLightValidationIfAvailable_ForTestOnly;
			var dbHitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals(dbHitsAfter, dbHitsBefore + 1);

			result = charge.EnableLightValidationIfAvailable_ForTestOnly;
			dbHitsAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals(dbHitsAfter, dbHitsBefore + 1);
		}

		void AssertLightValidationResultForCharge(bool shouldEnableLightValidation)
		{
			AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldEnableLightValidation);

			var debtor = TestObjectCreator.CreateOrgHeader("CROWN", false, true, "AUSYD", false);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001", true);
			var job = TestObjectCreator.CreateJob(shipment, debtor, 0m, TestObjectCreator.Agent, 0m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 20m);
			charge.JR_OH_SellAccount = debtor.PK;

			charge.RunPreSaveValidation();
			Assert(!charge.HasErrors);

			var anotherUserFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadedDebtor = anotherUserFactory.Load<OrgHeader>(debtor.PK);
			reloadedDebtor.OH_IsDebtor = false;
			anotherUserFactory.Save();
			Assert(!reloadedDebtor.OH_IsDebtor);

			charge.RunPreSaveValidation();
			AssertEquals(!shouldEnableLightValidation, charge.Notifications.ContainsNotificationContaining("Enter a valid Debtor."));
		}

		#endregion

		#region Cancel Changes Tests

		public void TestCancelChanges_OrphanedWIPAccrualDeletedAndOriginalWIPAccrualChangesAreCancelled()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 5m;
			Factory.Save();

			var originalWIP = charge.WIP;
			var originalACR = charge.Accrual;

			AssertNotNull(originalWIP);
			AssertNotNull(originalACR);

			originalWIP.Reverse();
			originalACR.Reverse();
			var orphanWIP = TestObjectCreator.CreateWIP(charge);
			var orphanACR = TestObjectCreator.CreateAccrual(charge);

			AssertEquals(charge.JR_AL_APLine, orphanACR.PK);
			AssertEquals(charge.JR_AL_ARLine, orphanWIP.PK);
			Assert(originalWIP.IsReversed);
			Assert(originalACR.IsReversed);
			Assert(!orphanWIP.IsReversed);
			Assert(!orphanACR.IsReversed);

			charge.CancelChanges();

			AssertEquals(charge.JR_AL_APLine, originalACR.PK);
			AssertEquals(charge.JR_AL_ARLine, originalWIP.PK);
			Assert(!originalWIP.IsReversed);
			Assert(!originalACR.IsReversed);
			Assert(orphanWIP.IsDeleted);
			Assert(orphanACR.IsDeleted);
		}

		public void TestCancelChanges_OrphanedJobConsolCostCancelled()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);
			var job = TestObjectCreator.CreateJob(shipment);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 10m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 20m, 20m);
			charge.JR_Desc = "Charge Code 2";

			Factory.Save();

			job.LoadCharges_ForTestOnly();
			AssertEquals(2, job.Charges.Count);
			Assert(job.Charges.Cast<Charge>().Any(c => c.JR_AC == TestObjectCreator.CC1.PK && c.JR_IsApportioned && c.JR_LocalCostAmt == 10m));
			Assert(job.Charges.Cast<Charge>().Any(c => c.JR_AC == TestObjectCreator.CC2.PK && !c.JR_IsApportioned && c.JR_LocalCostAmt == 20m));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolCost1InNewFactory = newFactory.Load<JobConsolCost>(consolCost1.PK);
			consolCost1InNewFactory.E6_AC_ChargeCode = TestObjectCreator.CC3.PK;
			newFactory.Save();

			consolCost1.E6_AC_ChargeCode = TestObjectCreator.CC4.PK;
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 20m);

			AssertEquals("Originally empty", ZGuid.Empty, charge.JR_E6);
			Assert("Already saved", charge.IsInDatabase);

			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);

			AssertEquals("Originally empty", ZGuid.Empty, charge.JR_E6Info.OriginalValue);
			AssertEquals("Matched on saving to consolcost", consolCost2.PK, charge.JR_E6);
			Assert("Still in DB", charge.IsInDatabase);
			Assert("Saving failed", !consolCost2.IsInDatabase);

			AssertNoExceptionThrown("While deleted from consolcost, should not get orphaning errors", consolCost2.CalculationStrategy.HandleDelete);
		}

		public void TestCancelChanges_WIPAccrualChangesAreCancelled()
		{
			var chargeDesc1 = "Charge Description 1";
			var chargeDesc2 = "Charge Description 2";
			var wIPDesc2 = "WIP Description 2";
			var accrualDesc2 = "Accrual Description 2";

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_Desc = chargeDesc1;
			charge.JR_OSSellAmt = 10m;
			charge.JR_OSCostAmt = 5m;
			Factory.Save();

			var wIP = charge.WIP;
			var aCR = charge.Accrual;

			AssertNotNull(wIP);
			AssertNotNull(aCR);

			AssertEquals("Charge original description", chargeDesc1, charge.JR_Desc);
			AssertEquals("WIP original description", chargeDesc1, wIP.AL_Desc);
			AssertEquals("Accrual original description", chargeDesc1, aCR.AL_Desc);

			charge.JR_Desc = chargeDesc2;
			wIP.AL_Desc = wIPDesc2;
			aCR.AL_Desc = accrualDesc2;

			AssertEquals("Charge description has changed", chargeDesc2, charge.JR_Desc);
			AssertEquals("WIP description has changed", wIPDesc2, wIP.AL_Desc);
			AssertEquals("Accrual description has changed", accrualDesc2, aCR.AL_Desc);

			charge.CancelChanges();

			AssertEquals("Charge description should be changed to original description", chargeDesc1, charge.JR_Desc);
			AssertEquals("WIP description should be changed to original description", chargeDesc1, wIP.AL_Desc);
			AssertEquals("Accrual description should be changed to original description", chargeDesc1, aCR.AL_Desc);
		}

		public void TestCancelChanges_ErrorReportedWhenPotentiallyOrphanedBizObjInFactory_ExcludedGuidFieldsList()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			var cfxLine = TestObjectCreator.CreateRevenueLine(Factory.New<Charge>(), Factory.New<ARInvoice>().PK);
			charge.JR_AL_CFXLine = cfxLine.PK;

			var expectedMessage = @"A foreign key business object has been created and might become orphaned after cancelling changes on charge. 
Please take appropriate steps to prevent this causing a critical validation error and update ForeignKeyColumnsAlreadyFixed list after your changes.
Job charge foreign key column : JR_AL_CFXLine";
			var expectedChargeMessage1 = string.Format(@"Charge info before cancelling changes : Charge: PK = {0}", charge.PK);
			var expectedChargeMessage2 = string.Format(@"Charge info after cancelling changes : Charge: PK = {0}", charge.PK);

			ErrorReporter.Clear();
			charge.CancelChanges();

			AssertEquals("Charge_CancelChanges_PotentiallyOrphanedObject_JR_AL_CFXLine", ErrorReporter.LastKeyReported);
			AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
			AssertContains(expectedChargeMessage1, ErrorReporter.LastMessageReported);
			AssertContains(expectedChargeMessage2, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCancelChanges_ErrorReportedWhenPotentiallyOrphanedBizObjInFactory_PersistentFieldsToCopyAndInValidOrderList()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			var creditor = Factory.New<OrgHeader>();
			charge.JR_OH_CostAccount = creditor.PK;

			var expectedMessage = @"A foreign key business object has been created and might become orphaned after cancelling changes on charge. 
Please take appropriate steps to prevent this causing a critical validation error and update ForeignKeyColumnsAlreadyFixed list after your changes.
Job charge foreign key column : JR_OH_CostAccount";
			var expectedChargeMessage1 = string.Format(@"Charge info before cancelling changes : Charge: PK = {0}", charge.PK);
			var expectedChargeMessage2 = string.Format(@"Charge info after cancelling changes : Charge: PK = {0}", charge.PK);

			ErrorReporter.Clear();
			charge.CancelChanges();

			AssertEquals("Charge_CancelChanges_PotentiallyOrphanedObject_JR_OH_CostAccount", ErrorReporter.LastKeyReported);
			AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
			AssertContains(expectedChargeMessage1, ErrorReporter.LastMessageReported);
			AssertContains(expectedChargeMessage2, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCancelChanges_ConvertInvalidZGuidtoGuid()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.Debtor);
			Factory.Save();

			charge.JR_OH_CostAccount = ZGuid.Empty;

			AssertNoExceptionThrown("Shouldn't throw convert Guid exception", charge.CancelChanges);
		}

		public void TestAllOrphaningErrorsAreReported()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			var creditor = Factory.New<OrgHeader>();
			charge.JR_OH_CostAccount = creditor.PK;

			var cfxLine = TestObjectCreator.CreateRevenueLine(Factory.New<Charge>(), Factory.New<ARInvoice>().PK);
			charge.JR_AL_CFXLine = cfxLine.PK;

			var expectedMessage = @"A foreign key business object has been created and might become orphaned after cancelling changes on charge. 
Please take appropriate steps to prevent this causing a critical validation error and update ForeignKeyColumnsAlreadyFixed list after your changes.
Job charge foreign key column : JR_OH_CostAccount, JR_AL_CFXLine";
			var expectedChargeMessage1 = string.Format(@"Charge info before cancelling changes : Charge: PK = {0}", charge.PK);
			var expectedChargeMessage2 = string.Format(@"Charge info after cancelling changes : Charge: PK = {0}", charge.PK);

			ErrorReporter.Clear();
			charge.CancelChanges();

			AssertEquals("Charge_CancelChanges_PotentiallyOrphanedObject_JR_OH_CostAccount", ErrorReporter.LastKeyReported);
			AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
			AssertContains(expectedChargeMessage1, ErrorReporter.LastMessageReported);
			AssertContains(expectedChargeMessage2, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestForeignKeyColumnsAlreadyFixedForNewForeignKeyColumns()
		{
			var excludedGuidFields = new[]
			{
					JobChargeSchema.JR_JH.Name,
					JobChargeSchema.JR_AL_CFXLine.Name,
					JobChargeSchema.JR_E6.Name,
					JobChargeSchema.JR_E6_GatewaySellHeader.Name,
					JobChargeSchema.JR_OP_Product.Name,
					JobChargeSchema.JR_CAL_APLine.Name,
					JobChargeSchema.JR_CAL_ARLine.Name,
					JobChargeSchema.JR_IsAPCashAdvance.Name,
					JobChargeSchema.JR_IsARCashAdvance.Name
				};

			var errorMessage = @"JobCharge table foreign key fields have been changed. 
If an object of the foreign key type can become orphaned when we cancel changes for charge, add new field to 'ForeignKeyColumnsAlreadyFixed'.
Ans also make appropriate changes for CancelChanges method in BaseCharge class to prevent any critical exceptions.
Otherwise add this new field to 'excludedGuidFields' in this unit test.";

			var persistentGuidPropertyNames = Factory.New<Charge>().ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.PropertyType == typeof(ZGuid) && info.IsPersistent).Select(x => x.Name).ToArray();
			var guidFieldsToCheck = persistentGuidPropertyNames.Except(Charge.PersistentFieldsToCopyAndInValidOrder).Except(excludedGuidFields);
			AssertContainsExactElementsInAnyOrder(errorMessage, guidFieldsToCheck, BaseCharge.ForeignKeyColumnsAlreadyFixed_ForTestOnly);
		}

		#endregion

		public void TestJR_AT_CostGSTRateForNullValue()
		{
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals(ZDate.Empty, TestCharge.JR_CostTaxDate);

			TestCharge.JR_CostTaxDate = ZDate.Today;
			TestCharge.JR_AT_CostGSTRate = ZGuid.Empty;
			AssertEquals(ZDate.Empty, TestCharge.JR_CostTaxDate);
		}

		public void TestJR_CostTaxDate()
		{
			TestCharge.JR_OSCostAmt = 8M;
			TestCharge.JR_AT_CostGSTRate = CreateTaxRate().PK;
			TestCharge.JR_CostTaxDate = ZDate.Today;
			AssertAmounts(ZDate.Today, 8.8M, 0.8M);

			TestCharge.JR_CostTaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(ZDate.Today.AddDays(1), 8.24M, 0.24M);

			void AssertAmounts(ZDate taxDate, decimal totalAmount, decimal taxAmount)
			{
				AssertEquals(taxDate, TestCharge.JR_CostTaxDate);
				AssertEquals(totalAmount, TestCharge.LineTotalAmountOnInvoiceForJob);
				AssertEquals(taxAmount, TestCharge.LineGSTAmountOnInvoiceForJob);
			}
		}

		public void TestJR_CostTaxDate_ReadOnly()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var shipmentWithoutConsole = TestObjectCreator.CreateShipment("S0001998", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipmentWithoutConsole, false);
			var shipmentCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 12m, null);

			AssertEquals("JR_CostTaxDate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_CostTaxDateInfo.ReadOnly);

			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var invoicingAllowOverrideCostTaxIdCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxId);
			var securityRight = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowOverrideCostTaxId);
			var cachedSecurityValue = securityRight.IsAllowed;

			using (new DisposableAction(() => { }, () => securityRight.IsAllowed = cachedSecurityValue))
			{
				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					securityRight.IsAllowed = false;
					AssertEquals("JR_CostTaxDate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_CostTaxDateInfo.ReadOnly);
					securityRight.IsAllowed = true;
					AssertEquals("JR_CostTaxDate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_CostTaxDateInfo.ReadOnly);
				}

				using (AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetTemporaryValue(currentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					securityRight.IsAllowed = false;
					AssertEquals("JR_CostTaxDate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_CostTaxDateInfo.ReadOnly);
					securityRight.IsAllowed = true;
					AssertEquals("JR_CostTaxDate is not readonly as charge not related to consol but operator can modify TestObjectCreator.GST1", false, shipmentCharge.JR_CostTaxDateInfo.ReadOnly);

					var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001990");
					var shipmentWithConsol = TestObjectCreator.CreateShipment("S0001999", consol);
					var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI, 10M, true);

					Factory.Save();

					AssertEquals("One Apportionment charge should be created", 1, ((Job)shipmentWithConsol.Job).Charges.Count);
					var charge = ((Job)shipmentWithConsol.Job).Charges[0];
					AssertEquals("JR_AT_CostGSTRate is readonly as attached to a consol even though operator can modify TestObjectCreator.GST1", true, charge.JR_CostTaxDateInfo.ReadOnly);
				}
			}
		}

		public void TestJR_AT_SellGSTRateForNullValue()
		{
			TestCharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals(ZDate.Empty, TestCharge.JR_SellTaxDate);

			TestCharge.JR_SellTaxDate = ZDate.Today;
			TestCharge.JR_AT_SellGSTRate = ZGuid.Empty;
			AssertEquals(ZDate.Empty, TestCharge.JR_SellTaxDate);
		}

		public void TestJR_SellTaxDate()
		{
			TestCharge.JR_OSSellAmt = 8M;
			TestCharge.JR_AT_SellGSTRate = CreateTaxRate().PK;
			TestCharge.JR_SellTaxDate = ZDate.Today;
			AssertAmounts(ZDate.Today, 8.8M, 0.8M);

			TestCharge.JR_SellTaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(ZDate.Today.AddDays(1), 8.24M, 0.24M);

			void AssertAmounts(ZDate taxDate, decimal totalAmount, decimal taxAmount)
			{
				AssertEquals(taxDate, TestCharge.JR_SellTaxDate);
				AssertEquals(totalAmount, TestCharge.JR_Calc_OSSellAmtWithGST);
				AssertEquals(taxAmount, TestCharge.JR_OSSellGSTAmt_Calc);
			}
		}

		public void TestJR_SellTaxDate_ReadOnly()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var shipmentWithoutConsole = TestObjectCreator.CreateShipment("S0001998", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipmentWithoutConsole, false);
			var shipmentCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 10m, null, TestObjectCreator.AUD, 12m, TestObjectCreator.LocalClient);

			AssertEquals("JR_SellTaxDate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_SellTaxDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("JR_SellTaxDate is readonly as charge not related to consol and operator can not modify TestObjectCreator.GST1", true, shipmentCharge.JR_SellTaxDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("JR_SellTaxDate is not readonly as charge not related to consol but operator can modify TestObjectCreator.GST1", false, shipmentCharge.JR_SellTaxDateInfo.ReadOnly);

			TestObjectCreator.CreateRevenueLine(shipmentCharge, ZGuid.Empty);
			AssertEquals("JR_AT_CostGSTRate is readonly as posted", true, shipmentCharge.JR_CostTaxDateInfo.ReadOnly);
		}

		public void TestAddCallStackToCollectorServiceWhenLocalAmountSetToZero()
		{
			var collector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			var charge = Factory.NewWithValidTestData<TestingBaseCharge>();

			charge.JR_OSCostAmt = 100;
			charge.JR_LocalCostAmt = 0;
			AssertNotEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			Assert(!charge.JR_E6.IsValid);
			AssertContains("This case should not report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));

			charge.JR_E6 = cost.PK;

			charge.JR_OSCostAmt = 100;
			charge.JR_LocalCostAmt = 100;
			AssertNotEquals(0m, charge.JR_OSCostAmt);
			AssertNotEquals(0m, charge.JR_LocalCostAmt);
			AssertContains("This case should not report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));

			charge.JR_OSCostAmt = 100;
			cost.E6_OSCostAmount = 100;
			charge.JR_LocalCostAmt = 0;
			AssertNotEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertNotContains("This case should report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));
		}

		public void TestAddCallStackToCollectorServiceWhenLocalAmountSetToZeroAndOSCostAmountIsZero()
		{
			var collector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			var charge = Factory.NewWithValidTestData<TestingBaseCharge>();

			charge.JR_OSCostAmt = 0;
			charge.JR_LocalCostAmt = 0;
			AssertEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			Assert(!charge.JR_E6.IsValid);
			AssertContains("This case should not report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));

			charge.JR_E6 = cost.PK;

			charge.JR_OSCostAmt = 0;
			charge.JR_LocalCostAmt = 100;
			AssertNotEquals(0m, charge.JR_LocalCostAmt);
			AssertContains("This case should not report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));

			charge.JR_OSCostAmt = 0;
			cost.E6_OSCostAmount = 0;
			charge.JR_LocalCostAmt = 0;
			AssertEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertEquals(0m, cost.E6_OSCostAmount);
			AssertContains("This case should not report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));

			charge.JR_OSCostAmt = 0;
			cost.E6_OSCostAmount = 100;
			charge.JR_LocalCostAmt = 0;
			AssertEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertNotEquals(0m, cost.E6_OSCostAmount);
			AssertNotContains("This case should not report an error in the localamt setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero));
		}

		public void TestAddCallStackToCollectorServiceWhenLocalAmountIsZeroAndOSAmountIsNotZero()
		{
			var collector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var cost = Factory.NewWithValidTestData<JobConsolCost>();
			var charge = Factory.NewWithValidTestData<ApportionSplitCharge>();

			charge.JR_OSCostExRate = 0;
			charge.JR_OSCostAmt = 100;
			charge.JR_LocalCostAmt = 0;
			AssertNotEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			Assert(!charge.JR_E6.IsValid);
			AssertContains("This case should not report an error in the OS amount setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountIsZeroWithNonZeroOSAmount));

			charge.JR_E6 = cost.PK;

			charge.JR_OSCostAmt = 100;
			charge.JR_LocalCostAmt = 100;
			AssertNotEquals(0m, charge.JR_OSCostAmt);
			AssertNotEquals(0m, charge.JR_LocalCostAmt);
			AssertContains("This case should not report an error in the OS amount setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountIsZeroWithNonZeroOSAmount));

			cost.E6_LocalCostAmount = 200;
			charge.JR_LocalCostAmt = 0;
			charge.JR_OSCostAmt = 200;
			AssertNotEquals(0m, charge.JR_OSCostAmt);
			AssertEquals(0m, charge.JR_LocalCostAmt);
			AssertNotContains("This case should report an error in the OS amount setter", "There is no data collected for this PK", collector.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountIsZeroWithNonZeroOSAmount));
		}

		public void TestDelete_ErrorIsReported()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 258.456M, TestObjectCreator.AALSHI);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var invoiceCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 258.456M);
			apInvoice.ImportAllApportionmentsFromCosting();

			var chargePK = invoiceCost.ApportionmentCharges[0].PK;
			invoiceCost.ApportionmentCharges[0].Delete();

			var deletionCallStack = CriticalValidationInfoCollectorService.GetService(Factory)?.GetInfo(chargePK, CriticalValidationInfoCollectorServiceKeyType.ApportionmentChargeLinkedToInvoiceLineDeleted);
			AssertNotNull(deletionCallStack);
			AssertContains("ApportionmentChargeLinkedToInvoiceLineDeleted", deletionCallStack);
		}

		public void TestDelete_RecordUnexpectedDelete_WithoutMonitor()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("SHP001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("SHP002", consol);
			TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 300m);

			var charge1 = consolCost.ApportionmentCharges[0];
			var charge2 = consolCost.ApportionmentCharges[1];
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);

			charge1.Delete();
			charge2.Delete();

			var info = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);
			AssertEquals("DeleteNewApportionedChargeWhenSaveJobConsolCost: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", info.Trim());
		}

		public void TestDelete_RecordUnexpectedDelete_WithMonitor_ConsolCostNotSaved()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("SHP001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("SHP002", consol);
			TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 300m);

			var charge1 = consolCost.ApportionmentCharges[0];
			var charge2 = consolCost.ApportionmentCharges[1];
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				monitor.CollectApportionmentChargesInfo(consolCost);
				charge1.Delete();
				charge2.Delete();
			}

			var info = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);
			AssertContains("DeleteNewApportionedChargeWhenSaveJobConsolCost:", info);
			AssertContains($@"Charge Delete:
Charge: PK = {charge1.PK}", info);
			AssertContains($@"Charge Delete:
Charge: PK = {charge2.PK}", info);
		}

		public void TestDelete_RecordUnexpectedDelete_WithMonitor_ConsolCostSaved()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("SHP001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("SHP002", consol);
			TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 300m);
			Factory.Save();

			var charge1 = consolCost.ApportionmentCharges[0];
			var charge2 = consolCost.ApportionmentCharges[1];
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				monitor.CollectApportionmentChargesInfo(consolCost);
				charge1.Delete();
				charge2.Delete();
			}

			var info = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);
			AssertEquals("DeleteNewApportionedChargeWhenSaveJobConsolCost: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", info.Trim());
		}

		public void TestDelete_RecordUnexpectedDelete_WithMonitor_ChargesExistedInDB_ConsolCostSaved()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("SHP001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("SHP002", consol);
			TestObjectCreator.CreateJob(shipment1, false);
			TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 300m);

			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);

			using (DeleteApportionmentChargesWhenSaveJobConsolCostMonitor.AddTempService(Factory))
			{
				var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
				monitor.CollectApportionmentChargesInfo(consolCost);

				Factory.Save();

				var charge1 = consolCost.ApportionmentCharges[0];
				var charge2 = consolCost.ApportionmentCharges[1];

				Assert(charge1.IsInDatabase);
				Assert(charge2.IsInDatabase);

				charge1.Delete();
				charge2.Delete();

				var info = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(consolCost.PK, CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost);
				AssertContains("DeleteNewApportionedChargeWhenSaveJobConsolCost:", info);
				AssertContains($@"Charge Delete:
Charge: PK = {charge1.PK}", info);
				AssertContains($@"Charge Delete:
Charge: PK = {charge2.PK}", info);
			}
		}

		public void TestDelete_ErrorIsReportedOnChargeDeletion()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("SHP001", consol);
			TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 258.456M, TestObjectCreator.AALSHI);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD);
			apInvoice.SubmittedFromInvoicingForm = true;
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var invoiceCost = TestObjectCreator.CreateConsolCost(apInvoice, consol, TestObjectCreator.CC1, 258.456M);
			apInvoice.ImportAllApportionmentsFromCosting();

			var chargePK = invoiceCost.ApportionmentCharges[0].PK;
			var charge = Factory.Load<BaseCharge>(chargePK);
			charge.Delete();

			var deletionCallStack = CriticalValidationInfoCollectorService.GetService(Factory)?.GetInfo(chargePK, CriticalValidationInfoCollectorServiceKeyType.ApportionmentChargeLinkedToInvoiceLineDeleted);
			AssertNotNull(deletionCallStack);
			AssertContains("ApportionmentChargeLinkedToInvoiceLineDeleted", deletionCallStack);
		}

		public void TestChaningJR_LocalCostAmountDoesNotThrowException()
		{
			AssertNoExceptionThrown(() =>
			{
				var oldValue = Factory.RefreshEnabled;
				try
				{
					Factory.RefreshEnabled = false;

					ForwardingConsol consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					Factory.Save();

					ForwardingShipment shipment1 = consol.Shipments.AddNew();
					shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment1.JS_ActualChargeable = 200.0m;
					shipment1.JS_ActualWeight = 500m;
					shipment1.JS_UnitOfWeight = Core.Constants.Weight.LongTons;
					shipment1.JS_ActualVolume = 0m;
					shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

					var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
					cost.E6_OSCostAmount = 2500M;
					Factory.Save();

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var reloadedCharge = newFactory.Load<Charge>(cost.ApportionmentCharges[0].PK);

					cost.Delete();
					Factory.Save();

					reloadedCharge.JR_LocalCostAmt = 254M;
				}
				finally
				{
					Factory.RefreshEnabled = oldValue;
				}
			});
		}

		public void TestOldValueShouldNotEqualNewValueWithSetJR_AL_APLineAndJR_AL_ARLine()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var apLine = Factory.New<Accrual>();
			charge.JR_AL_APLine = apLine.PK;
			Assert(!charge.Accrual.IsReversed);

			charge.JR_AL_APLine = apLine.PK;
			Assert(!charge.Accrual.IsReversed);

			var charge2 = Factory.New<TestingBaseCharge>();

			var arLine = Factory.New<WIP>();
			charge2.JR_AL_ARLine = arLine.PK;
			Assert(!charge2.WIP.IsReversed);

			charge2.JR_AL_ARLine = arLine.PK;
			Assert(!charge2.WIP.IsReversed);
		}

		public void TestNewLineHasBeenReversedWithSetJR_AL_APLineAndJR_AL_ARLine()
		{
			var charge = Factory.New<TestingBaseCharge>();

			var apLine = Factory.New<Accrual>();
			apLine.AL_ReverseDate = ZDateTime.Now;

			charge.JR_AL_APLine = apLine.PK;

			AssertEquals(ExceptionReporterTestListener.Instance.Count, 1);
			AssertContains("The new Accrual has been reversed", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var charge2 = Factory.New<TestingBaseCharge>();

			var arLine = Factory.New<WIP>();
			arLine.AL_ReverseDate = ZDateTime.Now;

			charge2.JR_AL_ARLine = arLine.PK;

			AssertEquals(ExceptionReporterTestListener.Instance.Count, 1);
			AssertContains("The new WIP has been reversed", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestJR_OSSellGSTAmt_Calc()
		{
			var charge = Factory.New<TestingBaseCharge>();
			TestObjectCreator.GST1.SetRate_ForTestOnly(1, 1);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_InvoiceType = "CUR";
			charge.JR_OSSellExRate = 14476m;

			charge.JR_OSSellAmt = 7500m;
			charge.JR_LocalSellAmt = 0.52m;
			AssertEquals(75m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals(0.01m, charge.JR_Sell_LocalGSTAmount);

			charge.JR_OSSellAmt = 7200m;
			charge.JR_LocalSellAmt = 0.5m;
			AssertEquals(0m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals(0m, charge.JR_Sell_LocalGSTAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(144.76m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals(0.01m, charge.JR_Sell_LocalGSTAmount);

			charge.JR_OSSellAmt = 7000m;
			charge.JR_LocalSellAmt = 0.48m;
			AssertEquals(0m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals(0m, charge.JR_Sell_LocalGSTAmount);
		}

		public void TestIfSetCostTaxDateTheValueHasNotChangedThenNotRecalculateCostGSTAmt()
		{
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GSTANDQST1WithDates.PK;
			AssertEquals(0m, TestCharge.JR_OSCostGSTAmt);

			TestCharge.JR_OSCostGSTAmt = 3m;
			TestCharge.JR_CostTaxDate = TestCharge.JR_CostTaxDate;
			AssertEquals(3m, TestCharge.JR_OSCostGSTAmt);
		}

		[TestDate(2019, 10, 03)]
		[SuspendCriticalValidation]
		public void TestErrorIsReportedWhenThereIsMismatchBetweenChargeAndConsolTaxDate()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			TestCharge.JR_E6 = consolCost.PK;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			TestCharge.JR_CostTaxDate = ZDateTime.Today.Date;

			AssertEquals("ApportionedChargeWithMismatched_TaxDate_1", ErrorReporter.LastKeyReported);
			AssertContains("JR_CostTaxDate = 03-Oct-19 00:00:00. But E6_TaxDate = ", ErrorReporter.LastMessageReported);
			AssertContains("Charge:", ErrorReporter.LastMessageReported);
			AssertContains("Job Consol Cost:", ErrorReporter.LastMessageReported);
			AssertContains("Enterprise.Accounting.Business.JobInvoicing.BaseCharge.set_JR_CostTaxDate(ZDate value)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2019, 10, 03)]
		[SuspendCriticalValidation]
		public void TestResumeActionOfCheckDatesMismatchFunctionalitySuspender()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			TestCharge.JR_CostTaxDate = ZDateTime.Today.Date;
			AssertNotEquals("Dates mismatch between consol cost and charge", consolCost.E6_TaxDate, TestCharge.JR_CostTaxDate);

			using (TestCharge.ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender())
			{
				TestCharge.JR_E6 = consolCost.PK;
			}

			AssertEquals("There should be no error report because TaxID is not set.", 0, ErrorReporter.TotalErrorCount);

			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			using (TestCharge.ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender())
			{
				TestCharge.JR_E6 = consolCost.PK;
			}

			AssertEquals("ApportionedChargeWithMismatched_TaxDate_1", ErrorReporter.LastKeyReported);
			AssertContains("JR_CostTaxDate = 03-Oct-19 00:00:00. But E6_TaxDate = ", ErrorReporter.LastMessageReported);
			AssertContains("Charge:", ErrorReporter.LastMessageReported);
			AssertContains("Job Consol Cost:", ErrorReporter.LastMessageReported);
			AssertContains("Enterprise.Accounting.Business.JobInvoicing.BaseCharge.CheckAndReportCostTaxDatesMismatch()", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCheckAndReportCostTaxDatesMismatch_NullParentConsolCost()
		{
			TestCharge.JR_CostTaxDate = ZDateTime.Today.Date;
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			AssertNoExceptionThrown(() =>
			{
				using (TestCharge.ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender())
				{
					TestCharge.JR_E6 = Guid.NewGuid();
					Assert(TestCharge.JR_E6.IsValid);
					AssertNull(TestCharge.ParentConsolCost);
				}
			});
		}

		public void TestUpdateJR_AT_CostGSTRateReadOnly()
		{
			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			TestCharge.UpdateJR_AT_CostGSTRateReadOnly_ForTestOnly();
			AssertEquals(ZGuid.Empty, TestCharge.JR_AT_CostGSTRate);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			fTestCharge = null;
			TestCharge.JR_E6 = consolCost.PK;

			TestCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			TestCharge.UpdateJR_AT_CostGSTRateReadOnly_ForTestOnly();
			AssertNotEquals(ZGuid.Empty, TestCharge.JR_AT_CostGSTRate);
		}

		public void TestJR_CostPlaceOfSupplyDefaultsGSTRate()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var newBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BR1");
				newBranch.GB_OH_OrgProxy = TestObjectCreator.ActiveOrg.PK;
				GlbCompany.CurrentCompany.Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var charge = PrepareTaxDefaultDataSetupForIndia();

					charge.JR_CostPlaceOfSupply = "";
					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_CostPlaceOfSupply = "DL";
					AssertEquals(TestObjectCreator.GST1WithDates.PK, charge.JR_AT_CostGSTRate);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_CostPlaceOfSupply = "KL";
					AssertEquals(TestObjectCreator.GST2WithDates.PK, charge.JR_AT_CostGSTRate);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_CostPlaceOfSupply = "ALX";
					AssertEquals(TestObjectCreator.GSTANDQST1WithDates.PK, charge.JR_AT_CostGSTRate);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_CostPlaceOfSupply = "";
					AssertEquals(TestObjectCreator.GST1WithDates.PK, charge.JR_AT_CostGSTRate);
				}
			}
		}

		public void TestJR_SellPlaceOfSupplyDefaultsGSTRate()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var newBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BR1");
				newBranch.GB_OH_OrgProxy = TestObjectCreator.ActiveOrg.PK;
				GlbCompany.CurrentCompany.Factory.Save();
				using (EnvProxy.Instance.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var charge = PrepareTaxDefaultDataSetupForIndia();

					charge.JR_SellPlaceOfSupply = "";
					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_SellPlaceOfSupply = "DL";
					AssertEquals(TestObjectCreator.GST1WithDates.PK, charge.JR_AT_SellGSTRate);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_SellPlaceOfSupply = "KL";
					AssertEquals(TestObjectCreator.GST2WithDates.PK, charge.JR_AT_SellGSTRate);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_SellPlaceOfSupply = "ALX";
					AssertEquals(TestObjectCreator.GSTANDQST1WithDates.PK, charge.JR_AT_SellGSTRate);

					charge.ChargeCode.ClearGSTRateCacheForTesting();
					charge.JR_SellPlaceOfSupply = "";
					AssertEquals(TestObjectCreator.GST1WithDates.PK, charge.JR_AT_SellGSTRate);
				}
			}
		}

		public void TestSellPlaceOfSupplyDefaulting_State()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			TestCharge.JR_JH = job.PK;

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				Assert(TestCharge.JR_AC.IsEmpty);
				Assert(TestCharge.JR_OH_SellAccount.IsEmpty);
				Assert(TestCharge.JR_SellPlaceOfSupply.IsEmpty);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "NSW", TestObjectCreator.Debtor);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(TestCharge.JR_OH_SellAccount.IsEmpty);
				Assert(TestCharge.JR_SellPlaceOfSupply.IsEmpty);

				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				AssertEquals("NSW", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_SellPlaceOfSupply = "ACT";
				TestCharge.JR_AC = TestCharge.JR_AC;
				TestCharge.JR_OH_SellAccount = TestCharge.JR_OH_SellAccount;
				AssertEquals("ACT", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("NSW", TestCharge.JR_SellPlaceOfSupply);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "WA", TestObjectCreator.Debtor1);
				TestCharge.JR_SellPlaceOfSupply = "QLD";
				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				AssertEquals("WA", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_SellPlaceOfSupply = "QLD";
				TestCharge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals("QLD", TestCharge.JR_SellPlaceOfSupply);

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					TestCharge.JR_GB = Env.CurrentBranchPK;
					AssertEquals("WA", TestCharge.JR_SellPlaceOfSupply);
				}
			}
		}

		public void TestSellPlaceOfSupplyDefaulting_TaxZone()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001", "CATOR", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment, false);
				Factory.Save();
				TestCharge.JR_JH = job.PK;

				Assert(TestCharge.JR_AC.IsEmpty);
				Assert(TestCharge.JR_OH_SellAccount.IsEmpty);
				Assert(TestCharge.JR_SellPlaceOfSupply.IsEmpty);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Revenue, PlaceOfSupplyTypes.TaxZone.Code, "ONTZ", TestObjectCreator.Debtor);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(TestCharge.JR_OH_SellAccount.IsEmpty);
				Assert(TestCharge.JR_SellPlaceOfSupply.IsEmpty);

				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				AssertEquals("ONTZ", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_SellPlaceOfSupply = "HSTC";
				TestCharge.JR_AC = TestCharge.JR_AC;
				TestCharge.JR_OH_SellAccount = TestCharge.JR_OH_SellAccount;
				AssertEquals("HSTC", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("ONTZ", TestCharge.JR_SellPlaceOfSupply);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Revenue, PlaceOfSupplyTypes.TaxZone.Code, "QUBC", TestObjectCreator.Debtor1);
				TestCharge.JR_SellPlaceOfSupply = "HSTC";
				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				AssertEquals("QUBC", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_SellPlaceOfSupply = "HSTC";
				TestCharge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals("HSTC", TestCharge.JR_SellPlaceOfSupply);

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					TestCharge.JR_GB = Env.CurrentBranchPK;
					AssertEquals("QUBC", TestCharge.JR_SellPlaceOfSupply);
				}
			}
		}

		public void TestSellTaxRateUpdatedOnDefaultingSameSellPlaceOfSupply()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			var nonCurrentBranch = TestObjectCreator.NonCurrentBranch;

			TestCharge.JR_JH = job.PK;

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				AssertEquals(currentBranch.PK, TestCharge.JR_GB);
				Assert(TestCharge.JR_AC.IsEmpty);
				Assert(TestCharge.JR_OH_SellAccount.IsEmpty);
				Assert(TestCharge.JR_SellPlaceOfSupply.IsEmpty);
				Assert(TestCharge.JR_AT_SellGSTRate.IsEmpty);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				TestCharge.JR_SellPlaceOfSupply = "NSW";
				Assert(TestCharge.JR_OH_SellAccount.IsEmpty);
				Assert(TestCharge.JR_AT_SellGSTRate.IsEmpty);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "NSW", org: TestObjectCreator.Debtor);

				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				AssertEquals("NSW", TestCharge.JR_SellPlaceOfSupply);
				AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_SellGSTRate);

				TestCharge.JR_AT_SellGSTRate = ZGuid.Empty;

				TestCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("NSW", TestCharge.JR_SellPlaceOfSupply);
				AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_SellGSTRate);

				TestCharge.JR_AT_SellGSTRate = ZGuid.Empty;

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					TestCharge.JR_GB = nonCurrentBranch.PK;
					AssertEquals("NSW", TestCharge.JR_SellPlaceOfSupply);
					AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_SellGSTRate);
				}

				TestCharge.JR_AT_SellGSTRate = ZGuid.Empty;

				TestCharge.JR_GB = currentBranch.PK;
				AssertEquals("NSW", TestCharge.JR_SellPlaceOfSupply);
				AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_SellGSTRate);
			}
		}

		public void TestCostPlaceOfSupplyDefaulting_State()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();
			TestCharge.JR_JH = job.PK;
			Assert("Pre-condition: not apportioned", !TestCharge.JR_IsApportioned);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				Assert(TestCharge.JR_AC.IsEmpty);
				Assert(TestCharge.JR_OH_CostAccount.IsEmpty);
				Assert(TestCharge.JR_CostPlaceOfSupply.IsEmpty);

				var currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
				var nonCurrentBranch = TestObjectCreator.NonCurrentBranch;
				nonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("OBR", true, true).PK;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: currentBranch);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(TestCharge.JR_OH_CostAccount.IsEmpty);
				Assert(TestCharge.JR_CostPlaceOfSupply.IsEmpty);

				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);

				TestCharge.JR_CostPlaceOfSupply = "ACT";
				TestCharge.JR_AC = TestCharge.JR_AC;
				TestCharge.JR_OH_CostAccount = TestCharge.JR_OH_CostAccount;
				AssertEquals("ACT", TestCharge.JR_CostPlaceOfSupply);

				TestCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);

				TestCharge.JR_CostPlaceOfSupply = "QLD";
				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "WA", branch: currentBranch);
				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "ACT", branch: nonCurrentBranch);

				TestCharge.JR_GB = nonCurrentBranch.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					TestCharge.JR_GB = Env.CurrentBranchPK;
					AssertEquals("WA", TestCharge.JR_CostPlaceOfSupply);
				}
			}
		}

		public void TestCostPlaceOfSupplyDefaulting_TaxZone()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.TaxZone.Code))
			{
				var shipment = TestObjectCreator.CreateShipment("S00001", "CATOR", "JPTYO");
				var job = TestObjectCreator.CreateJob(shipment, false);
				Factory.Save();
				TestCharge.JR_JH = job.PK;
				Assert("Pre-condition: not apportioned", !TestCharge.JR_IsApportioned);

				Assert(TestCharge.JR_AC.IsEmpty);
				Assert(TestCharge.JR_OH_CostAccount.IsEmpty);
				Assert(TestCharge.JR_CostPlaceOfSupply.IsEmpty);

				var currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
				var nonCurrentBranch = TestObjectCreator.NonCurrentBranch;
				nonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("OBR", true, true).PK;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.TaxZone.Code, "ONTZ", branch: currentBranch);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(TestCharge.JR_OH_CostAccount.IsEmpty);
				Assert(TestCharge.JR_CostPlaceOfSupply.IsEmpty);

				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals("ONTZ", TestCharge.JR_CostPlaceOfSupply);

				TestCharge.JR_CostPlaceOfSupply = "HSTC";
				TestCharge.JR_AC = TestCharge.JR_AC;
				TestCharge.JR_OH_CostAccount = TestCharge.JR_OH_CostAccount;
				AssertEquals("HSTC", TestCharge.JR_CostPlaceOfSupply);

				TestCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("ONTZ", TestCharge.JR_CostPlaceOfSupply);

				TestCharge.JR_CostPlaceOfSupply = "QUBC";
				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
				AssertEquals("ONTZ", TestCharge.JR_CostPlaceOfSupply);

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.TaxZone.Code, "QUBC", branch: currentBranch);
				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.TaxZone.Code, "HSTC", branch: nonCurrentBranch);

				TestCharge.JR_GB = nonCurrentBranch.PK;
				AssertEquals("ONTZ", TestCharge.JR_CostPlaceOfSupply);

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					TestCharge.JR_GB = Env.CurrentBranchPK;
					AssertEquals("QUBC", TestCharge.JR_CostPlaceOfSupply);
				}
			}
		}

		public void TestCostTaxRateUpdatedOnDefaultingSameCostPlaceOfSupply()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			var nonCurrentBranch = TestObjectCreator.NonCurrentBranch;

			TestCharge.JR_JH = job.PK;
			Assert("Pre-condition: not apportioned", !TestCharge.JR_IsApportioned);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code))
			{
				AssertEquals(currentBranch.PK, TestCharge.JR_GB);
				Assert(TestCharge.JR_AC.IsEmpty);
				Assert(TestCharge.JR_OH_CostAccount.IsEmpty);
				Assert(TestCharge.JR_CostPlaceOfSupply.IsEmpty);
				Assert(TestCharge.JR_AT_CostGSTRate.IsEmpty);

				nonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("OBR", true, true).PK;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: currentBranch);

				TestCharge.JR_AC = TestObjectCreator.CC1.PK;
				TestCharge.JR_CostPlaceOfSupply = "NSW";
				Assert(TestCharge.JR_OH_CostAccount.IsEmpty);
				Assert(TestCharge.JR_AT_CostGSTRate.IsEmpty);

				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_CostGSTRate);

				TestCharge.JR_AT_CostGSTRate = ZGuid.Empty;

				TestCharge.JR_AC = TestObjectCreator.CC10.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_CostGSTRate);

				TestCharge.JR_AT_CostGSTRate = ZGuid.Empty;

				AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(TestCharge.InvoicingJob.PlugInData, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: nonCurrentBranch);

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					TestCharge.JR_GB = nonCurrentBranch.PK;
					AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);
					AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_CostGSTRate);
				}

				TestCharge.JR_AT_CostGSTRate = ZGuid.Empty;

				TestCharge.JR_GB = currentBranch.PK;
				AssertEquals("NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals(TestObjectCreator.GST1.PK, TestCharge.JR_AT_CostGSTRate);
			}
		}

		public void TestClearPlaceOfSupplyWhenNoMatchingResult()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			AssertEquals(AccountingMasterFilesRegistry.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, AccountingMasterFilesRegistry.Instance.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.Value);
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: branch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "NSW", branch: GlbBranch.CurrentBranch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "WA", org: TestObjectCreator.Debtor1);

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			TestCharge.JR_JH = job.PK;
			Assert("Pre-condition: not apportioned", !TestCharge.JR_IsApportioned);

			TestCharge.JR_OH_CostAccount = ZGuid.Empty;
			TestCharge.JR_OH_SellAccount = ZGuid.Empty;
			TestCharge.JR_CostPlaceOfSupply = ZString.Empty;
			TestCharge.JR_SellPlaceOfSupply = ZString.Empty;
			AssertEquals("PreCondition", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
			AssertEquals("PreCondition", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);

			TestCharge.JR_AC = TestObjectCreator.CC1.PK;
			TestCharge.JR_GB = branch.PK;
			TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
			AssertPosMatchingWithChargeCode();

			TestCharge.JR_AC = ZGuid.Empty;
			AssertPosMatchingWithoutChargeCode();

			TestCharge.JR_AC = TestObjectCreator.CC1.PK;
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Revenue, PlaceOfSupplyTypes.State.Code, "StateNotExisted", org: TestObjectCreator.Debtor1);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "StateNotExisted", branch: branch);
			AccPlaceOfSupplyTestHelper.SetUpPOSConfigurationToReturn_ForTestOnly(null, CostSell.Cost, PlaceOfSupplyTypes.State.Code, "StateNotExisted", branch: GlbBranch.CurrentBranch);
			AssertPostmatchingWithInvalidState();

			void AssertPosMatchingWithChargeCode()
			{
				AssertNotEquals("PreCondition", ZGuid.Empty, TestCharge.JR_AC);
				AssertEquals("PreCondition", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("PreCondition", "WA", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_GB = ZGuid.Empty;
				AssertEquals("Should not be empty when matching result not empty.(it will match result with loged-in branch)", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should not be empty when matching result not empty", "WA", TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_GB = branch.PK;
				AssertEquals("Should not be empty when matching result not empty", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should not be empty when matching result not empty", "WA", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_OH_CostAccount = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should not be empty when matching result not empty", "WA", TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals("Should not be empty when matching result not empty", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should not be empty when matching result not empty", "WA", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Should not be empty when matching result not empty", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				AssertEquals("Should not be empty when matching result not empty", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should not be empty when matching result not empty", "WA", TestCharge.JR_SellPlaceOfSupply);
			}

			void AssertPostmatchingWithInvalidState()
			{
				AssertNull("PreCondition", branch.OrgProxy.MainAddress.RelatedState);
				AssertNull("PreCondition", GlbBranch.CurrentBranch.OrgProxy.MainAddress.RelatedState);
				AssertNull("PreCondition", TestObjectCreator.Debtor1.MainAddress.RelatedState);
				AssertNotEquals("PreCondition", ZGuid.Empty, TestCharge.JR_AC);
				AssertEquals("PreCondition", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("PreCondition", "WA", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Should not be empty when matching result not empty", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				AssertEquals("Should not be empty when matching result not empty", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty, that debtor's state is not existed", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_OH_CostAccount = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals("Should be empty when matching result empty, that branch's state is not existed", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_GB = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_GB = branch.PK;
				AssertEquals("Should be empty when matching result empty, that branch's state is not existed", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
			}

			void AssertPosMatchingWithoutChargeCode()
			{
				AssertEquals("PreCondition", ZGuid.Empty, TestCharge.JR_AC);
				AssertEquals("PreCondition", "NSW", TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("PreCondition", "WA", TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_GB = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_GB = branch.PK;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_OH_CostAccount = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);

				TestCharge.JR_OH_SellAccount = ZGuid.Empty;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
				TestCharge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_CostPlaceOfSupply);
				AssertEquals("Should be empty when matching result empty", ZString.Empty, TestCharge.JR_SellPlaceOfSupply);
			}
		}

		public virtual void TestJR_RL_NKOrigin()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_RL_NKOrigin = "AUSYD";

			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = Factory.New<Charge>();
				charge.JR_JH = job.PK;
				AssertEquals("Charge Origin property should read from underlying shipment", "AUSYD", charge.JR_RL_NKOrigin);
			}
		}

		public virtual void TestJR_RL_NKDestination()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_RL_NKDestination = "NZAKL";

			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = Factory.New<Charge>();
				charge.JR_JH = job.PK;
				AssertEquals("Charge Destination property should read from underlying shipment", "NZAKL", charge.JR_RL_NKDestination);
			}
		}

		protected BaseCharge PrepareTaxDefaultDataSetupForIndia(bool creditorDebtorAndOrgProxyAreInSameState = true, bool addALXRule = true)
		{
			if (creditorDebtorAndOrgProxyAreInSameState)
			{
				TestObjectCreator.ActiveOrg.MainAddress.OA_RL_NKRelatedPortCode = TestObjectCreator.Creditor1.MainAddress.OA_RL_NKRelatedPortCode = TestObjectCreator.Debtor1.MainAddress.OA_RL_NKRelatedPortCode = "INDEL";
				TestObjectCreator.ActiveOrg.MainAddress.OA_RN_NKCountryCode = TestObjectCreator.Creditor1.MainAddress.OA_RN_NKCountryCode = TestObjectCreator.Debtor1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
				TestObjectCreator.ActiveOrg.MainAddress.OA_State = TestObjectCreator.Creditor1.MainAddress.OA_State = TestObjectCreator.Debtor1.MainAddress.OA_State = "DL";
			}
			else
			{
				TestObjectCreator.ActiveOrg.MainAddress.OA_RL_NKRelatedPortCode = "INDEL";
				TestObjectCreator.ActiveOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
				TestObjectCreator.ActiveOrg.MainAddress.OA_State = "DL";

				TestObjectCreator.Creditor1.MainAddress.OA_RL_NKRelatedPortCode = TestObjectCreator.Debtor1.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				TestObjectCreator.Creditor1.MainAddress.OA_RN_NKCountryCode = TestObjectCreator.Debtor1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				TestObjectCreator.Creditor1.MainAddress.OA_State = TestObjectCreator.Debtor1.MainAddress.OA_State = "NSW";
			}

			TestObjectCreator.Debtor1.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var chargeCode = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);

			var taxOverride1 = chargeCode.TaxOverrides.AddNew();
			taxOverride1.AO_ParentID = chargeCode.PK;
			taxOverride1.AO_CostSellAll = "ALL";
			taxOverride1.AO_Direction = "ALL";
			taxOverride1.AO_IncoTerm = "ALL";
			taxOverride1.AO_JobType = "ALL";
			taxOverride1.AO_Origin = "ALL";
			taxOverride1.AO_Destination = "ALL";
			taxOverride1.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride1.AO_HomeCountryOrZone = "BST";
			taxOverride1.AO_AT = TestObjectCreator.GST1WithDates.PK;

			var taxOverride2 = chargeCode.TaxOverrides.AddNew();
			taxOverride2.AO_ParentID = chargeCode.PK;
			taxOverride2.AO_CostSellAll = "ALL";
			taxOverride2.AO_Direction = "ALL";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_JobType = "ALL";
			taxOverride2.AO_Origin = "ALL";
			taxOverride2.AO_Destination = "ALL";
			taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride2.AO_HomeCountryOrZone = "BSX";
			taxOverride2.AO_AT = TestObjectCreator.GST2WithDates.PK;

			if (addALXRule)
			{
				var taxOverride3 = chargeCode.TaxOverrides.AddNew();
				taxOverride3.AO_ParentID = chargeCode.PK;
				taxOverride3.AO_CostSellAll = "ALL";
				taxOverride3.AO_Direction = "ALL";
				taxOverride3.AO_IncoTerm = "ALL";
				taxOverride3.AO_JobType = "ALL";
				taxOverride3.AO_Origin = "ALL";
				taxOverride3.AO_Destination = "ALL";
				taxOverride3.AO_TaxRegCntryOrGroup = "ALL";
				taxOverride3.AO_HomeCountryOrZone = "ALX";
				taxOverride3.AO_AT = TestObjectCreator.GSTANDQST1WithDates.PK;
			}

			var quotedBooking = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			var job = TestObjectCreator.CreateJob(quotedBooking, false);
			var charge = GetChargeWithValidData(job, chargeCode);
			charge.SuspendValidation();
			charge.JR_AC = chargeCode.PK;
			charge.InvoicingJob.Parent = quotedBooking;
			charge.InvoicingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			charge.InvoicingJob.JH_OA_LocalChargesAddr = TestObjectCreator.Creditor1.MainAddress.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor1.PK;

			Factory.Save();

			return charge;
		}

		AccTaxRate CreateTaxRate()
		{
			var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
			rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
			return rate;
		}

		public void TestRelatedWIPAmountShouldBeEqualsToChargeAmountWhenCFXIsChanged()
		{
			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var orgPK = TestObjectCreator.AALSHI;
				orgPK.OH_IsDebtor = true;

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment, false);

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 9909.2m, 9909.2m);
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
				charge.JR_OH_SellAccount = orgPK.PK;

				var exchangeRate = TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0.6534m);
				exchangeRate.JF_OH_Org = orgPK.PK;
				exchangeRate.JF_CFXPercent = 3m;
				exchangeRate.JF_OrgType = "DEB";
				Factory.Save();

				var postManager = new InvoicingPostManager(job);
				var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue).GetAllARTransactions();
				AssertEquals("Pre-condition", 1, transactions.Length);
				Factory.Save();

				exchangeRate.JF_BaseRate = 0.6855m;
				exchangeRate.JF_CFXPercent = 0m;
				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var transaction = newFactory.Load<ARInvoice>(transactions[0].PK);

				new TestObjectCreator(newFactory).ReverseTransaction(transaction, out string reverseError);
				AssertNull("Reversing is successful", reverseError);

				AssertNoExceptionThrown("Should not be getting 'Related REV amount is not the same as charge amount'", newFactory.Save);
				AssertEquals("JR_CFXAmt should be 0.", 0m, charge.JR_CFXAmt);
				AssertEquals("AL_LineAmount should be -9909.2.", -9909.2m, charge.ARLine.AL_LineAmount);
				AssertEquals("JR_LocalSellInvoiceAmt should be 9909.", 9909.2m, charge.JR_LocalSellInvoiceAmt);
			}
		}

		public void TestCanDeleteIfJobReadyForFinancialClosure()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 1m, 1m);
			Factory.Save();
			AssertEquals("", charge.ReasonForNotAbleToDelete);

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;

			var securityRightForModifyChargeWhichJobIsJFC = Env.Security.ModifyChargesforFinancialClosureJob;
			var oldValue = securityRightForModifyChargeWhichJobIsJFC.IsAllowed;

			using (new DisposableAction(() => securityRightForModifyChargeWhichJobIsJFC.IsAllowed = false, () => securityRightForModifyChargeWhichJobIsJFC.IsAllowed = oldValue))
			{
				AssertEquals("Job.IsReadyForRevenuePosting", true, job.IsReadyForFinancialClosureWithoutModifySecurity);
				AssertContains(AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage, charge.ReasonForNotAbleToDelete);
			}

			using (new DisposableAction(() => securityRightForModifyChargeWhichJobIsJFC.IsAllowed = true, () => securityRightForModifyChargeWhichJobIsJFC.IsAllowed = oldValue))
			{
				AssertEquals("Job.IsReadyForRevenuePosting", false, charge.Job.IsReadyForFinancialClosureWithoutModifySecurity);
				AssertEquals("", charge.ReasonForNotAbleToDelete);
			}
		}

		#region JobChargeIsChangedByDifferentCompany

		public void TestReportJobChargeIsChangedByDifferentCompany_When_ChargeIsChangedBeforeCallingOnFactorySavingBeforeTransactionCore()
		{
			var charge = GetNewBusinessObjectSafeSaving() as BaseCharge;
			Factory.Save();
			AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_Desc += "A";
			Factory.Save();
			AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_Desc += "A";
			using (Factory.SetTempContext(BusinessContext.InvoicingPlugInGUI))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}
			AssertReportJobChargeIsChangedByDifferentCompany(
				expectedUserContextCompany: TestObjectCreator.NonCurrentCompanyBranch.Company
				, charge
				, "Factory Level : (InvoicingPlugInGUI)");
		}

		public void TestReportJobChargeIsChangedByDifferentCompany_HasBeenValidatedByDifferentCompany()
		{
			var charge = GetNewBusinessObjectSafeSaving() as BaseCharge;
			Factory.Save();
			AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_Desc += "A";
			using (Factory.SetTempContext(BusinessContext.InvoicingPlugInGUI))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				charge.RunPreSaveValidation();
				Factory.Save();
			}
			AssertReportJobChargeIsChangedByDifferentCompany(
				expectedUserContextCompany: TestObjectCreator.NonCurrentCompanyBranch.Company
				, charge
				, "Factory Level : (InvoicingPlugInGUI),BizObj Level : (HasBeenValidatedByDifferentCompany)");
		}

		public void TestReportJobChargeIsChangedByDifferentCompany_RunInServiceTask()
		{
			var charge = GetNewBusinessObjectSafeSaving() as BaseCharge;
			Factory.Save();
			AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_Desc += "A";
			using (Env.Instance.TemporaryServiceTaskContext("AAA", true))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}
			AssertReportJobChargeIsChangedByDifferentCompany(
				expectedUserContextCompany: TestObjectCreator.NonCurrentCompanyBranch.Company
				, charge
				, expectedBusinessContextInfo: "None");
		}

		public void TestReportJobChargeIsChangedByDifferentCompany_DoesNotReport_WhenChargeModifiedInOnFactorySavingBeforeTransactionCore()
		{
			var charge = GetNewBusinessObjectSafeSaving() as BaseCharge;
			Factory.Save();
			AssertEquals("PreCondition", 0, ExceptionReporterTestListener.Instance.Count);

			charge.OnFactorySavingBeforeTransactionCore_InvokeTestOnly += (JobCharge jobCharge) => jobCharge.JR_Desc += "A";

			using (Factory.SetTempContext(BusinessContext.InvoicingPlugInGUI))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestJobChargeCreatedInCurrentCompany_SkipsValidationInDifferentCompany()
		{
			var charge = GetNewBusinessObjectSafeSaving() as BaseCharge;
			var chargeCode = TestObjectCreator.CreateChargeCode("MRG", "Test Charge Code", Core.Constants.ChargeType.Margin, 1m, null, null);
			charge.JR_AC = chargeCode.PK;
			charge.RunPreSaveValidation();
			AssertNoErrors("Expected no errors in JR_GBInfo after validation in the current company", charge.JR_GBInfo);
			AssertNoErrors("Expected no errors in JR_ACInfo after validation in the current company", charge.JR_ACInfo);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				charge.RunPreSaveValidation();
				AssertNoErrors("Expected no errors in JR_GBInfo after validation in a different company context", charge.JR_GBInfo);
				AssertNoErrors("Expected no errors in JR_ACInfo after validation in a different company context", charge.JR_ACInfo);
				Assert(charge.HasContext(BusinessContext.HasBeenValidatedByDifferentCompany));
			}
		}

		void AssertReportJobChargeIsChangedByDifferentCompany(GlbCompany expectedUserContextCompany, BaseCharge expectedReportedCharge, string expectedBusinessContextInfo)
		{
			AssertEquals("LastKeyReported", "JobChargeIsChangedByDifferentCompany_4", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			AssertContainsInOrder("LastMessageReported", ExceptionReporterTestListener.Instance.GetExceptionMessage(0)
				, "Charge is not designed to be changed in different company."
				, "PK = "
				, $"Business Contexts = {expectedBusinessContextInfo}"
				, "Fields with changes:"
				, "Constructor StackTrace:\r\n   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)");

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestJR_InvoiceType_PropertySetterCollects_JobChargeJR_InvoiceType_PropertyValueSet()
		{
			AssertPropertySetterCollectsInfo((charge) => { charge.JR_InvoiceType = "FRT"; }, ZString.Empty, (ZString)"FRT", CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_InvoiceType_PropertyValueSet);
		}

		public void TestJR_OH_CostAccount_PropertySetterCollects_JobChargeJR_OH_CostAccount_PropertyValueSet()
		{
			var newGuid = ZGuid.NewZGuid();
			AssertPropertySetterCollectsInfo((charge) => { charge.JR_OH_CostAccount = newGuid; }, ZGuid.Empty, newGuid, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OH_CostAccount_PropertyValueSet);
		}

		public void TestJR_CostReference_PropertySetterCollects_JobChargeJR_CostReference_PropertyValueSet()
		{
			AssertPropertySetterCollectsInfo((charge) => { charge.JR_CostReference = "REF"; }, ZString.Empty, (ZString)"REF", CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_CostReference_PropertyValueSet);
		}

		public void TestJR_OH_SellAccount_PropertySetterCollects_JobChargeJR_OH_SellAccount_PropertyValueSet()
		{
			var newGuid = ZGuid.NewZGuid();
			AssertPropertySetterCollectsInfo((charge) => { charge.JR_OH_SellAccount = newGuid; }, ZGuid.Empty, newGuid, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OH_SellAccount_PropertyValueSet);
		}

		public void TestJR_E6_PropertySetterCollects_JobChargeJR_E6_PropertyValueSet()
		{
			var newGuid = ZGuid.NewZGuid();
			AssertPropertySetterCollectsInfo((charge) => { charge.JR_E6 = newGuid; }, ZGuid.Empty, newGuid, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_E6_PropertyValueSet);
		}

		public void TestJR_GC_PropertySetterCollects_JobChargeJR_JCNotEqualToCurrentCompany()
		{
			var newCompany = ZGuid.NewZGuid();
			var charge = (BaseCharge)GetNewBusinessObject();
			AssertContains("When key is accessed for the first time", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_JCNotEqualToCurrentCompany));

			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals("\r\nJobChargeJR_JCNotEqualToCurrentCompany: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_JCNotEqualToCurrentCompany));

			var oldCompany = charge.JR_GC;
			charge.JR_GC = newCompany;

			AssertContains("When key is accessed for the second time", $@"OldValue: {oldCompany}, NewValue: {newCompany}
   at System.Environment.GetStackTrace", CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_JCNotEqualToCurrentCompany));
		}

		void AssertPropertySetterCollectsInfo<T>(Action<BaseCharge> propertySetter, T expectedOldValue, T expectedNewValue, CriticalValidationInfoCollectorServiceKeyType keyType)
		{
			var charge = (BaseCharge)GetNewBusinessObject();
			AssertContains("When key is accessed for the first time", "There is no data collected for this PK", CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(charge.PK, keyType));

			propertySetter(charge);
			AssertContains("When key is accessed for the second time", $@"OldValue: {expectedOldValue}, NewValue: {expectedNewValue}
   at System.Environment.GetStackTrace", CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(charge.PK, keyType));
		}

		public void TestJobChargeJR_InvoiceType_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("JobChargeJR_InvoiceType_PropertyValueSet:");
		}

		public void TestJobChargeJR_OH_CostAccount_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("JobChargeJR_OH_CostAccount_PropertyValueSet:");
		}

		public void TestJobChargeJR_CostReference_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("JobChargeJR_CostReference_PropertyValueSet:");
		}

		public void TestJobChargeJR_OH_SellAccount_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("JobChargeJR_OH_SellAccount_PropertyValueSet:");
		}

		public void TestJobChargeJR_E6_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("JobChargeJR_E6_PropertyValueSet:");
		}

		public void TestJobChargeJR_GC_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("JobChargeJR_JCNotEqualToCurrentCompany:");
		}

		public void TestJobChargeJR_OSSellExRate_PropertyValueSet_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("OsSellExRateGetChangedDuringRunningNonAccountingCode:");
		}

		public void TestJobChargeConsolCost_CreationStackTrace_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany_Default()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("Parent JobConsolCost Constructor StackTrace:\r\nParent ConsolCost had not be loaded or not existed or the \"Collect Constructor Call Stack Details\" registry is not enabled."
				, (charge) => {
					AssertEquals("PreCondition", false, charge.JR_E6.IsValid);
					return null;
				});
		}

		public void TestJobChargeConsolCost_CreationStackTrace_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany_TracingOn()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("Parent JobConsolCost Constructor StackTrace:\r\n   at Enterprise.Accounting.Business.ConsolCosting.JobConsolCost..ctor"
					, (charge) => {
						charge.JR_E6 = cost.PK;
						AssertNotNull("PreCondition", charge.ParentConsolCost);
						return null;
					});
			}
		}

		public void TestJobChargeConsolCost_UserContextSwitchLog_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany_Default()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("UserContextSwitchLog:\r\nNo UserContextSwitchLog");
		}

		public void TestJobChargeConsolCost_UserContextSwitchLog_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany_TracingOn()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("UserContextSwitchLog:\r\nOldUserContext Company: EDI, Branch: BNE, User: CWSupportNewUserContext Company: DEM, Branch: DEM, User: CWSupport   at Enterprise.Environment.Env"
				, (_) => {
					var tracer = Env.StartContextSwitchTrace(new UserContextSwitchLogger());
					var tempContext = Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK);
					return new DisposableAction(() => {
						tempContext.Dispose();
						tracer.Dispose();
					});
				});
		}

		public void TestJobChargeConsolCost_OsSellExRate_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany_Default()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("OsSellExRateGetChangedDuringRunningNonAccountingCode: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.");
		}

		public void TestJobChargeConsolCost_OsSellExRate_InfoAddedToErrorReportJobChargeIsChangedByDifferentCompany_TracingOn()
		{
			AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany("OsSellExRateGetChangedDuringRunningNonAccountingCode:\r\n   at Enterprise.MasterFiles.Business.JobCharge.<>c.<set_JR_OSSellExRate>"
				, (charge) => {
					var tempContext = Factory.SetTempContext(BusinessContext.NonAccountingCode);
					charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
					charge.JR_OSSellExRate += 0.1M;
					return new DisposableAction(() => {
						tempContext.Dispose();
					});
				});
		}

		public void AssertInfoCollectedOnProperty_AddedToErrorReportJobChargeIsChangedByDifferentCompany(string infoCollected, Func<BaseCharge, IDisposable> additioalSetting = null)
		{
			var charge = GetNewBusinessObjectSafeSaving() as BaseCharge;
			charge.JR_Desc += "A";

			using (additioalSetting?.Invoke(charge))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Factory.Save();
			}

			AssertEquals("pre-condition", "JobChargeIsChangedByDifferentCompany_4", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			AssertContains("Exception message content", $"{infoCollected}", ExceptionReporterTestListener.Instance.GetExceptionMessage(0));
			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region Implementation

		BaseCharge fTestCharge;
		protected BaseCharge TestCharge
		{
			get { return fTestCharge ?? (fTestCharge = (BaseCharge)GetNewBusinessObject()); }
		}

		protected abstract BaseCharge GetChargeWithValidData(Job invoicingJob, AccChargeCode chargeCode);

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var charge = (BaseCharge)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			charge.StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender();
			charge.JR_IsCostTaxAmountOverridden = true;

			return charge;
		}

		protected override void SetUp()
		{
			base.SetUp();

			fTestCharge = null;
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		protected override void TearDown()
		{
			base.TearDown();

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		public class OneOffQuoteHost : ForwardingShipment, IJobInvoicingPlugIn
		{
			public OneOffQuoteHost(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
			{
				return new OneOffQuoteHostInvoicingSupporter(this);
			}
		}

		public class OneOffQuoteHostInvoicingSupporter : ForwardingShipment.ForwardingShipmentInvoicingSupporter
		{
			public OneOffQuoteHostInvoicingSupporter(OneOffQuoteHost parent)
				: base(parent)
			{
			}

			public override JobInvoicingConsumerType ConsumerType
			{
				get { return JobInvoicingConsumerTypes.OneOffQuotation; }
			}

			public override ZString DefaultChargeGroup
			{
				get
				{
					return fDefaultChargeGroup;
				}
			}
			public ZString fDefaultChargeGroup;

			public delegate OrgHeader DefaultOrgAccessor(DefaultCreditorSetting defaultCreditorSetting);

			public override OrgHeader GetDefaultDebtor(AccChargeCode chargeCode, JobHeader job, ZString relatedJobNumber) { return fDefaultDebtor ?? base.GetDefaultDebtor(chargeCode, job, relatedJobNumber); }

			public OrgHeader fDefaultDebtor;

			public DefaultOrgAccessor DefaultCreditor { get; set; }
			public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
			{
				return DefaultCreditor == null ? null : DefaultCreditor(defaultCreditorSetting);
			}
		}

		protected BaseCharge fBaseCharge;
		protected virtual BaseCharge ABaseCharge
		{
			get { return fBaseCharge ?? (fBaseCharge = TestJob.Charges.AddNew()); }
		}

		protected Job fTestJob;
		protected Job TestJob
		{
			get
			{
				if (fTestJob == null)
				{
					fTestJob = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0, null, 0);
					fTestJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				}
				return fTestJob;
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		class TestingBaseCharge : BaseCharge
		{
			public TestingBaseCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnCostExchangeRateChangedCore(object sender, EventArgs e)
			{
				OnCostExchangeRateChangedCalledCount++;
				base.OnCostExchangeRateChangedCore(sender, e);
			}

			public int OnCostExchangeRateChangedCalledCount { get; private set; }

			protected override void OnRevenueExchangeRateChangedCore(object sender, EventArgs e)
			{
				OnRevenueExchangeRateChangedCalledCount++;
				base.OnRevenueExchangeRateChangedCore(sender, e);
			}

			public int OnRevenueExchangeRateChangedCalledCount { get; private set; }

			protected override void OnSellInvoiceExchangeRateChangedCore(object sender, EventArgs e)
			{
				OnSellInvoiceExchangeRateChangedCalledCount++;
				base.OnSellInvoiceExchangeRateChangedCore(sender, e);
			}

			protected internal override ZDecimal CalculateCFXAmt()
			{
				throw new NotImplementedException();
			}

			public int OnSellInvoiceExchangeRateChangedCalledCount { get; private set; }
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}
}
