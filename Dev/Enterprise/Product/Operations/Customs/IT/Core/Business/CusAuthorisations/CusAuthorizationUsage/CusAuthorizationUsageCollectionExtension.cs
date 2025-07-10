using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.IT.Business;

internal static class CusAuthorizationUsageCollectionExtension
{
	public static bool HasAuthorizationOfType(this IEnumerable<CusAuthorizationUsage> authorizations, string type)
	{
		return authorizations.Any(x => x.AGC_Code == type);
	}
}
