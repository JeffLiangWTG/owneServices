using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared.JobComInvHeaderCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class IeCustomsValuationCalculator : EU.Business.Declaration.EuCustomsValuationCalculator
	{
		public IeCustomsValuationCalculator(IChargeApportionee chargeApportionee) : base(chargeApportionee)
		{
		}

		public override ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency)
		{
			var isChargeType2X = false;
			var charges = ChargeApportionee.Charges?.Cast<JobComInvCharge>();
			var apportionedCharges = ChargeApportionee.ApportionedCharges?.Cast<JobComInvCharge>();

			if (charges != null || apportionedCharges != null)
			{
				isChargeType2X = charges?.Any(c => c.J7_ChargeType == AISChargeCodeList.Codes._2X) ?? false;
				isChargeType2X = isChargeType2X || (apportionedCharges?.Any(c => c.J7_ChargeType == AISChargeCodeList.Codes._2X) ?? false);
			}

			if (isChargeType2X is not true)
			{
				return base.GetAmountToAddToITOTForStatistical(currency);
			}
			return GetChargeValue(currency, ChargeApportionee);
		}

		static ZDecimal GetChargeValue(RefCurrency currency, IChargeApportionee chargeApportionee)
		{
			var result = 0m;
			result += CalculateChargeValue(currency, chargeApportionee.Charges);
			result += CalculateChargeValue(currency, chargeApportionee.ApportionedCharges);
			return result;
		}

		static ZDecimal CalculateChargeValue(RefCurrency currency, IJobComInvChargeBaseCollection<JobComInvCharge> charges)
		{
			var result = ZDecimal.Zero;
			var colName = JobComInvHeaderChargeSchema.J7_IsStatisticalValueApplicable.Name;

			if (charges.CurrencyConverter != null)
			{
				var amountToAdd = new Money(0, currency);
				var amountToSubtract = new Money(0, currency);

				foreach (var charge in charges)
				{
					if (charge.ChargeCode != null && charge.J7_ChargeType != AISChargeCodeList.Codes._2X)
					{
						var isApplicable = new ZBool(charge[colName]);

						if (charges.ShouldAddAmountToITOT(charge, isApplicable))
						{
							amountToAdd = charges.CurrencyConverter.Add(amountToAdd, charge.Money);
						}
						else if (charges.ShouldSubtractAmountFromITOT(charge, isApplicable))
						{
							amountToSubtract = charges.CurrencyConverter.Add(amountToSubtract, charge.Money);
						}
					}
				}
				result = charges.CurrencyConverter.ConvertExact(charges.CurrencyConverter.Subtract(amountToAdd, amountToSubtract), currency).Amount;
			}

			return result;
		}
	}
}
