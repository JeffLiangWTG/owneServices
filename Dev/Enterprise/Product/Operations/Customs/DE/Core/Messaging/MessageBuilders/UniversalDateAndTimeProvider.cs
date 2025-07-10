using System;
using System.Globalization;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public class UniversalDateAndTimeProvider : IDateAndTime
	{
		public UniversalDateAndTimeProvider(bool zeroSeconds = false)
		{
			utcTime = ZDateTime.UtcNow.ToDateTime();
			utcTime = utcTime.AddMilliseconds(-utcTime.Millisecond);
			if (zeroSeconds)
			{
				utcTime = utcTime.ZeroFromSecond();
			}
		}

		public DateTime DateAndTime => utcTime;

		public DateTime Date => utcTime.Date;

		public string Time => utcTime.TimeOfDay.ToString("hh':'mm':'ss", CultureInfo.InvariantCulture);

		readonly DateTime utcTime;
	}
}
