using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	class ComponentViewFilterMenuItem : MultiChildCheckedFilterMenuItem<BMComponent>
	{
		internal ComponentViewFilterMenuItem(BMBoardSectionViewModel viewModel, BMComponentControl componentControl, BMComponentSectionConfiguration sectionConfiguration)
			: base(Res.GetString("801a46f7-c005-4285-9d0f-ecc8d21a847d", "Component View Filter"), viewModel)
		{
			this.viewModel = viewModel;
			this.componentControl = componentControl;
			this.sectionConfiguration = sectionConfiguration;
		}

		readonly BMBoardSectionViewModel viewModel;
		readonly BMComponentControl componentControl;
		readonly BMComponentSectionConfiguration sectionConfiguration;

		protected override IMultiOptionFilter GetOrCreateMultiOptionFilter()
		{
			return (IMultiOptionFilter)GetCurrentFilter(typeof(ComponentViewFilter)) ?? new ComponentViewFilter();
		}

		protected override IEnumerable<BMComponent> GetMenuItemValues()
		{
			foreach (var component in sectionConfiguration.Section.ApplicableComponents.OrderBy(o => o.FC_DisplaySequence))
			{
				yield return component;
				foreach (var childComponent in component.ChildComponents.OrderBy(o => o.FC_DisplaySequence))
				{
					yield return childComponent;
				}
			}
		}

		protected override string GetSubMenuItemText(BMComponent value)
		{
			return string.Format(CultureInfo.CurrentCulture, @"{0}{1}", (value.IsChildComponent ? "   " : string.Empty), value.FC_Name);
		}

		protected override string GetSubMenuItemTooltip(BMComponent value)
		{
			if (value.IsChildComponent)
			{
				return Res.GetString("196ce1cd-191c-4428-9a15-5645acc61000", "Show only items whose penetrated sub-components include {0}", value.FC_Name);
			}

			return Res.GetString("c2c2fd2c-442c-4b53-90af-a9c4a7be2f39", "Show only items whose Current Component is {0}", value.FC_Name);
		}

		protected override void OnFilterToggled()
		{
			base.OnFilterToggled();
			FilterApplicator.ToggleComponentViewFilterCore(Filter, viewModel, sectionConfiguration, componentControl);
		}

		protected override bool ValueEqualsCurrentlySelectedOption(BMComponent value)
		{
			return FilterApplicator.ValueEqualsRiskComponent(Filter, value.PK);
		}
	}
}
