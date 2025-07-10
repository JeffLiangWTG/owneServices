using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class DiscrepancyAllocator
	{
		public DiscrepancyAllocator(InvoicingBase invoicingBase)
		{
			InvoicingBase = invoicingBase;
		}

		readonly InvoicingBase InvoicingBase;

		public void Allocate()
		{
			bool isGSTIncludeAmountNeedUpdate = InvoicingBase.GSTInclusiveAmountNeedUpdate;
			try
			{
				InvoicingBase.GSTInclusiveAmountNeedUpdate = true;
				if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable)
				{
					AllocateOSTaxVariance();
				}
				AllocateOsExTaxVariance();
			}
			finally
			{
				InvoicingBase.GSTInclusiveAmountNeedUpdate = isGSTIncludeAmountNeedUpdate;
			}

			if (!InvoicingBase.IsValidationSuspended)
			{
				InvoicingBase.Lines.RunPreSaveValidation();
			}
		}

		void AllocateOSTaxVariance()
		{
			decimal totalTaxVarianceBeforeAdjustment = InvoicingBase.ExpectedInvoiceTaxTotal - InvoicingBase.AH_OSTaxAmount;

			if (totalTaxVarianceBeforeAdjustment != 0)
			{
				decimal aH_OSTaxAmountBeforeAdjustment = InvoicingBase.AH_OSTaxAmount;

				if (aH_OSTaxAmountBeforeAdjustment != 0)
				{
					List<ZGuid> consolCosts = new List<ZGuid>();

					foreach (InvoicingLineBase line in InvoicingBase.Lines)
					{
						if (line.AL_OSTaxAmount != 0)
						{
							if (line.IsPopulatedFromImportedApportionment)
							{
								JobConsolCost consolCost = FindJobConsolCostForLine(line);

								if (consolCost != null && !consolCosts.Contains(consolCost.PK) && consolCost.E6_OSGSTAmount_Calc != 0m)
								{
									decimal consolCostAdjustmentUnrounded = totalTaxVarianceBeforeAdjustment * (consolCost.E6_OSGSTAmount_Calc / aH_OSTaxAmountBeforeAdjustment);
									decimal consolCostOSGSTAmountAdjustment = AccountingUtils.Round(consolCostAdjustmentUnrounded, consolCost.E6_RX_NKCurrency);
									decimal consolCostOSCostAmountAdjustment = AccountingUtils.Round(consolCostAdjustmentUnrounded * (consolCost.E6_OSCostAmount / consolCost.E6_OSGSTAmount_Calc), consolCost.E6_RX_NKCurrency);

									decimal newOSCostAmount = consolCost.E6_OSCostAmount + consolCostOSCostAmountAdjustment;
									decimal newOSGSTAmount = consolCost.E6_OSGSTAmount_Calc + consolCostOSGSTAmountAdjustment;

									consolCost.E6_OSCostAmount = newOSCostAmount;
									consolCost.E6_OSGSTAmount_Calc = newOSGSTAmount;
									consolCosts.Add(consolCost.PK);
								}
							}
							else
							{
								decimal lineTaxAmountToAdjustUnrounded = totalTaxVarianceBeforeAdjustment * (line.AL_OSTaxAmount / aH_OSTaxAmountBeforeAdjustment);
								decimal lineTaxAmountToAdjust = AccountingUtils.Round(lineTaxAmountToAdjustUnrounded, line.AL_RX_NKTransactionCurrency);
								decimal lineExTaxAmountToAdjust = AccountingUtils.Round(lineTaxAmountToAdjustUnrounded * (line.AL_OSExTaxAmount / line.AL_OSTaxAmount), line.AL_RX_NKTransactionCurrency);

								decimal newAL_OSExTaxAmount = line.AL_OSExTaxAmount + lineExTaxAmountToAdjust;
								decimal newAL_OSTaxAmount = line.AL_OSTaxAmount + lineTaxAmountToAdjust;

								line.AL_OSExTaxAmount = newAL_OSExTaxAmount;
								line.AL_OSTaxAmount = newAL_OSTaxAmount;
							}
						}
					}

					if (consolCosts.Count > 0)
					{
						InvoicingBase.ImportAllApportionmentsFromCosting();
					}
				}

				decimal taxVarianceAfterFirstAttempt = InvoicingBase.ExpectedInvoiceTaxTotal - InvoicingBase.AH_OSTaxAmount;

				if (taxVarianceAfterFirstAttempt != 0)
				{
					InvoicingLineBase line = FindLineWithGreatestTaxAmount();

					if (line.IsPopulatedFromImportedApportionment)
					{
						JobConsolCost consolCost = FindJobConsolCostForLine(line);

						if (consolCost != null && consolCost.E6_OSGSTAmount_Calc != 0m)
						{
							decimal consolCostTaxAmountToAdjust = AccountingUtils.Round(taxVarianceAfterFirstAttempt, consolCost.E6_RX_NKCurrency);
							decimal consolCostExTaxAmountToAdjust = AccountingUtils.Round(taxVarianceAfterFirstAttempt * (consolCost.E6_OSCostAmount / consolCost.E6_OSGSTAmount_Calc), consolCost.E6_RX_NKCurrency);

							decimal newOSCostAmount = consolCost.E6_OSCostAmount + consolCostExTaxAmountToAdjust;
							decimal newOSGSTAmount = consolCost.E6_OSGSTAmount_Calc + consolCostTaxAmountToAdjust;

							consolCost.E6_OSCostAmount = newOSCostAmount;
							consolCost.E6_OSGSTAmount_Calc = newOSGSTAmount;
						}

						InvoicingBase.ImportAllApportionmentsFromCosting();
					}
					else if (line.AL_OSTaxAmount != 0m)
					{
						decimal lineTaxAmountToAdjust = AccountingUtils.Round(taxVarianceAfterFirstAttempt, line.AL_RX_NKTransactionCurrency);
						decimal lineExTaxAmountToAdjust = AccountingUtils.Round(taxVarianceAfterFirstAttempt * (line.AL_OSExTaxAmount / line.AL_OSTaxAmount), line.AL_RX_NKTransactionCurrency);

						decimal newAL_OSExTaxAmount = line.AL_OSExTaxAmount + lineExTaxAmountToAdjust;
						decimal newAL_OSTaxAmount = line.AL_OSTaxAmount + lineTaxAmountToAdjust;

						line.AL_OSExTaxAmount = newAL_OSExTaxAmount;
						line.AL_OSTaxAmount = newAL_OSTaxAmount;
					}
				}
			}
		}

		void AllocateOsExTaxVariance()
		{
			decimal exTaxVarianceBeforeAdjustment = 0m;

			if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable)
			{
				exTaxVarianceBeforeAdjustment = InvoicingBase.ExpectedInvoiceExclTaxTotal - InvoicingBase.AH_OSExTaxAmount;
			}
			else if (InvoicingBase.Header != null && !InvoicingBase.Header.CompanyData.IsAPTaxApplicable)
			{
				exTaxVarianceBeforeAdjustment = InvoicingBase.ExpectedInvoiceTotal - InvoicingBase.AH_OSTotalAmount;
			}

			if (exTaxVarianceBeforeAdjustment != 0m)
			{
				decimal exTaxTotalForLinesWithNoTaxAmount = GetTotalOSExTaxAmountForLinesWithNoTaxAmount();

				if (exTaxTotalForLinesWithNoTaxAmount != 0)
				{
					List<ZGuid> consolCosts = new List<ZGuid>();

					foreach (InvoicingLineBase line in InvoicingBase.Lines)
					{
						if (line.AL_OSTaxAmount == 0)
						{
							if (line.IsPopulatedFromImportedApportionment)
							{
								JobConsolCost consolCost = FindJobConsolCostForLine(line);

								if (consolCost != null && !consolCosts.Contains(consolCost.PK))
								{
									decimal consolCostOSCostAmountAdjustment = AccountingUtils.Round(exTaxVarianceBeforeAdjustment * (consolCost.E6_OSCostAmount / exTaxTotalForLinesWithNoTaxAmount), consolCost.E6_RX_NKCurrency);
									consolCost.E6_OSCostAmount += consolCostOSCostAmountAdjustment;
									consolCosts.Add(consolCost.PK);
								}
							}
							else
							{
								decimal oSExTaxAmountAdjustment = AccountingUtils.Round(exTaxVarianceBeforeAdjustment * (line.AL_OSExTaxAmount / exTaxTotalForLinesWithNoTaxAmount), line.AL_RX_NKTransactionCurrency);
								line.AL_OSExTaxAmount += oSExTaxAmountAdjustment;
							}
						}
					}

					if (consolCosts.Count > 0)
					{
						InvoicingBase.ImportAllApportionmentsFromCosting();
					}
				}

				decimal exTaxVarianceAferAdjustingLines = 0m;

				if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable)
				{
					exTaxVarianceAferAdjustingLines = InvoicingBase.ExpectedInvoiceExclTaxTotal - InvoicingBase.AH_OSExTaxAmount;
				}
				else if (InvoicingBase.Header != null && !InvoicingBase.Header.CompanyData.IsAPTaxApplicable)
				{
					exTaxVarianceAferAdjustingLines = InvoicingBase.ExpectedInvoiceTotal - InvoicingBase.AH_OSTotalAmount;
				}

				if (exTaxVarianceAferAdjustingLines != 0)
				{
					InvoicingLineBase line = FindLineWithGreatestExTaxAmountAndNoTaxAmount();

					if (line.IsPopulatedFromImportedApportionment)
					{
						JobConsolCost consolCost = FindJobConsolCostForLine(line);

						if (consolCost != null)
						{
							consolCost.E6_OSCostAmount += exTaxVarianceAferAdjustingLines;
						}

						InvoicingBase.ImportAllApportionmentsFromCosting();
					}
					else
					{
						decimal lineTaxAmount = line.AL_OSTaxAmount;
						line.AL_OSExTaxAmount += exTaxVarianceAferAdjustingLines;
						line.AL_OSTaxAmount = lineTaxAmount;
					}
				}
			}
		}

		decimal GetTotalOSExTaxAmountForLinesWithNoTaxAmount()
		{
			decimal result = 0m;

			foreach (InvoicingLineBase line in InvoicingBase.Lines)
			{
				if (line.AL_OSTaxAmount == 0)
				{
					result += line.AL_OSExTaxAmount;
				}
			}

			return result;
		}

		InvoicingLineBase FindLineWithGreatestTaxAmount()
		{
			InvoicingLineBase result = null;

			foreach (InvoicingLineBase line in InvoicingBase.Lines)
			{
				if (result == null || (line.AL_OSTaxAmount != 0 && line.AL_OSTaxAmount > result.AL_OSTaxAmount))
				{
					result = line;
				}
			}

			return result;
		}

		InvoicingLineBase FindLineWithGreatestExTaxAmountAndNoTaxAmount()
		{
			InvoicingLineBase result = null;

			foreach (InvoicingLineBase line in InvoicingBase.Lines)
			{
				if (result == null || (line.AL_OSTaxAmount != 0 && line.AL_OSExTaxAmount > result.AL_OSExTaxAmount))
				{
					result = line;
				}
			}

			return result;
		}
		
		JobConsolCost FindJobConsolCostForLine(InvoicingLineBase line)
		{
			return line.Factory.Load<JobConsolCost>(line.ImportedApportionmentID);
		}

		public string Validate()
		{
			string result = string.Empty;
			if (InvoicingBase.Header == null)
			{
				result = Res.GetString("91d9c867-cd0a-4a53-887b-7538e4832f6e", "Please enter a creditor.");
			}
			else if (!InvoicingBase.ValidateExpectedInvoiceTotal && !InvoicingBase.IsExpectedTaxTotalVisible)
			{
				result = Res.GetString("ec98d6d2-e2bd-47ec-8eac-38315ba18051", "Expected Total not entered.\r\n\r\nPlease enter a value against \"Expected Total\" before running \"Auto-Allocate Discrepancy\".");
			}
			else if (!InvoicingBase.ValidateExpectedInvoiceTotal && InvoicingBase.IsExpectedTaxTotalVisible)
			{
				result = Res.GetString("fcb70e8e-56f0-4752-92b3-c92e1bb32568", "Expected Total not entered.\r\n\r\nPlease enter a value against \"Expected Total Including Tax\", \"Expected Total Tax\" and \"Expected Total Excluding Tax\" before running \"Auto-Allocate Discrepancy\".");
			}
			else if (InvoicingBase.Header != null && !InvoicingBase.Header.CompanyData.IsAPTaxApplicable
				&& InvoicingBase.ExpectedInvoiceTotal == 0m)
			{
				result = Res.GetString("f1ea5bea-55f8-4db6-9523-36eb31e70ddf", "\"Expected Total\" cannot be zero.\r\n\r\nPlease enter a value against \"Expected Total\" before running \"Auto-Allocate Discrepancy\".");
			}
			else if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable
				&& (InvoicingBase.ExpectedInvoiceExclTaxTotal == 0m || InvoicingBase.ExpectedInvoiceTotal == 0m))
			{
				result = Res.GetString("7d89b2c7-859b-4c42-b18c-c656cf4a0438", "\"Expected Total Including Tax\" cannot be zero.  \"Expected Total Excluding Tax\" cannot be zero.\r\n\r\nPlease enter a value against \"Expected Total Including Tax\" and \"Expected Total Excluding Tax\" before running \"Auto-Allocate Discrepancy\".");
			}
			else if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable
				&& InvoicingBase.ExpectedInvoiceTaxTotal > 0m && !InvoiceHasTaxLines)
			{
				result = Res.GetString("0b0da8bf-10a8-43ea-86dc-a677b3abb889", "The Expected Total Tax is greater than zero but there are no lines with tax.");
			}
			else if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable
				&& InvoicingBase.ExpectedInvoiceTaxTotal == 0m && InvoiceHasTaxLines)
			{
				result = Res.GetString("74ce0058-0f2c-490a-9b86-44de54a57667", "The Expected Total Tax is equal to zero but there are lines with tax.");
			}
			else if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable
				&& InvoicingBase.ExpectedInvoiceTotal != (InvoicingBase.ExpectedInvoiceExclTaxTotal + InvoicingBase.ExpectedInvoiceTaxTotal))
			{
				result = Res.GetString("e014695c-a337-47b3-a51d-3d5b0fd6736e", "The Expected Total Incl. Tax must equal the Expected Total Tax plus the Expected Total Excl. Tax.");
			}
			else if (InvoicingBase.Header != null && !InvoicingBase.Header.CompanyData.IsAPTaxApplicable
				&& (InvoicingBase.ExpectedInvoiceTotal == InvoicingBase.AH_OSTotalAmount || InvoicingBase.Lines.Count == 0))
			{
				result = Res.GetString("d0148520-428d-41be-913c-574caa9a31f0", "No discrepancy to allocate.");
			}
			else if (InvoicingBase.Header != null && InvoicingBase.Header.CompanyData.IsAPTaxApplicable &&
				((InvoicingBase.ExpectedInvoiceTotal == InvoicingBase.AH_OSTotalAmount
					&& InvoicingBase.ExpectedInvoiceTaxTotal == InvoicingBase.AH_OSTaxAmount
					&& InvoicingBase.ExpectedInvoiceExclTaxTotal == InvoicingBase.AH_OSExTaxAmount)
					|| InvoicingBase.Lines.Count == 0))
			{
				result = Res.GetString("d0148520-428d-41be-913c-574caa9a31f0", "No discrepancy to allocate.");
			}
			else if (InvoicingBase.Lines.Any((x) => { return ((InvoicingLineBase)x).AL_RX_NKTransactionCurrency != InvoicingBase.AH_RX_NKTransactionCurrency; }))
			{
				result = Res.GetString("05e29ecf-c93e-4015-a0af-3124c525b641", "All lines must have the same currency as the invoice currency.");
			}
			return result;
		}

		bool InvoiceHasTaxLines
		{
			get
			{
				bool result = false;

				foreach (InvoicingLineBase line in InvoicingBase.Lines)
				{
					if (line.AL_TaxRateCalc != 0m)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}
	}
}
