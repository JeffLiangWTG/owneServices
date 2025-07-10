using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	static class FilterApplicator
	{
		#region Multi Option Filter

		static internal bool ValueEqualsRiskComponent(IMultiOptionFilter filter, ZGuid riskComponent)
		{
			var current = ((BMComponent)filter.CurrentlySelectedOption);
			return current != null && riskComponent == current.PK;
		}

		static internal IMultiOptionFilter GetCurrentFilter(IFilterable filterable)
		{
			return (IMultiOptionFilter)filterable.FilterManager.AppliedFilters.OfType<ComponentViewFilter>().FirstOrDefault() ?? new ComponentViewFilter();
		}

		internal static void ToggleFilterCore(IFilterable filterable, IMultiOptionFilter filter, bool condition, object value)
		{
			if (IsFilterApplied(filterable, filter) && condition)
			{
				filter.CurrentlySelectedOption = null;
				filterable.FilterManager.RemoveFilter(filter, shouldRefresh: true);
			}
			else
			{
				filter.CurrentlySelectedOption = value;
				filterable.FilterManager.ApplyFilter(filter);
			}
		}

		internal static bool IsFilterApplied(IFilterable filterable, IMultiOptionFilter filter)
		{
			return filterable.FilterManager.IsApplied(filter);
		}

		#endregion

		#region Component View Filter

		static internal void ToggleComponentViewFilter(ZGuid riskComponent, IMultiOptionFilter filter, BMBoardSectionViewModel viewModel, BMComponentControl componentControl, BMComponentSectionConfiguration sectionConfiguration)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "Risk Component Factory" };
			var loadedComponent = factory.Load<BMComponent>(riskComponent);
			if (loadedComponent != null)
			{
				var filterable = (IFilterable)viewModel;
				var valueEqualsRiskComponent = ValueEqualsRiskComponent(filter, riskComponent);
				ToggleFilterCore(filterable, filter, valueEqualsRiskComponent, loadedComponent);
				ToggleComponentViewFilterCore(filter, viewModel, sectionConfiguration, componentControl);
			}
		}

		static internal void ToggleComponentViewFilterCore(IMultiOptionFilter filter, BMBoardSectionViewModel viewModel, BMComponentSectionConfiguration sectionConfiguration, BMComponentControl componentControl)
		{
			var option = filter.CurrentlySelectedOption != null ? (BMComponent)filter.CurrentlySelectedOption : null;
			viewModel.ComponentGrid.CurrentlySelectedComponent = option;

			if (ConstrainedModeHelper.IsInConstrainedMode(sectionConfiguration.Section.Component, sectionConfiguration.ApplicableReleaseGroupPK) && !sectionConfiguration.IsReleaseScheduler)
			{
				OverrideCellZones(option, viewModel, sectionConfiguration);
				SwitchZoneHeadings(option, viewModel, sectionConfiguration, componentControl);
				foreach (var channel in viewModel.PrimaryChannels)
				{
					channel.ClearCacheAndReload();
				}
				UpdateConstraintLine(option, viewModel, sectionConfiguration, componentControl);
			}

			UpdateBackgroundColoursAndFades(viewModel);
		}

		static void UpdateConstraintLine(BMComponent option, BMBoardSectionViewModel viewModel, BMComponentSectionConfiguration sectionConfiguration, BMComponentControl componentControl)
		{
			var buffer = option == null ? sectionConfiguration.Section.Component : (option.IsChildComponent ? option.ParentComponent : option);

			if (viewModel.IsInConstrainedMode && sectionConfiguration.ShowZones)
			{
				ComponentGridHelper.SetConstraintLine(viewModel.IsInConstrainedMode, sectionConfiguration, buffer, viewModel.ComponentGrid);
				componentControl.RefreshTaskPanels();
			}
		}

		static void UpdateBackgroundColoursAndFades(BMBoardSectionViewModel viewModel)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "ComponentViewFilterMenuItem.OnFilterToggled" };
			var section = factory.Load<BMBoardSection>(viewModel.SectionPK);

			var map = viewModel.ComponentGrid.CardAllocationMap;
			if (map != null)
			{
				viewModel.ComponentGrid.UpdateBackgroundColoursAndFades(viewModel, map, section);
			}
		}

		static void OverrideCellZones(BMComponent option, BMBoardSectionViewModel viewModel, BMComponentSectionConfiguration sectionConfiguration)
		{
			if (!sectionConfiguration.ShowChildComponentZones)
			{
				foreach (var cell in viewModel.ComponentGrid.CardCells)
				{
					if (option != null)
					{
						if ((cell.SubComponentZones.Any() &&
							(cell.ConstraintPKs.Contains(option.PK) ||
							cell.PreConstraintPKs.Contains(option.PK) || cell.PostConstraintPKs.Contains(option.PK))
							&& cell.ConstraintStatus != ConstraintStatus.ReadyForConstraint))
						{
							cell.ZoneOverride = cell.SubComponentZones[option.PK];
						}
						else if (cell.ConstraintStatus != ConstraintStatus.ReadyForConstraint)
						{
							cell.ZoneOverride = cell.Zone;
						}
						else
						{
							cell.ZoneOverride = null;
						}
					}
					else
					{
						cell.ZoneOverride = null;
					}
				}
			}
		}

		static void SwitchZoneHeadings(BMComponent option, BMBoardSectionViewModel viewModel, BMComponentSectionConfiguration sectionConfiguration, BMComponentControl componentControl)
		{
			if (!sectionConfiguration.ShowChildComponentZones)
			{
				var sizeCell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ZoneHeading);
				if (sizeCell != null)
				{
					var zoneHeadingGroups = viewModel.ComponentGrid.Cells.Where(c =>
					c.ContentType.In(CellContentType.ZoneHeading, CellContentType.SubComponentZoneHeading))
					.GroupBy(c => sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical ? c.Column : c.Row);

					var size = sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical ?
						viewModel.GetOriginalColumnWidth(sizeCell.Column) :
						viewModel.GetOriginalRowHeight(sizeCell.Row);

					var hideZoneHeading = option != null && option.PK.In(zoneHeadingGroups.SelectMany(z => z).Select(s => s.SubComponentHeadingPK).Distinct());

					foreach (var zoneHeadingGroup in zoneHeadingGroups)
					{
						var firstCell = zoneHeadingGroup.First();
						componentControl.EnsureAppropriateZoneHeadingsVisible(size, firstCell, sectionConfiguration.OrientationValue, option, hideZoneHeading);
					}
				}
			}
		}

		#endregion
	}
}
