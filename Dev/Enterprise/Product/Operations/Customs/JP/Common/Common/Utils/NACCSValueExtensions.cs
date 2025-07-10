using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common;

public static class NACCSValueExtensions
{
	public static ZString RemoveNonNumCharFromPhoneNumber(this ZString originalPhoneNumber, int maxLength = 14, bool isEDAOrIDA = false)
	{
		const string Prefix = "+";
		const string JapanCountryCodePrefix = "+81";

		var result = originalPhoneNumber;

		if (result.Length > maxLength)
		{
			if (isEDAOrIDA)
			{
				var hasJapanCountryCodePrefix = result.StartsWith(JapanCountryCodePrefix);
				result = result.KeepChars("+0123456789", "");
				result = result.Length > maxLength && hasJapanCountryCodePrefix ? result.Replace(JapanCountryCodePrefix, "0") : result;
			}
			else
			{
				var hasPrefix = result.StartsWith(Prefix);
				result = result.RemoveNonNumericCharacters().SubstringSafe(0, maxLength);
				result = result.Length < maxLength && hasPrefix ? Prefix + result : result;
			}
		}

		return result;
	}
}
