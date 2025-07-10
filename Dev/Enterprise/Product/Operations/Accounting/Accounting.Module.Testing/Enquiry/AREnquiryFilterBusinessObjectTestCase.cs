using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AREnquiryFilterBusinessObject))]
	public class AREnquiryFilterBusinessObjectTestCase : AccountingFilterStripBusinessObjectTestCase
	{
		#region TestOverdueAmountsAndCreditOnHold

		public void TestOverdueAmountsAndCreditOnHold()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var org1 = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			org1.CompanyData.OB_ARCreditLimit = 50m;
			org1.CompanyData.OB_AROnCreditHold = false;

			var org2 = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			org2.CompanyData.OB_ARCreditLimit = 100m;
			org2.CompanyData.OB_AROnCreditHold = false;

			#region Setup Transactions

			//Unpaid Non-Disbursement Transaction with due date before today
			var unpaidNonDSBInvoiceWithDueDateBeforeToday = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_OH = org1.PK;
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Unpaid Non-Disbursement Transaction with due date after today
			var unpaidNonDSBInvoiceWithDueDateAfterToday = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 250m, 0m, 250m, 0m);
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_OH = org1.PK;
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_DueDate = ZDateTime.Today.AddDays(6);

			//Unpaid Non-Disbursement Transaction for different org
			var unpaidNonDSBInvoiceForOrg2 = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 300m, 0m, 300m, 0m);
			unpaidNonDSBInvoiceForOrg2.AH_OH = org2.PK;
			unpaidNonDSBInvoiceForOrg2.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceForOrg2.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceForOrg2.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Unpaid Disbursement Transaction with due date before today
			var unpaidDSBInvoiceWithDueDateBeforeToday = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 350m, 0m, 350m, 0m);
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_OH = org1.PK;
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Unpaid Disbursement Transaction with due date after today
			var unpaidDSBInvoiceWithDueDateAfterToday = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 400m, 0m, 400m, 0m);
			unpaidDSBInvoiceWithDueDateAfterToday.AH_OH = org1.PK;
			unpaidDSBInvoiceWithDueDateAfterToday.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			unpaidDSBInvoiceWithDueDateAfterToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateAfterToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateAfterToday.AH_DueDate = ZDateTime.Today.AddDays(6);

			//Unpaid Disbursement Transaction for different org
			var unpaidDSBInvoiceForOrg2 = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 450m, 0m, 450m, 0m);
			unpaidDSBInvoiceForOrg2.AH_OH = org2.PK;
			unpaidDSBInvoiceForOrg2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			unpaidDSBInvoiceForOrg2.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceForOrg2.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceForOrg2.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Paid Non-Disbursement Transaction
			var paidNonDSBInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 500m, 0m, 500m, 0m);
			paidNonDSBInvoice.AH_OH = org1.PK;
			paidNonDSBInvoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			paidNonDSBInvoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			paidNonDSBInvoice.AH_DueDate = ZDateTime.Today.AddDays(-5);
			objectCreator.CreateAndMatchARReceiptForARInvoice(paidNonDSBInvoice);

			//Paid Disbursement Transaction
			var paidDSBInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			paidDSBInvoice.AH_OH = org1.PK;
			paidDSBInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			paidDSBInvoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			paidDSBInvoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			paidDSBInvoice.AH_DueDate = ZDateTime.Today.AddDays(-5);
			objectCreator.CreateAndMatchARReceiptForARInvoice(paidDSBInvoice);

			Factory.Save();

			#endregion

			var orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = org1.PK;
			AssertEquals(200m, TestFilterBizO.StandardOverdueAmount);
			AssertEquals(350m, TestFilterBizO.DisbursementOverdueAmount);
			AssertEquals(550m, TestFilterBizO.TotalOverdueAmount);
			Assert(!TestFilterBizO.IsCreditOnHold);

			orgFilter.Property = org2.PK;
			AssertEquals(300m, TestFilterBizO.StandardOverdueAmount);
			AssertEquals(450m, TestFilterBizO.DisbursementOverdueAmount);
			AssertEquals(750m, TestFilterBizO.TotalOverdueAmount);
			Assert(!TestFilterBizO.IsCreditOnHold);

			org1.CompanyData.OB_AROnCreditHold = true;
			org2.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			orgFilter.Property = org1.PK;
			Assert(TestFilterBizO.IsCreditOnHold);

			orgFilter.Property = org2.PK;
			Assert(TestFilterBizO.IsCreditOnHold);
		}

		#endregion

		#region Decimal Places

		public void TestZDecimalsHaveCorrectDecimalPlacesAREnquiryFilterBusinessObject()
		{
			var enquiry = new AREnquiryFilterBusinessObject();

			var localList = new List<string>
				{
					nameof(enquiry.CurrentDisbursementOutstanding),
					nameof(enquiry.FirstAgeingDisbursementOutstanding),
					nameof(enquiry.SecondAgeingDisbursementOutstanding),
					nameof(enquiry.ThirdAgeingDisbursementOutstanding),
					nameof(enquiry.TotalDisbursementOutstanding),
					nameof(enquiry.OnePeriodTotal),
					nameof(enquiry.TwoPeriodTotal),
					nameof(enquiry.ThreePeriodTotal),
					nameof(enquiry.TotalOutstandingAmount),
					nameof(enquiry.CurrentTotal),
					nameof(enquiry.OM_ARTreatDisbursementsAsStandardValue),
					nameof(enquiry.PostedRevenue),
					nameof(enquiry.UnrecognisedWIP),
					nameof(enquiry.RecognisedWIP),
					nameof(enquiry.SumOfTotalWIPAndRevenue),
					nameof(enquiry.CurrentBatchedTotal),
					nameof(enquiry.OnePeriodBatchedTotal),
					nameof(enquiry.TwoPeriodBatchedTotal),
					nameof(enquiry.ThreePeriodBatchedTotal),
					nameof(enquiry.BatchedOutstandingAmountTotal),
					nameof(enquiry.Claim)
				};

			var globalList = new List<string>
				{
					nameof(enquiry.GlobalCreditLimit),
					nameof(enquiry.GlobalCreditAvailable)
				};

			var tester = new DecimalPlacesAttributeTester(enquiry);
			tester.CheckLocalCurrency(localList, nameof(enquiry.LocalDecimals));
			tester.CheckNonLocalCurrency(globalList, nameof(enquiry.GlobalCreditCurrencyDecimals), nameof(enquiry.GlobalCreditCurrency), enquiry);
		}

		public void TestAgingDateType()
		{
			var enquiry = new AREnquiryFilterBusinessObject();
			AssertEquals(AccountingConfigurationRegistry.Instance.AgingOptionReceivables.Value, enquiry.AgingDateType);
		}

		public void TestDecimalPlacesAttributeApplyToAllZDecimalProperties()
		{
			var properties = typeof(AREnquiryFilterBusinessObject).GetProperties().Where(x => x.PropertyType == typeof(ZDecimal)).ToList();
			Assert(properties.All(x => Attribute.IsDefined(x, typeof(DecimalPlacesAttribute))));
		}

		#endregion

		#region TestTreatDisbursementsAsStandardUnderValue

		public void TestTreatDisbursementsAsStandardUnderValue()
		{
			OrgHeader testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testOrgHeader.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 111;

			Factory.Save();

			AssertEquals("The field: TreatDisbAsStandard should not be set", 0M, TestFilterBizO.OM_ARTreatDisbursementsAsStandardValue);

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrgHeader.PK;
			AssertEquals("The TreatDisbAsStandard field should be set to 111", 111M, TestFilterBizO.OM_ARTreatDisbursementsAsStandardValue);
		}

		#endregion

		#region TestLastPaymentReceiptDate

		public void TestLastPaymentReceiptDate()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();

			var testReceipt = Factory.New<ARReceipt>();
			testReceipt.AH_OH = testOrg.PK;
			testReceipt.AH_InvoiceDate = new ZDateTime(2002, 4, 5);
			testReceipt.AH_TransactionNum = "@";
			testReceipt.AH_GB = GlbBranch.CurrentBranch.PK;

			var testReceipt2 = Factory.New<ARReceipt>();
			testReceipt2.AH_OH = testOrg.PK;
			testReceipt2.AH_InvoiceDate = new ZDateTime(2002, 5, 6);
			testReceipt2.AH_TransactionNum = "$";
			testReceipt2.AH_GB = GlbBranch.CurrentBranch.PK;

			var testReceipt3 = Factory.New<ARReceipt>();
			testReceipt3.AH_OH = testOrg.PK;
			testReceipt3.AH_InvoiceDate = new ZDateTime(2002, 6, 7);
			testReceipt3.AH_TransactionNum = "%";
			testReceipt3.AH_GB = testBranch.PK;

			var testJournal = Factory.New<ARJournal>();
			testJournal.AH_OH = testOrg.PK;
			testJournal.AH_InvoiceDate = new ZDateTime(2002, 7, 8);
			testJournal.AH_TransactionNum = "^";
			testJournal.AH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			var orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg.PK;
			var expected = new ZDateTime(2002, 5, 6).Date.ToString();
			AssertEquals("Last payment date should be 06-May-02", expected, TestFilterBizO.LastPaymentReceiptDate);
		}

		#endregion

		public void TestSetInfoValuesForUnpostedRevenueFields()
		{
			var testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testOrgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var sqlQuery = string.Format("INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta) VALUES ('{0}', '{1}', 'AR', 10, 20, 30) , ('{2}', '{3}', 'AR', 50, 100, 150) ; INSERT INTO dbo.AccOrgBalance (Y3_OH, Y3_GC, Y3_Ledger, Y3_Balance, Y3_Recognized, Y3_Unrecognized) VALUES ('{0}', '{1}', 'AR', 40, 50, 60) , ('{2}', '{3}', 'AR', 100, 200, 300)",
					testOrgHeader.PK, GlbCompany.CurrentCompany.PK, testOrgHeader2.PK, GlbCompany.CurrentCompany.PK);

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrgHeader.PK;

			TestFilterBizO.SetInfoValues();
			AssertEquals("Posted Revenue", 70.00M, TestFilterBizO.PostedRevenue);
			AssertEquals("Recognised WIP", 60.00M, TestFilterBizO.RecognisedWIP);
			AssertEquals("Unrecognised WIP", 80.00M, TestFilterBizO.UnrecognisedWIP);
			AssertEquals("Total WIP + Revenue", 210.00M, TestFilterBizO.SumOfTotalWIPAndRevenue);
		}

		public void TestCreditBalanceWithMainSettlementGroupOrg()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_IsDebtor = true;
			testOrg1.MiscServ.OM_ARCreditLimit = 1000m;
			testOrg1.CompanyData.OB_ARCreditApproved = true;
			Factory.Save();

			var sqlQuery = string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) VALUES ('{0}', '{1}', 'AR', 10, 20, 30, 5) ; INSERT INTO dbo.AccOrgBalance (Y3_OH, Y3_GC, Y3_Ledger, Y3_Balance, Y3_Recognized, Y3_Unrecognized, Y3_Claim) VALUES ('{0}', '{1}', 'AR', 40, 50, 60, 30)",
					testOrg1.PK, GlbCompany.CurrentCompany.PK);

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = testOrg1.PK;

			TestFilterBizO.SetInfoValues();
			AssertEquals("Precondition: Posted Revenue", 1000M, TestFilterBizO.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 70M, TestFilterBizO.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 60M, TestFilterBizO.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 80M, TestFilterBizO.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 210M, TestFilterBizO.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 35M, TestFilterBizO.Claim);
			Assert("Precondition: Not Checked SettlementGroup", !testOrg1.CompanyData.OB_ARUseSettlementGroupCreditLimit);

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.Posted))
			{
				TestFilterBizO.SetInfoValues();
				AssertEquals(930M, TestFilterBizO.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					TestFilterBizO.SetInfoValues();
					AssertEquals(965M, TestFilterBizO.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedAndRecognized))
			{
				TestFilterBizO.SetInfoValues();
				AssertEquals(870M, TestFilterBizO.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					TestFilterBizO.SetInfoValues();
					AssertEquals(905M, TestFilterBizO.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				TestFilterBizO.SetInfoValues();
				AssertEquals(790M, TestFilterBizO.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					TestFilterBizO.SetInfoValues();
					AssertEquals(825M, TestFilterBizO.CreditBalance);
				}
			}
		}

		public void TestCreditBalanceForCheckedSettlementGroup()
		{
			var settlementGroupOrg = Factory.NewWithValidTestData<OrgHeader>();
			settlementGroupOrg.OH_IsDebtor = true;
			settlementGroupOrg.MiscServ.OM_ARCreditLimit = 10000m;
			settlementGroupOrg.CompanyData.OB_ARCreditApproved = true;

			var testSubOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testSubOrg1.OH_IsDebtor = true;
			testSubOrg1.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			testSubOrg1.CompanyData.OB_ARCreditApproved = true;
			testSubOrg1.ARSettlementGroupPK = settlementGroupOrg.PK;

			var testSubOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testSubOrg2.OH_IsDebtor = true;
			testSubOrg2.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			testSubOrg2.CompanyData.OB_ARCreditApproved = true;
			testSubOrg2.ARSettlementGroupPK = settlementGroupOrg.PK;

			Factory.Save();

			var sqlQuery = FormattableString.Invariant($@"INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) 
VALUES ('{settlementGroupOrg.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 10, 20, 30, 15) , 
('{testSubOrg1.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 20, 40, 60, 30) , 
('{testSubOrg2.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 40, 80, 120, 60); 
INSERT INTO dbo.AccOrgBalance (Y3_OH, Y3_GC, Y3_Ledger, Y3_Balance, Y3_Recognized, Y3_Unrecognized, Y3_Claim) 
VALUES ('{settlementGroupOrg.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 40, 50, 60, 20) , 
('{testSubOrg1.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 80, 100, 120, 40) , 
('{testSubOrg2.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 160, 200, 240, 80)");

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = settlementGroupOrg.PK;
			TestFilterBizO.SetInfoValues();
			AssertEquals("Precondition: Posted Revenue", 10000M, TestFilterBizO.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 70M, TestFilterBizO.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 60M, TestFilterBizO.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 80M, TestFilterBizO.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 210M, TestFilterBizO.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 35M, TestFilterBizO.Claim);
			Assert("Precondition: Not Checked SettlementGroup", !settlementGroupOrg.CompanyData.OB_ARUseSettlementGroupCreditLimit);

			orgFilter.Property = testSubOrg1.PK;
			TestFilterBizO.SetInfoValues();
			AssertEquals("Precondition: Posted Revenue", 10000M, TestFilterBizO.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 140M, TestFilterBizO.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 120M, TestFilterBizO.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 160M, TestFilterBizO.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 420M, TestFilterBizO.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 70M, TestFilterBizO.Claim);
			AssertEquals("Precondition: Checked SettlementGroup", settlementGroupOrg.OH_Code, TestFilterBizO.SettlementGroupCode);

			orgFilter.Property = testSubOrg2.PK;
			TestFilterBizO.SetInfoValues();
			AssertEquals("Precondition: Posted Revenue", 10000M, TestFilterBizO.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 280M, TestFilterBizO.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 240M, TestFilterBizO.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 320M, TestFilterBizO.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 840M, TestFilterBizO.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 140M, TestFilterBizO.Claim);
			AssertEquals("Precondition: Checked SettlementGroup", settlementGroupOrg.OH_Code, TestFilterBizO.SettlementGroupCode);

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.Posted))
			{
				orgFilter.Property = settlementGroupOrg.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(9510M, TestFilterBizO.CreditBalance);
				orgFilter.Property = testSubOrg1.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(9510M, TestFilterBizO.CreditBalance);
				orgFilter.Property = testSubOrg2.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(9510M, TestFilterBizO.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					orgFilter.Property = settlementGroupOrg.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(9755M, TestFilterBizO.CreditBalance);
					orgFilter.Property = testSubOrg1.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(9755M, TestFilterBizO.CreditBalance);
					orgFilter.Property = testSubOrg2.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(9755M, TestFilterBizO.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedAndRecognized))
			{
				orgFilter.Property = settlementGroupOrg.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(9090M, TestFilterBizO.CreditBalance);
				orgFilter.Property = testSubOrg1.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(9090M, TestFilterBizO.CreditBalance);
				orgFilter.Property = testSubOrg2.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(9090M, TestFilterBizO.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					orgFilter.Property = settlementGroupOrg.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(9335M, TestFilterBizO.CreditBalance);
					orgFilter.Property = testSubOrg1.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(9335M, TestFilterBizO.CreditBalance);
					orgFilter.Property = testSubOrg2.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(9335M, TestFilterBizO.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				orgFilter.Property = settlementGroupOrg.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(8530M, TestFilterBizO.CreditBalance);
				orgFilter.Property = testSubOrg1.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(8530M, TestFilterBizO.CreditBalance);
				orgFilter.Property = testSubOrg2.PK;
				TestFilterBizO.SetInfoValues();
				AssertEquals(8530M, TestFilterBizO.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					orgFilter.Property = settlementGroupOrg.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(8775M, TestFilterBizO.CreditBalance);
					orgFilter.Property = testSubOrg1.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(8775M, TestFilterBizO.CreditBalance);
					orgFilter.Property = testSubOrg2.PK;
					TestFilterBizO.SetInfoValues();
					AssertEquals(8775M, TestFilterBizO.CreditBalance);
				}
			}
		}

		#region Organisation List

		public void TestOrganisationList()
		{
			OrgHeader activeDebtor = Factory.NewWithValidTestData<OrgHeader>();
			activeDebtor.OH_IsActive = true;
			activeDebtor.OH_IsDebtor = true;
			activeDebtor.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader inactiveDebtor = Factory.NewWithValidTestData<OrgHeader>();
			inactiveDebtor.OH_IsActive = false;
			inactiveDebtor.OH_IsDebtor = true;
			inactiveDebtor.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader nonDebtorOrg = Factory.NewWithValidTestData<OrgHeader>();
			nonDebtorOrg.OH_IsActive = true;
			nonDebtorOrg.OH_IsDebtor = false;
			nonDebtorOrg.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			TestFilterBizO.OrganisationList_ForTestOnly.Load();

			Assert("AH_OHList should contain ActiveDebtor", TestFilterBizO.OrganisationList_ForTestOnly.Contains(activeDebtor));
			Assert("AH_OHList should contain InactiveDebtor", TestFilterBizO.OrganisationList_ForTestOnly.Contains(inactiveDebtor));
			Assert("AH_OHList should not contain NonDebtorOrg", !TestFilterBizO.OrganisationList_ForTestOnly.Contains(nonDebtorOrg));
		}

		#endregion

		#region TestSettlementGroupProperties

		public void TestSettlementGroupProperties()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.MiscServ.OM_ARCreditLimit = 100m;
			org1.CompanyData.OB_ARCreditApproved = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsDebtor = true;
			org2.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			org2.CompanyData.OB_ARCreditApproved = true;
			org2.ARSettlementGroupPK = org1.PK;

			Factory.Save();

			AssertEquals("The field: CreditLimit should not be set", 0M, TestFilterBizO.CreditLimit);
			AssertEquals("The field: CreditBalance should not be set", 0M, TestFilterBizO.CreditBalance);
			AssertEquals("The field: SettlementGroupCode should not be set", "", TestFilterBizO.SettlementGroupCode);
			Assert("The field: UseSettlementGroupCreditLimit should not be ticked", !TestFilterBizO.UseSettlementGroupCreditLimit);
			AssertEquals("The field: SettlementGroupString should not be set", "", TestFilterBizO.SettlementGroupString);

			ModuleGuidFilter orgFilter = (ModuleGuidFilter)TestFilterBizO["Organisation"];
			orgFilter.IsActive = true;

			orgFilter.Property = org1.PK;
			AssertEquals("The CreditLimit field should be set to 100", 100M, TestFilterBizO.CreditLimit);
			AssertEquals("The CreditBalance field should be set to 100", 100M, TestFilterBizO.CreditBalance);
			AssertEquals("The SettlementGroupCode field should be set", org1.OH_Code, TestFilterBizO.SettlementGroupCode);
			Assert("The UseSettlementGroupCreditLimit field should not be ticked", !TestFilterBizO.UseSettlementGroupCreditLimit);
			AssertEquals("Other debtors are using the credit limit of this Settlement Group.", TestFilterBizO.SettlementGroupString);

			orgFilter.Property = org2.PK;
			AssertEquals("The CreditLimit field should be set to org1's", 100M, TestFilterBizO.CreditLimit);
			AssertEquals("The CreditBalance field should be set to org1's", 100M, TestFilterBizO.CreditBalance);
			AssertEquals("The SettlementGroupCode field should be set to org1's", org1.OH_Code, TestFilterBizO.SettlementGroupCode);
			Assert("The UseSettlementGroupCreditLimit field should be ticked", TestFilterBizO.UseSettlementGroupCreditLimit);
			AssertEquals("", TestFilterBizO.SettlementGroupString);
		}

		#endregion

		#region TestReceivablesEnquiryHasRelatedClaimFilter

		public void TestReceivablesEnquiryHasRelatedClaimFilter()
		{
			var testReceipt1 = Factory.New<ARInvoice>();
			var testReceipt2 = Factory.New<ARInvoice>();
			var testReceipt3 = Factory.New<ARInvoice>();
			var testReceipt4 = Factory.New<ARInvoice>();
			var testReceipt5 = Factory.New<ARInvoice>();
			Factory.Save();

			var receivablesEnquiryFilterBizo = new AREnquiryFilterBusinessObject();
			var filters = receivablesEnquiryFilterBizo.ModuleFilters;

			var hasRelatedClaimFilter = ((ModuleTextFilter)filters["Has Related Claim"]);
			var list = hasRelatedClaimFilter.List as CodeDescriptionPairList;
			AssertNotNull(list);
			AssertEquals("list Count", 2, list.Count);
			Assert("No AR Claim", list.ContainsCode("No AR Claim"));
			Assert("Has AR Claim", list.ContainsCode("Has AR Claim"));

			hasRelatedClaimFilter.Property = "No AR Claim";
			hasRelatedClaimFilter.IsActive = true;
			var testTransactions = new TransactionHeaderCollection(Factory, receivablesEnquiryFilterBizo.Filter);
			testTransactions.Load();

			AssertEquals("All five AR invoices with no claim should be contained in the list", 5, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("All five AR invoices with no claim should be contained in the list", new[] { testReceipt1, testReceipt2, testReceipt3, testReceipt4, testReceipt5 }, testTransactions);

			hasRelatedClaimFilter.Property = "Has AR Claim";
			testTransactions = new TransactionHeaderCollection(Factory, receivablesEnquiryFilterBizo.Filter);
			testTransactions.Load();

			AssertEquals("No AR invoices with claim are found", 0, testTransactions.Count);

			var claim1 = Factory.NewWithValidTestData<ARAccQueryClaim>();
			claim1.AY_AH = testReceipt2.PK;
			var claim2 = Factory.NewWithValidTestData<ARAccQueryClaim>();
			claim2.AY_AH = testReceipt3.PK;
			var claim3 = Factory.NewWithValidTestData<ARAccQueryClaim>();
			claim3.AY_AH = testReceipt4.PK;

			Factory.Save();

			hasRelatedClaimFilter.Property = "Has AR Claim";
			testTransactions = new TransactionHeaderCollection(Factory, receivablesEnquiryFilterBizo.Filter);
			testTransactions.Load();

			AssertEquals("All three AR invoices with claim should be contained in the list", 3, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("All three AR invoices with claim should be contained in the list", new[] { testReceipt2, testReceipt3, testReceipt4 }, testTransactions);

			hasRelatedClaimFilter.Property = "No AR Claim";
			testTransactions = new TransactionHeaderCollection(Factory, receivablesEnquiryFilterBizo.Filter);
			testTransactions.Load();

			AssertEquals("All two AR invoices with no claim should be contained in the list", 2, testTransactions.Count);
			AssertContainsExactElementsInAnyOrder("All two AR invoices with no claim should be contained in the list", new[] { testReceipt1, testReceipt5 }, testTransactions);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AREnquiryFilterBusinessObject();
		}

		protected override void SetUp()
		{
			TestFilterBizO = (AREnquiryFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		protected AREnquiryFilterBusinessObject TestFilterBizO;

		#endregion
	}
}
