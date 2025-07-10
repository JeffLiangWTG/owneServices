using System;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.Business.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class CASSBillingClaimCreatorTest : TestCaseWithFactory
	{
		#region Test Methods

		public void TestGetClaimWarningMessage_ClaimExistsAndNeedToNotifyUser()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			string expectedMessage = "Claim already exists for this MAWB and will be linked to this new AP invoice.";

			var testCASSBillingLine = new CASSBillingLine(Factory);
			CreateTestAccQueryClaim(testCASSBillingLine, 500M);

			var claim = UpdateTestAccQueryClaim(testCASSBillingLine, 300.00M, 500, true);
			var cassBillingClaim = new CASSBillingClaimCreator(testCASSBillingLine);

			var message = cassBillingClaim.GetClaimWarningMessage();
			AssertEquals(expectedMessage, message);
		}

		public void TestGetClaimWarningMessage_ClaimExistsAndNeedsToBeClosed()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			string expectedMessage = "Claim already exists for this MAWB and will be closed as accepted, unless overridden using the option below.";

			var testCASSBillingLine = new CASSBillingLine(Factory);
			CreateTestAccQueryClaim(testCASSBillingLine, 335.50M);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);

			testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = UpdateTestAccQueryClaim(testCASSBillingLine, 200.00M, 335.50M, true);
			var cassBillingClaim = new CASSBillingClaimCreator(testCASSBillingLine);

			var message = cassBillingClaim.GetClaimWarningMessage();
			AssertEquals(expectedMessage, message);
		}

		public void TestGetClaimType_CAO()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			AccQueryClaim claim = CreateTestAccQueryClaim(testCASSBillingLine, 600.00M);
			AssertEquals("Overbilled should return CAO", QueryClaimTypeCodeList.Codes.QCType5, claim.AY_QueryClaimType);
		}

		public void TestGetClaimType_CAU()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			AssertEquals("Underbilled should return CAU", QueryClaimTypeCodeList.Codes.QCType6, claim.AY_QueryClaimType);
		}

		public void TestGetClaimAmount_NotInEnterprise_ShouldReturnCASSCostValue()
		{
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestNotInEnterpriseAccQueryClaim(testCASSBillingLine, weightChargePP: 100M, masterBillNumber: "12312312");
			AssertEquals("NotInEnterprise should return CASS Cost Value", testCASSBillingLine.CASSCostValue, claim.AY_QueryClaimAmount);
		}

		public void TestGetClaimAmount()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			var expectedAmount = (ZDecimal)(-1 * testCASSBillingLine.CostDifference);
			AssertEquals("should return Amount Difference", expectedAmount, claim.AY_QueryClaimAmount);
		}

		public void TestUpdateClaim()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);

			var testCASSBillingLine = new CASSBillingLine(Factory);
			var existingClaim = CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			var originalAmount = existingClaim.AY_QueryClaimAmount;

			testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = UpdateTestAccQueryClaim(testCASSBillingLine, 180, 100, false);
			var expectedAmount = originalAmount + testCASSBillingLine.NetCASSCost;
			AssertEquals(QueryClaimStatusCodeList.Codes.QCStatus7RejectedWithDCRCCRNotClosed, claim.AY_QueryClaimStatus);
			AssertEquals(expectedAmount, claim.AY_QueryClaimAmount);
		}

		public void TestUpdateClaimThatCanBeClosed_Yes()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var existingClaim = CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);

			testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = UpdateTestAccQueryClaim(testCASSBillingLine, 200.00M, 100, true);
			AssertEquals(QueryClaimStatusCodeList.Codes.QCStatus8AcceptedNotClosed, claim.AY_QueryClaimStatus);
		}

		public void TestUpdateClaimThatCanBeClosed_No()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);

			testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = UpdateTestAccQueryClaim(testCASSBillingLine, 200.00M, 100, false);
			AssertEquals(QueryClaimStatusCodeList.Codes.QCStatus9AcceptedClosed, claim.AY_QueryClaimStatus);
		}

		public void TestCreateClaimLogWithCASSBillingDetails()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			string logDetails =
				$@"MAWB : 17267828073
Issue Date : {testCASSBillingLine.IssueDate.ToShortDateString()}
Airline : 
Load : DELEJ
Discharge : MXMEX
Consol Weight : 0
CASS Weight : 2150 KG
Weight Difference : 2150 KG
Accruals : 200.0000 AUD
Net CASS Charges : 100.00 AUD
Is CASS Amendment : N
Status : Underbilled";

			string expectedNote =
				ZDateTime.Now.ToLongTimeString() + " "
				+ GlbStaff.CurrentUser.GS_Code + " - " + GlbCompany.CurrentCompany.GC_Code + " - " + $"Claim Created for CASS under-billing of charges on {testCASSBillingLine.MAWBNumber}. The variance amount is {testCASSBillingLine.CASSCostCurrencyCode} -100.0000"
				+ "\r\n" + logDetails + "\r\n" + ZString.Replicate('-', 118) + "\r\n";

			var noteStatusDescription = claim.Details;
			AssertEquals(expectedNote, noteStatusDescription);
		}

		public void TestCreateNotInEnterpriseClaimStatusAppendLog()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestNotInEnterpriseAccQueryClaim(testCASSBillingLine, 600M, "12312312");
			AssertEquals(CASSBillingLine.StatusType.NotInSystem, testCASSBillingLine.Status);
			var expected = $"Claim Created for MAWB {testCASSBillingLine.MAWBNumber} appearing in the CASS billing file, but is missing in {BrandingFactory.Instance.ProductName}.";
			AssertContains(expected, claim.Details, false);
		}

		public void TestCreateUnderBillingClaimStatusAppendLog()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestAccQueryClaim(testCASSBillingLine, 100.00M);
			AssertEquals(CASSBillingLine.StatusType.Underbilled, testCASSBillingLine.Status);
			var expected = $"Claim Created for CASS under-billing of charges on {testCASSBillingLine.MAWBNumber}. The variance amount is {testCASSBillingLine.CASSCostCurrencyCode} {Math.Round(claim.AY_QueryClaimAmount, 4)}";
			AssertContains(expected, claim.Details, false);
		}

		public void TestCreateOverBillingClaimStatusAppendLog()
		{
			var testCASSBillingLine = new CASSBillingLine(Factory);
			var claim = CreateTestAccQueryClaim(testCASSBillingLine, 1450.00M);
			AssertEquals(CASSBillingLine.StatusType.Overbilled, testCASSBillingLine.Status);
			var expected = $"Claim Created for CASS over-billing of charges on {testCASSBillingLine.MAWBNumber}. The variance amount is {testCASSBillingLine.CASSCostCurrencyCode} {Math.Round(claim.AY_QueryClaimAmount, 4)}";
			AssertContains(expected, claim.Details, false);
		}

		public void TestGetClaimType_UnderBilled_ClaimStatus_ShouldNotReturnError()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var underBilledCassBillingLine = new CASSBillingLine(Factory);
			CreateTestAccQueryClaim(underBilledCassBillingLine, 100M);
		}

		public void TestGetClaimType_OverBilled_ClaimStatus_TestGetClaimType_UnderBilled_ClaimStatus_ShouldNotReturnError()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.OverBilled);
			var overBilledCassBillingLine = new CASSBillingLine(Factory);
			CreateTestAccQueryClaim(overBilledCassBillingLine, 1500M);
		}

		public void TestGetClaimType_NotInEnterprise_ClaimStatus_TestGetClaimType_UnderBilled_ClaimStatus_ShouldNotReturnError()
		{
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.BothOverUnderBilled);
			var notInEnterpriseLine = new CASSBillingLine(Factory);
			CreateTestNotInEnterpriseAccQueryClaim(notInEnterpriseLine, 600M, "12312312");
		}

		#endregion

		#region Methods

		void SetUpBillingLineAndBizos(CASSBillingLine testCASSBillingLine,
			decimal acrAmountForShipment1 = 100M, decimal acrAmountForShipment2 = 100M,
			decimal pWCAmount = 0, decimal adjustedPWCAmount = 0, string currency = "AUD",
			bool setupConsol = true)
		{
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine("172"), TestObjectCreator.AALSHI);
			TestObjectCreator.AddActiveContactIfRequires(TestObjectCreator.AALSHI);
			Factory.Save();

			if (pWCAmount > 0)
			{
				TestObjectCreator.SetupCASSCostComponent(testCASSBillingLine, isAdjustedAmount: false, pWCAmount: pWCAmount, pVCAmount: 0, pCCAmount: 0, cOAAmount: 0, cOMAmount: 0, dOIAmount: 0, currency: currency);
			}

			if (adjustedPWCAmount > 0)
			{
				TestObjectCreator.SetupCASSCostComponent(testCASSBillingLine, isAdjustedAmount: true, pWCAmount: adjustedPWCAmount, pVCAmount: 0, pCCAmount: 0, cOAAmount: 0, cOMAmount: 0, dOIAmount: 0, currency: currency);
			}

			if (setupConsol)
			{
				string origin = RefUNLOCO.LoadFromIATA(Factory, "LEJ").RL_Code;
				string destination = RefUNLOCO.LoadFromIATA(Factory, "MEX").RL_Code;

				SetupAssociatedBizos(acrAmountForShipment1, acrAmountForShipment2, origin, destination, testCASSBillingLine.CASSCostCurrency, testCASSBillingLine.MAWBNumber);
			}
			Factory.Save();
		}

		void SetupAssociatedBizos(decimal acrAmountForShipment1, decimal acrAmountForShipment2, ZString origin, ZString destination, RefCurrency currency, ZString mawbNumber)
		{
			TestObjectCreator.AUD.SetCustomsRate(DateTime.Today, DateTime.Today.AddDays(1), 1M);

			var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			consol.JK_MasterBillNum = mawbNumber;

			var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
			var shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, "FRT", currency, acrAmountForShipment1, null, currency, acrAmountForShipment1, null);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.FRT, "FRT", currency, acrAmountForShipment2, null, currency, acrAmountForShipment2, null);

			TestObjectCreator.SetExchangeRate(job1, currency, 2M);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
		}

		APInvoice CreateAPInvoiceForTest(string currency = "AUD")
		{
			var refCurrency = TestObjectCreator.GetCurrency(currency);
			return TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", refCurrency, refCurrency.GetCustomsRate(ZDateTime.Today), 200M, 20M, 0M, 200M, 20M, 0M);
		}

		AccQueryClaim CreateTestAccQueryClaim(CASSBillingLine testCASSBillingLine, decimal weightChargePP, string currency = "AUD")
		{
			SetUpBillingLineAndBizos(testCASSBillingLine, pWCAmount: weightChargePP, currency: currency);
			testCASSBillingLine.ForceRecalculateData();

			var cassBillingClaim = new CASSBillingClaimCreator(testCASSBillingLine);
			cassBillingClaim.CreateOrUpdateClaim(CreateAPInvoiceForTest(currency), false);
			Factory.Save();

			return Factory.Load<AccQueryClaim>(new ZQuery())[0];
		}

		AccQueryClaim UpdateTestAccQueryClaim(CASSBillingLine testCASSBillingLine, decimal pwcAmount, decimal adjustedPWCAmount, bool isOverrideAutoClose)
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP002", TestObjectCreator.AUD, 1M, 200M, 20M, 0M, 200M, 20M, 0M);

			SetUpBillingLineAndBizos(testCASSBillingLine, pWCAmount: pwcAmount, adjustedPWCAmount: adjustedPWCAmount, setupConsol: false);
			testCASSBillingLine.ForceRecalculateData();

			var cassBillingClaim = new CASSBillingClaimCreator(testCASSBillingLine);
			cassBillingClaim.CreateOrUpdateClaim(apInvoice, isOverrideAutoClose);
			Factory.Save();

			return Factory.Load<AccQueryClaim>(new ZQuery())[0];
		}

		AccQueryClaim CreateTestNotInEnterpriseAccQueryClaim(CASSBillingLine testCASSBillingLine, decimal weightChargePP, string masterBillNumber)
		{
			SetUpBillingLineAndBizos(testCASSBillingLine);
			testCASSBillingLine.ForceRecalculateData();
			TestObjectCreator.SetupCASSCostComponent(testCASSBillingLine, false, 801.00M, 151.00M, 51.00M, 201.00M, 101.00M, 19.92M, awbSerialNumber: masterBillNumber);

			(testCASSBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = weightChargePP;
			var cassBillingClaim = new CASSBillingClaimCreator(testCASSBillingLine);
			cassBillingClaim.CreateOrUpdateClaim(CreateAPInvoiceForTest("AUD"), false);

			Factory.Save();

			return Factory.Load<AccQueryClaim>(new ZQuery())[0];
		}

		#endregion

		#region TestObjectCreator
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
