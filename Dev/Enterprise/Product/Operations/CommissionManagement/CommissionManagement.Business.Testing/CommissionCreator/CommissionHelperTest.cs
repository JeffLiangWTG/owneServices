using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing.Commission.CommissionCreator
{
	public class CommissionHelperTest : CommissionCreatorTestCase
	{
		public void TestGetNonReversedCommissionHeaders_Job()
		{
			SetupTransactionsAndCommissions();
			Job.Close(null, null);
			Factory.Save();

			var commissionHelper = new CommissionHelper();
			var reversedCommissionHeaders = commissionHelper.GetNonReversedCommissionHeaders(Factory, Job);
			AssertEquals("Should return 1 commission header.", 1, reversedCommissionHeaders.Count);
		}

		public void TestGetNonReversedCommissionHeaders_Transaction()
		{
			var testHelper = new CommissionTestObjectCreator(Factory);
			testHelper.SetupTransactionsAndCommissions();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			var approvalItem = new CommissionAgreementApprovalItem(wizard, testHelper.CommissionAgreement2);
			approvalItem.IsInclude = true;
			wizard.CommissionAgreementApprovalItemCollection.Add(approvalItem);
			wizard.ShouldAddToCalculationQueue = false;

			wizard.Approve(null);

			Factory.Save();

			AssertEquals("Commissions should be overridden and cancelled", 3, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			var commissionHelper = new CommissionHelper();
			var nonReversedCommissionHeaders = commissionHelper.GetNonReversedCommissionHeaders(Factory, testHelper.Invoice);

			AssertNotNull("Non overridden header should be returned.", nonReversedCommissionHeaders.CommissionHeaders);
			AssertEquals("Non overridden header should be returned.", 1, nonReversedCommissionHeaders.CommissionHeaders.Length);

			var nonReversedHeader = (AccCommissionHeader)nonReversedCommissionHeaders.CommissionHeaders[0];
			nonReversedHeader.MarkAsOverriden(true);

			Factory.Save();

			nonReversedCommissionHeaders = commissionHelper.GetNonReversedCommissionHeaders(Factory, testHelper.Invoice);

			AssertEquals("Overriden headers should not be returned", null, nonReversedCommissionHeaders.CommissionHeaders);
		}

		public void TestGetNonReversedCommissionHeaders_NoResultsForReversedHeader()
		{
			SetupTransactionsAndCommissions();
			Job.Close(null, null);
			Factory.Save();

			var header = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Invoice.PK)).FirstOrDefault();
			AssertNotNull(header);

			var reversalCreator = new ReversalTransactionCommissionCreator(Invoice);
			reversalCreator.CreateReversalCommissions(header);

			Factory.Save();

			var commissionHelper = new CommissionHelper();
			var nonReversedCommissionHeaders = commissionHelper.GetNonReversedCommissionHeaders(Factory, Job);
			AssertEquals("Should return 0 commission headers as it has been reversed.", 0, nonReversedCommissionHeaders.Count);
		}

		public void TestGetNonReversedCommissionHeaders_NoResultsForReversedHeader_FixedCommissionType()
		{
			SetupTransactionsAndCommissionsForFixed();
			Job.Close(null, null);
			Factory.Save();

			var header = Factory.Load<AccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Invoice.PK)).FirstOrDefault();
			AssertNotNull(header);

			var line = Factory.Load<AccCommissionLine>(new ZQuery(AccCommissionLineSchema.CL0_ParentID, header.PK));
			AssertNotNull(line);

			var commissionHelper = new CommissionHelper();
			var nonReversedCommissionHeadersBefore = commissionHelper.GetNonReversedCommissionHeaders(Factory, Job);
			AssertEquals("Should return 1 commission header.", 1, nonReversedCommissionHeadersBefore.Count);

			var reversalCreator = new ReversalTransactionCommissionCreator(Invoice);
			reversalCreator.CreateReversalCommissions(header);

			Factory.Save();

			var nonReversedCommissionHeaders = commissionHelper.GetNonReversedCommissionHeaders(Factory, Job);
			AssertEquals("Should return 0 commission headers as it has been reversed.", 0, nonReversedCommissionHeaders.Count);
		}

		public void TestCorrectCommissionsReversedForBulkTransactions()
		{
			SetupTransactionsAndCommissions();
			SetupBulkInvoice();

			Job1.Close(null, null);
			Job2.Close(null, null);
			Factory.Save();

			var commissionLines = Factory.Load<ViewCommissionLine>(new ZQuery(ViewCommissionLineSchema.VCL_AH, BulkInvoice.PK));
			AssertEquals("2 Jobs closed = 2 commission lines", 2, commissionLines.Length);

			var commissionHelper = new CommissionHelper();
			var reversedCommissionHeaders = commissionHelper.GetNonReversedCommissionHeaders(Factory, Job3);

			AssertEquals("This job should have zero non reversed commissions headers", 0, reversedCommissionHeaders.Count);
		}

		#region Implementation

		OrgHeader Org1;
		OrgHeader Org2;
		Job Job;
		InvoicingBase Invoice;
		InvoicingLineBase Line1;
		InvoicingLineBase Line2;
		InvoicingBase BulkInvoice;
		Job Job1;
		Job Job2;
		Job Job3;

		void SetupTransactionsAndCommissions()
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Org1.Addresses.AddNew();
			address1.OA_Address1 = "72 O'Riordan St";

			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = Org2.Addresses.AddNew();
			address2.OA_Address1 = "27 O'Riordan St";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ACL";

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();

			var commissionAgreement1 = opportunity1.ApprovedCommissionAgreements.AddNew();
			commissionAgreement1.CA0_OH_Customer = Org1.PK;
			commissionAgreement1.FillWithValidTestData();
			commissionAgreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement1.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement1.CA0_EffectiveDate = ZDate.Today;

			var item1 = commissionAgreement1.ProductItems.AddNew(true, "SHP");
			item1.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementPctRecipient1 = commissionAgreement1.Recipients.AddNew();
			agreementPctRecipient1.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient1.CAR_Share = 1;

			var agreementPctRecipient1Rate = agreementPctRecipient1.Rates.AddNew();
			agreementPctRecipient1Rate.CAT_CommissionPercentage = 10;

			var commissionAgreement2 = opportunity2.ApprovedCommissionAgreements.AddNew();
			commissionAgreement2.CA0_OH_Customer = Org2.PK;
			commissionAgreement2.FillWithValidTestData();
			commissionAgreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement2.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement2.CA0_EffectiveDate = ZDate.Today;

			var item2 = commissionAgreement2.ProductItems.AddNew(true, "SHP");
			item2.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementPctRecipient2 = commissionAgreement2.Recipients.AddNew();
			agreementPctRecipient2.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient2.CAR_Share = 1;

			var agreementPctRecipient2Rate = agreementPctRecipient2.Rates.AddNew();
			agreementPctRecipient2Rate.CAT_CommissionPercentage = 10;

			var shipment = TestObjectCreator.CreateShipment("1001");

			Job = TestObjectCreator.CreateJob(shipment);
			Job.JH_OA_LocalChargesAddr = address1.PK;
			AssertEquals("Precondition", Org1, Job.LocalCharges);

			Invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			Invoice.AH_InvoiceAmount = 1000;
			Invoice.AH_OH = Org1.PK;

			Line1 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			Line1.AL_LineType = TransactionLineTypes.Revenue;
			Line1.AL_JH = Job.PK;
			Line1.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.CC1.AC_IsCommissionable = true;

			Line2 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 200, 10, 0);
			Line2.AL_LineType = TransactionLineTypes.Accrual;
			Line2.AL_JH = Job.PK;
			Line2.AL_AC = TestObjectCreator.CC2.PK;
			TestObjectCreator.CC2.AC_IsCommissionable = false;

			TestObjectCreator.CreateCharge(Line1);

			Factory.Save();
		}

		void SetupTransactionsAndCommissionsForFixed()
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Org1.Addresses.AddNew();
			address1.OA_Address1 = "72 O'Riordan St";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ACL";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var commissionAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
			commissionAgreement.CA0_OH_Customer = Org1.PK;
			commissionAgreement.FillWithValidTestData();
			commissionAgreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement.CA0_EffectiveDate = ZDate.Today;

			var item = commissionAgreement.ProductItems.AddNew(true, "SHP");
			item.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementFixRecipient = commissionAgreement.Recipients.AddNew();
			agreementFixRecipient.CAR_GS_NKStaff = "ACL";
			agreementFixRecipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			agreementFixRecipient.CAR_Share = 1;

			var agreementFixRecipientRate = agreementFixRecipient.Rates.AddNew();
			agreementFixRecipientRate.CAT_CommissionAmount = 700;
			agreementFixRecipientRate.CAT_RX_NKCommissionCurrency = "AUD";

			var shipment = TestObjectCreator.CreateShipment("1001");

			Job = TestObjectCreator.CreateJob(shipment);
			Job.JH_OA_LocalChargesAddr = address1.PK;
			AssertEquals("Precondition", Org1, Job.LocalCharges);

			Invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			Invoice.AH_InvoiceAmount = 1000;
			Invoice.AH_OH = Org1.PK;

			Line1 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			Line1.AL_LineType = TransactionLineTypes.Revenue;
			Line1.AL_JH = Job.PK;
			Line1.AL_AC = TestObjectCreator.CC1.PK;
			TestObjectCreator.CC1.AC_IsCommissionable = true;

			Line2 = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 200, 10, 0);
			Line2.AL_LineType = TransactionLineTypes.Accrual;
			Line2.AL_JH = Job.PK;
			Line2.AL_AC = TestObjectCreator.CC2.PK;
			TestObjectCreator.CC2.AC_IsCommissionable = false;

			TestObjectCreator.CreateCharge(Line1);

			Factory.Save();
		}

		void SetupBulkInvoice()
		{
			var shipment1 = TestObjectCreator.CreateShipment("2001");
			shipment1.JS_E_DEP = ZDateTime.Today;
			var shipment2 = TestObjectCreator.CreateShipment("2002");
			shipment2.JS_E_DEP = ZDateTime.Today;
			var shipment3 = TestObjectCreator.CreateShipment("2003");
			shipment3.JS_E_DEP = ZDateTime.Today;

			Job1 = TestObjectCreator.CreateJob(shipment1);
			Job1.JH_OA_LocalChargesAddr = Org1.Addresses[0].PK;
			AssertEquals("Precondition", Org1, Job1.LocalCharges);

			Job2 = TestObjectCreator.CreateJob(shipment2);
			Job2.JH_OA_LocalChargesAddr = Org1.Addresses[0].PK;
			AssertEquals("Precondition", Org1, Job2.LocalCharges);

			Job3 = TestObjectCreator.CreateJob(shipment3);
			Job3.JH_OA_LocalChargesAddr = Org1.Addresses[0].PK;
			AssertEquals("Precondition", Org1, Job3.LocalCharges);

			BulkInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1);
			BulkInvoice.AH_InvoiceAmount = 1000;
			BulkInvoice.AH_OH = Org1.PK;

			var line1 = TestObjectCreator.CreateInvoiceLine(BulkInvoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line1.AL_LineType = TransactionLineTypes.Cost;
			line1.AL_JH = Job1.PK;
			line1.AL_AC = TestObjectCreator.CC4.PK;
			TestObjectCreator.CC4.AC_IsCommissionable = true;

			var line2 = TestObjectCreator.CreateInvoiceLine(BulkInvoice, TestObjectCreator.AUD, 1, 200, 10, 0);
			line2.AL_LineType = TransactionLineTypes.Cost;
			line2.AL_JH = Job2.PK;
			line2.AL_AC = TestObjectCreator.CC5.PK;
			TestObjectCreator.CC5.AC_IsCommissionable = true;

			var line3 = TestObjectCreator.CreateInvoiceLine(BulkInvoice, TestObjectCreator.AUD, 1, 300, 10, 0);
			line3.AL_LineType = TransactionLineTypes.Cost;
			line3.AL_JH = Job3.PK;
			line3.AL_AC = TestObjectCreator.CC6.PK;
			TestObjectCreator.CC6.AC_IsCommissionable = true;

			TestObjectCreator.CreateCharge(line1);
			TestObjectCreator.CreateCharge(line2);
			TestObjectCreator.CreateCharge(line3);

			Factory.Save();
		}

		#endregion
	}
}
