using System.Drawing;
using System.Linq;
using CargoWise.PAVE.Common.DTO;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentAcceptabilityStatusDetails
	{
		public ComponentAcceptabilityStatusDetails(BoardSectionAcceptabilityBandResult[] results)
		{
			Build(results);
		}

		public ComponentAcceptabilityStatus Status { get; private set; }
		public string Headline { get; private set; }
		public string Details { get; private set; }
		public Color BackgroundColor { get; private set; }
		public Color ForegroundColor { get; private set; }

		#region Implementation

		void Build(BoardSectionAcceptabilityBandResult[] results)
		{
			if (results.Any())
			{
				results = results.OrderByDescending(x => x.Result.Status).ThenBy(x => x.SectionBand.DisplayName).ToArray();
				Status = results.First().Result.Status;

				if (Status != ComponentAcceptabilityStatus.None)
				{
					Headline = Status.GetHeadlineText();
					Details = string.Join(System.Environment.NewLine, results.Where(x => x.Result.Status != ComponentAcceptabilityStatus.None).Select(x => GetResultDetails(x.SectionBand, x.Result)));
					BackgroundColor = Status.GetBackgroundColor();
					ForegroundColor = Status.GetForegroundColor();
				}
			}
		}

		static string GetResultDetails(IAcceptabilityBandOverride sectionBand, AcceptabilityBandResult result)
		{
			var resultValue = result.Value == null
					? Res.GetString("546dad8f-426d-4ec7-a8e9-2720b8e3ec8e", "could not determine result")
					: result.Value.Value.FormatWithNoMoreThanTwoDecimalPlaces();

			var boundaryValues = sectionBand.BoundaryValues;

			return Res.GetString("ff2cbfc7-fad2-412d-b83f-b09350f3ad38", "{0}: {1}: {2} (target is between {3} and {4})",
				/*0*/ result.Status.GetStatusText(),
				/*1*/ AcceptabilityBandResult.GetDisplayName(sectionBand.DisplayName, result.AggregatedLabel),
				/*2*/ resultValue,
				/*3*/ boundaryValues.CautionMin,
				/*4*/ boundaryValues.CautionMax);
		}

		#endregion

		#region SectionSubHeadingAppearence

		public ISectionSubHeadingAppearance HeadingAppearance
		{
			get { return new ComponentSubHeadingAppearence(this); }
		}

		// A thread safe representation of the results.
		class ComponentSubHeadingAppearence : ISectionSubHeadingAppearance
		{
			internal ComponentSubHeadingAppearence(ComponentAcceptabilityStatusDetails details)
			{
				SectionSubHeading = details.Headline;
				SectionSubHeadingDetailText = details.Details;
				SectionHeadingBackgroundColor = details.BackgroundColor;
				SectionHeadingForegroundColor = details.ForegroundColor;
			}

			public string SectionSubHeading { get; }

			public string SectionSubHeadingDetailText { get; }

			public Color SectionHeadingBackgroundColor { get; }

			public Color SectionHeadingForegroundColor { get; }
		}

		#endregion
	}
}
