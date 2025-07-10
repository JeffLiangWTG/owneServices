using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public static class AmendingTestHelper
	{
		public static void PopulateOriginalTransaction(InvoicingBase original, TestObjectCreator creator, AccTaxRate tax = null, AccChargeCode chargeCode = null, RefCurrency currency = null)
		{
			var lineType = original.AH_Ledger == LedgerTypes.AccountsReceivable ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;

			Job job = original.Factory.NewJobWithValidTestDataForTesting<Job>();
			original.AH_JH = job.PK;

			original.Lines.RemoveAndDeleteAll();
			Job job1 = original.Factory.NewJobWithValidTestDataForTesting<Job>();
			TransactionLine line1 = creator.CreateInvoiceLine(lineType, original, job1, chargeCode ?? creator.CC1, currency ?? creator.USD, 0.9m, "Line 1", 100m);
			if (tax != null)
			{
				line1.AL_AT = tax.PK;
				line1.AL_TaxDate = ZDate.Today.AddDays(-3);
			}
			var jobCharge1 = creator.CreateJobCharge(line1, job1, chargeCode ?? creator.CC1, currency ?? creator.USD);

			Job job2 = original.Factory.NewJobWithValidTestDataForTesting<Job>();
			TransactionLine line2 = creator.CreateInvoiceLine(lineType, original, job2, creator.CC2, creator.USD, 0.9m, "Line 2", 200m);
			if (tax != null)
			{
				line2.AL_AT = tax.PK;
				line2.AL_TaxDate = ZDate.Empty;
				line2.AL_TaxRateNumerator = 10;
			}
			var jobCharge2 = creator.CreateJobCharge(line2, job2, creator.CC2, creator.USD);

			// Have to add the third line to cause recalculating Jobs
			InvoicingLineBase line3 = (InvoicingLineBase)original.Lines.AddNew();
			original.Lines.RemoveAndDelete(line3);
			original.AH_FullyPaidDate = ZDateTime.Empty;
		}

		public static void AssertPopulatedTransaction(InvoicingBase original, InvoicingBase amending)
		{
			string expectedDescription = string.Format("AMENDMENT RELATED TO {0} {1}", original.AH_TransactionType.ToUpper(), original.AH_TransactionNum.ToUpper()).Trim();
			Assertion.AssertEquals("Description", expectedDescription, amending.AH_Desc);

			Assertion.AssertEquals("AH_OH", original.AH_OH, amending.AH_OH);
			Assertion.AssertEquals("AH_JH", original.AH_JH, amending.AH_JH);
			if (original.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				Assertion.AssertEquals("AH_ConsolidatedInvoiceRef", original.AH_ConsolidatedInvoiceRef + "/A", amending.AH_ConsolidatedInvoiceRef);
			}
			Assertion.AssertEquals("AH_RX_NKTransactionCurrency", original.AH_RX_NKTransactionCurrency, amending.AH_RX_NKTransactionCurrency);
			Assertion.AssertEquals("AH_ExchangeRate", original.AH_ExchangeRate, amending.AH_ExchangeRate);
			Assertion.AssertEquals("IsDisbursementOrFinal", original.IsDisbursementOrFinal, amending.IsDisbursementOrFinal);
			Assertion.AssertEquals("AH_GB_TaxBranch", original.AH_GB_TaxBranch, amending.AH_GB_TaxBranch);

			Assertion.AssertEquals("Should have two lines", 2, amending.Lines.Count);
			for (int i = 0; i < 2; i++)
			{
				Assertion.AssertEquals(string.Format("Line{0}.AL_LineType", i), original.Lines[i].AL_LineType, amending.Lines[i].AL_LineType);
				Assertion.AssertEquals(string.Format("Line{0}.AL_Desc", i), original.Lines[i].AL_Desc, amending.Lines[i].AL_Desc);
				Assertion.AssertEquals(string.Format("Line{0}.AL_UnitQty", i), original.Lines[i].AL_UnitQty, amending.Lines[i].AL_UnitQty);
				Assertion.AssertEquals(string.Format("Line{0}.AL_UnitPrice", i), original.Lines[i].AL_UnitPrice, amending.Lines[i].AL_UnitPrice);
				Assertion.AssertEquals(string.Format("Line{0}.AL_OSUnitPrice", i), original.Lines[i].AL_OSUnitPrice, amending.Lines[i].AL_OSUnitPrice);
				Assertion.AssertEquals(string.Format("Line{0}.AL_PreventInvoicePrintGrouping", i), original.Lines[i].AL_PreventInvoicePrintGrouping, amending.Lines[i].AL_PreventInvoicePrintGrouping);
				Assertion.AssertEquals(string.Format("Line{0}.AL_JH", i), original.Lines[i].AL_JH, amending.Lines[i].AL_JH);
				Assertion.AssertEquals(string.Format("Line{0}.AL_GE", i), original.Lines[i].AL_GE, amending.Lines[i].AL_GE);
				Assertion.AssertEquals(string.Format("Line{0}.AL_GB", i), original.Lines[i].AL_GB, amending.Lines[i].AL_GB);
				Assertion.AssertEquals(string.Format("Line{0}.AL_AG", i), original.Lines[i].AL_AG, amending.Lines[i].AL_AG);
				Assertion.AssertEquals(string.Format("Line{0}.AL_AC", i), original.Lines[i].AL_AC, amending.Lines[i].AL_AC);
				Assertion.AssertEquals(string.Format("Line{0}.AL_OH", i), original.Lines[i].AL_OH, amending.Lines[i].AL_OH);
				Assertion.AssertEquals(string.Format("Line{0}.AL_AG_PercentOf", i), original.Lines[i].AL_AG_PercentOf, amending.Lines[i].AL_AG_PercentOf);
				Assertion.AssertEquals(string.Format("Line{0}.AL_PercentageOfPeriod", i), original.Lines[i].AL_PercentageOfPeriod, amending.Lines[i].AL_PercentageOfPeriod);
				Assertion.AssertEquals(string.Format("Line{0}.AL_WithholdingTax", i), original.Lines[i].AL_WithholdingTax, amending.Lines[i].AL_WithholdingTax);
				Assertion.AssertEquals(string.Format("Line{0}.AL_PostPeriod", i), original.Lines[i].AL_PostPeriod, amending.Lines[i].AL_PostPeriod);
				Assertion.AssertEquals(string.Format("Line{0}.AL_PostToGL", i), "N", amending.Lines[i].AL_PostToGL);
				Assertion.AssertEquals(string.Format("Line{0}.AL_ReversePeriod", i), original.Lines[i].AL_ReversePeriod, amending.Lines[i].AL_ReversePeriod);
				Assertion.AssertEquals(string.Format("Line{0}.AL_ReverseToGL", i), "N", amending.Lines[i].AL_ReverseToGL);
				Assertion.AssertEquals(string.Format("Line{0}.AL_RX_NKTransactionCurrency", i), original.Lines[i].AL_RX_NKTransactionCurrency, amending.Lines[i].AL_RX_NKTransactionCurrency);
				Assertion.AssertEquals(string.Format("Line{0}.AL_AT", i), original.Lines[i].AL_AT, amending.Lines[i].AL_AT);
				var expectedTaxDate = original.Lines[i].AL_TaxDate.IsEmpty ? ZDate.Today : original.Lines[i].AL_TaxDate;
				expectedTaxDate = original.Lines[i].AL_AT.IsEmpty ? ZDate.Empty : expectedTaxDate;
				Assertion.AssertEquals(string.Format("Line{0}.AL_TaxDate", i), expectedTaxDate, amending.Lines[i].AL_TaxDate);
				Assertion.AssertEquals(string.Format("Line{0}.AL_AW", i), original.Lines[i].AL_AW, amending.Lines[i].AL_AW);
				Assertion.AssertEquals(string.Format("Line{0}.AL_GB_TaxBranch", i), original.Lines[i].AL_GB_TaxBranch, amending.Lines[i].AL_GB_TaxBranch);

				if (original.AH_TransactionType != amending.AH_TransactionType)
				{
					Assertion.AssertEquals(string.Format("Line{0}.AL_LineAmount", i), -original.Lines[i].AL_LineAmount, amending.Lines[i].AL_LineAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_OSAmount", i), -original.Lines[i].AL_OSAmount, amending.Lines[i].AL_OSAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_OSExTaxAmount", i), original.Lines[i].AL_OSExTaxAmount, amending.Lines[i].AL_OSExTaxAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_OSWHTAmount", i), -original.Lines[i].AL_OSWHTAmount, amending.Lines[i].AL_OSWHTAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_ExchangeRate", i), original.Lines[i].AL_ExchangeRate, amending.Lines[i].AL_ExchangeRate);
				}
				else
				{
					Assertion.AssertEquals(string.Format("Line{0}.AL_LineAmount", i), 0m, amending.Lines[i].AL_LineAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_OSAmount", i), 0m, amending.Lines[i].AL_OSAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_OSExTaxAmount", i), 0m, amending.Lines[i].AL_OSExTaxAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_OSWHTAmount", i), 0m, amending.Lines[i].AL_OSWHTAmount);
					Assertion.AssertEquals(string.Format("Line{0}.AL_ExchangeRate", i), 1m, amending.Lines[i].AL_ExchangeRate);
				}
			}
		}
	}
}
