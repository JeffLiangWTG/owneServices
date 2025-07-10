using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.CreditControlledDocumentsApproval
{
	[TestedType(typeof(OrganisationInBreach))]
	public class OrganisationInBreachTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStandardWIPsAndDisbursementWIPsBilledToThisJob_WhenApprovalIsCreatedForABusinessObjectThatDoesNotImplementIJobHeaderParentInterface()
		{
			var organisation = CreateTestOrganisation(50m, false, 0m, false);
			var cartage = ObjectCreator.CreateCartage();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			Factory.Save();

			Assert("Precondition - CommonCartageLeg does not implement IJobHeaderParent.", !(leg is IJobHeaderParent));
			var orgInBreach = new OrganisationInBreach();
			AssertNoExceptionThrown(() => orgInBreach.SetValues(organisation, "", leg));
			AssertEquals(ZDecimal.Zero, orgInBreach.StandardWIPsBilledToThisJob);
			AssertEquals(ZDecimal.Zero, orgInBreach.DisbursementWIPsBilledToThisJob);
		}

		public void TestCreditLimit_UsingSettlementGroupCreditLimit()
		{
			AssertCreditLimitTest(true);
		}

		public void TestCreditLimit_NotUsingSettlementGroupCreditLimit()
		{
			AssertCreditLimitTest(false);
		}

		void AssertCreditLimitTest(bool isUsingSettlementGroupCreditLimit)
		{
			var shipment = ObjectCreator.CreateShipment("S0001");
			var org = CreateTestOrganisation(2000m, isUsingSettlementGroupCreditLimit, 1000m, false);
			Factory.Save();

			var expectedCreditLimit = isUsingSettlementGroupCreditLimit ? 1000m : 2000m;
			var orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org, "", shipment);
			AssertEquals(expectedCreditLimit, orgInBreach.OrgCreditLimit);
			AssertEquals(isUsingSettlementGroupCreditLimit, orgInBreach.IsUsingSettlementGroupCreditLimit);
		}

		public void TestCreditOnHold()
		{
			var shipment = ObjectCreator.CreateShipment("S0001");
			var org = CreateTestOrganisation(2000m, false, 0m, true);
			Factory.Save();

			var orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org, "", shipment);
			Assert(orgInBreach.IsCreditOnHold);

			org.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();

			orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org, "", shipment);
			Assert(!orgInBreach.IsCreditOnHold);
		}

		public void TestOverCreditLimit()
		{
			var shipment = ObjectCreator.CreateShipment("S0001");
			var org = CreateTestOrganisation(100m, false, 0m, false);
			Factory.Save();

			//Posted Unpaid Revenue
			var unpaidARInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 150m, 0m, 150m, 0m);
			unpaidARInvoice.AH_OH = org.PK;

			//Posted Paid Revenue
			var paidARInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			paidARInvoice.AH_OH = org.PK;
			ObjectCreator.CreateAndMatchARReceiptForARInvoice(paidARInvoice);

			//Recognized WIP
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 250m, 250m);
			charge.JR_OH_SellAccount = org.PK;
			Factory.Save();

			//UnRecognized WIP
			using (AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);
				charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300m, 300m);
				charge.JR_OH_SellAccount = org.PK;
				Factory.Save();
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				var orgInBreach = new OrganisationInBreach();
				orgInBreach.SetValues(org, "", shipment);
				AssertEquals("Credit Limit - (Posted Unpaid Revenue + Recognized WIP + Unrecognized WIP)", 600m, orgInBreach.OverCreditLimit);
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreditLimitChecking.PostedAndRecognized))
			{
				var orgInBreach = new OrganisationInBreach();
				orgInBreach.SetValues(org, "", shipment);
				AssertEquals("Credit Limit - (Posted Unpaid Revenue + Recognized WIP)", 300m, orgInBreach.OverCreditLimit);
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreditLimitChecking.Posted))
			{
				var orgInBreach = new OrganisationInBreach();
				orgInBreach.SetValues(org, "", shipment);
				AssertEquals("Credit Limit - Posted Unpaid Revenue", 50m, orgInBreach.OverCreditLimit);
			}
		}

		public void TestStandardOverdueAmountAndDisbursementOverdueAmount()
		{
			var shipment = ObjectCreator.CreateShipment("S0001");
			var org1 = CreateTestOrganisation(50m, false, 0m, false);
			var org2 = CreateTestOrganisation(100m, false, 0m, false);

			#region Setup Transactions

			//Unpaid Non-Disbursement Transaction with due date before today
			var unpaidNonDSBInvoiceWithDueDateBeforeToday = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_OH = org1.PK;
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateBeforeToday.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Unpaid Non-Disbursement Transaction with due date after today
			var unpaidNonDSBInvoiceWithDueDateAfterToday = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 250m, 0m, 250m, 0m);
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_OH = org1.PK;
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceWithDueDateAfterToday.AH_DueDate = ZDateTime.Today.AddDays(6);

			//Unpaid Non-Disbursement Transaction for different org
			var unpaidNonDSBInvoiceForOrg2 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 300m, 0m, 300m, 0m);
			unpaidNonDSBInvoiceForOrg2.AH_OH = org2.PK;
			unpaidNonDSBInvoiceForOrg2.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceForOrg2.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidNonDSBInvoiceForOrg2.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Unpaid Disbursement Transaction with due date before today
			var unpaidDSBInvoiceWithDueDateBeforeToday = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 350m, 0m, 350m, 0m);
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_OH = org1.PK;
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateBeforeToday.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Unpaid Disbursement Transaction with due date after today
			var unpaidDSBInvoiceWithDueDateAfterToday = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 400m, 0m, 400m, 0m);
			unpaidDSBInvoiceWithDueDateAfterToday.AH_OH = org1.PK;
			unpaidDSBInvoiceWithDueDateAfterToday.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			unpaidDSBInvoiceWithDueDateAfterToday.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateAfterToday.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceWithDueDateAfterToday.AH_DueDate = ZDateTime.Today.AddDays(6);

			//Unpaid Disbursement Transaction for different org
			var unpaidDSBInvoiceForOrg2 = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 450m, 0m, 450m, 0m);
			unpaidDSBInvoiceForOrg2.AH_OH = org2.PK;
			unpaidDSBInvoiceForOrg2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			unpaidDSBInvoiceForOrg2.AH_PostDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceForOrg2.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			unpaidDSBInvoiceForOrg2.AH_DueDate = ZDateTime.Today.AddDays(-5);

			//Paid Non-Disbursement Transaction
			var paidNonDSBInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 500m, 0m, 500m, 0m);
			paidNonDSBInvoice.AH_OH = org1.PK;
			paidNonDSBInvoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			paidNonDSBInvoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			paidNonDSBInvoice.AH_DueDate = ZDateTime.Today.AddDays(-5);
			ObjectCreator.CreateAndMatchARReceiptForARInvoice(paidNonDSBInvoice);

			//Paid Disbursement Transaction
			var paidDSBInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), ObjectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			paidDSBInvoice.AH_OH = org1.PK;
			paidDSBInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			paidDSBInvoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			paidDSBInvoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			paidDSBInvoice.AH_DueDate = ZDateTime.Today.AddDays(-5);
			ObjectCreator.CreateAndMatchARReceiptForARInvoice(paidDSBInvoice);

			Factory.Save();

			#endregion

			var orgInBreach1 = new OrganisationInBreach();
			orgInBreach1.SetValues(org1, "", shipment);
			AssertEquals(200m, orgInBreach1.StandardOverdueAmount);
			AssertEquals(350m, orgInBreach1.DisbursementOverdueAmount);

			var orgInBreach2 = new OrganisationInBreach();
			orgInBreach2.SetValues(org2, "", shipment);
			AssertEquals(300m, orgInBreach2.StandardOverdueAmount);
			AssertEquals(450m, orgInBreach2.DisbursementOverdueAmount);
		}

		public void TestStandardWIPsAndDisbursementWIPsBilledToThisJob()
		{
			var org1 = CreateTestOrganisation(50m, false, 0m, false);
			var org2 = CreateTestOrganisation(100m, false, 0m, false);

			var nonDSBChargeCode = ObjectCreator.CreateChargeCode("AC1", "Non-DSB Charge Code", ChargeType.Margin, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1);
			var dBSChargeCode = ObjectCreator.CreateChargeCode("AC2", "DSB Charge Code", ChargeType.Disbursement, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1);

			var shipment1 = ObjectCreator.CreateShipment("S00001");
			var shipment1Job = ObjectCreator.CreateJob(shipment1, ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);

			var shipment1UnpaidNonDSBCharge = ObjectCreator.CreateCharge(shipment1Job, nonDSBChargeCode, 150m, 150m);
			shipment1UnpaidNonDSBCharge.JR_OH_SellAccount = org1.PK;

			var shipment1UnpaidDSBCharge = ObjectCreator.CreateCharge(shipment1Job, dBSChargeCode, 200m, 200m);
			shipment1UnpaidDSBCharge.JR_OH_SellAccount = org1.PK;

			var shipment1UnpaidNonDSBChargeForOrg2 = ObjectCreator.CreateCharge(shipment1Job, nonDSBChargeCode, 250m, 250m);
			shipment1UnpaidNonDSBChargeForOrg2.JR_OH_SellAccount = org2.PK;

			var shipment1UnpaidDSBChargeForOrg2 = ObjectCreator.CreateCharge(shipment1Job, dBSChargeCode, 300m, 300m);
			shipment1UnpaidDSBChargeForOrg2.JR_OH_SellAccount = org2.PK;

			var shipment2 = ObjectCreator.CreateShipment("S00002");
			var shipment2Job = ObjectCreator.CreateJob(shipment2, ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);

			var shipment2UnpaidNonDSBCharge = ObjectCreator.CreateCharge(shipment2Job, nonDSBChargeCode, 450m, 450m);
			shipment2UnpaidNonDSBCharge.JR_OH_SellAccount = org1.PK;

			var shipment2UnpaidDSBCharge = ObjectCreator.CreateCharge(shipment2Job, dBSChargeCode, 500m, 500m);
			shipment2UnpaidDSBCharge.JR_OH_SellAccount = org1.PK;

			Factory.Save();

			ObjectCreator.CreateRevenueLineAndCharge(shipment1Job.PK, 350m, "ZZAC1");
			ObjectCreator.CreateRevenueLineAndCharge(shipment1Job.PK, 400m, "ZZAC2");

			Factory.Save();

			var orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org1, "", shipment1);
			AssertEquals(150m, orgInBreach.StandardWIPsBilledToThisJob);
			AssertEquals(200m, orgInBreach.DisbursementWIPsBilledToThisJob);
		}

		public void TestStandardWIPsAndDisbursementWIPsBilledToThisJob_ChargeCodeHasTypeOverrides()
		{
			var org1 = CreateTestOrganisation(50m, false, 0m, false);

			var nonDSBChargeCode = ObjectCreator.CreateChargeCode("AC1", "Non-DSB Charge Code", ChargeType.Margin, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1);
			var typeOverride = nonDSBChargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_ChargeType = ChargeType.Disbursement;
			typeOverride.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			typeOverride.AN_JobDirection = "ALL";

			var dBSChargeCode = ObjectCreator.CreateChargeCode("AC2", "DSB Charge Code", ChargeType.Disbursement, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1);
			typeOverride = dBSChargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_ChargeType = ChargeType.Margin;
			typeOverride.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			typeOverride.AN_JobDirection = "ALL";

			Factory.Save();

			var shipment1 = ObjectCreator.CreateShipment("S00001");
			var shipment1Job = ObjectCreator.CreateJob(shipment1, ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);

			var shipment1UnpaidNonDSBCharge = ObjectCreator.CreateCharge(shipment1Job, nonDSBChargeCode, 150m, 150m);
			shipment1UnpaidNonDSBCharge.JR_OH_SellAccount = org1.PK;

			var shipment1UnpaidDSBCharge = ObjectCreator.CreateCharge(shipment1Job, dBSChargeCode, 200m, 200m);
			shipment1UnpaidDSBCharge.JR_OH_SellAccount = org1.PK;

			Factory.Save();

			var orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org1, "", shipment1);
			AssertEquals(200m, orgInBreach.StandardWIPsBilledToThisJob);
			AssertEquals(150m, orgInBreach.DisbursementWIPsBilledToThisJob);
		}

		public void TestStandardWIPsAndDisbursementWIPsBilledToThisJob_WhenApprovalIsCreatedFromAConsol()
		{
			var org1 = CreateTestOrganisation(50m, false, 0m, false);
			var nonDSBChargeCode = ObjectCreator.CreateChargeCode("AC1", "Non-DSB Charge Code", ChargeType.Margin, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1);
			var dBSChargeCode = ObjectCreator.CreateChargeCode("AC2", "DSB Charge Code", ChargeType.Disbursement, 100, ObjectCreator.GSTFREE1, ObjectCreator.WHTFREE1);

			var consol = ObjectCreator.CreateConsol();
			var shipment1 = ObjectCreator.CreateShipment("S0001", consol);
			var shipment2 = ObjectCreator.CreateShipment("S0002", consol);

			var shipment1Job = ObjectCreator.CreateJob(shipment1, ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);
			var shipment2Job = ObjectCreator.CreateJob(shipment2, ObjectCreator.LocalClient, 0m, ObjectCreator.Agent, 0m);

			var shipment1UnpaidNonDSBCharge = ObjectCreator.CreateCharge(shipment1Job, nonDSBChargeCode, 150m, 150m);
			shipment1UnpaidNonDSBCharge.JR_OH_SellAccount = org1.PK;

			Factory.Save();

			var orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org1, "", consol);
			AssertEquals(150m, orgInBreach.StandardWIPsBilledToThisJob);
			AssertEquals(0m, orgInBreach.DisbursementWIPsBilledToThisJob);

			var shipment2UnpaidDSBCharge = ObjectCreator.CreateCharge(shipment2Job, dBSChargeCode, 500m, 500m);
			shipment2UnpaidDSBCharge.JR_OH_SellAccount = org1.PK;
			Factory.Save();

			orgInBreach = new OrganisationInBreach();
			orgInBreach.SetValues(org1, "", consol);
			AssertEquals(150m, orgInBreach.StandardWIPsBilledToThisJob);
			AssertEquals(500m, orgInBreach.DisbursementWIPsBilledToThisJob);
		}

		OrgHeader CreateTestOrganisation(ZDecimal creditLimit, ZBool isUsingSettlementGroupCreditLimit, ZDecimal settlementGroupCreditLimit, ZBool isCreditOnHold)
		{
			var orgHeader = ObjectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			orgHeader.CompanyData.OB_ARCreditLimit = creditLimit;
			orgHeader.CompanyData.OB_AROnCreditHold = isCreditOnHold;
			if (isUsingSettlementGroupCreditLimit)
			{
				var settlementGroup = ObjectCreator.CreateOrgHeader("ABC", false, true);
				settlementGroup.CompanyData.OB_ARCreditLimit = settlementGroupCreditLimit;
				orgHeader.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
				orgHeader.ARSettlementGroupPK = settlementGroup.PK;
			}
			Factory.Save();
			return orgHeader;
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		#region NonPersistentBusinessObjectTestCase Overrides
		protected override BusinessObject GetNewBusinessObject() => new OrganisationInBreach();

		#endregion
	}
}
