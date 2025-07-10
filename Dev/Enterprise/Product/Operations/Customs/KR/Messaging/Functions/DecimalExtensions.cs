using System.Globalization;

namespace Enterprise.Customs.KR.Messaging
{
	public static class DecimalExtensions
	{
		public static int GetNumberDecimalPlaces(this decimal source)
		{
			var stringValue = source.ToString(CultureInfo.InvariantCulture).Split('.');

			var result = 0;
			if (stringValue.Length == 2)
			{
				result = stringValue[1].TrimEnd('0').Length;
			}

			return result;
		}
	}
}
