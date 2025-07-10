using System.Drawing;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;
using GridColor = Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.GridColor;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceRiskColorHelper
	{
		public static Color GetColorForRiskStatus(string status)
		{
			switch (status)
			{
				case Codes.Blocked:
				case Codes.PotentialRisk:
				case Codes.NotChecked:
				case Codes.Held:
				case Codes.HighRisk:
				case Codes.Incomplete:
				case Codes.Unknown:
					return GridColor.Block;
				case Codes.Released:
				case Codes.Clear:
					return GridColor.Clear;
				case Codes.OverrideClear:
					return GridColor.JobCleared;
				case Codes.PossibleRisk:
					return GridColor.PossibleRisk;
				case Codes.NotAssessed:
					return GridColor.NotAssessed;
				case Codes.NotApplicable:
					return GridColor.NotApplicable;
			}

			return GridColor.Undefined;
		}
	}
}
