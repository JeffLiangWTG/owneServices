namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	public partial class CASSBillingTest
	{
		public void TestCreateCASSClaimFor_MAWBPresentInSystem_CASSBillingLineStatusIsOverBilled_CASSRegistrySettingToOverBilled()
		{
			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 30.18m);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			AssertCreateCASSClaim(1450.00M, "CAO", true, shouldDeleteShipment2Job: true);
		}

		public void TestCreateCASSClaimFor_MAWBPresentInSystem_CASSBillingLineStatusIsOverBilled_CASSRegistrySettingToOverBilled_LessThenAllowedDiscrepancy()
		{
			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 30.19m);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			AssertCreateCASSClaim(1450.00M, "CAO", false);
		}

		public void TestCreateCASSClaim_CreateInvoice_Then_PostTransactions()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			AssertCreateCASSClaim(600.00M, "CAU", true, createInvoices: true);
		}

		public void TestCreateCASSClaim_CreateInvoice_Then_PostTransactions_LessThenAllowedDiscrepancy()
		{
			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 820);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			AssertCreateCASSClaim(600.00M, "CAU", false, createInvoices: true, shouldDeleteShipment2Job: true);
		}

		public void TestCreateCASSClaimFor_MAWBPresentInSystem_CASSBillingLineStatusIsUnderBilled_CASSRegistrySettingToBothOverAndUnderBilled()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			AssertCreateCASSClaim(600.00M, "CAU", true);
		}

		public void TestCreateCASSClaimFor_MAWBNotInSystem()
		{
			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 480.18m);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			AssertCreateCASSClaim(600.00M, "CAM", true, isMAWBDifferent: true);
		}

		public void TestCreateCASSClaimFor_MAWBNotInSystem_LessThenAllowedDiscrepancy()
		{
			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 480.19m);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			AssertCreateCASSClaim(600.00M, "CAM", false, isMAWBDifferent: true);
		}

		public void TestNoClaimCreated_For_CASSAmendment()
		{
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine("172"), TestObjectCreator.AALSHI);
			Factory.Save();

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: true);
			SetupAssociatedBizos(true);
			TestObjectCreator.AddActiveContactIfRequires(TestCASSBilling.Lines[0].Creditor);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertNoClaimInDataBase();
			TestCASSBilling.PostAPTransactions();
			AssertNoClaimInDataBase();
		}

		[TestDate(2018, 06, 27)]
		public void TestUpdateCASSClaim_DisplayWarning_For_ClaimAlreadyExist_CASSBillDifferenceNotEqualExistingClaimAmount()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.OverBilled);
			var awbNumber = "67828073";

			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);
			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber, 400M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));
			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();

			var adjustmentBilling = new CASSBilling(Factory);
			SetupCASSBilling(adjustmentBilling, 0);
			CreateBillingLine(adjustmentBilling, awbNumber, 350M, 400M);
			adjustmentBilling.OverrideAutoClosureOfCASSBillingClaim = true;
			adjustmentBilling.PostAPTransactions();
			AssertClaimUpdatedWithExpectedStatus("CCR", 50M);

			var expected = FormattableString.Invariant($@"{ZDateTime.Now.ToLongTimeString()} {GlbStaff.CurrentUser.GS_Code} - {GlbCompany.CurrentCompany.GC_Code} - MAWB billing amended by DCR/CCR records in the CASS file. However, billing discrepancy still exists and claim record is now linked to the latest billing.
MAWB : 17267828073
Issue Date : 27-Mar-18
Airline : CV
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 0.0000 AUD
Net CASS Charges : -50 AUD
Is CASS Amendment : Y
Status : Underbilled
{ZString.Replicate('-', 118)}
{ZDateTime.Now.ToLongTimeString()} {GlbStaff.CurrentUser.GS_Code} - {GlbCompany.CurrentCompany.GC_Code} - Claim Created for CASS over-billing of charges on 17267828073. The variance amount is AUD 100.0000
MAWB : 17267828073
Issue Date : 27-Mar-18
Airline : CV
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 300.0000 AUD
Net CASS Charges : 400 AUD
Is CASS Amendment : N
Status : Overbilled
{ZString.Replicate('-', 118)}");

			AssertClaimAppendLogEquals(expected);
		}

		[TestDate(2018, 06, 27)]
		public void TestUpdateCASSClaim_DisplayWarning_For_ClaimAlreadyExist_CASSBillDifferenceEqualsToExistingClaimAmount_AutoCloseAccepted()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.OverBilled);
			var awbNumber = "67828073";

			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);
			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber, 400M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));
			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();

			var adjustmentBilling = new CASSBilling(Factory);
			SetupCASSBilling(adjustmentBilling, 0);
			CreateBillingLine(adjustmentBilling, awbNumber, 300M, 400M);
			adjustmentBilling.OverrideAutoClosureOfCASSBillingClaim = true;
			adjustmentBilling.PostAPTransactions();
			AssertClaimUpdatedWithExpectedStatus("CCA", 100M);

			var expected = FormattableString.Invariant($@"{ZDateTime.Now.ToLongTimeString()} {GlbStaff.CurrentUser.GS_Code} - {GlbCompany.CurrentCompany.GC_Code} - MAWB billing amended by DCR/CCR records in the CASS file and billing discrepancy is addressed. Related claim can now be closed.
MAWB : 17267828073
Issue Date : 27-Mar-18
Airline : CV
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 0.0000 AUD
Net CASS Charges : -100 AUD
Is CASS Amendment : Y
Status : Underbilled
{ZString.Replicate('-', 118)}
{ZDateTime.Now.ToLongTimeString()} {GlbStaff.CurrentUser.GS_Code} - {GlbCompany.CurrentCompany.GC_Code} - Claim Created for CASS over-billing of charges on 17267828073. The variance amount is AUD 100.0000
MAWB : 17267828073
Issue Date : 27-Mar-18
Airline : CV
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 300.0000 AUD
Net CASS Charges : 400 AUD
Is CASS Amendment : N
Status : Overbilled
{ZString.Replicate('-', 118)}");

			AssertClaimAppendLogEquals(expected);
		}

		[TestDate(2018, 06, 27)]
		public void TestUpdateCASSClaim_DisplayWarning_For_ClaimAlreadyExist_CASSBillDifferenceEqualsToExistingClaimAmount_AutoCloseNotAccepted()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.OverBilled);
			var awbNumber = "67828073";

			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);
			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber, 400M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));
			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();

			var adjustmentBilling = new CASSBilling(Factory);
			SetupCASSBilling(adjustmentBilling, 0);
			CreateBillingLine(adjustmentBilling, awbNumber, 300M, 400M);
			adjustmentBilling.OverrideAutoClosureOfCASSBillingClaim = false;
			adjustmentBilling.PostAPTransactions();
			AssertClaimUpdatedWithExpectedStatus("CCC", 100M);

			var expected = FormattableString.Invariant($@"{ZDateTime.Now.ToLongTimeString()} {GlbStaff.CurrentUser.GS_Code} - {GlbCompany.CurrentCompany.GC_Code} - MAWB billing amended by DCR/CCR records in the CASS file and billing discrepancy is addressed. Related claim is now closed.
MAWB : 17267828073
Issue Date : 27-Mar-18
Airline : CV
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 0.0000 AUD
Net CASS Charges : -100 AUD
Is CASS Amendment : Y
Status : Underbilled
{ZString.Replicate('-', 118)}
{ZDateTime.Now.ToLongTimeString()} {GlbStaff.CurrentUser.GS_Code} - {GlbCompany.CurrentCompany.GC_Code} - Claim Created for CASS over-billing of charges on 17267828073. The variance amount is AUD 100.0000
MAWB : 17267828073
Issue Date : 27-Mar-18
Airline : CV
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 300.0000 AUD
Net CASS Charges : 400 AUD
Is CASS Amendment : N
Status : Overbilled
{ZString.Replicate('-', 118)}");

			AssertClaimAppendLogEquals(expected);
		}

		public void TestIsAutoCloseClaimOptionVisible()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.OverBilled);
			var awbNumber = "67828073";

			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);
			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber, 400M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));
			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();

			var adjustmentBilling = new CASSBilling(Factory);
			SetupCASSBilling(adjustmentBilling, 0);
			CreateBillingLine(adjustmentBilling, awbNumber, 300M, 400M);
			AssertEquals("Auto Close Claim Option should visible.", true, adjustmentBilling.ClaimExistsAndNeedsToBeClosed);

			adjustmentBilling = new CASSBilling(Factory);
			SetupCASSBilling(adjustmentBilling, 0);
			CreateBillingLine(adjustmentBilling, awbNumber, 350M, 400M);
			AssertEquals("Auto Close Claim Option should invisible.", false, adjustmentBilling.ClaimExistsAndNeedsToBeClosed);
		}

		public void TestSameAPInvoiceCanbeLinekedWithMultipleCASSClaims()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.OverBilled);

			var awbNumber1 = "67828073";
			var awbNumber2 = "67828074";
			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);

			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber1, 400M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));

			var originalBillingLine2 = CreateBillingLine(originalBilling, awbNumber2, 250M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine2, "C0002", ("S003", 100M), ("S004", 100M));

			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();

			var claim1 = LoadAndAssertClaimByMasterBillNumber(originalBillingLine1.MAWBNumber, "OPN", 100M);
			var claim2 = LoadAndAssertClaimByMasterBillNumber(originalBillingLine2.MAWBNumber, "OPN", 50M);
			AssertEquals("Should be linked to the same invoice", true, claim1.AY_AH == claim2.AY_AH);
			claim2.RunPreSaveValidation();
			AssertNoErrors(claim2);

			var adjustmentBilling = new CASSBilling(Factory);
			SetupCASSBilling(adjustmentBilling, 0);
			var adjustmentBillingLine1 = CreateBillingLine(adjustmentBilling, awbNumber1, 300M, 400M);

			adjustmentBilling.ForceRecalculateData();
			adjustmentBilling.CreateInvoices();
			adjustmentBilling.PostAPTransactions();
			adjustmentBilling.OverrideAutoClosureOfCASSBillingClaim = true;
			claim1 = LoadAndAssertClaimByMasterBillNumber(originalBillingLine1.MAWBNumber, "CCC", 100M);
			claim1.RunPreSaveValidation();
			AssertNoErrors(claim1);
		}

		public void TestAPInvoiceAmountIsNotComparedWithCASSClaimAmountInValidation()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.BothOverUnderBilled);

			var awbNumber1 = "67828073";
			var awbNumber2 = "67828074";
			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);

			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber1, 1500M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));
			var originalBillingLine2 = CreateBillingLine(originalBilling, awbNumber2, 400M, 800M);
			SetupConsolAndJobsForABillingLine(originalBillingLine2, "C0002", ("S003", 150M), ("S004", 150M));
			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();
			var claim1 = LoadAndAssertClaimByMasterBillNumber(originalBillingLine1.MAWBNumber, "OPN", 1200M);
			var invoice = Factory.Load<APInvoice>(claim1.AY_AH);

			Assert("Invoice amount is less than claim amount", invoice.AH_OSTotalAmount < claim1.AY_QueryClaimAmount);
			claim1.RunPreSaveValidation();
			AssertNoErrors(claim1);
		}

		public void TestNoAPInvoiceIsCreatedWhenCASSClaimHasError()
		{
			SetupCASSRelatedRegistriesAndCreditor(TestObjectCreator.AALSHI, "172", CASSAutoCreateClaim.BothOverUnderBilled, addActiveContact: false);

			var awbNumber1 = "67828073";
			var originalBilling = new CASSBilling(Factory);
			SetupCASSBilling(originalBilling, 0);

			var originalBillingLine1 = CreateBillingLine(originalBilling, awbNumber1, 1500M, 0M);
			SetupConsolAndJobsForABillingLine(originalBillingLine1, "C0001", ("S001", 150M), ("S002", 150M));
			Factory.Save();

			originalBilling.ForceRecalculateData();
			originalBilling.CreateInvoices();
			originalBilling.PostAPTransactions();

			AssertHasRowError(originalBilling.APTransactions[0], "Could not create/update claim with MAWB: 17267828073 because of following validation error: \r\nError - AY_OC: Enter a valid Contact Person.");
			var invoiceExists = Factory.ExistsInDatabase(AccTransactionHeader.Schema.TableName, new ZQuery(AccTransactionHeaderSchema.PK, originalBilling.APTransactions[0].PK));
			AssertEquals("Invoice does not exist", false, invoiceExists);
		}

		void AssertCreateCASSClaim(decimal weightChargePP, string expectedClaimType, bool createClaim, bool createInvoices = false, bool isMAWBDifferent = false, bool shouldDeleteShipment2Job = false)
		{
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine("172"), TestObjectCreator.AALSHI);
			TestObjectCreator.AddActiveContactIfRequires(TestObjectCreator.AALSHI);
			Factory.Save();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = weightChargePP;
			if (isMAWBDifferent)
			{
				SetupAssociatedBizosWithDifferentMAWB();
			}
			else
			{
				SetupAssociatedBizos(true);
			}

			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null, 1200, true, AllocationMethod.Shipment);
			var charge2 = consolCost.ApportionmentCharges.FindChargeForJob(Shipment2);
			charge2.JR_IsUsedForApportionment = false;

			if (shouldDeleteShipment2Job)
			{
				var shipment2Job = (Job)Shipment2.Job;
				var shipmentLevelChargeForShipment2 = shipment2Job.Charges.Cast<Charge>().First(x => x.JR_E6.IsEmpty);
				shipmentLevelChargeForShipment2.Delete();
				shipment2Job.Delete();
			}

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();

			if (createInvoices)
			{
				TestCASSBilling.CreateInvoices();
			}
			AssertNoClaimInDataBase();
			TestCASSBilling.PostAPTransactions();
			if (createClaim)
			{
				AssertClaimCreatedWithTypeAndStatus(expectedClaimType, "OPN");
				var expectAmount = (ZDecimal)(-1 * TestCASSBilling.Lines[0].CostDifference);
				AssertClaimAmountShouldEqualToCostDifference(expectAmount);
			}
			else
			{
				AssertNoClaimInDataBase();
			}
		}

		void AssertClaimCreatedWithTypeAndStatus(string expectedClaimType, string expectedStatus)
		{
			var claims = Factory.Load<AccQueryClaim>(new ZQuery());
			AssertEquals("There is 1 claim in the database", 1, claims.Length);
			AssertEquals($"The type of claim should be {expectedClaimType}, status is {expectedStatus}", $"{expectedClaimType} {expectedStatus}", $"{claims[0].AY_QueryClaimType} {claims[0].AY_QueryClaimStatus}");
		}

		void AssertClaimAmountShouldEqualToCostDifference(ZDecimal expected)
		{
			var claims = Factory.Load<AccQueryClaim>(new ZQuery());
			AssertEquals($"Claim amount should equal to expected {expected}", expected, claims[0].AY_QueryClaimAmount);
		}

		void AssertClaimUpdatedWithExpectedStatus(string status, decimal expectedAmount)
		{
			var claim = new BusinessObjectFactory().Load<AccQueryClaim>(new ZQuery())[0];
			AssertEquals($"The status should be {status}", status, $"{claim.AY_QueryClaimStatus}");
			AssertEquals("The amount of claim updated to Original Claim Amount - (Current Billing - Previous Billing)", expectedAmount, claim.AY_QueryClaimAmount);
		}

		AccQueryClaim LoadAndAssertClaimByMasterBillNumber(ZString mawbNumber, string expectedStatus, ZDecimal expectedAmount)
		{
			var claims = new BusinessObjectFactory().Load<AccQueryClaim>(new ZQuery(AccQueryClaimSchema.AY_MasterBillNumber, mawbNumber));
			AssertEquals($"Only one claim should exist in database", 1, claims.Length);
			AssertEquals($"The status should be {expectedStatus}", expectedStatus, $"{claims[0].AY_QueryClaimStatus}");
			AssertEquals("The amount of claim updated to Original Claim Amount - (Current Billing - Previous Billing)", expectedAmount, claims[0].AY_QueryClaimAmount);
			return claims[0];
		}

		void AssertNoClaimInDataBase()
		{
			var claims = Factory.Load<AccQueryClaim>(new ZQuery());
			AssertEquals("There is no claim in the database", 0, claims.Length);
		}

		void AssertClaimAppendLogEquals(string expected)
		{
			var claimLog = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, "AccQueryClaim"));
			AssertEquals("There is 1 claim in the database", 1, claimLog.Length);
			AssertMultilineASCIIEquals(expected, claimLog[0].ST_NoteDataAsText);
		}

		void SetupAssociatedBizosWithDifferentMAWB()
		{
			SetupAssociatedBizos(true);
			Consol.JK_MasterBillNum = "123456";
		}

		(ForwardingConsol consol, Job[] jobs) SetupConsolAndJobsForABillingLine(CASSBillingLine billingLine, string consolNo, params (string shipmentNumber, ZDecimal amount)[] amountsByShipments)
		{
			var origin = RefUNLOCO.LoadFromIATA(Factory, billingLine.LoadPortIATA).RL_Code;
			var destination = RefUNLOCO.LoadFromIATA(Factory, billingLine.DischargePortIATA).RL_Code;
			var (consol, shipmentWithjobs) = SetupConsolAndJobs(consolNo, origin, destination, billingLine.MAWBNumber, billingLine.CASSCostCurrency, BuidJobChargeInfo, amountsByShipments.Select(x => x.shipmentNumber).ToArray());

			shipmentWithjobs.ToList().ForEach(sj => TestObjectCreator.SetExchangeRate(sj.invoicingJob, billingLine.CASSCostCurrency, 1M));

			return (consol, shipmentWithjobs.Select(sj => sj.invoicingJob).ToArray());

			List<(AccChargeCode chargeCode, ZDecimal osAmount, ZDecimal localAmount)> BuidJobChargeInfo(ForwardingShipment shipment)
			{
				var jobChargeInfo = new List<(AccChargeCode chargeCode, ZDecimal osAmount, ZDecimal localAmount)>();
				amountsByShipments.Where(x => x.shipmentNumber == shipment.JS_UniqueConsignRef).Select(x => x.amount).ToList().ForEach(amount => jobChargeInfo.Add((TestObjectCreator.FRT, amount, amount)));
				return jobChargeInfo;
			}
		}

		CASSBillingLine CreateBillingLine(CASSBilling billing, string awbSerialNumber, decimal originalAmount, decimal adjustmentAmount)
		{
			var billingLine = billing.Lines.AddNew();
			TestObjectCreator.SetupCASSCostComponent(billingLine, isAdjustedAmount: false, pWCAmount: originalAmount, pVCAmount: 0M, pCCAmount: 0M, cOAAmount: 0M, cOMAmount: 0M, dOIAmount: 0M, awbSerialNumber: awbSerialNumber);
			if (adjustmentAmount > 0)
			{
				TestObjectCreator.SetupCASSCostComponent(billingLine, isAdjustedAmount: true, pWCAmount: adjustmentAmount, pVCAmount: 0M, pCCAmount: 0M, cOAAmount: 0M, cOMAmount: 0M, dOIAmount: 0M, awbSerialNumber: awbSerialNumber);
			}
			return billingLine;
		}

		void SetupCASSRelatedRegistriesAndCreditor(OrgHeader creditor, string airlinePrefix, string cassImportAutoCreateClaimRegistryValue, bool addActiveContact = true)
		{
			TestObjectCreator.GLHeader1.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			if (!TestObjectCreator.FRT.AC_DepartmentFilterList.Contains(GlbDepartment.CurrentDepartment.GE_Code))
			{
				TestObjectCreator.FRT.AC_DepartmentFilterList += FormattableString.Invariant($", {GlbDepartment.CurrentDepartment.GE_Code}");
			}

			Env.Registry.FreightChargeCode = TestObjectCreator.FRT.PK.ToGuid();

			var registryValue = new CASSFileImportDefaultTaxID();
			registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cassImportAutoCreateClaimRegistryValue);

			creditor.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, airlinePrefix).PK;
			if (addActiveContact)
			{
				TestObjectCreator.AddActiveContactIfRequires(creditor);
			}
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			Factory.Save();
		}
	}
}
