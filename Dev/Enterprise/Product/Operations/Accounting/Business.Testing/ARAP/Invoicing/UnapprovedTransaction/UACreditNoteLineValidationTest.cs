using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(UACreditNoteLine))]
	public class UACreditNoteLineValidationTest : APCreditNoteLineValidationTest
	{
		protected override Type InvoiceLineType
		{
			get { return typeof(UACreditNoteLine); }
		}

		protected override Type InvoiceType
		{
			get { return typeof(UACreditNote); }
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(UACreditNote);
		}

		public void TestCheckGenericCharge()
		{
			var creator = new TestObjectCreator(Factory);
			var creditNote = Factory.NewWithValidTestData<UACreditNote>();
			var line = creditNote.Lines.AddNew() as UACreditNoteLine;
			line.FillWithValidTestData();
			line.AL_AC = creator.CC1.PK;

			var claim = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim.AY_OH_Debtor = creator.AALSHI.PK;
			claim.AY_QueryClaimReference = ZString.Empty;
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			claim.AY_AH = invoice.PK;
			claim.RelatedUnapprovedCreditNote = creditNote;
			creditNote.RelatedClaim = claim;

			var validation = new UACreditNoteLineValidation(line);
			var errorMsg = @"This Charge Code cannot be used here. 
Charge Code must exist in Creditor company.";

			AssertNoError(line.GenericChargeInfo, errorMsg);
			validation.ValidateGenericCharge();
			AssertNoError(line.GenericChargeInfo, errorMsg);

			invoice.AH_GC = ZGuid.Empty;
			AssertNoError(line.GenericChargeInfo, errorMsg);
			validation.ValidateGenericCharge();
			AssertHasError("should have the error now.", line.GenericChargeInfo, errorMsg);
		}

		public override void TestAllowTaxRecoveryLineWithDifferentBranchWhenBranchLevelPostingIsEnabled()
		{
			Assert("This test is not applicable to UACreditNoteLineValidation", true);
		}
	}
}
