using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	static class NctsDepartureMovementHeaderPhase5RuleG0789Validation
	{
		public static void CheckCustomsOfficeAtBorderRuleG0789(ZPropertyInfo targetInfo, ZString customsOfficeAtBorder, NctsHeader header, string message)
		{
			if (header.CommonMovementHeader?.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleG0789_1Active: true })
			{
				CheckRuleG0789(targetInfo, customsOfficeAtBorder, header, message);
			}
		}

		public static void CheckCustomsOfficeAtBorderRuleG0789(ZPropertyInfo targetInfo, ZString customsOfficeAtBorder, DepartureCusTransportMeans transportMeans, string message)
		{
			if (transportMeans.ValidationDecider is IDepartureCusTransportMeansPhase5ValidationDecider { IsRuleG0789_1Active: true })
			{
				CheckRuleG0789(targetInfo, customsOfficeAtBorder, transportMeans.Header, message);
			}
		}

		static void CheckRuleG0789(ZPropertyInfo targetInfo, ZString customsOfficeAtBorder, NctsHeader header, string message)
		{
			if (customsOfficeAtBorder.IsEmpty)
			{
				return;
			}

			var effectiveCustomsOfficeCodes = GetEffectiveCustomsOfficeCodes(header);
			if (effectiveCustomsOfficeCodes.Count > 0
				&& !effectiveCustomsOfficeCodes.Contains(customsOfficeAtBorder))
			{
				targetInfo.AddMessageError(message);
			}
		}

		static HashSet<ZString> GetEffectiveCustomsOfficeCodes(NctsHeader header)
		{
			var result = new HashSet<ZString>();
			if (header.CommonMovementHeader is NctsCommonMovementHeader commonMovementHeader)
			{
				result.UnionWith(commonMovementHeader.TransitCustomsOfficeCodeList.Select(x => x.OfficeCode));
				result.UnionWith(commonMovementHeader.ExitForTransitCustomsOfficeCodeList.Select(x => x.OfficeCode));
				result.Add(commonMovementHeader.DestinationCustomsOfficeCode);
				result.RemoveWhere(x => x.IsEmpty);
			}
			return result;
		}
	}
}
