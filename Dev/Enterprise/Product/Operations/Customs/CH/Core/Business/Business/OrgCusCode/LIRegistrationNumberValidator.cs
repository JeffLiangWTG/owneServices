using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

internal class LIRegistrationNumberValidator
{
	internal static bool IsValidVATNumber(ZString vatNo)
	{
		return Regex.IsMatch(vatNo, "^[0-9]{5}$");
	}
}
