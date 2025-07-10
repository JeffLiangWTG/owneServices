using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Export.Business
{
	public static class JobPaymentBasisHelper
	{
		public static RatingBasis PopulateUniversalPaymentBases(IJobPaymentBasis paymentBasis)
		{
			var universalPaymentBasis = new RatingBasis
			{
				OriginKey = paymentBasis.AdapterID,
				OriginType = paymentBasis.AdapterType,
				OriginAdditionalReference = paymentBasis.ChargeableDescription,
				MinimumRate = paymentBasis.MinRate,
				MaximumRate = paymentBasis.MaxRate,
				FlatRate = paymentBasis.FlatRate,
				PerUnitRate = paymentBasis.PerUnitRate,
				RateReference = paymentBasis.RateReference,
			};

			if (paymentBasis.PerUnitRate != 0)
			{
				universalPaymentBasis.OriginQuantity = paymentBasis.ChargeableAmount;
				universalPaymentBasis.OriginQuantityUnit = new RatingUnit { Code = paymentBasis.ChargeableUnit, Class = ConvertUnitType(paymentBasis.ChargeableUnitType) };
				universalPaymentBasis.RateUnit = new RatingUnit { Code = paymentBasis.RateUnit, Class = ConvertUnitType(paymentBasis.RateUnitType) };
			}

			if (paymentBasis.RateUnit != "100")
			{
				universalPaymentBasis.Currency = new Currency() { Code = paymentBasis.RateCurrency, Description = paymentBasis.RateCurrencyDescription };
			}

			return universalPaymentBasis;
		}

		static RatingUnitClass ConvertUnitType(ZString chargeableUnitType)
		{
			if (Enum.TryParse(chargeableUnitType, true, out RatingUnitClass unitClass))
			{
				return unitClass;
			}

			return RatingUnitClass.None;
		}
	}
}
