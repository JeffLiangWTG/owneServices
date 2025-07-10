using System.Drawing;
using System.Globalization;
using System.Text;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobNetworkNodeViewModel : NodeViewModel
	{
		internal JobNetworkNodeViewModel(INetworkEntity entity, NetworkViewModel networkViewModel)
			: base(entity, networkViewModel)
		{
		}

		public override ProgressBar LowerBar
		{
			get
			{
				var estimateDetails = Entity is ShapeNetworkEntity s ? s.GetEstimateDetails() : null;
				
				if (estimateDetails is null)
				{
					return base.LowerBar;
				}

				var color = GetFadeBackgroundColor(estimateDetails.RiskReason);
				return new ProgressBar(color, (double)estimateDetails.PercentComplete);
			}
		}

		public override string LowerBarTooltip
		{
			get
			{
				var estimateDetails = Entity is ShapeNetworkEntity s ? s.GetEstimateDetails() : null;

				if (estimateDetails != null)
				{
					var result = new StringBuilder(Res.GetString("4e8ff739-5466-4bba-8f53-4f53a622dcbf", "{0}% complete ({1} of {2} complete)",
						Utilities.Round(estimateDetails.PercentComplete * 100m, 0),
						FormatHoursValue(estimateDetails.CompletedEstimateHours),
						FormatHoursValue(estimateDetails.TotalEstimateHours)
						));

					if (estimateDetails.RiskReason != RiskReason.None)
					{
						result.AppendLine();

						if (estimateDetails.RiskReason == RiskReason.OverEstimate)
						{
							result.Append(Res.GetString("6d425e7b-19bb-41b5-83d4-a7779e629489", "WARNING: This entity has already taken longer to complete than its planned duration"));
						}
						else if (estimateDetails.RiskReason == RiskReason.LateRelease)
						{
							result.Append(Res.GetString("0f76add1-7f25-48e3-935f-717c11cfdadb", "WARNING: This entity is scheduled to have started and is startable but has not been released to a buffer"));
						}
					}

					return result.ToString();
				}
				else
				{
					return null;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		static string FormatHoursValue(decimal hoursValue)
		{
			return Utilities.Round(hoursValue, 2).ToString(ChannelCapacity.DecimalStringFormat, CultureInfo.InvariantCulture) + " " + (hoursValue == 1m ? TimeConstants.TimeStrings.Hour : TimeConstants.TimeStrings.Hours);
		}

		static Color GetFadeBackgroundColor(RiskReason riskReason)
		{
			if (riskReason == RiskReason.None)
			{
				return Color.LightGray;
			}
			else
			{
				return RiskColor;
			}
		}

		public static Color RiskColor => Color.Orange;
	}
}
