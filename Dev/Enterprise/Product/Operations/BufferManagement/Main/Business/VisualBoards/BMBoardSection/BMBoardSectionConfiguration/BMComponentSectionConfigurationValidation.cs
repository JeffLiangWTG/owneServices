using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentSectionConfigurationValidation : BMBoardSectionConfigurationWithOverridableSectionNameValidation
	{
		public BMComponentSectionConfigurationValidation(BMComponentSectionConfiguration sectionConfiguration)
			: base(sectionConfiguration)
		{
			Parent = sectionConfiguration;
		}

		readonly BMComponentSectionConfiguration Parent;

		public void ValidateSubsections()
		{
			ValidateCalculatedProperty(Parent.SubsectionsInfo);
		}

		protected void CheckSubsections()
		{
			if (Parent.IsReleaseScheduler)
			{
				if (Parent.Subsections != 1)
				{
					Parent.SubsectionsInfo.AddError(Res.GetString("3b5b57b0-510d-4aba-9b8f-fe3d654aa66e", "Must be only one Subsection on a Release Scheduler board section."));
				}
			}
			else
			{
				CompareValidation.CheckGreaterThanOrEqualTo(Parent.SubsectionsInfo, 1);
			}
			ValidateCellsPerSubsection();
		}

		public void ValidateFlowDirection()
		{
			ValidateCalculatedProperty(Parent.FlowDirectionInfo);
		}

		protected void CheckFlowDirection()
		{
			MandatoryValidation.CheckEntered(Parent.FlowDirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FlowDirectionInfo);
		}

		public void ValidateTimeProgressionMode()
		{
			ValidateCalculatedProperty(Parent.TimeProgressionModeInfo);
		}

		protected void CheckTimeProgressionMode()
		{
			MandatoryValidation.CheckEntered(Parent.TimeProgressionModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TimeProgressionModeInfo);

			if (Parent.Section.Component != null && Parent.Section.Component.FC_Type == BMComponentTypeList.Codes.Buffer && Parent.TimeProgressionMode != TimeProgressionModeList.Codes.Age)
			{
				Parent.TimeProgressionModeInfo.AddError(Res.GetString("dce3f6d3-1df1-44ef-a9cd-3c8ea4c260aa", "Only the Age Time Progression Mode is valid for a buffer."));
			}
		}

		public void ValidateTimeField()
		{
			ValidateCalculatedProperty(Parent.TimeFieldInfo);
		}

		protected void CheckTimeField()
		{
			MandatoryValidation.CheckEntered(Parent.TimeFieldInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TimeFieldInfo);

			if (Parent.TimeProgressionMode == TimeProgressionModeList.Codes.Due && Parent.TimeField == TimeProgressionFieldList.Codes.WorkingTimeSinceStartable)
			{
				Parent.TimeFieldInfo.AddError(Res.GetString("02FCD363-4C7F-4E7B-B3B9-A7AB64AEF5D7", "Only the Age Time Progression Mode is valid for Working Time Since Task Became Startable."));
			}
		}

		public void ValidateMaxOverdueSlots()
		{
			ValidateCalculatedProperty(Parent.MaxOverdueSlotsInfo);
		}

		protected void CheckMaxOverdueSlots()
		{
			if (Parent.IsBucket)
			{
				CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.MaxOverdueSlotsInfo, Parent.CellsPerSubsectionInfo);
				CompareValidation.CheckGreaterThanOrEqualTo(Parent.MaxOverdueSlotsInfo, 0);
			}
			else if (Parent.MaxOverdueSlots != 0)
			{
				if (Parent.IsBuffer)
				{
					Parent.MaxOverdueSlotsInfo.AddError(Res.GetString("782d4de7-d421-4f94-abc9-354de149874d", "There should be no Overdue Slots specified for a Buffer component."));
				}
				else if (Parent.IsReleaseScheduler)
				{
					Parent.MaxOverdueSlotsInfo.AddError(Res.GetString("d39e2844-d1a3-4e32-b226-8916fe4147a8", "There should be no Overdue Slots specified for a Release Scheduler board section."));
				}
			}
		}

		public void ValidateLastCell()
		{
			ValidateCalculatedProperty(Parent.LastCellInfo);
		}

		protected void CheckLastCell()
		{
			if (Parent.IsWrapped)
			{
				MandatoryValidation.CheckEntered(Parent.LastCellInfo);
				ListValidation.ErrorIfInvalidCode(Parent.LastCellInfo);
			}
		}

		public void ValidateCellsPerSubsection()
		{
			ValidateCalculatedProperty(Parent.CellsPerSubsectionInfo);
		}

		protected void CheckCellsPerSubsection()
		{
			if (Parent.IsReleaseScheduler)
			{
				if (Parent.CellsPerSubsection != 1)
				{
					Parent.CellsPerSubsectionInfo.AddError(Res.GetString("5e157113-4604-48e9-8e6a-29a6fa9bb7d0", "Must be only one Cell Per Subsection on a Release Scheduler board section."));
				}
			}
			else if (Parent.IsBuffer)
			{
				if (!Parent.ShowZones)
				{
					CompareValidation.CheckGreaterThanOrEqualTo(Parent.CellsPerSubsectionInfo, MinCellsPerSubsection);
				}
				else
				{
					var limit = Parent.Subsections == 1 ? 4 : Parent.Subsections < 4 && Parent.CellsPerSubsection < 2 ? 2 : 1;
					CompareValidation.CheckGreaterThanOrEqualTo(Parent.CellsPerSubsectionInfo, limit); // Buffers always have four zones, so there must be at least 4 cells on the board
				}
			}
			else
			{
				CompareValidation.CheckGreaterThanOrEqualTo(Parent.CellsPerSubsectionInfo, MinCellsPerSubsection);
			}
		}

		const int MinCellsPerSubsection = 0;

		public void ValidateShowZones()
		{
			ValidateCalculatedProperty(Parent.ShowZonesInfo);
		}

		protected void CheckShowZones()
		{
			if (Parent.IsBucket && Parent.ShowZones)
			{
				Parent.ShowZonesInfo.AddError(Res.GetString("396142e4-ee3a-4bb5-8a46-60f20f91cea8", "Zones must be not turned on for Bucket sections."));
			}
			else if (Parent.IsBuffer && !Parent.ShowZones)
			{
				Parent.ShowZonesInfo.AddWarning(Res.GetString("9270d9d1-1a95-4cae-9858-de5d927c165f", "Zones will not be visible on this buffer section."));
			}
			else if (Parent.IsReleaseScheduler && Parent.ShowZones)
			{
				Parent.ShowZonesInfo.AddError(Res.GetString("76c29180-73e7-4ba4-817e-8352b54f80cd", "Zones must be not turned on for Release Scheduler sections."));
			}
		}

		public void ValidateShowChildComponentZones()
		{
			ValidateCalculatedProperty(Parent.ShowChildComponentZonesInfo);
		}

		protected void CheckShowChildComponentZones()
		{
			if (Parent.ShowChildComponentZones)
			{
				if (Parent.IsBucket)
				{
					Parent.ShowChildComponentZonesInfo.AddError(Res.GetString("ae77d0ba-5f8f-4153-bdc1-4bb41456241a", "Child component zones must be not turned on for Bucket sections."));
				}
				else if (Parent.IsBuffer && !Parent.ShowZones)
				{
					Parent.ShowChildComponentZonesInfo.AddError(Res.GetString("a177498d-01dd-45e2-92c9-19c73e03e99a", "In order to display child component zones, Show Zones needs to be selected."));
				}
			}
		}

		public void ValidatePanelLayoutStyle()
		{
			ValidateCalculatedProperty(Parent.PanelLayoutStyleInfo);
		}

		protected void CheckPanelLayoutStyle()
		{
			if (Parent.IsReleaseScheduler && Parent.PanelLayoutStyle != PanelLayoutTypeList.Codes.Stacked)
			{
				Parent.PanelLayoutStyleInfo.AddError(Res.GetString("d3848649-4a07-445a-9573-a449be92f724", "Release Scheduler sections must use the Stacked Panel Layout Style."));
			}
			else
			{
				MandatoryValidation.CheckEntered(Parent.PanelLayoutStyleInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PanelLayoutStyleInfo);
			}
		}

		public void ValidateTimePerCell()
		{
			ValidateCalculatedProperty(Parent.TimePerCellInfo);
		}

		protected void CheckTimePerCell()
		{
			if (Parent.CellsPerSubsection > 1 && Parent.ChannelSecondaryBy == BMConstants.ChannelByTimeCode)
			{
				MandatoryValidation.CheckEntered(Parent.TimePerCellInfo);
			}

			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.TimePerCellInfo);
		}

		public void ValidateChannelBy()
		{
			ValidateCalculatedProperty(Parent.ChannelByInfo);
		}

		bool IsChannelAndUnchanneledTheSameType(out string conflictType)
		{
			conflictType = "";
			if (Parent.ShowSecondaryUnchanneled)
			{
				foreach (var secondaryUnchanneled in Parent.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().Where(c => c.IsUnChanneled && !c.MSC_ChannelType.IsEmpty))
				{
					if ((Parent.ChannelBy == secondaryUnchanneled.MSC_ChannelType) || Parent.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ChannelType == secondaryUnchanneled.MSC_ChannelType))
					{
						conflictType = secondaryUnchanneled.MSC_ChannelType;
						return true;
					}
				}
			}

			if (Parent.ShowUnchanneled)
			{
				foreach (var primaryUnchanneled in Parent.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Where(c => c.IsUnChanneled && !c.MSC_ChannelType.IsEmpty))
				{
					if ((Parent.ChannelSecondaryBy == primaryUnchanneled.MSC_ChannelType) || Parent.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ChannelType == primaryUnchanneled.MSC_ChannelType))
					{
						conflictType = primaryUnchanneled.MSC_ChannelType;
						return true;
					}
				}
			}

			return false;
		}

		string ChannelAndUnchanneledConflictError(string conflictType)
		{
			return Res.GetString("b260e41b-5c0a-48ca-813b-ab3fe3b2c589", "Un-channeled shows items that do not belong in any channel. Having un-channeled by {0} and a {0} channel on the other axis will never return any results.", conflictType);
		}

		protected void CheckChannelBy()
		{
			string conflictType;
			if (IsChannelAndUnchanneledTheSameType(out conflictType))
			{
				Parent.ChannelByInfo.AddError(ChannelAndUnchanneledConflictError(conflictType));
			}
			if (!Parent.OverrideChannels)
			{
				MandatoryValidation.CheckEntered(Parent.ChannelByInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.ChannelByInfo);

			if (Parent.IsReleaseScheduler)
			{
				if (Parent.ChannelBy != ChannelTypeList.Codes.ReleaseSchedulerChannels)
				{
					Parent.ChannelByInfo.AddError(Res.GetString("99c98154-7fbb-4766-81cf-d3f1b9dbd7a6", "Release Scheduler board sections should have Release Scheduler Channels specified since channels are automatically chosen based on constrained resources."));
				}
			}
			else if (Parent.ChannelBy == ChannelTypeList.Codes.ReleaseSchedulerChannels)
			{
				Parent.ChannelByInfo.AddError(Res.GetString("4939fb6c-7481-48cb-8254-e76480dfeef3", "Release Scheduler Channels are only valid on Release Scheduler board sections."));
			}
		}

		public void ValidateChannelSecondaryBy()
		{
			ValidateCalculatedProperty(Parent.ChannelSecondaryByInfo);
		}

		bool IsPrimaryChannelAssigned()
		{
			return (!Parent.ChannelBy.IsEmpty && (Parent.ChannelBy != ChannelTypeList.Codes.NotChanneled)) || ((Parent.PrimaryAxisChannels.Count > 0) && Parent.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().All(c => !c.MSC_ChannelType.IsEmpty));
		}

		bool IsSecondaryChannelAssigned()
		{
			return (!Parent.ChannelSecondaryBy.IsEmpty && (Parent.ChannelSecondaryBy != ChannelTypeList.Codes.NotChanneled)) || ((Parent.SecondaryAxisChannels.Count > 0) && Parent.SecondaryAxisChannels.Cast<BMBoardSectionChannel>().All(c => !c.MSC_ChannelType.IsEmpty));
		}

		protected void CheckChannelSecondaryBy()
		{
			string conflictType;
			if (IsChannelAndUnchanneledTheSameType(out conflictType))
			{
				Parent.ChannelSecondaryByInfo.AddError(ChannelAndUnchanneledConflictError(conflictType));
			}

			if (!IsPrimaryChannelAssigned() && IsSecondaryChannelAssigned() && Parent.ChannelBy != ChannelTypeList.Codes.ReleaseSchedulerChannels)
			{
				if (Parent.ChannelSecondaryBy != ChannelTypeList.Codes.NotChanneled && Parent.ChannelSecondaryBy != BMConstants.ChannelByTimeCode)
				{
					Parent.ChannelSecondaryByInfo.AddError(Res.GetString("cd1b4f7f-395e-451c-9dda-3df62634210c", "When there are no primary channels, the only valid values for Channel Secondary By is by Time or Not Channeled"));
				}
			}
			if (Parent.IsReleaseScheduler)
			{
				if (Parent.ChannelSecondaryBy != ChannelTypeList.Codes.ReleaseSchedulerChannels)
				{
					Parent.ChannelSecondaryByInfo.AddError(Res.GetString("35eb4f7f-395e-451c-9dda-3df62634210c", "Release Scheduler board sections should have Release Scheduler Channels specified since secondary channels are used for released and un-released work."));
				}
			}
			else
			{
				if (!Parent.OverrideSecondaryChannels)
				{
					MandatoryValidation.CheckEntered(Parent.ChannelSecondaryByInfo);

					if (Parent.IsBuffer && Parent.ChannelSecondaryBy != BMConstants.ChannelByTimeCode)
					{
						Parent.ChannelSecondaryByInfo.AddError(Res.GetString("1fba28ac-d207-4041-86f8-52cfc7b60237", "Buffer components must have time as their secondary axis."));
					}
				}
				else
				{
					if (Parent.ChannelSecondaryBy == BMConstants.ChannelByTimeCode)
					{
						Parent.ChannelSecondaryByInfo.AddError(Res.GetString("a4862ff2-a388-4ea6-b656-c74563f81272", "Cannot show time units on the secondary axis when channels are overridden."));
					}
				}
				if (Parent.CellsPerSubsection > 1 && !Parent.ChannelSecondaryBy.IsEmpty && Parent.ChannelSecondaryBy != BMConstants.ChannelByTimeCode)
				{
					Parent.ChannelSecondaryByInfo.AddError(Res.GetString("6826f9a1-2dd0-4a27-a2b3-e8469ad7ce96", "When there is more than one cell per subsection, the only valid value for Channel Secondary By is by Time."));
				}
				ListValidation.ErrorIfInvalidCode(Parent.ChannelSecondaryByInfo);
			}
		}

		public void ValidateOverrideChannels()
		{
			ValidateCalculatedProperty(Parent.OverrideChannelsInfo);
		}

		protected void CheckOverrideChannels()
		{
			if (Parent.IsReleaseScheduler && Parent.OverrideChannels)
			{
				Parent.OverrideChannelsInfo.AddError(Res.GetString("885703de-5dc0-418a-8b7a-fdd430ade516", "Release Scheduler board sections should not override default channels since channels are automatically chosen based on constrained resources."));
			}
		}

		public void ValidateOverrideSecondaryChannels()
		{
			ValidateCalculatedProperty(Parent.OverrideSecondaryChannelsInfo);
		}

		protected void CheckOverrideSecondaryChannels()
		{
			if (Parent.OverrideSecondaryChannels && (Parent.IsBuffer || Parent.IsReleaseScheduler))
			{
				Parent.OverrideSecondaryChannelsInfo.AddError(Res.GetString("28827894-15e5-45c5-a4f9-eb3d1a79a90f", "Secondary channels for a Buffer cannot be overridden."));
			}
		}

		public void ValidateShowUnchanneled()
		{
			ValidateCalculatedProperty(Parent.ShowUnchanneledInfo);
		}

		protected void CheckShowUnchanneled()
		{
			if (Parent.IsReleaseScheduler && Parent.ShowUnchanneled)
			{
				Parent.ShowUnchanneledInfo.AddError(Res.GetString("adfc4175-f487-4f29-98fb-cb4f0318dcad", "Release Scheduler board sections should not show un-channeled work."));
			}
		}

		public void ValidateShowSecondaryUnchanneled()
		{
			ValidateCalculatedProperty(Parent.ShowSecondaryUnchanneledInfo);
		}

		protected void CheckShowSecondaryUnchanneled()
		{
			if (Parent.ShowSecondaryUnchanneled && (Parent.IsReleaseScheduler || Parent.IsBuffer))
			{
				Parent.ShowSecondaryUnchanneledInfo.AddError(Res.GetString("5d7dd2e9-d7b6-489c-976f-416c275aaf21", "Cannot show secondary un-channeled work for a Buffer."));
			}
		}

		public void ValidateSortPrimaryChannels()
		{
			ValidateCalculatedProperty(Parent.SortPrimaryChannelsInfo);
		}

		protected void CheckSortPrimaryChannels()
		{
		}

		public void ValidateSortSecondaryChannels()
		{
			ValidateCalculatedProperty(Parent.SortSecondaryChannelsInfo);
		}

		protected void CheckSortSecondaryChannels()
		{
		}

		public void ValidateReleaseGroupPK()
		{
			ValidateCalculatedProperty(Parent.ReleaseGroupPKInfo);
		}

		protected void CheckReleaseGroupPK()
		{
			if (Parent.Section.Board != null)
			{
				if (Parent.IsBucket && Parent.PrimaryAxisChannels.Count == 0 && Parent.SecondaryAxisChannels.Count == 0 && Parent.ReleaseGroupPK.IsEmpty)
				{
					Parent.ReleaseGroupPKInfo.AddError(Res.GetString("61bc935d-f00b-4a7d-b18f-c6d14ea135f7", "Please enter a Release Group or add channels to this board section. Without these it is likely there would be a large volume of work shown on this board section."));
				}

				ListValidation.ErrorIfInvalidPK(Parent.ReleaseGroupPKInfo);
			}
		}

		public void ValidateShowWorkInReleaseGroupOnly()
		{
			ValidateCalculatedProperty(Parent.ShowWorkInReleaseGroupOnlyInfo);
		}

		protected void CheckShowWorkInReleaseGroupOnly()
		{
			if (Parent.ReleaseGroupPK.IsEmpty && Parent.ShowWorkInReleaseGroupOnly)
			{
				Parent.ShowWorkInReleaseGroupOnlyInfo.AddWarning(Res.GetString("af2d6747-78d5-4e8a-8b8b-8de90626181d", "Please select a Release Group."));
			}

			if (!Parent.ShowWorkInReleaseGroupOnly)
			{
				if (Parent.Channels.Any())
				{
					return;
				}

				var system = Parent.Board?.System;

				if (system != null && system.Factory.Exists(typeof(BMSystemReleaseGroup), new ZQuery(BMSystemReleaseGroupSchema.FSG_FS_System, system.PK)))
				{
					Parent.ShowWorkInReleaseGroupOnlyInfo.AddWarning(Res.GetString("fd3d1c5d-49a4-4818-99a5-d857355b2060", "There may be many items shown on this section. We recommend filtering by Release Group to improve the performance of this board section."));
				}
			}
		}

		public void ValidateIsReleaseScheduler()
		{
			ValidateCalculatedProperty(Parent.IsReleaseSchedulerInfo);
		}

		protected void CheckIsReleaseScheduler()
		{
			if (Parent.IsReleaseScheduler && Parent.Section.Component != null && !Parent.Section.Component.IsBuffer)
			{
				Parent.IsReleaseSchedulerInfo.AddError(Res.GetString("ea8a0642-8c0b-49d9-aa1d-5331659a2036", "Release Scheduler board sections must be for a Buffer component."));
			}
		}

		public void ValidateCardType()
		{
			ValidateCalculatedProperty(Parent.CardTypeInfo);
		}

		protected void CheckCardType()
		{
			MandatoryValidation.CheckEntered(Parent.CardTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CardTypeInfo);

			if (!Parent.CardType.IsEmpty)
			{
				if (Parent.IsReleaseScheduler)
				{
					if (Parent.CardType != CardTypeList.Codes.Workflow)
					{
						Parent.CardTypeInfo.AddError(Res.GetString("f60872e1-644b-47a3-84b2-6ec4123dd5de", "Release Scheduler board sections can only display one ticket per workflow. Please use the {0} option.", CardTypeList.Codes.Workflow));
					}
				}
				else if (Parent.ShowWorkflowOrJobWorkflowCards && Parent.IsBuffer && Parent.PrimaryAxisChannels.Cast<BMBoardSectionChannel>().Any(c => c.MSC_ChannelType == ChannelTypeList.Codes.Resource))
				{
					Parent.CardTypeInfo.AddWarning(Res.GetString("66eb4001-8aee-427f-823b-864e6c281fad", "Showing one card per workflow may not be useful on a Buffer section with resource channels since workflow cards do not identify each task the resources need to complete."));
				}

				if (Parent.ShowJobWorkflowCards && Parent.WorkflowFilter != null)
				{
					var iBMFilter = (IBMFilterRuleFilterBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(ModuleIDs.BMFilterRule);

					if (iBMFilter.LoadFilterRuleLayout(Parent.WorkflowFilter))
					{
						var hasNotRelevantFilter = iBMFilter.ActiveFilterIdentifiers.Any(m => FiltersNotApplicableForJobWorkflows.Contains(m));

						if (hasNotRelevantFilter)
						{
							var codes = new ZStringBuilder();
							foreach (var code in FiltersNotApplicableForJobWorkflows)
							{
								codes.Append(code);
							}

							Parent.CardTypeInfo.AddWarning(Res.GetString("72EED5DC-8A96-435D-B4B3-A62A00BE47B1",
								"Selected workflow filter(s) are not applicable when showing tickets for Job-Level Workflow. It might be one of these filters: ") +
								codes.ToStringWithDelimiterBetweenAppends(", ") + ".");
						}
					}
				}
			}
		}

		static ZString[] FiltersNotApplicableForJobWorkflows
		{
			get
			{
				return new ZString[]
				{
					ProcessHeader.ModuleFilterConstants.BufferZone,
					ProcessHeader.ModuleFilterConstants.ConstraintStatus,
					ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					ProcessHeader.ModuleFilterConstants.LeadTime,
					ProcessHeader.ModuleFilterConstants.CurrentComponent,
					(NoResString)"Component Change Logs" // Module filter 'name'
				};
			}
		}

		public void ValidateBufferZone3Color()
		{
			ValidateCalculatedProperty(Parent.BufferZone3ColorInfo);
		}

		protected void CheckBufferZone3Color()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BufferZone3ColorInfo, Parent.Lookups.ColorList);
			if (Parent.BufferZone3Color == Color.Transparent.Name)
			{
				Parent.BufferZone3ColorInfo.AddError(Res.GetString("098869cf-2022-4915-aa8d-ebe620e01080", "Buffer Zone Colors can not be Transparent"));
			}
		}

		public void ValidateBufferZone2Color()
		{
			ValidateCalculatedProperty(Parent.BufferZone2ColorInfo);
		}

		protected void CheckBufferZone2Color()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BufferZone2ColorInfo, Parent.Lookups.ColorList);
			if (Parent.BufferZone2Color == Color.Transparent.Name)
			{
				Parent.BufferZone2ColorInfo.AddError(Res.GetString("a3be42e3-9b32-4b9c-b3b7-baac27fc882a", "Buffer Zone Colors can not be Transparent"));
			}
		}

		public void ValidateBufferZone1Color()
		{
			ValidateCalculatedProperty(Parent.BufferZone1ColorInfo);
		}

		protected void CheckBufferZone1Color()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BufferZone1ColorInfo, Parent.Lookups.ColorList);
			if (Parent.BufferZone1Color == Color.Transparent.Name)
			{
				Parent.BufferZone1ColorInfo.AddError(Res.GetString("8caf77d8-2cc6-47f2-a8b0-dce76424510d", "Buffer Zone Colors can not be Transparent"));
			}
		}

		public void ValidateBufferZone0Color()
		{
			ValidateCalculatedProperty(Parent.BufferZone0ColorInfo);
		}

		protected void CheckBufferZone0Color()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BufferZone0ColorInfo, Parent.Lookups.ColorList);
			if (Parent.BufferZone0Color == Color.Transparent.Name)
			{
				Parent.BufferZone0ColorInfo.AddError(Res.GetString("19d587da-e79a-454a-9e99-2e978e00a740", "Buffer Zone Colors can not be Transparent"));
			}
		}

		public void ValidateOverdueBackgroundColor()
		{
			ValidateCalculatedProperty(Parent.OverdueBackgroundColorInfo);
		}

		protected void CheckOverdueBackgroundColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OverdueBackgroundColorInfo, Parent.Lookups.ColorList);
		}

		public void ValidateOverdueForegroundColor()
		{
			ValidateCalculatedProperty(Parent.OverdueForegroundColorInfo);
		}

		protected void CheckOverdueForegroundColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OverdueForegroundColorInfo, Parent.Lookups.ColorList);
		}

		public void ValidateCountdownTargetBorderColor()
		{
			ValidateCalculatedProperty(Parent.CountdownTargetBorderColorInfo);
		}

		protected void CheckCountdownTargetBorderColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CountdownTargetBorderColorInfo, Parent.Lookups.ColorList);
		}

		public void ValidateCountdownTargetBorderStyle()
		{
			ValidateCalculatedProperty(Parent.CountdownTargetBorderStyleInfo);
		}

		protected void CheckCountdownTargetBorderStyle()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CountdownTargetBorderStyleInfo, Parent.BorderStyles);
		}

		public void ValidateCountdownStartableBorderColor()
		{
			ValidateCalculatedProperty(Parent.CountdownStartableBorderColorInfo);
		}

		protected void CheckCountdownStartableBorderColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CountdownStartableBorderColorInfo, Parent.Lookups.ColorList);
		}

		public void ValidateCountdownStartableBorderStyle()
		{
			ValidateCalculatedProperty(Parent.CountdownStartableBorderStyleInfo);
		}

		protected void CheckCountdownStartableBorderStyle()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CountdownStartableBorderStyleInfo, Parent.BorderStyles);
		}

		public void ValidateFadeBackgroundAtPercentage()
		{
			ValidateCalculatedProperty(Parent.FadeBackgroundAtPercentageInfo);
		}

		protected void CheckFadeBackgroundAtPercentage()
		{
			CompareValidation.CheckWithinRange(Parent.FadeBackgroundAtPercentageInfo, 0, 100);

			if (Parent.FadeBackgroundAtPercentage > 0)
			{
				if (Parent.CellsPerSubsection <= 1)
				{
					Parent.FadeBackgroundAtPercentageInfo.AddError(Res.GetString("9afd7ac5-cae6-4190-8b58-f36c1291e337", "When 'Background Fade Percent' is greater than 0, 'Cells Per Subsection' must be greater than 1."));
				}

				if (!Parent.IsBuffer && !Parent.IsBucket)
				{
					Parent.FadeBackgroundAtPercentageInfo.AddError(Res.GetString("a042b0db-ee1b-480b-a5ed-be8931e04fb4", "Background Fade Percent can only be used on Buffer and Bucket sections."));
				}
			}

			if (Parent.IsReleaseScheduler && Parent.FadeBackgroundAtPercentage > 0)
			{
				Parent.FadeBackgroundAtPercentageInfo.AddError(Res.GetString("90057dc2-7038-4e31-b87a-c70d21702037", "Must be no Fade Percentage on a Release Scheduler board section."));
			}
		}

		void ValidateFilters()
		{
			FilterValidator.ValidateFilter(Parent.WorkflowFilter);
			FilterValidator.ValidateFilter(Parent.TaskFilter);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSubsections();
			ValidateFlowDirection();
			ValidateLastCell();
			ValidateCellsPerSubsection();
			ValidateTimePerCell();
			ValidateChannelBy();
			ValidateChannelSecondaryBy();
			ValidateReleaseGroupPK();
			ValidateShowWorkInReleaseGroupOnly();
			ValidateTimeProgressionMode();
			ValidateTimeField();
			ValidateMaxOverdueSlots();
			ValidateBufferZone3Color();
			ValidateBufferZone2Color();
			ValidateBufferZone1Color();
			ValidateBufferZone0Color();
			ValidateOverdueBackgroundColor();
			ValidateOverdueForegroundColor();
			ValidateCountdownTargetBorderColor();
			ValidateCountdownTargetBorderStyle();
			ValidateCountdownStartableBorderColor();
			ValidateCountdownStartableBorderStyle();
			ValidateFadeBackgroundAtPercentage();
			ValidateOverrideChannels();
			ValidateOverrideSecondaryChannels();
			ValidateShowUnchanneled();
			ValidateShowSecondaryUnchanneled();
			ValidateIsReleaseScheduler();
			ValidateCardType();
			ValidateFilters();
			ValidateShowZones();
			ValidateShowChildComponentZones();
			ValidatePanelLayoutStyle();
			ValidateSectionNameOverride();
		}

		/// <summary>
		/// The auto validation type.
		/// </summary>
		public override Type AutoValidationType
		{
			get { return typeof(BMComponentSectionConfigurationValidation); }
		}

		protected override IBoardSectionNameOverridable SectionConfiguration => Parent;
	}
}
