using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CreditStatus.Testing
{
	[TestedType(typeof(CreditStatusBusinessObject))]
	class CreditStatusBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTreatDisbursementsAsStandardUnderValue()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "1111";
			testOrg1.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 111m;

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "2222";
			testOrg2.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 222m;

			Factory.Save();

			var creditStatusBizObj = new CreditStatusBusinessObject(Factory);
			AssertEquals("default value", 0M, creditStatusBizObj.OM_ARTreatDisbursementsAsStandardValue);

			creditStatusBizObj.OrganisationPK = testOrg1.PK;
			AssertEquals("test org 1", 111m, creditStatusBizObj.OM_ARTreatDisbursementsAsStandardValue);

			creditStatusBizObj.OrganisationPK = testOrg2.PK;
			AssertEquals("test org 2", 222m, creditStatusBizObj.OM_ARTreatDisbursementsAsStandardValue);
		}

		public void TestSettlementGroupProperties()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_IsDebtor = true;
			testOrg1.MiscServ.OM_ARCreditLimit = 100m;
			testOrg1.CompanyData.OB_ARCreditApproved = true;

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_IsDebtor = true;
			testOrg2.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			testOrg2.CompanyData.OB_ARCreditApproved = true;
			testOrg2.ARSettlementGroupPK = testOrg1.PK;

			Factory.Save();

			var creditStatusBizObj = new CreditStatusBusinessObject(Factory);

			AssertEquals("The field: CreditLimit should not be set", 0M, creditStatusBizObj.CreditLimit);
			AssertEquals("The field: CreditBalance should not be set", 0M, creditStatusBizObj.CreditBalance);
			AssertEquals("The field: SettlementGroupCode should not be set", "", creditStatusBizObj.SettlementGroupCode);
			Assert("The field: UseSettlementGroupCreditLimit should not be ticked", !creditStatusBizObj.UseSettlementGroupCreditLimit);
			AssertEquals("The field: SettlementGroupString should not be set", "", creditStatusBizObj.SettlementGroupString);

			creditStatusBizObj.OrganisationPK = testOrg1.PK;
			AssertEquals("The CreditLimit field should be set to 100", 100M, creditStatusBizObj.CreditLimit);
			AssertEquals("The CreditBalance field should be set to 100", 100M, creditStatusBizObj.CreditBalance);
			AssertEquals("The SettlementGroupCode field should be set", testOrg1.OH_Code, creditStatusBizObj.SettlementGroupCode);
			Assert("The UseSettlementGroupCreditLimit field should not be ticked", !creditStatusBizObj.UseSettlementGroupCreditLimit);
			AssertEquals("Other debtors are using the credit limit of this Settlement Group.", creditStatusBizObj.SettlementGroupString);

			creditStatusBizObj.OrganisationPK = testOrg2.PK;
			AssertEquals("The CreditLimit field should be set to org1's", 100M, creditStatusBizObj.CreditLimit);
			AssertEquals("The CreditBalance field should be set to org1's", 100M, creditStatusBizObj.CreditBalance);
			AssertEquals("The SettlementGroupCode field should be set to org1's", testOrg1.OH_Code, creditStatusBizObj.SettlementGroupCode);
			Assert("The UseSettlementGroupCreditLimit field should be ticked", creditStatusBizObj.UseSettlementGroupCreditLimit);
			AssertEquals("", creditStatusBizObj.SettlementGroupString);
		}

		public void TestCreditBalanceWithMainSettlementGroupOrg()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_IsDebtor = true;
			testOrg1.MiscServ.OM_ARCreditLimit = 1000m;
			testOrg1.CompanyData.OB_ARCreditApproved = true;
			Factory.Save();

			var sqlQuery = string.Format("INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) VALUES ('{0}', '{1}', 'AR', 10, 20, 30, 15) ; INSERT INTO dbo.AccOrgBalance (Y3_OH, Y3_GC, Y3_Ledger, Y3_Balance, Y3_Recognized, Y3_Unrecognized, Y3_Claim) VALUES ('{0}', '{1}', 'AR', 40, 50, 60, 20)",
					testOrg1.PK, GlbCompany.CurrentCompany.PK);

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}
			var creditStatusBizObj = new CreditStatusBusinessObject(Factory);
			creditStatusBizObj.OrganisationPK = testOrg1.PK;

			AssertEquals("Precondition: Posted Revenue", 1000M, creditStatusBizObj.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 70M, creditStatusBizObj.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 60M, creditStatusBizObj.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 80M, creditStatusBizObj.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 210M, creditStatusBizObj.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 35M, creditStatusBizObj.Claim);
			Assert("Precondition: Not Checked SettlementGroup", !testOrg1.CompanyData.OB_ARUseSettlementGroupCreditLimit);

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.Posted))
			{
				creditStatusBizObj.OrganisationPK = testOrg1.PK;
				AssertEquals(930M, creditStatusBizObj.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditStatusBizObj.OrganisationPK = testOrg1.PK;
					AssertEquals(965M, creditStatusBizObj.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedAndRecognized))
			{
				creditStatusBizObj.OrganisationPK = testOrg1.PK;
				AssertEquals(870M, creditStatusBizObj.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditStatusBizObj.OrganisationPK = testOrg1.PK;
					AssertEquals(905M, creditStatusBizObj.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				creditStatusBizObj.OrganisationPK = testOrg1.PK;
				AssertEquals(790M, creditStatusBizObj.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditStatusBizObj.OrganisationPK = testOrg1.PK;
					AssertEquals(825M, creditStatusBizObj.CreditBalance);
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

			var sqlQuery = $@"INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) 
VALUES ('{settlementGroupOrg.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 10, 20, 30, 15) , 
('{testSubOrg1.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 20, 40, 60, 30) , 
('{testSubOrg2.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 40, 80, 120, 60); 
INSERT INTO dbo.AccOrgBalance (Y3_OH, Y3_GC, Y3_Ledger, Y3_Balance, Y3_Recognized, Y3_Unrecognized, Y3_Claim) 
VALUES ('{settlementGroupOrg.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 40, 50, 60, 20) , 
('{testSubOrg1.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 80, 100, 120, 40) , 
('{testSubOrg2.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 160, 200, 240, 80)";

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}
			var creditStatusBizObj = new CreditStatusBusinessObject(Factory);
			creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
			AssertEquals("Precondition: Posted Revenue", 10000M, creditStatusBizObj.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 70M, creditStatusBizObj.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 60M, creditStatusBizObj.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 80M, creditStatusBizObj.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 210M, creditStatusBizObj.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 35M, creditStatusBizObj.Claim);
			Assert("Precondition: Not Checked SettlementGroup", !settlementGroupOrg.CompanyData.OB_ARUseSettlementGroupCreditLimit);

			creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
			AssertEquals("Precondition: Posted Revenue", 10000M, creditStatusBizObj.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 140M, creditStatusBizObj.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 120M, creditStatusBizObj.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 160M, creditStatusBizObj.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 420M, creditStatusBizObj.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 70M, creditStatusBizObj.Claim);
			AssertEquals("Precondition: Checked SettlementGroup", settlementGroupOrg.OH_Code, creditStatusBizObj.SettlementGroupCode);

			creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
			AssertEquals("Precondition: Posted Revenue", 10000M, creditStatusBizObj.CreditLimit);
			AssertEquals("Precondition: Posted Revenue", 280M, creditStatusBizObj.PostedRevenue);
			AssertEquals("Precondition: Recognised WIP", 240M, creditStatusBizObj.RecognisedWIP);
			AssertEquals("Precondition: Unrecognised WIP", 320M, creditStatusBizObj.UnrecognisedWIP);
			AssertEquals("Precondition: Total WIP + Revenue", 840M, creditStatusBizObj.SumOfTotalWIPAndRevenue);
			AssertEquals("Precondition: Claim Amount", 140M, creditStatusBizObj.Claim);
			AssertEquals("Precondition: Checked SettlementGroup", settlementGroupOrg.OH_Code, creditStatusBizObj.SettlementGroupCode);

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.Posted))
			{
				creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
				AssertEquals(9510M, creditStatusBizObj.CreditBalance);
				creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
				AssertEquals(9510M, creditStatusBizObj.CreditBalance);
				creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
				AssertEquals(9510M, creditStatusBizObj.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
					AssertEquals(9755M, creditStatusBizObj.CreditBalance);
					creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
					AssertEquals(9755M, creditStatusBizObj.CreditBalance);
					creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
					AssertEquals(9755M, creditStatusBizObj.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedAndRecognized))
			{
				creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
				AssertEquals(9090M, creditStatusBizObj.CreditBalance);
				creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
				AssertEquals(9090M, creditStatusBizObj.CreditBalance);
				creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
				AssertEquals(9090M, creditStatusBizObj.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
					AssertEquals(9335M, creditStatusBizObj.CreditBalance);
					creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
					AssertEquals(9335M, creditStatusBizObj.CreditBalance);
					creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
					AssertEquals(9335M, creditStatusBizObj.CreditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
				AssertEquals(8530M, creditStatusBizObj.CreditBalance);
				creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
				AssertEquals(8530M, creditStatusBizObj.CreditBalance);
				creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
				AssertEquals(8530M, creditStatusBizObj.CreditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditStatusBizObj.OrganisationPK = settlementGroupOrg.PK;
					AssertEquals(8775M, creditStatusBizObj.CreditBalance);
					creditStatusBizObj.OrganisationPK = testSubOrg1.PK;
					AssertEquals(8775M, creditStatusBizObj.CreditBalance);
					creditStatusBizObj.OrganisationPK = testSubOrg2.PK;
					AssertEquals(8775M, creditStatusBizObj.CreditBalance);
				}
			}
		}

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

			var creditStatusBizObj = new CreditStatusBusinessObject(Factory);
			AssertEquals(0m, creditStatusBizObj.StandardOverdueAmount);
			AssertEquals(0m, creditStatusBizObj.DisbursementOverdueAmount);
			AssertEquals(0m, creditStatusBizObj.TotalOverdueAmount);
			Assert(!creditStatusBizObj.IsCreditOnHold);

			creditStatusBizObj.OrganisationPK = org1.PK;
			AssertEquals(200m, creditStatusBizObj.StandardOverdueAmount);
			AssertEquals(350m, creditStatusBizObj.DisbursementOverdueAmount);
			AssertEquals(550m, creditStatusBizObj.TotalOverdueAmount);
			Assert(!creditStatusBizObj.IsCreditOnHold);

			creditStatusBizObj.OrganisationPK = org2.PK;
			AssertEquals(300m, creditStatusBizObj.StandardOverdueAmount);
			AssertEquals(450m, creditStatusBizObj.DisbursementOverdueAmount);
			AssertEquals(750m, creditStatusBizObj.TotalOverdueAmount);
			Assert(!creditStatusBizObj.IsCreditOnHold);

			org1.CompanyData.OB_AROnCreditHold = true;
			org2.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			creditStatusBizObj.OrganisationPK = org1.PK;
			Assert(creditStatusBizObj.IsCreditOnHold);

			creditStatusBizObj.OrganisationPK = org2.PK;
			Assert(creditStatusBizObj.IsCreditOnHold);
		}

		#endregion

		public void TestAgingDateTypeInfoValueChanged()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV002", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)invoice.Lines.AddNew();
			arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			arInvLine.AL_OSExTaxAmount = 100m;
			arInvLine.AL_AC = testObjectCreator.DSBChargeCode.PK;
			arInvLine.AL_AT = testObjectCreator.GST1.PK;
			var periodManagementTestHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodManagementTestHelper.SetupPeriods();
			var firstAgeDate = periodManagementTestHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			invoice.AH_InvoiceDate = firstAgeDate;
			invoice.AH_PostDate = firstAgeDate;
			Factory.Save();

			var creditStatusBusinessObject = new CreditStatusBusinessObject(Factory);
			creditStatusBusinessObject.OrganisationPK = testObjectCreator.Debtor.PK;

			creditStatusBusinessObject.AgingDateType = "AAA";
			AssertEquals(decimal.Zero, creditStatusBusinessObject.FirstAgeingStandardOutstanding);

			creditStatusBusinessObject.AgingDateType = AccountingConstants.AgingOptions.InvoiceDate;
			AssertEquals(110m, creditStatusBusinessObject.FirstAgeingStandardOutstanding);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreditStatusBusinessObject(Factory);
		}
	}
}
