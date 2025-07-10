using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingBaseCriticalValidationTest : TransactionHeaderWithLinesCriticalValidationTest
	{
		[TestDate(2023, 1, 1)]
		public void TestValidateTransactionHeaderComplianceBookWithEmptyPrintingAuthorizationNumber_ConvertARToAP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			testObjectCreator.NonCurrentCompany.OrgProxy.OH_IsCreditor = true;
			Factory.Save();

			InvoicingBase arInvoice;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, testObjectCreator.NonCurrentCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				arInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
				arInvoice.AH_OH = orgProxy.PK;
				arInvoice.AH_TransactionReference = "TestReference";
				Factory.Save();
			}

			testObjectCreator.CreateExchangeRate(testObjectCreator.AUD, 1);
			using (new DisposableAction(() => InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly = true, () => InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly = false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var converter = new UnapprovedTransactionConverter(new BusinessObjectFactory());
				converter.Candidates.Load();
				AssertEquals("Prerequisite: candidates collection should have loaded the 1 new invoices", 1, converter.Candidates.Count);

				var apInvoice = converter.ConvertToAP(arInvoice, false);
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Should NOT Show Error For Compliance Book With Empty PrintingAuthorizationNumber");
				AssertOnSavingCheck(apInvoice, testCase);
				AssertNull("Should NOT trigger error report", ErrorReporter.LastExceptionReported);
			}
		}

		[TestDate(2023, 1, 1)]
		public void TestValidateTransactionHeaderComplianceBookWithEmptyPrintingAuthorizationNumber()
		{
			using (new DisposableAction(() => InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly = true, () => InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly = false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = testObjectCreator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "TXI", 1, 100, 1);
				Factory.Save();

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
				invoice.AH_XD_ComplianceBook = sequence.PK;

				AssertNullOrEmpty("Percondition", sequence.XD_PrintingAuthorizationNumber);
				AssertEquals("Percondition", true, invoice.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber);
				AssertEquals("Percondition", true, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				AssertEquals("Percondition", false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value);

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Should Show Error For Compliance Book With Empty PrintingAuthorizationNumber",
					true,
					CriticalValidationErrorType.AbortOnSavingProcess,
$@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: {CriticalValidationMessageTemplate.PostInvoiceWhenComplianceBookWithEmptyPrintingAuthorizationnumberErrorMessage}");
				AssertOnSavingCheck(invoice, testCase);
				AssertNull("Should NOT trigger error report", ErrorReporter.LastExceptionReported);
			}
		}

		[TestDate(2012, 06, 15)]
		public void TestNoCriticalValidationResultOnDuplicateInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			OrgHeader organisation = testObjectCreator.GetOrganisation();

			ForwardingShipment shipment = testObjectCreator.CreateShipment("1234");
			Job job = Job.CreateWithMutex(Factory, shipment);
			job.JH_JobNum = "111";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Enterprise.Environment.Env.Registry.FreightChargeCode;
			charge.JR_AgentDeclaredSellAmt = 2000m;
			charge.JR_AgentDeclaredCostAmt = 1000m;
			charge.JR_IsIncludedInProfitShare = true;

			APInvoice invoice1 = (APInvoice)testObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", testObjectCreator.AUD, 1m);
			invoice1.AH_GB = otherBranch.PK;
			invoice1.AH_OH = organisation.PK;
			invoice1.AH_InvoiceDate = ZDateTime.BrettsBirthday;
			invoice1.AH_PostDate = ZDateTime.BrettsBirthday;
			APInvoiceLine line1 = (APInvoiceLine)testObjectCreator.CreateInvoiceLine(invoice1, 100, testObjectCreator.AUD, 1m);
			invoice1.Lines.Add(line1);
			line1.AL_JH = shipment.Job.PK;
			line1.AL_OH = testObjectCreator.AALSHI.PK;
			line1.AL_AC = testObjectCreator.CC1.PK;
			line1.AL_GB = invoice1.Company.Branches[0].PK;
			testObjectCreator.CreateCharge(line1);
			Factory.Save();

			APInvoice invoice2 = (APInvoice)testObjectCreator.CreateInvoice(typeof(APInvoice), "INV001", testObjectCreator.AUD, 1m);
			invoice2.AH_GB = otherBranch.PK;
			invoice2.AH_OH = organisation.PK;
			invoice2.AH_InvoiceDate = ZDateTime.BrettsBirthday;
			invoice2.AH_PostDate = ZDateTime.BrettsBirthday;
			APInvoiceLine line2 = (APInvoiceLine)testObjectCreator.CreateInvoiceLine(invoice2, 100, testObjectCreator.AUD, 1m);
			invoice2.Lines.Add(line2);
			line2.AL_JH = shipment.Job.PK;
			line2.AL_OH = testObjectCreator.AALSHI.PK;
			line2.AL_AC = testObjectCreator.CC1.PK;
			line2.AL_GB = invoice2.Company.Branches[0].PK;
			testObjectCreator.CreateCharge(line2);

			var expectedMessage = string.Empty;
			var expectedDescription = string.Empty;
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				expectedDescription,
				false,
				CriticalValidationErrorType.APTransactionNumberAlreadyUsedForAnotherOrganization_2,
				expectedDescription, expectedMessage);
			AssertOnSavingCheck(invoice2, testCase);
		}

		// Put ReversedTransactionsShouldNotHaveLinesLinkedToCharges test series here because we could not access Accounting logic in MasterFiles.AccTransactionHeaderCriticalValidation class.
		#region TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job3 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job4 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job5 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var apCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APCreditNote), "APCreditNote001", job1);
			var arCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARCreditNote), "ARCreditNote001", job2);
			var apInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APInvoice), "APInvoice001", job3);
			var arInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARInvoice), "ARInvoice001", job4);
			var jrj = CreateJobRevenueJournalWithLine(creator, job5, shouldCreateLinkedCharge: false);

			Factory.Save();

			PrepareInvoiceData(apCreditNote);
			PrepareInvoiceData(arCreditNote);
			PrepareInvoiceData(apInvoice);
			PrepareInvoiceData(arInvoice);
			PrepareInvoiceData(jrj);

			AssertEquals(apCreditNote.Lines[0].PK, job1.Charges[0].JR_AL_APLine);
			AssertEquals(arCreditNote.Lines[0].PK, job2.Charges[0].JR_AL_ARLine);
			AssertEquals(apInvoice.Lines[0].PK, job3.Charges[0].JR_AL_APLine);
			AssertEquals(arInvoice.Lines[0].PK, job4.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[0].PK, job5.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[1].PK, job5.Charges[1].JR_AL_ARLine);
			AssertEquals(6, Factory.Load<JobCharge>(new ZQuery()).Length);

			Func<TransactionHeaderWithLines, Job, TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = (transaction, job) =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction line of reversed invoice / credit note / job revenue journal should not be linked to job charge.",
					true,
					CriticalValidationErrorType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges_7,
					CriticalValidationMessageTemplate.ReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage,
					GetCancelledTransactionHeaderWithNoLineLinkedToChargeMessageBody(null, transaction, job));

				return testCase;
			};

			AssertOnSavingCheck(apCreditNote, testCaseGetter(apCreditNote, job1));
			AssertOnSavingCheck(arCreditNote, testCaseGetter(arCreditNote, job2));
			AssertOnSavingCheck(apInvoice, testCaseGetter(apInvoice, job3));
			AssertOnSavingCheck(arInvoice, testCaseGetter(arInvoice, job4));
			AssertOnSavingCheck(jrj, testCaseGetter(jrj, job5));
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_AccTransactionHeader()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job3 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job4 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job5 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var apCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APCreditNote), "APCreditNote001", job1);
			var arCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARCreditNote), "ARCreditNote001", job2);
			var apInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APInvoice), "APInvoice001", job3);
			var arInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARInvoice), "ARInvoice001", job4);
			var jrj = CreateJobRevenueJournalWithLine(creator, job5, shouldCreateLinkedCharge: false);

			Factory.Save();

			AssertEquals(apCreditNote.Lines[0].PK, job1.Charges[0].JR_AL_APLine);
			AssertEquals(arCreditNote.Lines[0].PK, job2.Charges[0].JR_AL_ARLine);
			AssertEquals(apInvoice.Lines[0].PK, job3.Charges[0].JR_AL_APLine);
			AssertEquals(arInvoice.Lines[0].PK, job4.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[0].PK, job5.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[1].PK, job5.Charges[1].JR_AL_ARLine);
			AssertEquals(6, Factory.Load<JobCharge>(new ZQuery()).Length);

			var apCreditNoteBase = Factory.Load<AccTransactionHeader>(apCreditNote.PK);
			var arCreditNoteBase = Factory.Load<AccTransactionHeader>(arCreditNote.PK);
			var apInvoiceBase = Factory.Load<AccTransactionHeader>(apInvoice.PK);
			var arInvoiceBase = Factory.Load<AccTransactionHeader>(arInvoice.PK);
			var jrjBase = Factory.Load<AccTransactionHeader>(jrj.PK);

			apCreditNoteBase.IsCancelled = true;
			arCreditNoteBase.IsCancelled = true;
			apInvoiceBase.IsCancelled = true;
			arInvoiceBase.IsCancelled = true;
			jrjBase.IsCancelled = true;

			Func<TransactionHeaderWithLines, Job, TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = (transaction, job) =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction line of reversed invoice / credit note / job revenue journal should not be linked to job charge.",
					true,
					CriticalValidationErrorType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges_7,
					CriticalValidationMessageTemplate.ReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage,
					GetCancelledTransactionHeaderWithNoLineLinkedToChargeMessageBody("This error is captured on an AccTransactionHeader which was not loaded as AR/AP Invoice/CreditNote or Job Revenue Journal.", transaction, job));

				return testCase;
			};

			AssertOnSavingCheck(apCreditNoteBase, testCaseGetter(apCreditNote, job1));
			AssertOnSavingCheck(arCreditNoteBase, testCaseGetter(arCreditNote, job2));
			AssertOnSavingCheck(apInvoiceBase, testCaseGetter(apInvoice, job3));
			AssertOnSavingCheck(arInvoiceBase, testCaseGetter(arInvoice, job4));
			AssertOnSavingCheck(jrjBase, testCaseGetter(jrj, job5));
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_NonPersisted()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job3 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job4 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job5 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var apCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APCreditNote), "APCreditNote001", job1);
			var arCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARCreditNote), "ARCreditNote001", job2);
			var apInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APInvoice), "APInvoice001", job3);
			var arInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARInvoice), "ARInvoice001", job4);
			var jrj = CreateJobRevenueJournalWithLine(creator, job5, shouldCreateLinkedCharge: true);

			jrj.AH_TransactionNum = "00001000";

			PrepareInvoiceData(apCreditNote);
			PrepareInvoiceData(arCreditNote);
			PrepareInvoiceData(apInvoice);
			PrepareInvoiceData(arInvoice);
			PrepareInvoiceData(jrj);

			AssertEquals(apCreditNote.Lines[0].PK, job1.Charges[0].JR_AL_APLine);
			AssertEquals(arCreditNote.Lines[0].PK, job2.Charges[0].JR_AL_ARLine);
			AssertEquals(apInvoice.Lines[0].PK, job3.Charges[0].JR_AL_APLine);
			AssertEquals(arInvoice.Lines[0].PK, job4.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[0].PK, job5.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[1].PK, job5.Charges[1].JR_AL_ARLine);
			AssertEquals(6, Factory.Load<JobCharge>(new ZQuery()).Length);
			AssertEquals(false, apCreditNote.IsInDatabase);
			AssertEquals(false, arCreditNote.IsInDatabase);
			AssertEquals(false, apInvoice.IsInDatabase);
			AssertEquals(false, arInvoice.IsInDatabase);
			AssertEquals(false, jrj.IsInDatabase);

			Func<TransactionHeaderWithLines, Job, TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = (transaction, job) =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction line of reversed invoice / credit note / job revenue journal should not be linked to job charge.",
					true,
					CriticalValidationErrorType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges_7,
					CriticalValidationMessageTemplate.ReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage,
					GetCancelledTransactionHeaderWithNoLineLinkedToChargeMessageBody(null, transaction, job));

				return testCase;
			};

			AssertOnSavingCheck(apCreditNote, testCaseGetter(apCreditNote, job1));
			AssertOnSavingCheck(arCreditNote, testCaseGetter(arCreditNote, job2));
			AssertOnSavingCheck(apInvoice, testCaseGetter(apInvoice, job3));
			AssertOnSavingCheck(arInvoice, testCaseGetter(arInvoice, job4));
			AssertOnSavingCheck(jrj, testCaseGetter(jrj, job5));
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_WithoutLines()
		{
			var creator = new TestObjectCreator(Factory);

			var apCreditNote = creator.CreateInvoice(typeof(APCreditNote), "APCreditNote001");
			var arCreditNote = creator.CreateInvoice(typeof(ARCreditNote), "ARCreditNote001");
			var apInvoice = creator.CreateInvoice(typeof(APInvoice), "APInvoice001");
			var arInvoice = creator.CreateInvoice(typeof(ARInvoice), "ARInvoice001");
			var job = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var jrj = CreateJobRevenueJournalWithLine(creator, job, shouldCreateLinkedCharge: false);
			jrj.Lines.RemoveAndDeleteAll();

			Factory.Save();

			PrepareInvoiceData(apCreditNote);
			PrepareInvoiceData(arCreditNote);
			PrepareInvoiceData(apInvoice);
			PrepareInvoiceData(arInvoice);
			PrepareInvoiceData(jrj);

			AssertEquals(0, apCreditNote.Lines.Count);
			AssertEquals(0, arCreditNote.Lines.Count);
			AssertEquals(0, apInvoice.Lines.Count);
			AssertEquals(0, arInvoice.Lines.Count);
			AssertEquals(0, jrj.Lines.Count);
			AssertEquals(0, Factory.Load<JobCharge>(new ZQuery()).Length);

			Func<TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = () =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors.");
				return testCase;
			};

			AssertOnSavingCheck(apCreditNote, testCaseGetter());
			AssertOnSavingCheck(arCreditNote, testCaseGetter());
			AssertOnSavingCheck(apInvoice, testCaseGetter());
			AssertOnSavingCheck(arInvoice, testCaseGetter());
			AssertOnSavingCheck(jrj, testCaseGetter());
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_WithoutCharges()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job3 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job4 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job5 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var apCreditNote = CreateInvoiceWithLine(creator, typeof(APCreditNote), "APCreditNote001", job1);
			var arCreditNote = CreateInvoiceWithLine(creator, typeof(ARCreditNote), "ARCreditNote001", job2);
			var apInvoice = CreateInvoiceWithLine(creator, typeof(APInvoice), "APInvoice001", job3);
			var arInvoice = CreateInvoiceWithLine(creator, typeof(ARInvoice), "ARInvoice001", job4);
			var jrj = CreateJobRevenueJournalWithLine(creator, job5, shouldCreateLinkedCharge: false);
			jrj.AH_TransactionNum = "TRAN101";

			PrepareInvoiceData(apCreditNote);
			PrepareInvoiceData(arCreditNote);
			PrepareInvoiceData(apInvoice);
			PrepareInvoiceData(arInvoice);
			PrepareInvoiceData(jrj);

			AssertEquals(1, apCreditNote.Lines.Count);
			AssertEquals(1, arCreditNote.Lines.Count);
			AssertEquals(1, apInvoice.Lines.Count);
			AssertEquals(1, arInvoice.Lines.Count);
			AssertEquals(2, jrj.Lines.Count);
			AssertEquals(0, Factory.Load<JobCharge>(new ZQuery()).Length);

			Func<TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = () =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors.");
				return testCase;
			};

			AssertOnSavingCheck(apCreditNote, testCaseGetter());
			AssertOnSavingCheck(arCreditNote, testCaseGetter());
			AssertOnSavingCheck(apInvoice, testCaseGetter());
			AssertOnSavingCheck(arInvoice, testCaseGetter());
			AssertOnSavingCheck(jrj, testCaseGetter());
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_NotCancelled()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job3 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job4 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job5 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var apCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APCreditNote), "APCreditNote001", job1);
			var arCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARCreditNote), "ARCreditNote001", job2);
			var apInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APInvoice), "APInvoice001", job3);
			var arInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARInvoice), "ARInvoice001", job4);
			var jrj = CreateJobRevenueJournalWithLine(creator, job5, shouldCreateLinkedCharge: true);
			jrj.AH_TransactionNum = "00001000";

			AssertEquals(apCreditNote.Lines[0].PK, job1.Charges[0].JR_AL_APLine);
			AssertEquals(arCreditNote.Lines[0].PK, job2.Charges[0].JR_AL_ARLine);
			AssertEquals(apInvoice.Lines[0].PK, job3.Charges[0].JR_AL_APLine);
			AssertEquals(arInvoice.Lines[0].PK, job4.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[0].PK, job5.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[1].PK, job5.Charges[1].JR_AL_ARLine);
			AssertEquals(6, Factory.Load<JobCharge>(new ZQuery()).Length);
			AssertEquals(false, apCreditNote.IsCancelled);
			AssertEquals(false, arCreditNote.IsCancelled);
			AssertEquals(false, apInvoice.IsCancelled);
			AssertEquals(false, arInvoice.IsCancelled);
			AssertEquals(false, jrj.IsCancelled);

			Func<TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = () =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors.");
				return testCase;
			};

			AssertOnSavingCheck(apCreditNote, testCaseGetter());
			AssertOnSavingCheck(arCreditNote, testCaseGetter());
			AssertOnSavingCheck(apInvoice, testCaseGetter());
			AssertOnSavingCheck(arInvoice, testCaseGetter());
			AssertOnSavingCheck(jrj, testCaseGetter());
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_Persisted()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job3 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job4 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job5 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var apCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APCreditNote), "APCreditNote001", job1);
			var arCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARCreditNote), "ARCreditNote001", job2);
			var apInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(APInvoice), "APInvoice001", job3);
			var arInvoice = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARInvoice), "ARInvoice001", job4);
			var jrj = CreateJobRevenueJournalWithLine(creator, job5, shouldCreateLinkedCharge: true);

			apCreditNote.IsCancelled = true;
			arCreditNote.IsCancelled = true;
			apInvoice.IsCancelled = true;
			arInvoice.IsCancelled = true;
			jrj.IsCancelled = true;

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			AssertEquals(apCreditNote.Lines[0].PK, job1.Charges[0].JR_AL_APLine);
			AssertEquals(arCreditNote.Lines[0].PK, job2.Charges[0].JR_AL_ARLine);
			AssertEquals(apInvoice.Lines[0].PK, job3.Charges[0].JR_AL_APLine);
			AssertEquals(arInvoice.Lines[0].PK, job4.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[0].PK, job5.Charges[0].JR_AL_ARLine);
			AssertEquals(jrj.Lines[1].PK, job5.Charges[1].JR_AL_ARLine);
			AssertEquals(6, Factory.Load<JobCharge>(new ZQuery()).Length);
			AssertEquals(true, apCreditNote.IsCancelled);
			AssertEquals(true, arCreditNote.IsCancelled);
			AssertEquals(true, apInvoice.IsCancelled);
			AssertEquals(true, arInvoice.IsCancelled);
			AssertEquals(true, jrj.IsCancelled);
			AssertEquals(true, apCreditNote.IsInDatabase);
			AssertEquals(true, arCreditNote.IsInDatabase);
			AssertEquals(true, apInvoice.IsInDatabase);
			AssertEquals(true, arInvoice.IsInDatabase);
			AssertEquals(true, jrj.IsInDatabase);
			AssertEquals(false, apCreditNote.AH_IsCancelledInfo.HasChanges);
			AssertEquals(false, arCreditNote.AH_IsCancelledInfo.HasChanges);
			AssertEquals(false, apInvoice.AH_IsCancelledInfo.HasChanges);
			AssertEquals(false, arInvoice.AH_IsCancelledInfo.HasChanges);
			AssertEquals(false, jrj.AH_IsCancelledInfo.HasChanges);

			Func<TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = () =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors.");
				return testCase;
			};

			AssertOnSavingCheck(apCreditNote, testCaseGetter());
			AssertOnSavingCheck(arCreditNote, testCaseGetter());
			AssertOnSavingCheck(apInvoice, testCaseGetter());
			AssertOnSavingCheck(arInvoice, testCaseGetter());
			AssertOnSavingCheck(jrj, testCaseGetter());
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_OtherTransactionType()
		{
			var creator = new TestObjectCreator(Factory);

			var glJournal = creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2016, 2, 15), new ZDateTime(2016, 2, 20));
			glJournal.AH_TransactionNum = "00001000";
			var glJournalLine = creator.CreateGLJournalLine(glJournal, 0m, DebitCredit.CR, creator.GLHeader1.PK);

			Func<TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = () =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors.");
				return testCase;
			};

			AssertOnSavingCheck(glJournal, testCaseGetter());
		}

		public void TestCheckCancelledTransactionHeaderWithNoLineLinkedToCharge_ApprovalRequest()
		{
			var creator = new TestObjectCreator(Factory);

			var job = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var arCreditNote = CreateInvoiceWithLineLinkedToCharge(creator, typeof(ARCreditNote), "ARCreditNote001", job);
			var debtor = creator.CreateOrgHeader("TST", true, true);

			arCreditNote.AH_OH = debtor.PK;
			arCreditNote.AH_Desc = "Test Desc";

			var request = CreateApprovalRequest(arCreditNote, Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
			var request2 = CreateApprovalRequest(arCreditNote, Core.Constants.GenApprovalRequestApprovalStatus.Posted);
			Factory.Save();

			AssertEquals(arCreditNote.Lines[0].PK, job.Charges[0].JR_AL_ARLine);
			AssertEquals(1, Factory.Load<JobCharge>(new ZQuery()).Length);
			var arCreditNoteBase = Factory.Load<AccTransactionHeader>(arCreditNote.PK);

			arCreditNoteBase.IsCancelled = true;

			Func<TransactionHeaderWithLines, Job, TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = (transaction, job1) =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction line of reversed invoice / credit note / job revenue journal should not be linked to job charge.",
					true,
					CriticalValidationErrorType.ReversedTransactionsShouldNotHaveLinesLinkedToCharges_7,
					CriticalValidationMessageTemplate.ReversedTransactionsShouldNotHaveLinesLinkedToChargesErrorMessage,
					GetCancelledTransactionHeaderWithNoLineLinkedToChargeMessageBody("This error is captured on an AccTransactionHeader which was not loaded as AR/AP Invoice/CreditNote or Job Revenue Journal.", transaction, job, request, request2));

				return testCase;
			};

			AssertOnSavingCheck(arCreditNoteBase, testCaseGetter(arCreditNote, job));
		}

		GenApprovalRequest CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;

			return approvalRequest;
		}

		[TestDate(2009, 10, 20)]
		public void TestTransactionHeaderMustHaveTransactionNumberOnSaving()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.InvoicingBaseDidNotCreateTransactionNumberOnSaving);

			var testObjectCreator = new TestObjectCreator(Factory);
			var debtor = testObjectCreator.CreateOrgHeader("TST", true, true);
			Factory.Save();

			var transaction = Factory.New<ARInvoiceForTest>();
			transaction.AH_OH = debtor.PK;
			transaction.AH_Desc = "Test Desc";

			AssertEquals(ZString.Empty, transaction.AH_TransactionNum);

			var expectedMessagePart = new string[] {
				CriticalValidationMessageTemplate.TransactionHeaderMustHaveTransactionNumber,
				"Header: PK = ",
				@"InvoicingBaseDidNotCreateTransactionNumberOnSaving:
TransactionHeader OnSavingCore Info:
FactorySaveCountUsedToGenerateTransactionNumberFromTransactionHeader: 0
NumberFountainForTransactionNumber: NULL
NeedToUpdateTransactionNumberFromFountain: True",
				"TransactionHeaderTransactionNumberSetToEmpty: There is no data collected for this key." };

			// In order to test OnSavingCore, should not use AssertOnSavingCheck.
			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());

			foreach (var msg in expectedMessagePart)
			{
				AssertContains(msg, ex.DeveloperErrorMessage);
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		[TestDate(2009, 10, 20)]
		public void TestExistingWrongTransactionWithEmptyTransactionNumberOnSaving()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.InvoicingBaseDidNotCreateTransactionNumberOnSaving);

			var testObjectCreator = new TestObjectCreator(Factory);
			var debtor = testObjectCreator.CreateOrgHeader("TST", true, true);
			Factory.Save();

			var transaction = Factory.New<ARInvoiceForTest>();
			transaction.AH_OH = debtor.PK;
			transaction.AH_Desc = "Test Desc";
			transaction.AH_TransactionNum = null;

			//Intend to create an invalid transaction header with null number.
			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Factory.Save();
			}

			transaction.AH_Desc = "newDesc";

			AssertEquals(ZString.Empty, transaction.AH_TransactionNum);
			Assert(!transaction.AH_TransactionNumInfo.HasChanges);

			AssertNoExceptionThrown("no exception for existing bad data", () => Factory.Save());
		}

		InvoicingBase CreateInvoiceWithLineLinkedToCharge(TestObjectCreator creator, Type invoiceType, string transactionNum, Job job)
		{
			var invoice = CreateInvoiceWithLine(creator, invoiceType, transactionNum, job);
			creator.CreateCharge(invoice.Lines[0]);
			return invoice;
		}

		InvoicingBase CreateInvoiceWithLine(TestObjectCreator creator, Type invoiceType, string transactionNum, Job job)
		{
			var invoice = creator.CreateInvoice(invoiceType, transactionNum);
			var line = creator.CreateInvoiceLine(invoice, job, creator.FRT, 100m, creator.AUD, 1);
			return invoice;
		}

		//the JobRevenueJournal.OnFactorySavingBeforeTransactionCore() creates charges automatically.
		JobRevenueJournal CreateJobRevenueJournalWithLine(TestObjectCreator creator, Job job, bool shouldCreateLinkedCharge = false)
		{
			var jrj = creator.CreateJobRevenueJournal(creator.FRT, job, 100m);
			if (shouldCreateLinkedCharge)
			{
				var charge1 = creator.CreateCharge(jrj.Lines[0]);
				var charge2 = creator.CreateCharge(jrj.Lines[1]);
			}
			return jrj;
		}

		string[] GetCancelledTransactionHeaderWithNoLineLinkedToChargeMessageBody(string headingText, TransactionHeaderWithLines transaction, Job job, params GenApprovalRequest[] approvalRequest)
		{
			var messages = new List<string>();
			if (!string.IsNullOrWhiteSpace(headingText))
			{
				messages.Add(headingText);
			}

			messages.Add(string.Format("Header: PK = {0}, Ledger = {1}, Transaction Type = {2},", transaction.PK, transaction.AH_Ledger, transaction.AH_TransactionType));
			messages.Add("Line Details:");

			foreach (TransactionLine line in transaction.Lines)
			{
				messages.Add(string.Format("Line: PK = {0}, Charge Code = FRT,", line.PK));
			}

			foreach (Charge charge in job.Charges)
			{
				if (transaction is JobRevenueJournal || !transaction.IsInDatabase)
				{
					messages.Add(string.Format("Charge: PK = {0}, Type = Charge,", charge.PK));
				}
				else
				{
					messages.Add(string.Format("Charge: PK = {0}, Type = Charge,", charge.PK));
				}
			}

			foreach (var request in approvalRequest)
			{
				messages.Add(string.Format("Approval Request: PK = {0}, Approval Status = {1},", request.PK, request.XP_ApprovalStatus));
			}

			return messages.ToArray();
		}

		void PrepareInvoiceData(TransactionHeaderWithLines transactionHeader)
		{
			transactionHeader.IsCancelled = true;
			(transactionHeader as IMatching)?.GenerateMatchLinks();
		}

		#endregion

		public void TestCancelledUnapprovedPayableTransactionsShouldNotHaveLinesLinkedToCharges()
		{
			var creator = new TestObjectCreator(Factory);
			var job1 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);
			var job2 = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var creditNote = creator.CreateInvoiceWithLine(typeof(UACreditNote), "UACreditNote001", creator.AUD, 1, 100, 0, 100, 0);
			var invoice = creator.CreateInvoiceWithLine(typeof(UAInvoice), "UAInvoice001", creator.AUD, 1, 100, 0, 100, 0);

			creditNote.AH_JH = job1.PK;
			invoice.AH_JH = job2.PK;
			creditNote.Lines[0].AL_JH = job1.PK;
			invoice.Lines[0].AL_JH = job2.PK;
			creditNote.Lines[0].AL_AC = creator.FRT.PK;
			invoice.Lines[0].AL_AC = creator.FRT.PK;

			creator.CreateCharge(creditNote.Lines[0]);
			creator.CreateCharge(invoice.Lines[0]);

			creditNote.IsCancelled = true;
			invoice.IsCancelled = true;

			Func<InvoicingBase, Charge, TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = (transaction, charge) =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
	@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Cancelled Unapproved Payable Transactions should not have transaction lines.",
									true,
									CriticalValidationErrorType.CancelledUnapprovedPayableTransactionsShouldNotHaveLines_2,
								CriticalValidationMessageTemplate.CancelledUnapprovedPayableTransactionsShouldNotHaveLinesErrorMessage,
									string.Format("Header: PK = {0}, Ledger = {1}, Transaction Type = {2},", transaction.PK, transaction.AH_Ledger, transaction.AH_TransactionType),
									"Line Details:",
									string.Format("Line: PK = {0}, Charge Code = FRT,", transaction.Lines[0].PK),
									string.Format("Charge: PK = {0}, Type = Charge,", charge.PK));

				return testCase;
			};

			AssertOnSavingCheck(creditNote, testCaseGetter(creditNote, job1.Charges[0]));
			AssertOnSavingCheck(invoice, testCaseGetter(invoice, job2.Charges[0]));
		}

		public void TestShouldNotReportCriticalValidationError()
		{
			var unapprovedInvoice = Factory.NewWithValidTestData<APInvoice>();
			unapprovedInvoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			unapprovedInvoice.AH_TransactionType = TransactionTypes.UAInvoice;
			unapprovedInvoice.AH_TransactionNum = null;
			unapprovedInvoice.AH_TransactionCategory = "SBC";
			AssertEquals(ZString.Empty, unapprovedInvoice.AH_TransactionNum);

			var incompleteInvoice = Factory.NewWithValidTestData<APInvoice>();
			incompleteInvoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			incompleteInvoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			incompleteInvoice.AH_TransactionNum = null;
			incompleteInvoice.AH_TransactionCategory = "SBC";
			AssertEquals(ZString.Empty, incompleteInvoice.AH_TransactionNum);

			var pendingAllocationTransaction = Factory.NewWithValidTestData<APInvoice>();
			pendingAllocationTransaction.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			pendingAllocationTransaction.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			pendingAllocationTransaction.AH_TransactionNum = null;
			AssertEquals(ZString.Empty, pendingAllocationTransaction.AH_TransactionNum);

			CombineAssertions(() =>
			{
				AssertOnSavingCheck(unapprovedInvoice, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail against selfbilling UnapprovedPayable Invoice."));
				AssertOnSavingCheck(incompleteInvoice, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail against selfbilling Incomplete Invoice."));
				AssertOnSavingCheck(pendingAllocationTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail against pending Allocation Transaction."));
			});
		}

		public void TestShouldNotReportCriticalValidationErrorForExchangeRate()
		{
			var unapprovedInvoice = Factory.NewWithValidTestData<APInvoice>();
			unapprovedInvoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			unapprovedInvoice.AH_TransactionType = TransactionTypes.UAInvoice;
			unapprovedInvoice.AH_ExchangeRate = 0m;
			unapprovedInvoice.AH_TransactionCategory = "SBC";
			Assert(unapprovedInvoice.IsSelfBillingInvoice);

			var incompleteInvoice = Factory.NewWithValidTestData<APInvoice>();
			incompleteInvoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
			incompleteInvoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			incompleteInvoice.AH_ExchangeRate = 0m;
			incompleteInvoice.AH_TransactionCategory = "SBC";
			Assert(incompleteInvoice.IsSelfBillingInvoice);

			var pendingAllocationTransaction = Factory.NewWithValidTestData<APInvoice>();
			pendingAllocationTransaction.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			pendingAllocationTransaction.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			pendingAllocationTransaction.AH_ExchangeRate = 0m;

			CombineAssertions(() =>
			{
				AssertOnSavingCheck(unapprovedInvoice, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail against selfbilling UnapprovedPayable Invoice."));
				AssertOnSavingCheck(incompleteInvoice, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail against selfbilling Incomplete Invoice."));
				AssertOnSavingCheck(pendingAllocationTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail against pending Allocation Transaction."));
			});
		}

		public void TestShouldNotReportCriticalValidationErrorForIsSelfBillingINInvoiceWithoutTransactionNumber()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoiceWithApprovalRequest = testObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(testObjectCreator.AALSHI, 100);

			invoiceWithApprovalRequest.IsSelfBillingInvoice = true;
			invoiceWithApprovalRequest.AH_TransactionNum = string.Empty;
			invoiceWithApprovalRequest.AH_Ledger = LedgerTypes.IncompleteTransactions;
			invoiceWithApprovalRequest.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			var accTransactionHeader = Factory.Load<AccTransactionHeader>(invoiceWithApprovalRequest.PK);

			Assert(accTransactionHeader.IsSelfBillingInvoice);

			AssertOnSavingCheck(accTransactionHeader, new TestCaseDefinition_ForSeparateTestsMethods("Should not fail for is selfbilling incomplete transactions without transaction number."));
		}

		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = base.GetTestCases();
			result.AddRange(GetGetMoreInfoToErrorWhenLocalInvoiceAmountNotEqualToForeignAmountCases());
			return result;
		}

		List<TestCaseDefinitionWithDelegate_Obsolete> GetGetMoreInfoToErrorWhenLocalInvoiceAmountNotEqualToForeignAmountCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote) })
			{
				Type type_cachedForDelegates = type;

				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("GetMoreInfo when LocalInvoiceAmount is not equal to ForeignAmount, type = {0}", type_cachedForDelegates.Name),
					delegate(BusinessObjectFactory factory)
					{
						NUnit.Framework.TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);
						ForwardingConsol consol = factory.NewWithValidTestData<ForwardingConsol>();
						consol.JK_UniqueConsignRef = "C0001";
						factory.Save();

						JobHeader job = factory.NewJobWithValidTestDataForTesting<JobHeader>();
						job.JH_JobNum = "123456";
						AccChargeCode chargeCode = factory.NewWithValidTestData<AccChargeCode>();
						chargeCode.AC_Code = "AAA";
						AccGLHeader glAccount = factory.NewWithValidTestData<AccGLHeader>();
						glAccount.AG_AccountNum = "7890.12.34";
						JobConsolCost consolCost = factory.NewWithValidTestData<JobConsolCost>();
						using (consolCost.ReportSettingParentSuspender.GetSuspender())
						{
							consolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
						}

						InvoicingBase parent = (InvoicingBase)factory.NewWithValidTestData(type_cachedForDelegates);
						parent.AH_ExchangeRate = 1;
						parent.AH_InvoiceAmount = 25.10M;
						parent.AH_GSTAmount = 1.00M;
						parent.AH_OSTotal = 40.05M;
						parent.AH_OutstandingAmount = 26.1M;

						FieldInfo field = typeof(DependentTransactionLineCollection).GetField("UpdateHeaderAmountsIsRunningNow", BindingFlags.Instance | BindingFlags.NonPublic);
						field.SetValue(parent.Lines, true);

						InvoicingLineBase line = (InvoicingLineBase)factory.NewWithValidTestData(parent.DependentTransactionLineType);
						line.AL_JH = job.PK;
						line.AL_AC = chargeCode.PK;
						line.AL_LineAmount = 5.10m;
						line.AL_GSTVAT = 0m;
						line.AL_OSAmount = 15.05m;
						parent.Lines.Add(line);
						ApportionSplitCharge apportionCharge = factory.NewWithValidTestData<ApportionSplitCharge>();
						apportionCharge.JR_JH = job.PK;
						apportionCharge.JR_AL_APLine = apportionCharge.JR_AL_ARLine = line.PK;
						apportionCharge.JR_E6 = consolCost.PK;
						line.ApportionmentChargeImportedFrom = apportionCharge;

						line = (InvoicingLineBase)factory.NewWithValidTestData(parent.DependentTransactionLineType);
						line.AL_AG = glAccount.PK;
						line.AL_LineAmount = 20.00m;
						line.AL_GSTVAT = 1m;
						line.AL_OSAmount = 25m;
						parent.Lines.Add(line);

						return parent;
					}, true, CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4, CriticalValidationMessageTemplate.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1ErrorMessage("AUD", 1),
(@"Transaction Header:
- Local Amount: 26.10
- Foreign Currency Amount: 40.05
- Currency: AUD
- Exchange Rate: 1
- Company: EDI
- Company Local Currency: AUD


The items causing this issue are as follows:
Transaction Line:
- Charge Code: AAA
- Job Number: 123456
- Branch: BNE
- Department: BRN
- Local Amount: 5.10
- Foreign Currency Amount: 15.05
- Consol #: C0001

Transaction Line:
- Charge Code: 7890.12.34
- Job Number: Empty
- Branch: BNE
- Department: BRN
- Local Amount: 21.00
- Foreign Currency Amount: 25")));
			}

			return result;
		}

		protected override bool CheckSuppliedFactoryUsed { get { return false; } }

		class ARInvoiceForTest : ARInvoice
		{
			public ARInvoiceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
			{
				get
				{
					return null;
				}
			}
		}
	}
}
