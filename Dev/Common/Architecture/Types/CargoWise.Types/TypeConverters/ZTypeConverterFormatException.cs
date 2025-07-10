using System;
using CargoWise.Common;

namespace CargoWise.Types
{
	public static class Extensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void RethrowForConvertFrom(this FormatException ex, object value)
		{
			Argument.NotNull(ex, nameof(ex));
			throw new FormatException(ex.Message + " (in ConvertFrom, value = " + FormatValue(value) + ")", ex);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void RethrowForConvertTo(this FormatException ex, object value, Type destinationType)
		{
			Argument.NotNull(ex, nameof(ex));
			throw new FormatException(ex.Message + " (in ConvertTo, value = " + FormatValue(value) + "; originalType = '" + value?.GetType() + "'; destinationType = '" + destinationType + "')", ex);
		}

		public static void ReThrowInvalidCastExceptionForConvertTo(this InvalidCastException ex, object value, Type destinationType)
		{
			Argument.NotNull(ex, nameof(ex));
			throw new InvalidCastException(ex.Message + " (in ConvertTo, value = " + FormatValue(value) + "; value.GetType() = '" + value?.GetType() + "'; destinationType = '" + destinationType + "')", ex);
		}

		static string FormatValue(object value)
		{
			string result;
			if (value == null)
			{
				result = "null";
			}
			else if (value is DBNull)
			{
				result = "DBNull.Value";
			}
			else
			{
				result = "'" + value.ToString() + "'";
			}
			return result;
		}
	}
}
