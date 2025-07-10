using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.GUI.Properties;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	public abstract class BasicCheckedFilterMenuItem : FilterMenuItemBase
	{
		protected BasicCheckedFilterMenuItem(string text, string toolTipText)
			: base(text)
		{
			ToolTipText = toolTipText;
		}

		#region Event Handlers

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			ApplyFilter();
			UpdateAppliedVisualState();
		}

		#endregion

		#region Filter Application

		void ApplyFilter()
		{
			foreach (var filterable in GetFilterables())
			{
				filterable.FilterManager.ToggleFilter(Filter);
			}
		}

		protected void UpdateAppliedVisualState()
		{
			Checked = IsFilterApplied();
			Image = Checked ? Resources.tick : null;
		}

		bool IsFilterApplied()
		{
			return GetFilterables().Any(f => f.FilterManager.IsApplied(Filter));
		}

		protected abstract IEnumerable<IFilterable> GetFilterables();

		#endregion
	}
}
