using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Forwarding.Business
{
	public class ForwardingShipmentCustomsInformationValidation : ZValidation
	{
		public ForwardingShipmentCustomsInformationValidation(ForwardingShipmentCustomsInformation parent)
			: base(parent)
		{
			Parent = Argument.NotNull(parent, "parent");
		}

		protected ForwardingShipmentCustomsInformation Parent { get; }

		public override Type AutoValidationType => typeof(ForwardingShipmentCustomsInformationValidation);

		public override void ValidateAll()
		{
			ValidateDestinationExchangeRate();
		}

		public void ValidateDestinationExchangeRate()
		{
			ValidateCalculatedProperty(Parent.DestinationExchangeRateInfo);
		}

		protected void CheckDestinationExchangeRate()
		{
			var destCurrency = RefCurrency.LoadFromCurrencyCode(Parent.Shipment.Factory, Parent.DestinationCurrencyCode);
			if (destCurrency != null && Parent.Shipment.GoodsValueCurr is RefCurrency originCurrency && destCurrency.RX_Code != originCurrency.RX_Code && Parent.DateForDestinationExchangeRate.IsValid)
			{
				var foundRateDate = ZDateTime.Empty;
				var foundRate = Parent.CurrencyConverter.GetExchangeRate(destCurrency, out foundRateDate);
				if (foundRate.IsEmpty)
				{
					Parent.DestinationExchangeRateInfo.AddWarning(Res.GetString("ca084169-8fe6-4ac7-a1f1-69fa2c439fbc", "There is no valid exchange rate found for destination currency {0} for {1}.", destCurrency.RX_Code, Parent.DateForDestinationExchangeRate.ToString("d")));
				}
				else if ((Parent.DateForDestinationExchangeRate.Date - foundRateDate.Date).TotalDays > MaximumDaysToFallbackBeforeWarningShown)
				{
					Parent.DestinationExchangeRateInfo.AddWarning(Res.GetString("7a1704cb-8a9f-450e-8597-eb53fb8754cd", "There is no exchange rate in database for {0}. The closest match of the exchange rate for destination currency {1} is the rate for the date {2}. This exchange rate will be used in calculating the destination value.", Parent.DateForDestinationExchangeRate.Date.ToString("d"), destCurrency.RX_Code, foundRateDate.Date.ToString("d")));
				}
			}
		}

		int MaximumDaysToFallbackBeforeWarningShown
		{
			get
			{
				int result = 0;
				var dateOfValudation = Parent.DateForDestinationExchangeRate;
				if (dateOfValudation.IsValid)
				{
					switch (dateOfValudation.DayOfWeek)
					{
						case DayOfWeek.Sunday:
							result = 1;
							break;
						case DayOfWeek.Monday:
							result = 2;
							break;
					}
				}
				return result;
			}
		}
	}
}
