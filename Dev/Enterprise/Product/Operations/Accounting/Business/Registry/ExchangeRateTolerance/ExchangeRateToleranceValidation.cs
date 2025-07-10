using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class ExchangeRateToleranceValidation
	{
		public ExchangeRateToleranceValidation(ExchangeRateTolerance parent)
		{
			this.Parent = parent;
		}

		readonly ExchangeRateTolerance Parent;

		public void ValidateExchangeRateTolerancePercentage()
		{
			if (Parent.ExchangeRateTolerancePercentage < 0 || Parent.ExchangeRateTolerancePercentage > 100)
			{
				Parent.ExchangeRateTolerancePercentageInfo.AddNotification(NotificationType.Error, ResString.GetMultilingualString("82CEE9DA-CF44-4066-ACCC-8AA772E90DB9", "Value should be between 0 to 100."));
			}
		}

		public void ValidateCurrency()
		{
			MandatoryValidation.CheckEntered(Parent.CurrencyInfo, (IMultilingualString)ResString.GetMultilingualString("6B6D55BB-B913-417B-9815-D99D69D8A351", "Currency"));

			if (Parent.Currency != ExchangeRateToleranceLookups.AllCurrencyCode)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CurrencyInfo, Parent.Lookups.CurrencyList);
			}

			var comparingCollection = Parent.ParentCollection?.Cast<ExchangeRateTolerance>().Except(new[] { Parent }) ?? Array.Empty<ExchangeRateTolerance>();
			if (comparingCollection.Any() && comparingCollection.Any(x => x.Currency == Parent.Currency))
			{
				Parent.CurrencyInfo.AddNotification(NotificationType.Error, ResString.GetMultilingualString("032FF9CE-D59E-46CF-9E06-1C95708264EC", "Only single configuration per currency should be allowed."));
			}
		}
	}
}
