using System;
using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public static class ZStringExtensions
	{
		public static ZString WrapInDoubleQuotes(this ZString value)
		{
			return ZString.Format("\"{0}\"", value);
		}

		public static ZString TrimOneDoubleQuoteFromEachEnd(this ZString value)
		{
			int trimStartAmount = value.Length > 1 && value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
			int trimEndAmount = value.Length > 1 && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
			return value.Substring(trimStartAmount, value.Length - trimStartAmount - trimEndAmount);
		}
	}
}
