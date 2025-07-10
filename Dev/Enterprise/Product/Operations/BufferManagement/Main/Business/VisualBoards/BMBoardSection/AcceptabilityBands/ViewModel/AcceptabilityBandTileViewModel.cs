using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandTileViewModel : NonPersistentBusinessObject
	{
		public AcceptabilityBandTileViewModel(BoardSectionAcceptabilityBand band, BMBoardSectionViewModel sectionViewModel, AcceptabilityBandResult result)
		{
			SectionViewModel = sectionViewModel;

			AcceptabilityBandPK = band.AcceptabilityBandPK;
			BoardSectionAcceptabilityBandPK = band.PK;
			ReleaseGroupPK = band.BoardSection.SectionConfiguration.ApplicableReleaseGroupPK;

			DisplaySequence = band.DisplaySequence;
			DisplayName = AcceptabilityBandResult.GetDisplayName(band.DisplayName, result.AggregatedLabel);
			ResultUnits = band.DisplayUnits;
			ShouldFilterByReleaseGroup = band.FiltersByReleaseGroup;
			ShouldFilterBySection = band.FiltersBySection;

			var acceptabilityBand = band.AcceptabilityBand;
			if (acceptabilityBand != null)
			{
				BandName = acceptabilityBand.BAB_Name;
				BoundaryValues = band.BoundaryValues;
			}

			CalculationDuration = result.CalculationDuration;
			ResultToDisplayCalculationTimeUtc = result.AccurateAsOfTimeUtc;

			var resultText = new StringBuilder();

			if (result.Value.HasValue)
			{
				resultText.Append(result.Value.Value.FormatWithNoMoreThanTwoDecimalPlaces());
				if (!string.IsNullOrEmpty(ResultUnits))
				{
					resultText.Append(" ");
					resultText.Append(ResultUnits);
				}
			}

			Result = resultText.ToString();
			StatusText = result.Status.GetHeadlineText();
			Status = result.Status;
			IsResultPending = result.IsResultPending;
		}

		public BMBoardSectionViewModel SectionViewModel { get; }
		public ZGuid AcceptabilityBandPK { get; }
		public ZGuid BoardSectionAcceptabilityBandPK { get; }
		public ZGuid ReleaseGroupPK { get; }
		public ZInt DisplaySequence { get; }
		public ZString BandName { get; }
		public ZString DisplayName { get; }
		public ZString Result { get; }
		public ZString ResultUnits { get; }
		public TimeSpan CalculationDuration { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Calculated from a timespan")]
		public decimal CalculationTimeInSeconds => Utilities.Round(Convert.ToDecimal(CalculationDuration.TotalSeconds), 1);
		public ZDateTime ResultToDisplayCalculationTimeUtc { get; private set; }
		public ZDateTime ResultToDisplayCalculationTimeLocal => ResultToDisplayCalculationTimeUtc == ZDateTime.Empty ? ZDateTime.Empty : ResultToDisplayCalculationTimeUtc.ToDateTime().ToLocalTime();
		public bool IsResultPending { get; }
		public ZString StatusText { get; }
		public ComponentAcceptabilityStatus Status { get; }
		public AcceptabilityBandBoundaryValues BoundaryValues { get; }
		public AcceptabilityBandVisualizationOption ShouldFilterByReleaseGroup { get; }
		public AcceptabilityBandVisualizationOption ShouldFilterBySection { get; }

		#region For Test
#if DEBUG

		public void SetCalculationTimeAndDuration_ForTesting(TimeSpan calculationDuration, ZDateTime actualTimeUtc)
		{
			CalculationDuration = calculationDuration;
			ResultToDisplayCalculationTimeUtc = actualTimeUtc;
		}

		public AcceptabilityBandSqlBuilderParameters CreateParametersForCalculation_ForTest(BMComponentAcceptabilityBand band)
		{
			var parameters = new AcceptabilityBandSqlBuilderParameters(band, ShouldFilterByReleaseGroup, ShouldFilterBySection)
			{
				ReleaseGroupPK = ReleaseGroupPK,
				BoundaryValues = BoundaryValues ?? band.BoundaryValues,
				MaximumItems = 1,
			};

			return parameters;
		}
#endif
		#endregion
	}
}
