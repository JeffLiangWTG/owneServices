using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Enterprise.Customs.DE.Business
{
	public static class CusLineTariffDetailHelper
	{
		public static class KnownUoMs
		{
			public const string HLT = "HLT";
			public const string HLT6 = "HLT6";
		}

		public static class ExciseTypes
		{
			public const string _10 = "10";
			public const string _20 = "20";
			public const string _30 = "30";
			public const string _40 = "40";
			public const string _50 = "50";
		}

		public const string AttributeName = "ExciseType";

		internal static bool IsPercentAlcoholMandatory(CusLineTariffDetail tariffDetail)
		{
			var result = false;
			var universalTariff = tariffDetail.UniversalTariff;
			if (universalTariff != null)
			{
				result = universalTariff.GetAttributes(AttributeName).Any(attribute =>
				{
					if (percentAlcoholMandatoryMatrix.TryGetValue(attribute.ZZ3_Value, out var mandatoryUoMs))
					{
						return mandatoryUoMs.Contains(tariffDetail.BZ_UQ1);
					}
					return false;
				});
			}
			return result;
		}

		static ImmutableDictionary<string, HashSet<string>> percentAlcoholMandatoryMatrix => new Dictionary<string, HashSet<string>>
		{
			{ ExciseTypes._20, new HashSet<string> { KnownUoMs.HLT } },
			{ ExciseTypes._30, new HashSet<string> { KnownUoMs.HLT, KnownUoMs.HLT6 } },
			{ ExciseTypes._40, new HashSet<string> { KnownUoMs.HLT } },
			{ ExciseTypes._50, new HashSet<string> { KnownUoMs.HLT } }
		}.ToImmutableDictionary();
	}
}
