using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class UACreditNoteValidationTest : APCreditNoteValidationTest
	{
		public override void TestCheckAH_LocalTotalAmount_DetectsCreditedExceedInvoiced()
		{
			Assert("Test is not applicable", true);
		}

		public override void TestCheckAH_LocalTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			Assert("Test is not applicable", true);
		}

		protected override Type InvoiceType
		{
			get { return typeof(UACreditNote); }
		}

		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as UACreditNoteValidation;
		}

		public new void TestCheckAH_OHCreditLimit()
		{
			Assert("Not for this transaction type", true);
		}

		public override void TestCheckTransactionNumForUAInvNumAlreadyExist_Standard()
		{
			Assert("Not for this transaction type", true);
		}

		public override void TestCheckTransactionNumForUAInvNumAlreadyExist_Calendar()
		{
			Assert("Not for this transaction type", true);
		}

		public override void TestUAInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			Assert("Not for this transaction type", true);
		}

		public void TestBranchDepartmentCombinationValidation_UACreditNoteUACreditNoteLine()
		{
			var header = Factory.NewWithValidTestData<UACreditNote>();
			var line = Factory.NewWithValidTestData<UACreditNoteLine>();
			line.AL_AH = header.PK;
			Factory.Save();

			var headerValidation = new UACreditNoteValidation(header);
			var lineValidation = new UACreditNoteLineValidation(line);

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, header,
				() => { headerValidation.ValidateAH_GE(); }, header.AH_GEInfo);

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, line,
				() => { lineValidation.ValidateAL_GE(); }, line.AL_GEInfo);
		}
	}
}
