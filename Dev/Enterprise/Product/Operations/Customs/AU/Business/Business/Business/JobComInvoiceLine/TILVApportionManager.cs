using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class TILVApportionManager
	{
		public void ApportionTILV(JobDeclaration declaration)
		{
			ApportionIntoInvoiceLines(declaration);

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				if (!invoice.IsValidationSuspended)
				{
					invoice.Validation.ValidateJZ_Calc_TNI();
					invoice.JZ_Calc_TNIInfo.RefreshBinding();
				}
			}
		}

		void ApportionIntoInvoiceLines(JobDeclaration declaration)
		{
			var amountToApportion = Money.Empty;
			var standardInvoices = new List<JobComInvoiceHeader>();
			var hasManualTILV = false;

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				amountToApportion = declaration.TopGroupInvoice.CurrencyConverter.Add(amountToApportion, GetTransportAndInsuranceFromCharges(invoice));

				if (TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice))
				{
					ApportionTILV(GetTransportAndInsuranceToApportion(invoice), invoice.CurrencyConverter, (JobComInvoiceLine[])invoice.JobComInvoiceLines.ToArray(typeof(JobComInvoiceLine)), !invoice.AddInfo.ZA_TILV.IsEmpty);
					amountToApportion = declaration.TopGroupInvoice.CurrencyConverter.Subtract(amountToApportion, GetApportionedTILV(invoice));
					hasManualTILV |= !invoice.AddInfo.ZA_TILV.IsEmpty;
				}
				else
				{
					standardInvoices.Add(invoice);
				}
			}

			if (amountToApportion.Amount < 0)
			{
				amountToApportion = new Money(0, amountToApportion.Currency);
			}

			if (standardInvoices.Count != 0)
			{
				ApportionTILV(amountToApportion, declaration.TopGroupInvoice.CurrencyConverter, standardInvoices.SelectMany(i => i.JobComInvoiceLines.Cast<JobComInvoiceLine>()).ToArray(), hasManualTILV);
			}
		}

		Money GetTransportAndInsuranceToApportion(JobComInvoiceHeader invoice)
		{
			if (!invoice.AddInfo.ZA_TILV.IsEmpty)
			{
				return invoice.AddInfo.TILVMoney;
			}

			return GetTransportAndInsuranceFromCharges(invoice);
		}

		Money GetTransportAndInsuranceFromCharges(JobComInvoiceHeader invoice)
		{
			Money result = Money.Empty;

			foreach (BaseJobComInvHeaderCharge charge in invoice.Charges)
			{
				if (!charge.J7_IsDutiable && charge.J7_IsGSTApplicable && charge.Currency != null)
				{
					result = invoice.CurrencyConverter.Add(result, charge.Money);
				}
			}

			foreach (BaseJobComInvHeaderCharge charge in invoice.GroupCharges)
			{
				if (!charge.J7_IsDutiable && charge.J7_IsGSTApplicable && charge.Currency != null)
				{
					result = invoice.CurrencyConverter.Add(result, charge.Money);
				}
			}

			return result;
		}

		Money GetApportionedTILV(JobComInvoiceHeader invoice)
		{
			Money result = Money.Empty;

			foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
			{
				result = invoice.CurrencyConverter.Add(result, line.TransportAndInsurance);
			}

			return result;
		}

		void ApportionTILV(Money amountToApportion, CurrencyConverter currencyConverter, JobComInvoiceLine[] invoiceLines, bool hasManualTILV)
		{
			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				invoiceLine.AddInfo.ZA_CalcTILV_Hidden = ZString.Empty;
			}

			// amountToApportion can be non-positive in 2 cases:
			// 1) There are no charges to apportion. In this case we should leave TILV empty as it is.
			// 2) There were charges to apportion, but due to manual adjustments the remaining amount is zero. In this case we should specify zero remaining TILV on the lines.
			if (amountToApportion.Currency != null && (hasManualTILV || amountToApportion.Amount > 0))
			{
				GetTotalCustomsValueOfLinesWithoutTILV(currencyConverter, invoiceLines, out ZDecimal totalCustomsValueOfLinesWithoutTILV, out Money totalTILVFromLines, out bool hasLinesWithAdjustment);

				//There are TILVs overriden in some lines
				if (hasManualTILV || hasLinesWithAdjustment || (totalTILVFromLines != null && totalTILVFromLines.IsValid))
				{
					if (totalCustomsValueOfLinesWithoutTILV > 0m)
					{
						Money difference = amountToApportion;

						if (totalTILVFromLines != null && totalTILVFromLines.IsValid)
						{
							difference = currencyConverter.Add(amountToApportion, new Money(totalTILVFromLines.Amount * -1, totalTILVFromLines.Currency));
						}

						if (difference.IsValid)
						{
							if (difference.Amount < 0)
							{
								difference = new Money(0m, difference.Currency);
							}

							ApportionTILVToLinesWithoutTILV(invoiceLines, difference, totalCustomsValueOfLinesWithoutTILV);
						}
					}
				}
			}
		}

		void GetTotalCustomsValueOfLinesWithoutTILV(CurrencyConverter currencyConverter, IEnumerable<JobComInvoiceLine> invoiceLines, out ZDecimal totalCustomsValueOfLinesWithoutTILV, out Money totalTILVFromLines, out bool hasLinesWithAdjustment)
		{
			totalCustomsValueOfLinesWithoutTILV = 0m;
			totalTILVFromLines = null;
			hasLinesWithAdjustment = false;

			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				hasLinesWithAdjustment |= !invoiceLine.AddInfo.ZA_ADJ.IsEmpty;

				if (invoiceLine.AddInfo.ZA_TILV.IsEmpty)
				{
					totalCustomsValueOfLinesWithoutTILV += GetNegativeAdjustedCustomsValue(invoiceLine);
				}
				else
				{
					Money tilvMoney = invoiceLine.AddInfo.TILVMoney;

					if (tilvMoney.IsValid)
					{
						if (totalTILVFromLines == null)
						{
							totalTILVFromLines = invoiceLine.AddInfo.TILVMoney;
						}
						else
						{
							totalTILVFromLines = currencyConverter.Add(totalTILVFromLines, tilvMoney);
						}
					}
				}
			}
		}

		void ApportionTILVToLinesWithoutTILV(IEnumerable<JobComInvoiceLine> invoiceLines, Money amounToApportion, ZDecimal totalCustomsValue)
		{
			ZDecimal totalApportioned = 0m;
			JobComInvoiceLine lineWithMaxCustomsValue = null;

			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				if (invoiceLine.AddInfo.ZA_TILV.IsEmpty)
				{
					if (lineWithMaxCustomsValue == null || GetNegativeAdjustedCustomsValue(lineWithMaxCustomsValue) < GetNegativeAdjustedCustomsValue(invoiceLine))
					{
						lineWithMaxCustomsValue = invoiceLine;
					}

					ZDecimal apportionedAmount = amounToApportion.Amount * (GetNegativeAdjustedCustomsValue(invoiceLine) / totalCustomsValue);
					apportionedAmount = apportionedAmount.Round(2);

					invoiceLine.AddInfo.ZA_CalcTILV_Hidden = new Money(apportionedAmount, amounToApportion.Currency).ToString();

					totalApportioned += apportionedAmount;
				}
			}

			if (amounToApportion.Amount != totalApportioned && lineWithMaxCustomsValue != null)
			{
				ZDecimal amount = lineWithMaxCustomsValue.AddInfo.TILVMoney.Amount;

				lineWithMaxCustomsValue.AddInfo.ZA_CalcTILV_Hidden = new Money(amount + (amounToApportion.Amount - totalApportioned), lineWithMaxCustomsValue.AddInfo.TILVMoney.Currency).ToString();
			}
		}

		ZDecimal GetNegativeAdjustedCustomsValue(JobComInvoiceLine invoiceLine)
		{
			ZDecimal result = invoiceLine.JI_CustomsValue;

			if (result < 0m)
			{
				result = 0m;
			}

			return result;
		}
	}
}
