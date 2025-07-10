using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.BE.Business.Declaration;

public static class ChargeProviderHelper
{
	public const string AnyIncoTerms = "";

	public static bool IsListed(this IEnumerable<(string, string)> definationList, string incoToCheck, string chargeTypeToCheck)
	{
		var result = false;
		result = definationList.Any(((string inco, string charge) listed) => (string.IsNullOrEmpty(listed.inco) || incoToCheck == listed.inco) && (listed.charge == chargeTypeToCheck));
		return result;
	}
}
