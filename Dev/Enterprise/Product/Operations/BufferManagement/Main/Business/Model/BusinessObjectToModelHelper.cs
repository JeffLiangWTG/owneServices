using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.PAVE.Common.Model;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Model = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Business
{
	public static class BusinessObjectToModelHelper
	{
		#region Tag

		public static Tag ToModel(this TagMagnitude tagMagnitude)
		{
			var color = tagMagnitude.GetColor();
			var colorHex = !color.IsEmpty && color.IsKnownColor ? color.ToHex() : null;

			return new Tag()
			{
				PK = tagMagnitude.PK.ToGuid(),
				Color = colorHex,
				Code = tagMagnitude.TGM_Code,
				BorderStyle = tagMagnitude.BorderStyle,
				Description = tagMagnitude.Description,
				DisplayText = tagMagnitude.DisplayText,
				ApplyColorToBorder = tagMagnitude.ApplyColorToBorder,
				VisualStylePriority = tagMagnitude.VisualStylePriority,
				ApplyColorToBackground = tagMagnitude.ApplyColorToBackground,
			};
		}

		#endregion

		#region Task

		public static Task ToModel(this ProcessTask processTask)
		{
			if (processTask == null || processTask.ProcessHeader == null)
			{
				return null;
			}

			(var estTimeToComplete, var stdEst, var lowEst, var highEst) =
				TaskDurationCalculator.ConvertDatesToMinutes(processTask.P9_EstimatedTimeToComplete, processTask.P9_EstDuration, processTask.P9_EstimateVariationFactor);

			return new Task
			{
				ID = processTask.P9_TaskID,
				PK = processTask.PK.ToGuid(),
				Type = processTask.P9_Type,
				Note = processTask.P9_CardNote,
				Status = processTask.P9_Status,
				Sequence = processTask.P9_Sequence,
				Description = processTask.LongDescription,
				ComponentPK = processTask.ProcessHeader.FH_FC_CurrentComponent.ToGuidSafe(),
				WorkflowPK = processTask.P9_FH_ProcessHeader.ToGuidSafe(),
				StaffCode = processTask.P9_GS_NKAssignedStaffMember,
				CapabilityPK = processTask.P9_G4_RequiredCapability.ToNunableGuidSafe(),
				GroupPK = processTask.P9_GG_AssignedGroup.ToNunableGuidSafe(),
				ReleaseGroupPK = processTask.ProcessHeader.FH_GG_ReleaseGroup.ToNunableGuidSafe(),
				LowEstimatedMinutes = lowEst,
				HighEstimatedMinutes = highEst,
				StandardEstimatedMinutes = stdEst,
				EstimatedMinutesToComplete = estTimeToComplete,
				EstimateVariationFactor = processTask.P9_EstimateVariationFactor,
				IsStartable = processTask.IsStartable_ExcludingReplenishment(),
			};
		}

		#endregion

		#region Staff

		public static Staff ToModel(this GlbStaff glbStaff)
		{
			if (glbStaff == null)
			{
				return null;
			}

			return new Staff
			{
				PK = glbStaff.PK.ToGuid(),
				Code = glbStaff.GS_Code,
				FullName = glbStaff.GS_FullName,
				FriendlyName = glbStaff.GS_FriendlyName,
				IsActive = glbStaff.GS_IsActive,
				GroupPKs = glbStaff.Groups.Select(g => g.PK.ToGuid()).ToArray(),
				Capabilities = glbStaff.Capabilities.Cast<GlbCapability>().Select(c => c.ToModel()).ToArray(),
			};
		}

		#endregion

		#region Capability

		public static Capability ToModel(this GlbCapability glbCapability)
		{
			if (glbCapability == null)
			{
				return null;
			}

			return new Capability
			{
				PK = glbCapability.PK.ToGuid(),
				Code = glbCapability.G4_Code,
				Name = glbCapability.G4_Description,
				Scope = Capability.GetCapabilityScope(glbCapability.G4_CapacityScope),
			};
		}

		#endregion

		#region ProcessHeader

		public static Model.Workflow ToModel(this ProcessHeader processHeader)
		{
			if (processHeader == null)
			{
				return null;
			}

			Job job = null;

			if (processHeader.Parent is ICodeDescription parentJob)
			{
				job = new Job
				{
					Code = parentJob.Code,
					Description = parentJob.Description
				};
			}

			IBufferedItem GetBufferedItem(IChildBufferedItem childBufferedItem)
			{
				if (childBufferedItem.UseParentBufferPenetration && childBufferedItem.Parent is IChildBufferedItem parent)
				{
					if (parent != null && !parent.IsClosed)
					{
						return GetBufferedItem(parent);
					}
				}

				return childBufferedItem;
			}

			var bufferedItem = GetBufferedItem(processHeader);

			return new Model.Workflow
			{
				PK = processHeader.PK.ToGuid(),
				Job = job,
				Nudge = processHeader.EffectiveNudge,
				JobPK = processHeader.FH_ParentId.ToGuidSafe(),
				Status = processHeader.FH_Status,
				ParentPK = processHeader.WorkflowParent?.PK.ToNunableGuidSafe(),
				Description = processHeader.FH_CompletionStatement,
				ComponentPK = processHeader.FH_FC_CurrentComponent.ToGuidSafe(),
				ReleaseGroupPK = processHeader.FH_GG_ReleaseGroup.ToNunableGuidSafe(),
				ReleaseDateTimeUTC = bufferedItem.StartableTime,
				PlannedDurationInMinutes = bufferedItem.PlannedDurationInMinutes,
				RemainingEstimateInMinutes = bufferedItem.RemainingEstimateInMinutes,
			};
		}

		#endregion

		#region Board

		public static Board ToModel(this BMBoard board)
		{
			if (board == null)
			{
				return null;
			}

			return new Board()
			{
				PK = board.PK.ToGuid(),
				Name = board.MB_Name,
				Sections = board.Sections
					.Where(s => s.Component == null || s.Component.FC_IsActive)
					.Select(s => s.ToModel()).ToArray(),
			};
		}

		public static ComponentSection ToModel(this BMBoardSection section)
		{
			var type = section.MS_SectionType;
			var sectionModel = new ComponentSection();

			if (section.Component != null && type == BMConstants.ComponentSectionType && section.SectionConfiguration != null)
			{
				DefaultChannelsProvider.RefreshChannels(section.SectionConfiguration, section.Factory);
				sectionModel = section.SectionConfiguration.ToModel();
			}

			sectionModel.PK = section.PK.ToGuid();
			sectionModel.BoardPK = section.MS_MB_Board.ToGuid();
			sectionModel.SectionType = type;
			sectionModel.Name = section.SectionName;
			sectionModel.Layout = new SectionLayout()
			{
				Row = section.Row,
				Column = section.Column,
				RowSpan = section.RowSpan,
				ColSpan = section.ColSpan,
				RowHeightPercent = section.RowHeightPercent,
				ColWidthPercent = section.ColWidthPercent,
			};

			return sectionModel;
		}

		static ComponentSection ToModel(this BMComponentSectionConfiguration config)
		{
			var section = config.Section;
			IBranchDepartmentProvider branchDepartmentProvider = section;

			return new ComponentSection()
			{
				Components = GetComponents(section.AllComponents),
				ComponentType = section.Component.FC_Type,
				IsBucket = config.IsBucket,
				IsBuffer = config.IsBuffer,
				BranchPK = branchDepartmentProvider.GetBranch(section.Factory)?.PK.ToNunableGuidSafe(),
				DepartmentPK = (branchDepartmentProvider.GetDepartment(section.Factory) as BusinessObject)?.PK.ToNunableGuidSafe(),
				ChannelBy = config.ChannelBy,
				OverrideChannels = config.OverrideChannels,
				Channels = GetChannels(config.OrderedPrimaryAxisChannels),
				SubSections = config.Subsections,
				CellsPerSubSection = config.CellsPerSubsection,
				FlowDirection = config.FlowDirection,
				LastCellPosition = config.LastCell,
				MaxOverdueSlots = config.MaxOverdueSlots,
				FadeBackgroundAtPercentage = config.FadeBackgroundAtPercentage,
				IsReleaseScheduler = config.IsReleaseScheduler,
				EnableShowCurrentItemsFilterByDefault = config.EnableShowCurrentItemsFilterByDefault,
				ShowZones = config.ShowZones,
				BufferZonesColors = GetBufferZonesColors(section),
				Cells = GetSectionCells(section),
				BackgroundColor = !section.BackgroundColor.IsEmpty ? section.BackgroundColorValue.ToHex() : null,
				HideCapabilityTasksFromResourceChannels = config.HideCapabilityTasksFromResourceChannels,
				HideResourceTasksFromCapabilityChannels = config.HideResourceTasksFromCapabilityChannels,
				ReleaseGroupPK = config.ReleaseGroupPK.ToNunableGuidSafe(),
				ShowWorkInReleaseGroupOnly = config.ShowWorkInReleaseGroupOnly,
				AcceptabilityBands = GetAcceptabilityBands(config),
			};
		}

		static IEnumerable<SectionComponent> GetComponents(IEnumerable<BMComponent> bmComponents)
		{
			return bmComponents
				.Select(c => new SectionComponent()
				{
					PK = c.PK.ToGuid(),
					Name = c.FC_Name,
					SizeInMinutes = c.FC_BufferTimespanInMinutes,
					OffsetInMinutes = c.FC_OffsetInMinutes
				}).ToArray();
		}

		static IEnumerable<Channel> GetChannels(IEnumerable<BMBoardSectionChannel> bmBoardSectionChannels)
		{
			return bmBoardSectionChannels
				.Select(c => new Channel(c.EntityPK.ToNunableGuidSafe(), GetChannelType(c.MSC_ChannelType)))
				.ToArray();
		}

		static Dictionary<int, string> GetBufferZonesColors(BMBoardSection section)
		{
			if (!section.SectionConfiguration.IsBuffer)
			{
				return new Dictionary<int, string>();
			}

			return Enumerable.Range(0, 4).ToDictionary(zone => zone, zone =>
			{
				var color = section.GetZoneColor(zone, useDefaults: false);
				return color != Color.Empty ? color.ToHex() : null;
			});
		}

		static ChannelType GetChannelType(string type)
		{
			switch (type)
			{
				case ChannelTypeList.Codes.Capability:
					return ChannelType.Capability;
				case ChannelTypeList.Codes.Group:
					return ChannelType.Group;
				case ChannelTypeList.Codes.Resource:
					return ChannelType.Resource;
				case ChannelTypeList.Codes.Tag:
					return ChannelType.Tag;
				default:
					return ChannelType.Undefined;
			}
		}

		static IEnumerable<SectionAcceptabilityBand> GetAcceptabilityBands(BMComponentSectionConfiguration config)
		{
			return config.AcceptabilityBands
				.Cast<BoardSectionAcceptabilityBand>()
				.OrderBy(acceptabilityBand => acceptabilityBand.DisplaySequence)
				.Where(acceptabilityBand => acceptabilityBand.SelectedShowOnOption != AcceptabilityBandShowOnOption.Heading)
				.Select(acceptabilityBand => new SectionAcceptabilityBand
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
				}).ToArray();
		}

		static IEnumerable<SectionCell> GetSectionCells(BMBoardSection section)
		{
			var config = section.SectionConfiguration;
			var componentGridCells =
				new ComponentGrid(config.PrimaryAxisChannels, config.SecondaryAxisChannels, section, isPreview: false, section.IsInConstrainedMode)
							.CardCells
							.ToArray();
			var minTimePercent = config.IsBuffer && componentGridCells.Length > 0 ? componentGridCells.Min(c => c.TimePercent) : decimal.Zero;
			var cells = componentGridCells
				.DistinctBy(cell => cell.TimeIndex)
				.Select(cell => new SectionCell
				{
					Name = cell.Label,
					IndexLowerBoundary = config.IsBuffer ? Utilities.Round(cell.TimePercent - minTimePercent, 1) : cell.TimeIndex,
					Zone = cell.Zone
				})
				.OrderByDescending(c => c.IndexLowerBoundary)
				.ToArray();
			return cells;
		}

		#endregion

		#region TypeConverters

		#region ToGuid

		static Guid? ToNunableGuidSafe(this ZGuid zGuid)
		{
			if (!zGuid.IsValid)
			{
				return null;
			}

			return zGuid.ToGuidSafe();
		}

		static Guid ToGuidSafe(this ZGuid zGuid)
		{
			if (zGuid.IsValid)
			{
				return zGuid.ToGuid();
			}

			return Guid.Empty;
		}

		#endregion

		#endregion
	}
}
