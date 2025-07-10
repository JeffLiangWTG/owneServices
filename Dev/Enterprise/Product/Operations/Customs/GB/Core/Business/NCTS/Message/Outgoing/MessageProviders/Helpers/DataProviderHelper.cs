using System;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public static class DataProviderHelper
	{
		public static DateTime GetProviderDateTime(ZDateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}
	}
}
