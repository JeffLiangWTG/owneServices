using System;
using System.Globalization;
using System.Text;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.DebuggerDisplay("{DisplayText} (ResultFound={ResultFound})")]
	public class AcceptabilityBandResult
	{
		public AcceptabilityBandResult(bool resultFound, decimal? value, string aggregatedLabel, ZGuid? bandPK = null)
		{
			BandPK = bandPK ?? ZGuid.Empty;
			ResultFound = resultFound;
			Value = value;
			AggregatedLabel = aggregatedLabel;
		}

		public ZGuid BandPK { get; }
		public string ContextKey { get; internal set; } // for debuging purposes
		public ComponentAcceptabilityStatus Status { get; internal set; }
		public AcceptabilityStatusPolarity? StatusPolarity { get; internal set; }
		public bool ResultFound { get; }
		public decimal? Value { get; private set; }

		public TimeSpan CalculationDuration { get; internal set; }

		public ZDateTime AccurateAsOfTimeUtc { get; internal set; }

#if DEBUG
		public void SetAccurateAsOfTimeUtc_ForTest(ZDateTime timeToSetTo)
		{
			AccurateAsOfTimeUtc = timeToSetTo;
		}
#endif

		public string AggregatedLabel { get; internal set; }

		public bool IsResultPending { get; internal set; }

		public string DisplayText => GetDisplayText(withPolarity: false);

		public void SetStatus(AcceptabilityBandBoundaryValues boundaryValues)
		{
			if (!ResultFound)
			{
				Status = ComponentAcceptabilityStatus.None;
			}
			else if (Value == null)
			{
				Status = ComponentAcceptabilityStatus.HighRisk;
			}
			else
			{
				ComponentAcceptabilityStatus status;
				AcceptabilityStatusPolarity polarity;
				var resultValue = Value.Value;

				if (resultValue >= boundaryValues.ExcellentMin && resultValue <= boundaryValues.ExcellentMax)
				{
					status = ComponentAcceptabilityStatus.Excellent;
					polarity = AcceptabilityStatusPolarity.Middle;
				}
				else if (resultValue >= boundaryValues.GoodMin && resultValue <= boundaryValues.GoodMax)
				{
					status = ComponentAcceptabilityStatus.Good;
					polarity = resultValue < boundaryValues.ExcellentMin ? AcceptabilityStatusPolarity.Low : AcceptabilityStatusPolarity.High;
				}
				else if (resultValue >= boundaryValues.CautionMin && resultValue <= boundaryValues.CautionMax)
				{
					status = ComponentAcceptabilityStatus.Caution;
					polarity = resultValue < boundaryValues.GoodMin ? AcceptabilityStatusPolarity.Low : AcceptabilityStatusPolarity.High;
				}
				else
				{
					status = ComponentAcceptabilityStatus.HighRisk;
					polarity = resultValue < boundaryValues.CautionMin ? AcceptabilityStatusPolarity.Low : AcceptabilityStatusPolarity.High;
				}

				Status = status;
				StatusPolarity = polarity;
			}
		}

		public static string GetDisplayName(string bandDisplayName, string aggregatedLabel)
		{
			if (string.IsNullOrEmpty(bandDisplayName))
			{
				return aggregatedLabel;
			}

			if (string.IsNullOrEmpty(aggregatedLabel))
			{
				return bandDisplayName;
			}

			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", bandDisplayName, aggregatedLabel);
		}

		string GetDisplayText(bool withPolarity)
		{
			var result = new StringBuilder(Status.GetStatusText());

			if (withPolarity)
			{
				if (StatusPolarity.HasValue && StatusPolarity != AcceptabilityStatusPolarity.Middle)
				{
					result.AppendFormat(CultureInfo.InvariantCulture, " ({0})", GetPolarityText(StatusPolarity.Value));
				}
				else if (Status == ComponentAcceptabilityStatus.HighRisk && !Value.HasValue)
				{
					result.Append(" " + Res.GetString("c6082e81-4e64-4135-b6e0-d9c06a68f638", "(due to no value being calculated)"));
				}
			}

			if (Status != ComponentAcceptabilityStatus.None && Value.HasValue)
			{
				result.Append(": ");
				result.Append(Value.Value.ToString("0.00", CultureInfo.CurrentCulture));
			}

			return result.ToString();
		}

		static string GetPolarityText(AcceptabilityStatusPolarity polarity)
		{
			switch (polarity)
			{
				case AcceptabilityStatusPolarity.High:
					return Res.GetString("78dc595c-7f07-4ebd-baea-d3be13d64719", "upper");

				case AcceptabilityStatusPolarity.Low:
					return Res.GetString("be94953a-0345-49be-ad61-277863e54604", "lower");

				default:
					throw new ArgumentException("Invalid polarity", nameof(polarity));
			}
		}

		internal static AcceptabilityBandResult CreateEmptyResultForPendingCalculation()
		{
			return new AcceptabilityBandResult(false, null, null) { IsResultPending = true };
		}
	}
}
