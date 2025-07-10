using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class StringInternerExtension
	{
		public static ZString InternValue(this StringInterner stringInterner, ZString value)
		{
			if (value.IsEmpty)
			{
				return value;
			}
			var rawString = value.ToString();
			var internValue = stringInterner.InternValue(rawString);
			return !object.ReferenceEquals(rawString, internValue) ? new ZString(internValue) : value;
		}

		public static object InternValue(this StringInterner stringInterner, object value)
		{
			var stringValue = value as string;
			if (stringValue != null)
			{
				return stringInterner.InternValue(stringValue);
			}
			else if (value is ZString)
			{
				return stringInterner.InternValue((ZString)value);
			}
			else
			{
				return value;
			}
		}
	}
}
