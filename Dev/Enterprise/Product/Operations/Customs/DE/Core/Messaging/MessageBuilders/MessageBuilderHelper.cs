using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public static class MessageBuilderHelper
	{
		public static string LeftOrNull(this ZString value, int length) => value.IsEmpty ? null : (string)value.Left(length);

		public static string ValueOrNullIfEmpty(this ZString s)
		{
			string result = null;
			if (!s.IsEmpty)
			{
				result = s;
			}
			return result;
		}

		public static string MapBoolToString(this ZBool value, string trueValue, string falseValue) => value.IsEmpty ? string.Empty : value ? trueValue : falseValue;

		public static string MapBoolTo10(this ZBool value) => MapBoolToString(value, trueValue: "1", falseValue: "0");

		public static string MapBoolTo10(this bool value) => MapBoolToString(value, trueValue: "1", falseValue: "0");

		public static string MapBoolToJN(this ZBool value) => MapBoolToString(value, trueValue: "J", falseValue: "N");

		public static string MapBoolToJN(this bool value) => MapBoolToString(value, trueValue: "J", falseValue: "N");

		public static ZDecimal FormatDecimal(this ZDecimal input, int decimalPlaces)
		{
			return input.Round(decimalPlaces).Normalize();
		}
	}
}
