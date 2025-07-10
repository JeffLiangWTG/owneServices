using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public abstract class ValuationMethodDataCreator<THeader>
		where THeader : ValuationMethodData, new()
	{
		public THeader Create(JobComInvoiceHeader invoice, CurrencyConverter converter)
		{
			var valuation = new THeader();
			valuation.BaseAmount = invoice.JZ_InvoiceAmount;
			valuation.ExchangeRate = invoice.JZ_InvoiceCurrExRate;

			var currencyKRW = RefCurrency.LoadFromCurrencyCode(invoice.Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			valuation.AmountInKRW = invoice.JZ_InvoiceAmountInLocalCurrency;
			valuation.Additions = PopulateChargeAmountKRW(invoice, ImportChargeMethodCodeList.GetAdditionalAmountList(invoice.JZ_ValuationCode)
															, converter, new Func<JobComInvCharge, bool>(charge => !charge.J7_Calc_IsIncludedInInvoiceAmount && charge.J7_IsDutiable));
			valuation.Deductions = PopulateChargeAmountKRW(invoice, ImportChargeMethodCodeList.GetDeductionAmountList(invoice.JZ_ValuationCode)
															, converter, new Func<JobComInvCharge, bool>(charge => charge.J7_Calc_IsIncludedInInvoiceAmount && !charge.J7_IsDutiable));

			PopulateMoreMethodData(valuation, invoice, converter);

			return valuation;
		}

		protected virtual void PopulateMoreMethodData(THeader valuation, JobComInvoiceHeader invoice, CurrencyConverter converter)
		{
		}

		ChargeAmountKRW[] PopulateChargeAmountKRW(JobComInvoiceHeader invoice, string[] chargeTypes, CurrencyConverter converter, Func<JobComInvCharge, bool> needsCalculation)
		{
			var result = new List<ChargeAmountKRW>();
			foreach (var type in chargeTypes)
			{
				if (chargeTypes.Length > 0)
				{
					Money money = Money.Empty;

					var charges = invoice.Charges.Where(x => x.J7_ChargeType == type);
					foreach (var charge in charges)
					{
						if (needsCalculation(charge))
						{
							money = converter.Add(money, converter.ConvertExact(charge.Money, converter.LocalCurrency));
						}
					}

					var apportionedCharges = invoice.GroupCharges.Where(x => x.J7_ChargeType == type);
					foreach (var apportionedCharge in apportionedCharges)
					{
						if (needsCalculation(apportionedCharge))
						{
							money = converter.Add(money, converter.ConvertExact(apportionedCharge.Money, converter.LocalCurrency));
						}
					}

					var chargeAmountKRW = new ChargeAmountKRW() { Type = ValuationCodeList.GetValuationMethod(invoice.JZ_ValuationCode) + type, Amount = money.Amount.Truncate() };

					result.Add(chargeAmountKRW);
				}
			}
			return result.ToArray();
		}
	}
}
