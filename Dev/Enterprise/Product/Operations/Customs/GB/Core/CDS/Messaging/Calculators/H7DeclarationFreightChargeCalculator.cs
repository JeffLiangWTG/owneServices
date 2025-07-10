using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.CDS.Messaging.Calculators
{
	public class H7DeclarationFreightChargeCalculator
	{
		public H7DeclarationFreightChargeCalculator(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
		}

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;

		public IAmountAndCurrency CalculateFreightChargeAmount()
		{
			var includedCharges = new List<string> { ChargesProvider.AirFreightCode, CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Codes.OverseasInsurance };
			var targetCurrency = FindTargetCurrency(includedCharges);
			var chargeAmount = 0m;
			foreach (JobComInvCharge charge in declaration.JobComInvoiceGroupHeaders[0].Charges)
			{
				if (includedCharges.Contains(charge.J7_ChargeType))
				{
					if ((charge.Currency?.Code ?? string.Empty) == targetCurrency)
					{
						chargeAmount += charge.J7_Amount;
					}
					else
					{
						var amount = ConvertToTargetCurrency(targetCurrency, charge);
						chargeAmount += amount;
					}
				}
			}
			return AmountAndCurrencyWrapper.New(chargeAmount, targetCurrency);
		}

		decimal ConvertToTargetCurrency(string targetCurrency, JobComInvCharge charge)
		{
			var factory = entryHeader.Factory;
			var currency = RefCurrency.LoadFromCurrencyCode(factory, charge.Currency?.Code ?? string.Empty);
			var money = new Money(charge.J7_Amount, currency);
			var requiredCurrency = RefCurrency.LoadFromCurrencyCode(factory, targetCurrency);
			money = ((ICurrencyConverterProvider)entryHeader.Declaration).CurrencyConverter.ConvertRounded(money, requiredCurrency);
			return money.Amount;
		}

		string FindTargetCurrency(List<string> includedCharges)
		{
			var groupHeaderCharges = declaration.JobComInvoiceGroupHeaders[0].Charges.Cast<GroupInvoiceCharge>().Where(x => includedCharges.Contains(x.J7_ChargeType));

			var distinctGroupHeaderCharges = from charge in groupHeaderCharges
											 group charge by charge.Currency?.Code ?? string.Empty;

			if (distinctGroupHeaderCharges.Count() == 1)
			{
				return distinctGroupHeaderCharges.FirstOrDefault().Key;
			}
			else
			{
				return Core.Constants.CurrencyCodes.UnitedKingdom;
			}
		}
	}
}
