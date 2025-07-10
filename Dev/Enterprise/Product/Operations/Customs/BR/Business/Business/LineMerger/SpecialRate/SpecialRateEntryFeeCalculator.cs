using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class SpecialRateEntryFeeCalculator
	{
		public SpecialRateEntryFeeCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public void UpdateFeesOnEntryLine()
		{
			var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();

			foreach (var taxes in invoiceLines.SelectMany(x => x.SpecialCaseTaxes.Cast<SpecialCaseTax>().Where(t => t.TaxType == SpecialCaseTaxTypeList.Codes.QuantityPerUnit)).GroupBy(x => x.TaxGroup))
			{
				var tax = taxes.First();
				if (tax.RateOrUnitValue > 0)
				{
					var rateCode = taxes.Key;
					var invoiceLine = tax.InvoiceLine;

					var valuePerUnit = invoiceLine.CurrencyConverter.ConvertExact(new Money(tax.RateOrUnitValue, tax.Currency), invoiceLine.LocalCurrency, false).Amount;
					if (valuePerUnit > 0)
					{
						var feeType = GetFeeType(rateCode);
						if (!feeType.IsEmpty)
						{
							var totalQuantity = taxes.Sum(x => x.Quantity);

							var fee = entryLine.Fees.AddOrUpdate(feeType, Utilities.Round(totalQuantity * valuePerUnit, 2));
							fee.CF_BaseValue = totalQuantity;
							fee.CF_Rate = Utilities.Round(valuePerUnit, 5);
							fee.CF_MethodOfCalculation = tax.TaxType;
						}
					}
				}
			}
		}

		ZString GetFeeType(ZString rateCode)
		{
			return CusRefRateCodeView.Loader.LoadByRateCode(entryLine.Factory, Core.Constants.CountryCodes.Brazil, rateCode).FirstOrDefault()?.ZY1_RateType ?? ZString.Empty;
		}
	}
}
