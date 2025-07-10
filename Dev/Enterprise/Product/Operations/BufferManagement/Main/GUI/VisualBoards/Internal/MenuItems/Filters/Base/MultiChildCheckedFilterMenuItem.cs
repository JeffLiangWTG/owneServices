using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.Properties;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public abstract class MultiChildCheckedFilterMenuItem<T> : FilterMenuItemBase
	{
		protected MultiChildCheckedFilterMenuItem(string text, IFilterable filterable)
			: base(text)
		{
			Argument.NotNull(filterable, "filterable");

			this.filterable = filterable;

			DropDownOpeningHandler = FilterMenuItem_DropDownOpening;
			DropDownOpening += DropDownOpeningHandler;
			DropDownItems.Add(ZMenuItem.Separator);
		}

		public EventHandler DropDownOpeningHandler;

		readonly IFilterable filterable;

		#region Dropdown Menu

		void FilterMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			DropDownItems.Clear();

			var isApplied = FilterApplicator.IsFilterApplied(filterable, Filter);

			foreach (var value in GetMenuItemValues())
			{
				var menuItem = new SubMenuItem(GetSubMenuItemText(value), GetSubMenuItemTooltip(value), value, this);

				if (isApplied && ValueEqualsCurrentlySelectedOption(value))
				{
					menuItem.Checked = true;
					menuItem.Image = Resources.tick;
				}

				DropDownItems.Add(menuItem);
			}
		}

		protected virtual bool ValueEqualsCurrentlySelectedOption(T value) => value.Equals(Filter.CurrentlySelectedOption);

		protected abstract IMultiOptionFilter GetOrCreateMultiOptionFilter();
		protected abstract IEnumerable<T> GetMenuItemValues();
		protected abstract string GetSubMenuItemText(T value);
		protected abstract string GetSubMenuItemTooltip(T value);

		protected sealed override IBoardFilter GetOrCreateFilter()
		{
			return GetOrCreateMultiOptionFilter();
		}

		protected sealed override IBoardFilter GetCurrentFilter(Type filterType)
		{
			return filterable.FilterManager.AppliedFilters.FirstOrDefault(filterType.IsInstanceOfType);
		}

		#endregion

		#region Filter Application

		protected new IMultiOptionFilter Filter
		{
			get { return (IMultiOptionFilter)base.Filter; }
		}

		internal void ToggleFilter(T value)
		{
			FilterApplicator.ToggleFilterCore(filterable, Filter, ValueEqualsCurrentlySelectedOption(value), value);
			OnFilterToggled();
		}

		protected virtual void OnFilterToggled()
		{
		}

		#endregion

		#region SubMenuItem

		class SubMenuItem : ZToolStripMenuItem
		{
			internal SubMenuItem(string text, string toolTip, T value, MultiChildCheckedFilterMenuItem<T> parentMenuItem)
				: base(text)
			{
				this.value = value;
				this.parentMenuItem = parentMenuItem;

				ToolTipText = toolTip;
			}

			readonly T value;
			readonly MultiChildCheckedFilterMenuItem<T> parentMenuItem;

			protected override void OnClick(EventArgs e)
			{
				base.OnClick(e);

				parentMenuItem.ToggleFilter(value);
			}
		}

		#endregion
	}
}
