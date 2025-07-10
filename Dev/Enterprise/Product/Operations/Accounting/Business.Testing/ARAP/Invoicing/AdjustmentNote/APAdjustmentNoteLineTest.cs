using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APAdjustmentNoteLine))]
	public class APAdjustmentNoteLineTest : InvoicingLineBaseTest
	{
		protected override Type MasterHeaderType
		{
			get { return typeof(APAdjustmentNote); }
		}

		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		public void TestValidationForIncompleteTransactionLine()
		{
			var invoiceLine = (InvoicingLineBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());

			invoiceLine.Factory.SetContext(BusinessContext.SavingIncompleteTransaction);
			AssertEquals("Validation Type for Incomplete", typeof(IncompleteInvoicingLineBaseValidation), invoiceLine.Validation.GetType());

			invoiceLine.Factory.RemoveContext(BusinessContext.SavingIncompleteTransaction);
			AssertNotEquals("Validation Type for Regular", typeof(IncompleteInvoicingLineBaseValidation), invoiceLine.Validation.GetType());
		}
	}
}
