using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared.JobComInvHeaderCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public static class ChargeCollectionHelper
	{
		public static ZDecimal AmountToAddToITOTForStatisticalChargesES(this IJobComInvChargeBaseCollection<JobComInvCharge> collection, RefCurrency currency, CurrencyConverter currencyConverter)
		{
			ZDecimal result = 0;

			if (currencyConverter != null)
			{
				var amountToAdd = new Money(0, currency);
				var amountToSubtract = new Money(0, currency);
				var applicableCustomsCharges = collection
					.Where(c => c?.ChargeCode is CustomsChargeCode && new ZBool(c[JobComInvHeaderChargeSchema.Constants.J7_IsStatisticalValueApplicable]));
				foreach (var charge in applicableCustomsCharges)
				{
					bool isDutiable = new ZBool(charge[JobComInvHeaderChargeSchema.J7_IsDutiable]);
					var chargeOperationType = ((CustomsChargeCode)charge.ChargeCode).ChargeOperationType;

					if (chargeOperationType == ChargeCodeOperationType.Added && collection.ShouldAddAmountToITOT(charge, !isDutiable))
					{
						amountToAdd = currencyConverter.Add(amountToAdd, charge.Money);
					}
					else if (chargeOperationType == ChargeCodeOperationType.Deducted && collection.ShouldSubtractAmountFromITOT(charge, !isDutiable))
					{
						amountToSubtract = currencyConverter.Add(amountToSubtract, charge.Money);
					}
				}

				result = currencyConverter.ConvertExact(currencyConverter.Subtract(amountToAdd, amountToSubtract), currency).Amount;
			}

			return result;
		}
	}
}
