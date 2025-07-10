using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	class JobClosedCommissionCreatorTest : CommissionCreatorTestCase
	{
		#region Tests

		public void TestAddCommissionCalculationQueueItem()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var shipment = objectCreator.CreateShipment("S001");
			var job = objectCreator.CreateJob(shipment);
			var invoice1999 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1999.AH_JH = job.PK;
			invoice1999.AH_PostDate = new ZDateTime(1999, 1, 1);

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.RunCommissionCreationInBackground.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creator = new JobClosedCommissionCreatorForTesting(job, new ZDateTime(2004, 1, 1));
				creator.PostQueueItemOrCreateCommissionsOnJobClosed();
			}

			var queueItem = Factory.LoadTop1<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_JH, job.PK));

			AssertNotNull("Should be a queue item for the job", queueItem);
			AssertEquals("Operation on Queue item", OrgCommissionCalculationQueueOperationCodeList.Codes.Creation, queueItem.CAQ_Operation);
			AssertEquals("Job Closed Date on Queue item", new ZDateTime(2004, 1, 1), queueItem.CAQ_JobClosedDate);
		}

		public void TestCreateCommissions()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var shipment = objectCreator.CreateShipment("S001");
			var job = objectCreator.CreateJob(shipment);
			var invoice1999 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1999.AH_JH = job.PK;
			invoice1999.AH_PostDate = new ZDateTime(1999, 1, 1);

			var invoice2000 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2000.AH_JH = job.PK;
			invoice2000.AH_PostDate = new ZDateTime(2000, 1, 1);

			var invoice2003 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2003.AH_JH = job.PK;
			invoice2003.AH_PostDate = new ZDateTime(2003, 1, 1);

			var invoice2005 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2005.AH_JH = job.PK;
			invoice2005.AH_PostDate = new ZDateTime(2005, 1, 1);

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_JH = job.PK;
			creditNote.AH_PostDate = new ZDateTime(2003, 1, 1);

			var adjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			adjustmentNote.AH_JH = job.PK;
			adjustmentNote.AH_PostDate = new ZDateTime(2003, 1, 1);

			var jobRevJournal = Factory.NewWithValidTestData<JobRevenueJournal>();
			jobRevJournal.AH_JH = job.PK;
			jobRevJournal.AH_PostDate = new ZDateTime(2003, 1, 1);

			var overpayment = Factory.NewWithValidTestData<AROverpayment>();
			overpayment.AH_JH = job.PK;
			overpayment.AH_PostDate = new ZDateTime(2003, 1, 1);

			var jrj = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_PostDate = new ZDateTime(2003, 1, 1);

			var invoice2004 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2004.AH_JH = job.PK;
			invoice2004.AH_PostDate = new ZDateTime(2004, 1, 1, 23, 10, 10);

			Factory.Save();

			using (Factory.SetTempContext(Enterprise.Integration.Accounting.BusinessContext.CalculateCommissionOnJobClosure))
			{
				var creator = new JobClosedCommissionCreatorForTesting(job, new ZDateTime(2004, 1, 1));
				creator.CreateCommissions(new CreateCommissionContext { FromDate = new ZDateTime(2000, 1, 1) });

				AssertContainsExactElementsInAnyOrder(new ZGuid[] { invoice2000.PK, invoice2003.PK, creditNote.PK, adjustmentNote.PK, jrj.PK, invoice2004.PK }, creator.JobRelatedTransactionsCreatedCommissionForTest.Select(x => x.PK));

				foreach (var invoiceBizObj in creator.JobRelatedTransactionsCreatedCommissionForTest)
				{
					AssertEquals("Invoice is loaded in the job Factory", job.Factory, invoiceBizObj.Factory);
				}
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCreateCommissions_LastContext()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var shipment = objectCreator.CreateShipment("S001");
			var job = objectCreator.CreateJob(shipment);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			job.JH_GC = company.PK;
			var invoice1999 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1999.AH_GC = company.PK;
			invoice1999.AH_GB = branch.PK;
			invoice1999.AH_JH = job.PK;
			invoice1999.AH_PostDate = new ZDateTime(1999, 1, 1);

			Factory.Save();

			using (Factory.SetTempContext(Enterprise.Integration.Accounting.BusinessContext.CalculateCommissionOnJobClosure))
			{
				var creator = new JobClosedCommissionCreatorForTesting(job, new ZDateTime(2004, 1, 1));

				OrganisationsDataRegistry.Instance.OverwriteOldValuesOnJobClosed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				creator.CreateCommissionsOnJobClosed();
				AssertEquals("Overwrite Old Values default", false, creator.LastContext.OverwriteOldValues);

				OrganisationsDataRegistry.Instance.OverwriteOldValuesOnJobClosed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				creator.CreateCommissionsOnJobClosed();
				AssertEquals("Overwrite Old Values", true, creator.LastContext.OverwriteOldValues);

				OrganisationsDataRegistry.Instance.OverwriteOldValuesOnJobClosed.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				creator.CreateCommissionsOnJobClosed();
				AssertEquals("Overwrite Old Values default at company level", false, creator.LastContext.OverwriteOldValues);

				OrganisationsDataRegistry.Instance.OverwriteOldValuesOnJobClosed.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				creator.CreateCommissionsOnJobClosed();
				AssertEquals("Overwrite Old Values at company level", true, creator.LastContext.OverwriteOldValues);
			}
		}

		public void TestCreateReversalCreditNoteCommissions()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var shipment = objectCreator.CreateShipment("S001");
			var job = objectCreator.CreateJob(shipment);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = job.PK;
			invoice.AH_PostDate = new ZDateTime(2003, 1, 1);
			var invoiceLine = objectCreator.CreateARInvoiceLineWithJobCharge(invoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1.0m, "AR Line", 100m, TestObjectCreator.GST1.PK);

			var amendingCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingCreditNote1.AH_JH = job.PK;
			amendingCreditNote1.AH_PostDate = new ZDateTime(2003, 1, 2);
			amendingCreditNote1.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingCreditNoteLine1 = objectCreator.CreateARCreditNoteLine(amendingCreditNote1, job, TestObjectCreator.CC2, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line0");
			amendingCreditNoteLine1.AL_AT = TestObjectCreator.GST1.PK;

			var amendingCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingCreditNote2.AH_JH = job.PK;
			amendingCreditNote2.AH_PostDate = new ZDateTime(2002, 12, 31);
			amendingCreditNote2.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingCreditNoteLine2 = objectCreator.CreateARCreditNoteLine(amendingCreditNote2, job, TestObjectCreator.CC2, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line1");
			amendingCreditNoteLine2.AL_AT = TestObjectCreator.GST1.PK;

			var amendingPartialCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote1.AH_JH = job.PK;
			amendingPartialCreditNote1.AH_PostDate = new ZDateTime(2003, 1, 2);
			amendingPartialCreditNote1.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote1Line = objectCreator.CreateARCreditNoteLine(amendingPartialCreditNote1, job, TestObjectCreator.CC2, 80m, TestObjectCreator.AUD, 1.0m, "Credit Line2");
			amendingPartialCreditNote1Line.AL_AT = TestObjectCreator.GST1.PK;

			var amendingPartialCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote2.AH_JH = job.PK;
			amendingPartialCreditNote2.AH_PostDate = new ZDateTime(2003, 1, 2);
			amendingPartialCreditNote2.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote2Line = objectCreator.CreateARCreditNoteLine(amendingPartialCreditNote2, job, TestObjectCreator.CC2, 100m, TestObjectCreator.CNY, 1.0m, "Credit Line3");
			amendingPartialCreditNote2Line.AL_AT = TestObjectCreator.GST1.PK;

			var amendingPartialCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
			amendingPartialCreditNote3.AH_JH = job.PK;
			amendingPartialCreditNote3.AH_PostDate = new ZDateTime(2003, 1, 2);
			amendingPartialCreditNote3.AH_TransactionBelongsToGroup = invoice.PK;
			var amendingPartialCreditNote3Line = objectCreator.CreateARCreditNoteLine(amendingPartialCreditNote3, job, TestObjectCreator.CC3, 100m, TestObjectCreator.AUD, 1.0m, "Credit Line4");
			amendingPartialCreditNote3Line.AL_AT = TestObjectCreator.GST1.PK;

			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.AH_JH = job.PK;
			creditNote.AH_PostDate = new ZDateTime(2003, 1, 2);

			Factory.Save();

			using (Factory.SetTempContext(Enterprise.Integration.Accounting.BusinessContext.CalculateCommissionOnJobClosure))
			{
				var creator = new JobClosedCommissionCreatorForTesting(job, new ZDateTime(2004, 1, 1));
				creator.CreateCommissions(new CreateCommissionContext { FromDate = new ZDateTime(2000, 1, 1) });

				AssertContainsExactElementsInAnyOrder(new ZGuid[] { amendingCreditNote1.PK, amendingCreditNote2.PK }, creator.ReversalTransactionsCreatedCommissionForTest.Select(x => x.PK));
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { invoice.PK, creditNote.PK, amendingPartialCreditNote1.PK, amendingPartialCreditNote2.PK, amendingPartialCreditNote3.PK }, creator.JobRelatedTransactionsCreatedCommissionForTest.Select(x => x.PK));

				foreach (var invoiceBizObj in creator.ReversalTransactionsCreatedCommissionForTest)
				{
					AssertEquals("Invoice is loaded in the job Factory", job.Factory, invoiceBizObj.Factory);
				}
			}
		}

		public void TestCommissionsRegeneratedWhenChangingOriginBackToOriginal()
		{
			OrganisationsDataRegistry.Instance.OverwriteOldValuesOnJobClosed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupTransactionsAndCommissions();

			var originalOrigin = Shipment.JS_RL_NKOrigin;

			var newOrigin = Factory.NewWithValidTestData<RefUNLOCO>();
			newOrigin.RL_Code = "TESTO";

			Job.Close(null, null);
			Factory.Save();

			AssertEquals("Pre-condition: 3 commission lines should be created.", 3, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			Shipment.JS_RL_NKOrigin = newOrigin.RL_Code;
			Factory.Save();

			AssertEquals("6 commission lines should be created (3 original + 3 reversal).", 6, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			Shipment.JS_RL_NKOrigin = originalOrigin;
			Job.ReOpen();
			Job.Close(null, null);
			Factory.Save();

			AssertEquals("9 commission lines should be created (3 original + 3 original reversal + 3 original reinstate).", 9, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
		}

		#endregion

		#region Implementation

		OrgHeader Org1;
		Job Job;
		ForwardingShipment Shipment;
		InvoicingBase Invoice;
		InvoicingLineBase Line1;
		InvoicingLineBase Line2;

		void SetupTransactionsAndCommissions()
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address = Org1.Addresses.AddNew();
			address.OA_Address1 = "72 O'Riordan St";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ACL";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TST";

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();

			var commissionAgreement = opportunity1.ApprovedCommissionAgreements.AddNew();
			commissionAgreement.CA0_OH_Customer = Org1.PK;
			commissionAgreement.FillWithValidTestData();
			commissionAgreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement.CA0_EffectiveDate = ZDate.Today;

			var item = commissionAgreement.ProductItems.AddNew(true, "SHP");
			item.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;

			var agreementPctRecipient = commissionAgreement.Recipients.AddNew();
			agreementPctRecipient.CAR_GS_NKStaff = staff1.GS_Code;
			agreementPctRecipient.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient.CAR_Share = 1;

			var agreementPctRecipient1Rate = agreementPctRecipient.Rates.AddNew();
			agreementPctRecipient1Rate.CAT_CommissionPercentage = 10;

			var agreementFixRecipient = commissionAgreement.Recipients.AddNew();
			agreementFixRecipient.CAR_GS_NKStaff = staff2.GS_Code;
			agreementFixRecipient.CAR_CommissionType = CommissionTypes.Codes.FIX;
			agreementFixRecipient.CAR_Share = 1;
			var agreementFixRecipientRate = agreementFixRecipient.Rates.AddNew();
			agreementFixRecipientRate.CAT_CommissionAmount = 400;
			agreementFixRecipientRate.CAT_RX_NKCommissionCurrency = "AUD";

			Shipment = TestObjectCreator.CreateShipment("1001");

			Job = TestObjectCreator.CreateJob(Shipment);
			Job.JH_OA_LocalChargesAddr = address.PK;
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
			Line2.AL_LineType = TransactionLineTypes.Revenue;
			Line2.AL_JH = Job.PK;
			Line2.AL_AC = TestObjectCreator.CC2.PK;
			TestObjectCreator.CC2.AC_IsCommissionable = true;

			TestObjectCreator.CreateCharge(Line1);
			TestObjectCreator.CreateCharge(Line2);

			Factory.Save();
		}

		#endregion
	}

	[TestedType(typeof(JobClosedCommissionCreator.JobClosedCommissionTransactionFilter))]
	class JobClosedCommissionTransactionFilterTest : JobInvoicePrintingFilterTest
	{
		public override void TestFilteredCollectionsWithSecuritySettings()
		{
			Assert(true);
		}

		public override void TestInvoiceListForGatewayBilling()
		{
			Assert(true);
		}

		public override void TestDontLoadAnyJobsWhenNoJobsArePassed()
		{
			Assert(true);
		}

		public override void TestRefreshInvoiceWithLines()
		{
			Factory.Save();

			TestObjectCreator cR = new TestObjectCreator(Factory);

			Shipment1 = CreateShipment();
			JobHeader testJob1 = CreateJobHeader(Shipment1);
			Shipment1.JS_UniqueConsignRef = "1";

			AccChargeCode testChargeCode1 = cR.CreateChargeCode("TST", "Test Charge Code", "REV", 0m, null, null, "ALL");
			AccChargeCode testChargeCode2 = cR.CreateChargeCode("TST_2", "Test Charge Code", "REV", 0m, null, null, "ALL");

			InvoicingLineBase testLine1 = (InvoicingLineBase)Invoice1_4.Lines.AddNew();
			InvoicingLineBase testLine2 = (InvoicingLineBase)Invoice1_4.Lines.AddNew();

			testLine1.AL_JH = testJob1.PK;
			testLine2.AL_JH = testJob1.PK;

			testLine1.AL_AC = testChargeCode1.PK;
			testLine2.AL_AC = testChargeCode2.PK;

			testLine1.AL_AH = Invoice1_4.PK;
			testLine2.AL_AH = Invoice1_4.PK;

			TestObjectCreator.CreateJobCharge(testLine1, testJob1, testChargeCode1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(testLine2, testJob1, testChargeCode1, TestObjectCreator.AUD);

			Factory.Save();

			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Consol, Header1);
			printingFilter.RefreshInvoiceList();
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, testJob1.PK, 9);
		}

		public override void TestIsFreightConsol()
		{
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			AssertEquals("Is Freight Consol", false, printingFilter.IsFreightConsol);

			GlbDepartment gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			{
				printingFilter = GetNewBusinessObject(Shipment1, Header1);
				AssertEquals("Is Freight Consol", false, printingFilter.IsFreightConsol);

				printingFilter = GetNewBusinessObject(Consol, Header1);
				AssertEquals("Is Freight Consol", false, printingFilter.IsFreightConsol);
			}
		}

		public override void TestRefreshInvoiceListWithConsol()
		{
			Consol.Shipments.Add(Shipment1);
			Factory.Save();

			var printingFilter = GetNewBusinessObject(Consol, Header1);
			printingFilter.RefreshInvoiceList();
			AssertEquals("Invoice Count", 9, printingFilter.Transactions.Count);

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, Header1.PK, 6);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, Header1.PK, 3);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, Header1.PK, 1);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, Header1.PK, 2);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, Header1.PK, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, Header1.PK, 1);

			AssertRefreshInvoiceList(printingFilter, Org3.PK, ZString.Empty, Header1.PK, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.Invoice, Header1.PK, 0);
			AssertRefreshInvoiceList(printingFilter, Org3.PK, TransactionTypes.CreditNote, Header1.PK, 0);

			AssertRefreshInvoiceList(printingFilter, New1.PK, ZString.Empty, Header1.PK, 1);
			AssertRefreshInvoiceList(printingFilter, New1.PK, TransactionTypes.Invoice, Header1.PK, 1);
			AssertRefreshInvoiceList(printingFilter, New1.PK, TransactionTypes.CreditNote, Header1.PK, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, Header1.PK, 9);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, Header1.PK, 5);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, Header1.PK, 2);
		}

		public override void TestRefreshInvoiceListWithShipment()
		{
			Factory.Save();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			printingFilter.RefreshInvoiceList();

			AssertEquals("Invoice Count", 9, printingFilter.Transactions.Count);

			AssertRefreshInvoiceList(printingFilter, Org1.PK, ZString.Empty, ZGuid.Empty, 6);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.Invoice, ZGuid.Empty, 3);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.CreditNote, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org1.PK, TransactionTypes.JobRevenueJournal, Header1.PK, 1);

			AssertRefreshInvoiceList(printingFilter, Org2.PK, ZString.Empty, ZGuid.Empty, 2);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.Invoice, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.CreditNote, ZGuid.Empty, 1);
			AssertRefreshInvoiceList(printingFilter, Org2.PK, TransactionTypes.JobRevenueJournal, ZGuid.Empty, 0);

			AssertRefreshInvoiceList(printingFilter, New1.PK, ZString.Empty, Header1.PK, 1);
			AssertRefreshInvoiceList(printingFilter, New1.PK, TransactionTypes.Invoice, Header1.PK, 1);
			AssertRefreshInvoiceList(printingFilter, New1.PK, TransactionTypes.CreditNote, Header1.PK, 0);

			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, ZString.Empty, ZGuid.Empty, 9);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.Invoice, ZGuid.Empty, 5);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.CreditNote, ZGuid.Empty, 2);
			AssertRefreshInvoiceList(printingFilter, ZGuid.Empty, TransactionTypes.JobRevenueJournal, ZGuid.Empty, 1);
		}

		public override void TestRefreshInvoiceListWithDefaultPostDateFilter()
		{
			Factory.Save();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			printingFilter.RefreshInvoiceList();

			AssertEquals("Should find all invoices with Header1", 9, printingFilter.Transactions.Count);
		}

		public override void TestResetInvoiceList()
		{
			Factory.Save();
			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			printingFilter.RefreshInvoiceList();
			AssertEquals("Invoice Count", 9, printingFilter.Transactions.Count);

			printingFilter.Transactions.RemoveAll();
			AssertEquals("Invoice Count", 0, printingFilter.Transactions.Count);

			printingFilter.ResetInvoiceList();
			AssertEquals("Debtor", ZGuid.Empty, printingFilter.DebtorOrCreditor);
			AssertEquals("Job Number", ZGuid.Empty, printingFilter.JobNumber);
			AssertEquals("Transaction Type", ZString.Empty, printingFilter.TransactionType);
			AssertEquals("Invoice Count", 9, printingFilter.Transactions.Count);
		}

		public override void TestExcludeReversed()
		{
			Factory.Save();
			Invoice1_4.AH_TransactionBelongsToGroup = Invoice1_1.PK;
			Invoice1_5.AH_TransactionBelongsToGroup = Invoice1_3.PK;
			new ReversingFactory().NewReversing(Invoice1_5).Reverse();
			Factory.Save();

			JobInvoicePrintingFilter printingFilter = GetNewBusinessObject(Shipment1, Header1);
			printingFilter.RefreshInvoiceList();

			AssertEquals("Should find all invoices with Header1 except reversal", 8, printingFilter.Transactions.Count);
		}

		public void TestLoadJobRevenueJournal()
		{
			var jrj = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, Header1, 100);
			jrj.JournalLines[0].AL_GE = jrj.JournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			jrj.AH_PostDate = new ZDateTime(2003, 1, 1);
			jrj.AH_OH = Org1.PK;

			Factory.Save();
			var printingFilter = GetNewBusinessObject(Shipment1, Header1);

			printingFilter.RefreshInvoiceList();
			AssertEquals(10, printingFilter.Transactions.Count);
			AssertNotNull(printingFilter.Transactions.FindByPK(jrj.PK));
		}

		public override void TestIndexHintOnAH_JHIsAdded()
		{
			Factory.Save();
			using (Db.Connection.TrackExecutedCommands())
			{
				var printingFilter = GetNewBusinessObject(Shipment1, Header1);
				printingFilter.RefreshInvoiceList();
				var executedQuery = Db.Connection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AccTransactionHeader")).Item1;
				AssertContains("Index hint is added when calculating commission to avoid poor SQL indexes when calculating commission", "FROM dbo.AccTransactionHeader WITH (FORCESEEK, INDEX(FK_RX__AH_JH, PK_UC__AH_PK))", executedQuery);
			}
		}

		public override void TestUseReadOnlyFactory()
		{
			Assert(true);
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(TestJob.Parent as IBusiness, TestJob);
		}

		protected override JobInvoicePrintingFilter GetNewBusinessObject(IBusiness hostBusinessObject, Job jobHeader)
		{
			return new JobClosedCommissionCreator.JobClosedCommissionTransactionFilter(jobHeader, ZDateTime.MinSmallDateTimeValue, ZDateTime.Now);
		}

		protected override JobInvoicePrintingFilter GetNewBusinessObject(ForwardingConsol hostBusinessObject, Job[] jobHeaders)
		{
			throw new NotSupportedException("Not Supported");
		}

		#region Get Query Overrides

		protected override JobInvoicePrintingFilter CreateInstanceForTest(IBusiness hostBusinessObject, Job jobHeader, ZDateTime from, ZDateTime to)
		{
			return new JobClosedCommissionCreator.JobClosedCommissionTransactionFilter(jobHeader, from, to);
		}

		public override void TestGetQueryForConsolWithoutJobNumber()
		{
			Assert("JobClosedCommissionTransactionFilter will not have a consol host bizO.", true);
		}

		[TestDate(2017, 10, 15)]
		public override void TestGetQueryWithJobNumber()
		{
			TestAPGetQueryWithJobNumberCore(true);
			TestARGetQueryWithJobNumberCore(true);
			TestJobCostGetQueryWithJobNumberCore();
		}

		#endregion

		public Job TestJob
		{
			get
			{
				if (testJob == null)
				{
					var shipment = ObjectCreator.CreateShipment("S001");
					testJob = ObjectCreator.CreateJob(shipment);
					Factory.Save();
				}
				return testJob;
			}
		}
		Job testJob;

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (objectCreator == null)
				{
					objectCreator = new TestObjectCreator(Factory);
				}
				return objectCreator;
			}
		}
		TestObjectCreator objectCreator;
	}

	class JobClosedCommissionCreatorForTesting : JobClosedCommissionCreator
	{
		public JobClosedCommissionCreatorForTesting(JobHeader job, ZDateTime jobCloseTime) : base(job, jobCloseTime)
		{
		}

		internal IList<ICommissionableTransaction> JobRelatedTransactionsCreatedCommissionForTest = new List<ICommissionableTransaction>();

		internal IList<ICommissionableTransaction> ReversalTransactionsCreatedCommissionForTest = new List<ICommissionableTransaction>();

		internal CreateCommissionContext LastContext;

		protected override void CreateJobRelatedTransactionCommissions(ICommissionableTransaction transaction, CreateCommissionContext context)
		{
			LastContext = context;
			base.CreateJobRelatedTransactionCommissions(transaction, context);
		}

		protected override void CreateJobRelatedTransactionCommissions(ICommissionableTransaction transaction, bool isAmendingCreditNote)
		{
			if (isAmendingCreditNote)
			{
				if (JobRelatedTransactionsCreatedCommissionForTest.FirstOrDefault(c => c.PK == transaction.AH_TransactionBelongsToGroup) != null)
				{
					ReversalTransactionsCreatedCommissionForTest.Add(transaction);
				}
			}
			else
			{
				JobRelatedTransactionsCreatedCommissionForTest.Add(transaction);
			}
		}
	}
}
