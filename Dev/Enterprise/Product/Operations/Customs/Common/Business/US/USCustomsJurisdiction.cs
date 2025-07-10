using System.Collections.Generic;

namespace Enterprise.Customs.Common.US
{
	public static class USCustomsJurisdiction
	{
		public static IEnumerable<string> Countries
		{
			get
			{
				yield return Core.Constants.CountryCodes.UnitedStates;
				yield return Core.Constants.CountryCodes.PuertoRico;
			}
		}
	}
}
