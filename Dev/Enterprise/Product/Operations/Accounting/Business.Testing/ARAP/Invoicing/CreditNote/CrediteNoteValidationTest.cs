using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class CrediteNoteValidationTest<T> : InvoiceBaseValidationTest
				where T : CreditNote
	{
		[TestDate(2020, 2, 2)]
		public void TestOriginalTransactions()
		{
			var correctOrg = TestObjectCreator.CreateOrgHeader("ORG275PLUS", true, true);
			var wrongOrg = TestObjectCreator.CreateOrgHeader("ORG29", true, true);

			var incorrectLedgerOriginalInvoice = GetWrongLedgerOriginalInvoice(correctOrg);
			var incorrectOrgOriginalInvoice = GetWrongOrgOriginalInvoice(wrongOrg);
			var correctOriginalInvoice = GetCorrectOriginalInvoice(correctOrg);
			Factory.Save();

			var creditNote = GetNewCreditNote(correctOrg);
			creditNote.AH_OriginalInvoiceDate = ZDate.Today.AddDays(-3);
			creditNote.AH_OriginalTransactionNum = "ADC00334";

			AssertNoErrors(creditNote.OriginalTransactionReferenceInfo);

			creditNote.OriginalTransactionReference = correctOriginalInvoice.PK;
			AssertNoErrors(creditNote.OriginalTransactionReferenceInfo);

			creditNote.OriginalTransactionReference = incorrectLedgerOriginalInvoice.PK;
			AssertHasError(creditNote.OriginalTransactionReferenceInfo, "Enter a valid selection.");

			creditNote.OriginalTransactionReference = incorrectOrgOriginalInvoice.PK;
			AssertHasError(creditNote.OriginalTransactionReferenceInfo, "Enter a valid selection.");
		}

		protected abstract InvoicingBase GetCorrectOriginalInvoice(OrgHeader header);
		protected abstract InvoicingBase GetWrongOrgOriginalInvoice(OrgHeader header);
		protected abstract InvoicingBase GetWrongLedgerOriginalInvoice(OrgHeader header);
		T GetNewCreditNote(OrgHeader org) => TestObjectCreator.CreateInvoice(typeof(T), organisation: org) as T;

		public override void TestCheckTransactionNumForAPInvNumAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForAPInvNumAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestAPInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			Assert(true);
		}

		public override void TestJobInvoicingExist()
		{
			Assert(true);
		}

		protected override Type InvoiceType => typeof(T);
	}
}
