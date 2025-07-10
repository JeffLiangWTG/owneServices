using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper
{
	public class BaseInvoiceDocLineRollUpper
	{
		public BaseInvoiceDocLineRollUpper(DocARBaseInvoice docARBaseInvoice)
		{
			DocARBaseInvoice = docARBaseInvoice;
		}

		protected DocARBaseInvoice DocARBaseInvoice { get; }

		public DocARInvoiceLineForRollUp CreateWrapperForRolledUpLine(IZType description, ZDecimal amount, ZDecimal taxAmount, ZDecimal localAmount, ZDecimal localGSTVAT, DocARInvoiceLine.AmountWithExchangeRate exchangeRates, DocChargeCode chargeCode)
		{
			var rollUpWrapper = DocARInvoiceLineForRollUp.New(DocARBaseInvoice.Factory);
			rollUpWrapper.SetLineDescription(description);
			rollUpWrapper.OSExTaxAmount = amount;
			rollUpWrapper.OSTaxAmount = taxAmount;

			if (chargeCode != null)
			{
				rollUpWrapper.ChargeCode = chargeCode;
			}

			if (taxAmount != 0)
			{
				rollUpWrapper.OSTaxDisplay = FormatGSTAmount(taxAmount);
			}
			else
			{
				if (rollUpWrapper.IsCommentLine)
				{
					rollUpWrapper.OSTaxDisplay = "";
				}
				else
				{
					rollUpWrapper.OSTaxDisplay = Res.GetString("1dcf9d29-afd6-413f-a98e-903e161416f2", "N/A");
				}
			}
			rollUpWrapper.LineAmount = localAmount;
			rollUpWrapper.GSTVAT = localGSTVAT;
			rollUpWrapper.Currency = DocARBaseInvoice.Currency;

			rollUpWrapper.ExchangeRateAndAmount = exchangeRates.ToString();

			return rollUpWrapper;
		}

		public DocARInvoiceLineForRollUp CreateWrapperForRolledUpLine(IZType description, ZDecimal amount, ZDecimal gSTAmount, ZDecimal localAmount, ZDecimal localGSTVAT, DocARInvoiceLine.AmountWithExchangeRate exchangeRates, DocARInvoiceLineCollection rolledUpLines, DocChargeCode chargeCode)
		{
			var rollUpWrapper = CreateWrapperForRolledUpLine(description, amount, gSTAmount, localAmount, localGSTVAT, exchangeRates, chargeCode);
			if (rolledUpLines.Count > 0)
			{
				rollUpWrapper.Invoice = rolledUpLines[0].Invoice;
			}
			PrepareRollUpLineForGrouping(rollUpWrapper, rolledUpLines);
			return rollUpWrapper;
		}

		public virtual void PrepareRollUpLineForGrouping(DocARInvoiceLineForRollUp rollUpLine, DocARInvoiceLineCollection rolledUpLines)
		{
			if (rolledUpLines != null && rolledUpLines.Count > 0)
			{
				if (!rollUpLine.IsSpacerLine && !rollUpLine.IsCommentLine)
				{
					var originalLinesWithPOS = rolledUpLines.OfType<DocARInvoiceLine>().ToArray();
					var firstRolledUpLine = originalLinesWithPOS.FirstOrDefault();
					if (firstRolledUpLine != null)
					{
						var firstFPOSValue = firstRolledUpLine.FixedPlaceOfSupply;
						var firstFPOSLabelValue = firstRolledUpLine.FixedPlaceOfSupplyLabel;
						if (originalLinesWithPOS.All(x => x.FixedPlaceOfSupply == firstFPOSValue && x.FixedPlaceOfSupplyLabel == firstFPOSLabelValue))
						{
							rollUpLine.FixedPlaceOfSupply = firstFPOSValue;
							rollUpLine.FixedPlaceOfSupplyLabel = firstFPOSLabelValue;
						}
					}
				}

				rollUpLine.DisplayTaxGroupCode = rolledUpLines[0].DisplayTaxGroupCode;

				if (!rollUpLine.IsSpacerLine && !rollUpLine.IsSubTotalLine && !rollUpLine.IsCommentLine && rolledUpLines.OnlyOneJob)
				{
					foreach (IDocARInvoiceLine line in rolledUpLines)
					{
						if (line.ChargeCode != null)
						{
							ZString chargeCode = line.ChargeCode.Code;
							rollUpLine.AmountSplittedByChargeCode.Add(DocAmountByChargeCode.New(new AmountByChargeCode(chargeCode, line.OSAmount, line.OSExTaxAmount), DocARBaseInvoice.Factory));
						}
					}
				}

				bool displayTax = !rollUpLine.IsSpacerLine && !rollUpLine.IsSubTotalLine;
				if (rolledUpLines.IsAtLeastOneTaxRateSpecified && rolledUpLines.FirstReportableTaxRate.Rate != null)
				{
					rollUpLine.OSGSTAmount = rolledUpLines.TotalOSGSTAmount;
					rollUpLine.OSEDUAmount = rolledUpLines.TotalOSEDUAmount;
					rollUpLine.OSQSTAmount = rolledUpLines.TotalOSQSTAmount;
					rollUpLine.OSRETAmount = rolledUpLines.TotalOSRETAmount;
					rollUpLine.OSSPVAmount = rolledUpLines.TotalOSSPVAmount;
					rollUpLine.OSSERAmount = rolledUpLines.TotalOSSERAmount;
					rollUpLine.OSIntegratedGSTAmount = rolledUpLines.TotalOSIntegratedGSTAmount;
					rollUpLine.OSCentreGSTAmount = rolledUpLines.TotalOSCentreGSTAmount;
					rollUpLine.OSStateGSTAmount = rolledUpLines.TotalOSStateGSTAmount;

					if (displayTax)
					{
						if (!rolledUpLines.IsSameTaxRateForAllLines)
						{
							rollUpLine.OSTaxDisplay = this.FormatTaxAmount(rollUpLine.OSTaxAmount);
							rollUpLine.TaxAmountDisplay = this.FormatTaxAmount(rollUpLine.OSTaxAmount);
							// if lines have different tax rate, it's impossible to display one single tax for all of them, so we just display the tax amount here.
							rollUpLine.OSTaxDisplayNoAsterisksWithRegistryRule = rollUpLine.OSTaxDisplay;
						}
						else
						{
							(rollUpLine.TaxRate, rollUpLine.TaxRateAmount_Raw, rollUpLine.TaxExtraRateAmount) = rolledUpLines.FirstReportableTaxRate;
							rollUpLine.ShowPercentInGSTDisplay = !AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value;
							rollUpLine.OSTaxDisplay = rollUpLine.GetOSTaxAmountDisplay();
							rollUpLine.TaxAmountDisplay = rollUpLine.GetTaxAmountDisplay();
							rollUpLine.OSTaxDisplayNoAsterisksWithRegistryRule = rollUpLine.GetOSTaxAmountDisplayWithRegistryRule();
						}

						rollUpLine.TaxRateAsterisksAsNumbers = rolledUpLines.AsterisksAsNumbers;
						rollUpLine.OSTaxDisplay += rolledUpLines.Asterisks;
					}
				}
				else if (displayTax)
				{
					if (!rolledUpLines.IsAtLeastOneTaxRateSpecified)
					{
						if (!rollUpLine.IsCommentLine)
						{
							rollUpLine.TaxAmountDisplay = rollUpLine.OSTaxDisplay = Res.GetString("1dcf9d29-afd6-413f-a98e-903e161416f2", "N/A");
						}
					}
					else if (rolledUpLines.FirstReportableTaxRate.Rate == null)
					{
						rollUpLine.TaxAmountDisplay = rollUpLine.OSTaxDisplay = ResString.GetMultilingualString("20924e9d-3937-4acf-a4e2-8a9c859342b0", "Not Applicable");
						rollUpLine.OSTaxDisplayNoAsterisksWithRegistryRule = AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.NotReportable);

						rollUpLine.TaxRateAsterisksAsNumbers = rolledUpLines.AsterisksAsNumbers;
						rollUpLine.OSTaxDisplay += rolledUpLines.Asterisks;
					}
				}
			}
		}

		string FormatGSTAmount(ZDecimal gstAmount) => FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(gstAmount, DocARBaseInvoice.Currency);

		protected string FormatTaxAmount(ZDecimal amount) => FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(amount, DocARBaseInvoice.Currency);
	}
}
