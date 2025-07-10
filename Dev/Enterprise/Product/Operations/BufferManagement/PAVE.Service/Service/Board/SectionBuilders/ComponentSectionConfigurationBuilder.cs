using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Service
{
	internal class ComponentSectionConfigurationBuilder : SectionConfigurationBuilder
	{
		internal ComponentSectionConfigurationBuilder(BMBoardSection section)
			: base(section, SectionType.Component)
		{ }

		BMComponentSectionConfiguration SectionConfig => Section.SectionConfiguration;
		BusinessObjectFactory Factory => Section.Factory;

		protected override ISectionConfiguration BuildCore()
		{
			return new ComponentSectionConfigurationDTO()
			{
				SectionPK = Section.PK.ToGuid(),
				Name = SectionConfig.SectionName,
				ComponentType = SectionConfig.IsBuffer ? ComponentType.Buffer : ComponentType.Bucket,
				Layout = CreateLayout(),
				Components = CreateComponents(),
				SubSections = SectionConfig.Subsections,
				CellsPerSubSection = SectionConfig.CellsPerSubsection,
				FlowDirection = SectionHelper.CreateFlowDirection(SectionConfig.FlowDirection),
				LastCellPosition = CreateLastCellPosition(),
				MaxOverdueSlots = SectionConfig.MaxOverdueSlots,
				FadeBackgroundAtPercentage = SectionConfig.FadeBackgroundAtPercentage,
				IsReleaseScheduler = SectionConfig.IsReleaseScheduler,
				EnableShowCurrentItemsFilterByDefault = SectionConfig.EnableShowCurrentItemsFilterByDefault,
				ShowZones = SectionConfig.ShowZones,
				BufferZonesColors = SectionConfig.IsBuffer ? CreateBufferZonesColors() : null,
				PrimaryChannels = CreateChannels(SectionConfig.OverrideChannels, SectionConfig.ChannelBy, SectionConfig.OrderedPrimaryAxisChannels),
				SecondaryChannels = CreateChannels(SectionConfig.OverrideSecondaryChannels, SectionConfig.ChannelSecondaryBy, SectionConfig.OrderedSecondaryAxisChannels),
				Cells = CreateCells(SectionConfig.PrimaryAxisChannels, SectionConfig.SecondaryAxisChannels),
				AcceptabilityBands = CreateAcceptabilityBands(),
				BackgroundColor = CreateBackgroundColor()
			};
		}

		#region Components

		IEnumerable<ComponentDTO> CreateComponents()
		{
			return Section.AllComponents
				.Select(component => new ComponentDTO
				{
					PK = component.PK.ToGuid(),
					Name = component.FC_Name
				});
		}

		#endregion

		#region LastCell

		LastCellPosition? CreateLastCellPosition()
		{
			switch (SectionConfig.LastCell)
			{
				case LastCellList.Codes.Bottom:
					return LastCellPosition.Bottom;
				case LastCellList.Codes.Left:
					return LastCellPosition.Left;
				case LastCellList.Codes.Right:
					return LastCellPosition.Right;
				case LastCellList.Codes.Top:
					return LastCellPosition.Top;
				default:
					return null;
			}
		}

		#endregion

		#region BackgroundColors

		Dictionary<int, string> CreateBufferZonesColors()
		{
			return Enumerable.Range(0, 4).ToDictionary(zone => zone, zone =>
			{
				var color = Section.GetZoneColor(zone, useDefaults: false);
				return color != Color.Empty ? color.ToHex() : null;
			});
		}

		string CreateBackgroundColor()
		{
			return !Section.BackgroundColor.IsEmpty
				? Section.BackgroundColorValue.ToHex()
				: null;
		}

		#endregion

		#region Channels

		SectionChannelsConfigurationDTO CreateChannels(bool overrideChannels, string channelBy, IEnumerable<BMBoardSectionChannel> sectionChannels)
		{
			var sectionChannelByType = SectionHelper.ToChannelTypeDTO(channelBy, overrideChannels);
			IEnumerable<ChannelDTO> channels;

			if (sectionChannelByType == ChannelType.NotChanneled)
			{
				channels = new ChannelDTO
				{
					EntityPK = Guid.Empty,
					Type = ChannelType.NotChanneled,
					Name = string.Empty
				}.WrapWithEnumerable().ToArray();
			}
			else
			{
				channels = sectionChannels.Select(channel => new ChannelDTO
				{
					EntityPK = channel.MSC_ParentID.IsEmpty ? Guid.Empty : channel.MSC_ParentID.ToGuid(),
					Type = SectionHelper.ToChannelTypeDTO(channel.MSC_ChannelType),
					Code = channel.MSC_ChannelType == ChannelTypeList.Codes.Resource ? GetChannelStaffCode(channel) : null,
					Name = GetChannelName(channel)
				}).ToArray();
			}

			return new SectionChannelsConfigurationDTO
			{
				ChannelBy = sectionChannelByType,
				Channels = channels
			};
		}

		string GetChannelStaffCode(BMBoardSectionChannel channel)
		{
			var bizo = channel.GetChannelBusinessObject(Factory);
			var staff = bizo as GlbStaff;

			return staff?.GS_Code;
		}

		string GetChannelName(BMBoardSectionChannel sectionChannel)
		{
			if (sectionChannel.MSC_ChannelType == ChannelTypeList.Codes.NotChanneled)
			{
				return BMConstants.UnchanneledDisplayName;
			}

			if (sectionChannel.MSC_ChannelType == ChannelTypeList.Codes.ReleaseSchedulerChannels)
			{
				//Will be implemented when implement Release Scheduler Channels in WAVE.
				return null;
			}

			if (sectionChannel.MSC_ParentID.IsEmpty)
			{
				return BMConstants.UnchanneledDisplayName;
			}

			var parentEntity = sectionChannel.GetChannelBusinessObject(Factory);

			switch (parentEntity)
			{
				case GlbStaff staff:
					return new ChannelNameBuilder(staff.GS_FullName, staff.GS_FriendlyName, staff.GS_Code).Build().GetDisplayableName(ChannelTypeList.Codes.Resource);
				case GlbGroup group:
					return group.GG_Desc;
				case GlbCapability capability:
					return capability.G4_Description;
				case TagMagnitude tag:
					return tag.DisplayText;
				default:
					return null;
			}
		}

		#endregion

		#region Cells

		IEnumerable<CellDTO> CreateCells(BMBoardSectionChannelCollection primaryChannels, BMBoardSectionChannelCollection secondaryChannels)
		{
			if (SectionConfig.IsReleaseScheduler)
			{
				//Will be implemented when implement Release Scheduler Channels in WAVE.
				return null;
			}

			var componentGridCells = new ComponentGrid(primaryChannels, secondaryChannels, Section, false, Section.IsInConstrainedMode)
				.CardCells
				.ToList();
			var minTimePercent = SectionConfig.IsBuffer ? componentGridCells.Min(c => c.TimePercent) : decimal.Zero;

			return componentGridCells.DistinctBy(cell => cell.TimeIndex)
				.Select(cell => new CellDTO
				{
					Name = cell.Label,
					IndexLowerBoundary = SectionConfig.IsBuffer ? Utilities.Round(cell.TimePercent - minTimePercent, 1) : cell.TimeIndex,
					Zone = cell.Zone
				})
				.OrderByDescending(c => c.IndexLowerBoundary)
				.ToList();
		}

		#endregion

		#region Acceptability Bands

		IEnumerable<AcceptabilityBandDTO> CreateAcceptabilityBands()
		{
			return SectionConfig.AcceptabilityBands
				.Cast<BoardSectionAcceptabilityBand>()
				.OrderBy(acceptabilityBand => acceptabilityBand.DisplaySequence)
				.Where(acceptabilityBand => acceptabilityBand.SelectedShowOnOption != AcceptabilityBandShowOnOption.Heading)
				.Select(acceptabilityBand => new AcceptabilityBandDTO
				{
					PK = acceptabilityBand.AcceptabilityBandPK.ToGuid(),
					DisplayName = acceptabilityBand.DisplayName,
					DisplayUnits = acceptabilityBand.DisplayUnits,
					BoundaryValues = new[] {
						acceptabilityBand.BoundaryValues.CautionMin,
						acceptabilityBand.BoundaryValues.GoodMin,
						acceptabilityBand.BoundaryValues.ExcellentMin,
						acceptabilityBand.BoundaryValues.ExcellentMax,
						acceptabilityBand.BoundaryValues.GoodMax,
						acceptabilityBand.BoundaryValues.CautionMax
					}
				});
		}

		#endregion
	}
}
