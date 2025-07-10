using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public static class ChannelHeadingViewModelBuilder
	{
		#region Constants

		static Color GoodColor
		{
			get { return BMConstants.GoodBoardColor.FadeTowardsWhite(fadeFactor: 2.5f); }
		}

		static Color HighRiskColor
		{
			get { return BMConstants.HighRiskBoardColor.FadeTowardsWhite(fadeFactor: 2.5f); }
		}

		static Color OvertimeColor
		{
			get { return BMConstants.CautionBoardColor.FadeTowardsWhite(fadeFactor: 2.5f); }
		}

		public static decimal GradientSizePercent
		{
			get { return 0.1m; }
		}

		#endregion

		#region API
		public static ChannelHeadingViewModelSet MakeViewModelSet(this IEnumerable<IVisualBoardChannel> channels, BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap, PropertyCache cache = null)
		{
			var comparer = new IVisualBoardChannelEqualityComparer();
			var distinctChannels = channels.Distinct(comparer).ToArray();
			var roadRunnerDetails = viewModel.PopulateRoadRunnerDetails(factory, cache ?? viewModel.Cache, allocationMap, distinctChannels, viewModel.ReleaseGroupPK);

			var ccrStatuses = GetCcrCandidancies(distinctChannels, comparer, viewModel, factory);
			var viewModels = distinctChannels.ToImmutableDictionary(key => key, key => MakeViewModel(comparer, key, ccrStatuses, factory, viewModel, allocationMap), comparer);
			return new ChannelHeadingViewModelSet(viewModels, roadRunnerDetails);
		}

		#endregion

		#region Implementation

		static ChannelHeadingViewModel MakeViewModel(IEqualityComparer<IVisualBoardChannel> comparer, IVisualBoardChannel channel, Dictionary<IVisualBoardChannel, ChannelHeadingViewModel.ThumbnailViewModel> ccrThumbnails, BusinessObjectFactory factory, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap)
		{
			var section = BMBoardSection.Load(factory, viewModel.SectionPK);
			var headingCell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => comparer.Equals(c.Channel, channel) && c.ContentType == CellContentType.ChannelHeading) ?? GetDummyCell(channel, section);

			var thumbnails = ccrThumbnails[channel];
			var background = GetBackgroundViewModel(allocationMap, section, viewModel, channel, headingCell);
			var status = new ChannelHeadingViewModel.StatusViewModel(channel.EntityType == ChannelTypeList.Codes.Resource, channel.Status, channel.ToolTipStatus, channel.RiskComponentPK);

			return new ChannelHeadingViewModel(thumbnails, background, status);
		}

		/// <summary>
		/// This is a hack to ensure that poorly configured tests do not explode here. I am very sorry. :(
		/// </summary>
		static CellContent GetDummyCell(IVisualBoardChannel channel, BMBoardSection section)
		{
			return new CellContent(0, 0, CellContentType.ChannelHeading, section?.SectionConfiguration) { Channel = channel };
		}

		static Dictionary<IVisualBoardChannel, ChannelHeadingViewModel.ThumbnailViewModel> GetCcrCandidancies(IVisualBoardChannel[] channels, IVisualBoardChannelEqualityComparer comparer, BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			var result = new Dictionary<IVisualBoardChannel, ChannelHeadingViewModel.ThumbnailViewModel>(channels.Length, comparer);
			var staffChannels = channels.Where(c => c.EntityType == ChannelTypeList.Codes.Resource && !string.IsNullOrEmpty(c.ChannelEntityCode)).ToDictionary(s => s.ChannelEntityCode);
			var service = factory.ServiceContainer.GetService<CapacityConstrainedResourcesCacheService>();

			if (service != null)
			{
				var statuses = staffChannels.Keys.Select(s => service.GetCapacityConstrainedResourceStatus(s, viewModel.ComponentPK));

				foreach (var link in statuses)
				{
					if (link.IsDesignatedCCR)
					{
						var channel = staffChannels[link.StaffCode];
						result[channel] = new ChannelHeadingViewModel.ThumbnailViewModel(channel.StatusImage, link.TimeConsideredCCR, CCRCandidacy.Designate, link.IsPersistentlyOverloaded);
					}
					else if (link.IsPersistentlyOverloaded && link.CapacityConstraintDetectedUtc.IsValid)
					{
						var channel = staffChannels[link.StaffCode];
						result[channel] = new ChannelHeadingViewModel.ThumbnailViewModel(channel.StatusImage, link.TimeConsideredCCR, CCRCandidacy.Candidate, link.IsPersistentlyOverloaded);
					}
				}
			}
			else
			{
				ErrorReporter.ReportOnce(nameof(CapacityConstrainedResourcesCacheService) + " was not present in the factory's ServiceContainer. Factory: " + factory.NameForDebugging);
			}

			foreach (var channel in channels.Where(c => !result.ContainsKey(c)))
			{
				result[channel] = new ChannelHeadingViewModel.ThumbnailViewModel(channel.StatusImage, ZString.Empty, CCRCandidacy.None, false);
			}

			return result;
		}

		static ChannelHeadingViewModel.BackgroundViewModel GetBackgroundViewModel(CardAllocationMap allocationMap, BMBoardSection section, BMBoardSectionViewModel viewModel, IVisualBoardChannel channel, CellContent cell)
		{
			if (viewModel.IsBuffer && channel.EntityType == ChannelTypeList.Codes.Resource && channel.EntityPK.IsValid && !IsCapacityCalculationDisabled())
			{
				var utilisedCapacityPercent = Math.Max(0m, new ChannelCapacity(channel, cell, section, viewModel, allocationMap).UtilisedCapacityPercent);
				var fadeStartColor = GetBackgroundFadeColor(utilisedCapacityPercent, section, allocationMap, channel, viewModel);
				var fadeStartPosition = Math.Max(0m, utilisedCapacityPercent - (GradientSizePercent / 2.0m));
				var fadePercent = (float)Math.Min(1m, fadeStartPosition);

				return new ChannelHeadingViewModel.BackgroundViewModel(fadeStartColor, fadePercent, utilisedCapacityPercent > 0);
			}
			else
			{
				return new ChannelHeadingViewModel.BackgroundViewModel(GetNonFadeBackgroundColor(channel, viewModel.IsBuffer), channel.ForegroundColor);
			}
		}

		static Color? GetBackgroundFadeColor(decimal utilisedCapacityPercent, BMBoardSection section, CardAllocationMap allocationMap, IVisualBoardChannel channel, BMBoardSectionViewModel viewModel)
		{
			if (utilisedCapacityPercent == 0m)
			{
				return null;
			}
			if (utilisedCapacityPercent > BMConstants.ResourceCapacityUtilisationOverloadFactor || channel.IsHighRisk)
			{
				return HighRiskColor;
			}
			else if (channel.IsOvertime)
			{
				return OvertimeColor;
			}

			var workingZone = viewModel.ComponentGrid.GetChannelZone(channel, section, allocationMap);
			if (workingZone != null)
			{
				switch (workingZone.Value)
				{
					case 3:
					case 2:
						return GoodColor;

					case 1:
					case 0:
						if (!channel.IsCCRChannel(section, viewModel.IsInConstrainedMode) || channel.IsHighRisk)
						{
							return HighRiskColor;
						}
						else if (channel.IsCCRChannel(section, viewModel.IsInConstrainedMode) && !channel.IsHighRisk)
						{
							return GoodColor;
						}
						break;
				}
			}

			return null;
		}

		static Color GetNonFadeBackgroundColor(IVisualBoardChannel channel, bool isBuffer)
		{
			if (isBuffer && channel.IsOvertime && IsCapacityCalculationDisabled())
			{
				return BMConstants.CautionBoardColor;
			}

			return channel.BackgroundColor;
		}

		static bool IsCapacityCalculationDisabled()
		{
			return BMSRegistry.Instance.DisableCapacityCalculations.Value;
		}

		#endregion
	}
}
