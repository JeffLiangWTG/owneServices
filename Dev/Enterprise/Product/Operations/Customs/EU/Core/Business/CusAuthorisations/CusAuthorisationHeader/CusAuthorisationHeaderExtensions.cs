using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class CusAuthorisationHeaderExtensions
	{
		public static List<ZString> TypeThatHaveAGuidLOCRuleValueFrom => new List<ZString>
		{
			UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations,
		};
	}
}
