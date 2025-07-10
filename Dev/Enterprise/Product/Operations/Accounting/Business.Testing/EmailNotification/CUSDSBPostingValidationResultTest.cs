using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class CUSDSBPostingValidationResultTest : NUnit.Framework.TestCase
	{
		public void TestNotifications()
		{
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			AssertEquals("HasErrors", false, result.HasErrors);
			AssertEquals("HasMessages", false, result.HasMessages);
			AssertEquals("ContainsMessage", false, result.ContainsMessage("1"));
			result.AddWarning("789");
			AssertEquals("HasErrors", false, result.HasErrors);
			AssertEquals("HasMessages", true, result.HasMessages);
			AssertEquals("ContainsMessage", true, result.ContainsMessage("8"));
			result.AddError("123");
			AssertEquals("HasErrors", true, result.HasErrors);
			AssertEquals("HasMessages", true, result.HasMessages);
			AssertEquals("ContainsMessage", true, result.ContainsMessage("2"));
			result = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			AssertEquals("HasErrors", false, result.HasErrors);
			result.AddAPDiscrepancyCharge(new ChargesSummary[] { GetChargeSummary() });
			AssertEquals("HasErrors should not take Discrepancy Charges into consideration", false, result.HasErrors);
			AssertEquals("HasMessages should take Discrepancy Charges into consideration", true, result.HasMessages);
		}

		public void TestMessageIncludingDiscrepancyDetails()
		{
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			result.AddAPDiscrepancyCharge(new ChargesSummary[] { GetChargeSummary() });
			result.APInvoiceDetail = "978342890";
			result.AddWarning("You have been warned.");
			AssertEquals(@"Processing result for 1
You have been warned.
There is discrepancy between Customs amount and an existing AP invoice, 978342890", result.MessageIncludingDiscrepancyDetails);
		}

		public void TestDiscrepancyCharge()
		{
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			AssertEquals("No discrepancy charge", 0, result.APDiscrepancyCharges.Length);
			result.AddAPDiscrepancyCharge(new ChargesSummary[] { GetChargeSummary() });
			AssertEquals("Discrepancy charge", 1, result.APDiscrepancyCharges.Length);
			AssertEquals("Discrepancy charge", 0, result.ARDiscrepancyCharges.Length);
			result.AddARDiscrepancyCharge(new ChargesSummary[] { GetChargeSummary() });
			AssertEquals("Discrepancy charge", 1, result.ARDiscrepancyCharges.Length);
		}

		public void TestMerge()
		{
			CUSDSBPostingValidationResult result1 = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			result1.AddWarning("789");
			AssertEquals("HasError", false, result1.HasErrors);
			AssertEquals("DiscrepancyCharges", 0, result1.APDiscrepancyCharges.Length);
			CUSDSBPostingValidationResult result2 = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			result2.AddError("123");
			result2.AddAPDiscrepancyCharge(new ChargesSummary[] { GetChargeSummary() });
			result1.Merge(result2);
			AssertEquals("HasError", true, result1.HasErrors);
			AssertEquals("DiscrepancyCharges", 1, result1.APDiscrepancyCharges.Length);
		}

		public void TestShouldNotMergeWithOtherResultsForDifferentJob()
		{
			CUSDSBPostingValidationResult result1 = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			CUSDSBPostingValidationResult result2 = new CUSDSBPostingValidationResult("2", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{
				result1.Merge(result2);
			}

			);
		}

		ChargesSummary GetChargeSummary()
		{
			ChargesSummary chargeSummary = new ChargesSummary();
			chargeSummary.ChargeCode = "1";
			chargeSummary.Description = "2";
			chargeSummary.AdditionalDescription = "3";
			chargeSummary.ExistingExTaxAmount = 10m;
			chargeSummary.ExistingTaxAmount = 20m;
			chargeSummary.NewExTaxAmount = 30m;
			chargeSummary.NewTaxAmount = 40m;
			return chargeSummary;
		}
	}
}