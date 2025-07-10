using System;
using System.Globalization;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Messaging
{
	public class CentralEuropeanStandardDateAndTimeProvider : IDateAndTime
	{
		public CentralEuropeanStandardDateAndTimeProvider(bool zeroSeconds = false)
		{
			centralEasterStandardTime = TimeZoneInfo.ConvertTime(ZDateTime.UtcNow.ToDateTime(), TimeZoneInfo.FindSystemTimeZoneById((NoResString)"Central European Standard Time"));// Time Zone Id;
			centralEasterStandardTime = centralEasterStandardTime.AddMilliseconds(-centralEasterStandardTime.Millisecond);
			if (zeroSeconds)
			{
				centralEasterStandardTime = centralEasterStandardTime.ZeroFromSecond();
			}
		}

		public DateTime DateAndTime => centralEasterStandardTime;

		public DateTime Date => centralEasterStandardTime.Date;

		public string Time => centralEasterStandardTime.TimeOfDay.ToString("hh':'mm':'ss", CultureInfo.InvariantCulture);

		readonly DateTime centralEasterStandardTime;
	}
}
