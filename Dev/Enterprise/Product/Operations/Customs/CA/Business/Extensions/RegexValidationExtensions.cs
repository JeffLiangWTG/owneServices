using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public static class RegexValidationExtensions
	{
		public static bool HasCharactersNotSupportedByCAMessaging(this ZString value)
		{
			return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, DisallowedCharactersPattern);
		}

		public static bool HasInvalidSymbolsForCCN(this ZString value)
		{
			return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, "[^- A-Za-z0-9]");
		}

		public const string DisallowedCharactersPattern = @"[^"" #$%&'^()*+,-./;:<=>?@\[\]_`{}~A-Za-z0-9]";
	}
}
