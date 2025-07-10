using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class ConversionExtensions
	{
		public static string ToUTF8FromBase64(this ZString base64String) =>
			((string)base64String).ToUTF8FromBase64();

		public static string ToUTF8FromBase64(this string base64String)
		{
			var data = Convert.FromBase64String(base64String);
			return MessageEncoding.UTF8WithoutBOM.GetString(data);
		}

		public static string FormatDecimals(this ZDecimal dm, string formatString)
			   => dm.ToString(formatString, CultureInfo.InvariantCulture);

		public static string FormatNullableDate(this ZDateTime? dt, string formatString)
		   => dt.HasValue ? dt.Value.ToString(formatString, CultureInfo.InvariantCulture) : string.Empty;

		public static string FallbackOnNullOrEmpty(this string s, string fallback) => !string.IsNullOrEmpty(s) ? s : fallback;

		public static string FallbackOnNullOrEmpty(this string s, Func<string> fallback) => !string.IsNullOrEmpty(s) ? s : fallback();

		public static bool IsTrue(this string s)
			=> s.FallbackOnNullOrEmpty(string.Empty).Trim().Equals((NoResString)"true", StringComparison.OrdinalIgnoreCase); // Bool Comparison
	}
}
