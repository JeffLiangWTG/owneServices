using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ChannelCapacity
	{
		public ChannelCapacity(IVisualBoardChannel channel, CellContent channelCell, BMBoardSection section, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap)
			: this(channel, channelCell, section, section.Component, viewModel, allocationMap)
		{
			var additionalComponentCapacities = section.AdditionalActiveComponents.Select(c => new ChannelCapacity(channel, channelCell, section, c, viewModel, allocationMap)).ToArray();

			if (IsChannelForRealResource(channel) && additionalComponentCapacities.Length > 0 && !section.SectionConfiguration.IsReleaseScheduler && section.Component.IsBuffer)
			{
				var capacities = new[] { this }.Concat(additionalComponentCapacities);
				var totalMessages = new[] { TotalCapacityMessage }.Concat(capacities.Select(c => FormatIndentedComponentHours(c.totalCapacityValue, c.totalCapacityForWorkInvolvingCCRValue, c.componentName, isInConstrainedMode)));
				totalCapacityMessage = string.Join(System.Environment.NewLine, totalMessages);

				var availableMessages = capacities.Select(c => FormatIndentedComponentHours(c.totalAvailableCapacityValue, c.totalAvailableCapacityForWorkInvolvingCCRValue, c.componentName, isInConstrainedMode));
				availableCapacityMessage = string.Join(System.Environment.NewLine, new[] { AvailableCapacityMessage }.Concat(availableMessages));

				calculationTimeMessage = string.Join(System.Environment.NewLine, capacities.Select(c => Res.GetString("5651ed41-50ca-4a14-9760-fee4392d4399", "Calculated at: {0} ({1})", c.calculatedTimeLocal.ToStandardDateTimeString(), c.componentName)));
			}
			else if (calculatedTimeLocal.IsValid)
			{
				calculationTimeMessage = Res.GetString("ca94607f-4200-48fa-bda6-d28b52850b7b", "Calculated at: {0}", calculatedTimeLocal.ToStandardDateTimeString());
			}

			var totalAllocatedMessages = new[] { totalAllocatedInChannelMessage }.Concat(additionalComponentCapacities.Select(c => c.totalAllocatedInChannelMessage));
			totalAllocatedInChannelMessage = string.Join(System.Environment.NewLine + System.Environment.NewLine, totalAllocatedMessages);
		}

		ChannelCapacity(IVisualBoardChannel channel, CellContent channelCell, BMBoardSection section, BMComponent component, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap)
		{
			Argument.NotNull(section, "section");
			Argument.NotNull(viewModel, "viewModel");
			Argument.NotNull(channelCell, "channelCell");

			componentName = component.FC_Name;
			channelName = channel != null ? channel.GetChannelName(DisplayNameType.FullName) : channelCell.Label;

			if (IsCapacityCalculationDisabled())
			{
				return;
			}

			if (IsChannelForRealOrUnChanneledResource(channel))
			{
				var resource = section.Factory.Load<GlbStaff>(channel.EntityPK);

				if (!section.SectionConfiguration.IsReleaseScheduler && component.IsBuffer && resource != null)
				{
					var capacityBreakdown = ResourceChannel.GetCapacity(viewModel.Cache, resource, component);

					isInConstrainedMode = viewModel.IsInConstrainedMode;
					calculatedTimeLocal = capacityBreakdown.CalculatedTimeUtc.ToLocalBranchTime(section.Factory);

					totalCapacityMessage = FormatTotalCapacityMessage(capacityBreakdown, isInConstrainedMode);
					availableCapacityMessage = FormatAvailableCapacityMessage(capacityBreakdown, isInConstrainedMode);

					var disableZoneMultiplier = ExperimentalSettingsProvider.ZoneMultipliersDisabled(component);

					totalAllocatedInChannelMessage = FormatAllocatedCapacityInComponentMessage(capacityBreakdown, component, disableZoneMultiplier);

					totalCapacityValue = capacityBreakdown.FullCapacity;
					totalCapacityForWorkInvolvingCCRValue = capacityBreakdown.FullCapacityForWorkInvolvingCCR;
					totalAvailableCapacityValue = capacityBreakdown.AvailableCapacity;
					totalAvailableCapacityForWorkInvolvingCCRValue = capacityBreakdown.AvailableCapacityForWorkInvolvingCCR;
					totalAllocatedInChannelValue = capacityBreakdown.UtilisedCapacity;

					if (disableZoneMultiplier && string.IsNullOrEmpty(zoneBreakdownDisabledMessage))
					{
						zoneBreakdownDisabledMessage = ZoneBreakdownDisabledMessage;
					}
				}
				else
				{
					var componentPks = section.SectionConfiguration.IsReleaseScheduler ? System.Array.Empty<ZGuid>() : new[] { component.PK };
					string groupName;

					if (!section.SectionConfiguration.IsReleaseScheduler && section.AdditionalActiveComponents.Any())
					{
						groupName = Res.GetString("3833ab24-0d77-4006-b61e-9f3cc82a7fab", "Total Assigned in {0}", componentName);
					}
					else
					{
						groupName = Res.GetString("194fa155-a526-40b4-b95e-05f7adbd4122", "Total Assigned");
					}

					totalAllocatedInChannelMessage = GetAllocatedTaskDurationForChannel(groupName, channelCell, section.SectionConfiguration, viewModel, allocationMap, componentPks);
				}
			}
			else
			{
				totalAllocatedInChannelMessage = GetAllocatedTaskDurationForChannel(Res.GetString("6b761363-b76f-48ac-9784-e85bb8a7f861", "{0} Total", componentName), channelCell, section.SectionConfiguration, viewModel, allocationMap, component.PK);
			}
		}

		#region Fields

		readonly string componentName;
		readonly string channelName;

		readonly string totalCapacityMessage;
		readonly string availableCapacityMessage;
		readonly string totalAllocatedInChannelMessage;
		readonly string zoneBreakdownDisabledMessage;
		readonly string calculationTimeMessage;

		readonly decimal totalCapacityValue;
		readonly decimal totalCapacityForWorkInvolvingCCRValue;
		readonly decimal totalAvailableCapacityValue;
		readonly decimal totalAvailableCapacityForWorkInvolvingCCRValue;
		readonly decimal totalAllocatedInChannelValue;

		readonly bool isInConstrainedMode;

		readonly ZDateTime calculatedTimeLocal;

		#endregion

		public decimal UtilisedCapacityPercent
		{
			get
			{
				if (totalCapacityValue == 0 && totalAllocatedInChannelValue == 0)
				{
					return 0m;
				}
				else
				{
					return totalCapacityValue == 0 ? 1m : totalAllocatedInChannelValue / totalCapacityValue;
				}
			}
		}

		public string Caption
		{
			get { return channelName; }
		}

		public string Message
		{
			get
			{
				return IsCapacityCalculationDisabled()
					? GetCapacityCalculationDisabledMessage()
					: string.Join(System.Environment.NewLine, GetCapacitySections());
			}
		}

		#region Implementation

		static string GetCapacityCalculationDisabledMessage()
		{
			var registryItemPath = BMSRegistry.Instance.DisableCapacityCalculations.GetLocation();
			return Res.GetString("c8c7a9af-eb67-4d4b-a2cc-c3ae101ff141", "Capacity unavailable because the [{0}] registry item is enabled.", registryItemPath);
		}

		static bool IsCapacityCalculationDisabled()
		{
			return BMSRegistry.Instance.DisableCapacityCalculations.Value;
		}

		IEnumerable<string> GetCapacitySections()
		{
			foreach (var message in new[] { totalCapacityMessage, totalAllocatedInChannelMessage, zoneBreakdownDisabledMessage, availableCapacityMessage, calculationTimeMessage })
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					yield return string.Format("{0}{1}", message, System.Environment.NewLine);
				}
			}
		}

		bool IsChannelForRealResource(IVisualBoardChannel channel)
		{
			return IsChannelForRealOrUnChanneledResource(channel) && channel.EntityPK.IsValid;
		}

		bool IsChannelForRealOrUnChanneledResource(IVisualBoardChannel channel)
		{
			return channel != null && channel.EntityType == ChannelTypeList.Codes.Resource;
		}

		#endregion

		#region Resource Message

		static string FormatTotalCapacityMessage(IResourceCapacity capacityBreakdown, bool isInConstrainedMode)
		{
			return FormatCapacityMessage(TotalCapacityMessage, capacityBreakdown.FullCapacity, isInConstrainedMode ? capacityBreakdown.FullCapacityForWorkInvolvingCCR : null);
		}

		static string FormatAvailableCapacityMessage(IResourceCapacity capacityBreakdown, bool isInConstrainedMode)
		{
			return FormatCapacityMessage(AvailableCapacityMessage, capacityBreakdown.AvailableCapacity, isInConstrainedMode ? capacityBreakdown.AvailableCapacityForWorkInvolvingCCR : null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static string FormatCapacityMessage(string headingText, decimal hours, decimal? hoursForWorkInvolvingCCR)
		{
			var result = new StringBuilder(headingText);

			result.AppendFormat(CultureInfo.InvariantCulture, " {0} {1}", hours.ToString(DecimalStringFormat, CultureInfo.InvariantCulture), HoursString);

			if (hoursForWorkInvolvingCCR.HasValue && hoursForWorkInvolvingCCR.Value != hours)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, " ({0})", GetHoursForWorkInvolvingCCRMessage(hoursForWorkInvolvingCCR.Value));
			}

			return result.ToString();
		}

		string FormatAllocatedCapacityInComponentMessage(IResourceCapacity breakdown, BMComponent component, bool disableZoneMultiplier)
		{
			var result = new StringBuilder(Res.GetString("d1fde3b9-7e7a-49d8-bcf4-90b824ed473d", "Allocated Capacity in {0}: {1} {2}", component.FC_Name, breakdown.UtilisedCapacity.ToString(DecimalStringFormat, CultureInfo.InvariantCulture), HoursString));

			if (!disableZoneMultiplier)
			{
				result.AppendLine();
				result.AppendLine(FormatAllocatedCapacityInZoneMessage(Res.GetString("ff98a9fc-7544-47db-85d6-23580d72828f", "Zone 0"), breakdown.GetZoneReservedCapacity(0), breakdown.GetZoneAllocatedCapacity(0)));
				result.AppendLine(FormatAllocatedCapacityInZoneMessage(Res.GetString("2a38223c-aeac-4a99-9fd4-d4579b951cbf", "Zone 1"), breakdown.GetZoneReservedCapacity(1), breakdown.GetZoneAllocatedCapacity(1)));
				result.AppendLine(FormatAllocatedCapacityInZoneMessage(Res.GetString("bf432107-0e87-41ed-b516-f9de3fc4f17f", "Zone 2"), breakdown.GetZoneReservedCapacity(2), breakdown.GetZoneAllocatedCapacity(2)));
				result.Append(FormatAllocatedCapacityInZoneMessage(Res.GetString("71da9a5a-717d-4981-831a-b452ba95eeea", "Zone 3"), breakdown.GetZoneReservedCapacity(3), breakdown.GetZoneAllocatedCapacity(3)));
			}
			return result.ToString();
		}

		static string FormatAllocatedCapacityInZoneMessage(string zone, decimal zoneReservedCapacity, decimal zoneAllocatedCapacity)
		{
			if (zoneAllocatedCapacity != zoneReservedCapacity)
			{
				return "    " + Res.GetString("e00346f0-70d8-4d5e-afad-53975aa0a683", "{0}: {1} {2} (consumed {3} {2})",
					/*0*/ zone,
					/*1*/ zoneAllocatedCapacity.ToString(DecimalStringFormat, CultureInfo.InvariantCulture),
					/*2*/ HoursString,
					/*3*/ zoneReservedCapacity.ToString(DecimalStringFormat, CultureInfo.InvariantCulture)
					);
			}
			else
			{
				return FormatIndentedComponentHours(zoneAllocatedCapacity, null, zone, isInConstrainedMode: false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static string FormatIndentedComponentHours(decimal hours, decimal? hoursForWorkInvolvingCCR, string name, bool isInConstrainedMode)
		{
			var result = "    " + Res.GetString("573717f9-df49-4e86-8b2f-d84fa3f55481", "{0}: {1} {2}", name, hours.ToString(DecimalStringFormat, CultureInfo.InvariantCulture), HoursString);

			if (isInConstrainedMode && hoursForWorkInvolvingCCR != null && hoursForWorkInvolvingCCR.Value != hours)
			{
				result += string.Format(CultureInfo.InvariantCulture, " ({0})", GetHoursForWorkInvolvingCCRMessage(hoursForWorkInvolvingCCR.Value));
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static string GetHoursForWorkInvolvingCCRMessage(decimal hours)
		{
			return Res.GetString("af43ef47-9a64-4039-9453-5eefaa343fb7", "{0} hours for work involving a CCR", hours.ToString(DecimalStringFormat, CultureInfo.InvariantCulture));
		}

		static string HoursString
		{
			get { return TimeConstants.TimeStrings.Hours; }
		}

		static string TotalCapacityMessage
		{
			get { return Res.GetString("a9736882-f8f9-4489-af7a-379c76e4948d", "Total Capacity:"); }
		}

		static string AvailableCapacityMessage
		{
			get { return Res.GetString("cc2a5941-5e06-49fe-9132-34e22280cbd2", "Available Capacity:"); }
		}

		static string ZoneBreakdownDisabledMessage => Res.GetString("93f5e22b-9860-4051-9c26-d953e67e7569", @"Please note: Capacity breakdowns by zone are not available because the simple capacity calculator is enabled for a Buffer Management System that this visual board is based on.
Enabling the simple capacity calculator provides a significant performance improvement and is recommended.
If capacity breakdowns by zone are required for your organization, please raise a CR5 incident for the Buffer Management (BUF) module and include the visual board name.");

		public const string DecimalStringFormat = "0.##";

		#endregion

		#region Non-Resource Message

		static string GetAllocatedTaskDurationForChannel(string groupName, CellContent channelCell, BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap, params ZGuid[] componentPks)
		{
			var allocatedCapacity = 0m;

			if (channelCell.ContentType == CellContentType.CCRHeading)
			{
				allocatedCapacity = GetAllocatedTaskDurationForCCRHeadings(channelCell, sectionConfiguration, viewModel, allocationMap, componentPks);
			}
			else
			{
				var primaryAxes = Lazy.Create(() =>
					channelCell.ContentType == CellContentType.ZoneHeading
					? viewModel.ComponentGrid.Cells.Where(cell => cell.Zone == channelCell.Zone).Select(cell => cell.PrimaryAxis).Distinct()
					: new[] { channelCell.PrimaryAxis });
				var secondaryAxes = Lazy.Create(() =>
					channelCell.ContentType == CellContentType.ZoneHeading
					? viewModel.ComponentGrid.Cells.Where(cell => cell.Zone == channelCell.Zone).Select(cell => cell.SecondaryAxis).Distinct()
					: new[] { channelCell.SecondaryAxis });

				var headerOrientation = channelCell.GetHeaderOrientation(sectionConfiguration);
				allocatedCapacity = headerOrientation == sectionConfiguration.OrientationValue
					? ComponentGrid.GetTaskEstimatesInCells(allocationMap, primaryAxes: primaryAxes.Value, components: componentPks)
					: ComponentGrid.GetTaskEstimatesInCells(allocationMap, secondaryAxes: secondaryAxes.Value, components: componentPks);
			}

			return string.Format(CultureInfo.InvariantCulture, "{0}: {1} {2}", groupName, allocatedCapacity.ToString(DecimalStringFormat, CultureInfo.InvariantCulture), HoursString);
		}

		#endregion

		#region CCR Headings Estimates

		static decimal GetAllocatedTaskDurationForCCRHeadings(CellContent channelCell, BMComponentSectionConfiguration sectionConfiguration, BMBoardSectionViewModel viewModel, CardAllocationMap allocationMap, params ZGuid[] componentPks)
		{
			var allocatedCapacity = 0m;

			var ccrHeadingCell = channelCell as CCRHeadingCellContent;

			if (ccrHeadingCell != null)
			{
				var ccrChannelsPrimaryAxes = GetCCRChannelsPrimaryAxes(sectionConfiguration.Section, viewModel, channelCell.PrimaryAxis);
				var ccrHeadingCells = viewModel.ComponentGrid.Cells.Where(c => c.PrimaryAxis == channelCell.PrimaryAxis);

				var ccrChannelsSecondaryAxes = ccrHeadingCells
					.Where(c => c.Zone == channelCell.Zone && (c.Zone == 2 || ((CCRHeadingCellContent)c).CCRStatus == ccrHeadingCell.CCRStatus))
					.Select(c => c.SecondaryAxis);

				allocatedCapacity = ComponentGrid.GetTaskEstimatesInCells(
					allocationMap,
					primaryAxes: ccrChannelsPrimaryAxes,
					secondaryAxes: ccrChannelsSecondaryAxes,
					components: componentPks);
			}

			return allocatedCapacity;
		}

		static IEnumerable<int> GetCCRChannelsPrimaryAxes(BMBoardSection section, BMBoardSectionViewModel viewModel, int ccrHeadingsPrimaryAxis)
		{
			return viewModel.ComponentGrid.Cells
				.Where(c => c.SecondaryAxis == 0 && c.PrimaryAxis > ccrHeadingsPrimaryAxis)
				.TakeWhile(c => c.Channel != null && c.Channel.IsCCRChannel(section))
				.Select(c => c.PrimaryAxis);
		}

		#endregion
	}
}
