using System;
using CargoWise.Types;

namespace Enterprise.CustomerService.Business
{
	static class IncidentDetailsValidator
	{
		public static bool HasWesternCharactersAndIsTooShort(ZString details)
		{
			return details.IsWesternEuropeanOrEmpty && details.ToString().Split(new string[] { " ", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length < 5;
		}

		public static bool HasNonWesternCharactersAndIsTooShort(ZString details)
		{
			return details.IsEmpty || (!details.IsWesternEuropeanOrEmpty && details.Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Length < 5);
		}
	}
}
