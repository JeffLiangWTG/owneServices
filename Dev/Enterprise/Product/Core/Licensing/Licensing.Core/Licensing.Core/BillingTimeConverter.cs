using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;

namespace Enterprise.Licensing
{
	public class BillingTimeConverter
	{
		public BillingTimeConverter()
		{
			registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
		}
		readonly IProductRegistrationKey registrationKey;

		public ZDateTime ConvertUtcToTimeInBillingTimeZone(ZDateTime utcTime)
		{
			if (!utcTime.IsValid)
			{
				return ZDateTime.Empty;
			}

			return utcTime < registrationKey.NextUtcOffsetEffectiveTimeUtc
					? utcTime.AddHours(registrationKey.CurrentBillingTimeZoneUtcOffset)
					: utcTime.AddHours(registrationKey.NextBillingTimeZoneUtcOffset);
		}

		public ZDateTime ConvertTimeInBillingTimeZoneToUtc(ZDateTime localTime)
		{
			if (!localTime.IsValid)
			{
				return ZDateTime.Empty;
			}

			return localTime < registrationKey.NextUtcOffsetEffectiveTimeUtc.AddHours(registrationKey.NextBillingTimeZoneUtcOffset)
					? localTime.AddHours(-registrationKey.CurrentBillingTimeZoneUtcOffset)
					: localTime.AddHours(-registrationKey.NextBillingTimeZoneUtcOffset);
		}
	}
}
