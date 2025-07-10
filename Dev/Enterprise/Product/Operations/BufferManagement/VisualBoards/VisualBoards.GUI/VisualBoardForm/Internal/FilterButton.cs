using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public class FilterButton : ZButton
	{
		internal FilterButton(IFilterable filterable)
		{
			this.filterable = filterable;
			this.BackgroundImage = Properties.Resources.filter;

			Click += FilterButton_Click;

			filterable.FilterManager.FiltersUpdated += Filters_FiltersUpdated;
		}

#if DEBUG
		public
#endif
 readonly IFilterable filterable;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				filterable.FilterManager.FiltersUpdated -= Filters_FiltersUpdated;
			}
		}

		void Filters_FiltersUpdated(object sender, EventArgs e)
		{
			var filters = filterable.FilterManager.AppliedAndChildFilters.ToArray();
			hasFilters = filters.Length > 0;

			if (hasFilters)
			{
				this.ToolTipCaption = ResString.GetMultilingualString("1a55f244-6808-422b-a8dc-6f158d8e52a8", "Filters applied:{0}{1}", System.Environment.NewLine, string.Join(System.Environment.NewLine, filters.Select(f => f.FilterName)));
			}
			else
			{
				this.ToolTipCaption = ResString.GetMultilingualString("8b02fe44-0bfc-4d66-a681-28f7bb7dc4ac", "Filters");
			}
#if WINZOR
				if (hasFilters)
				{
					this.ExtraStyleString = (ZArchitecture.Core.NoResString)"outline: 1px solid red;";
				}
				else
				{
					this.ExtraStyleString = string.Empty;
				}			
#endif
			Invalidate();
		}

		void FilterButton_Click(object sender, EventArgs e)
		{
			if (hasFilters)
			{
				ZFormModaliser.ShowDialogAndDispose(new BoardFiltersForm(filterable));
			}
			else
			{
				Globals.Message.Show(Res.GetString("5e92cb35-6279-4df9-8eed-a8d6b98c7236", "There are no filters currently applied."));
			}
		}

#if !WINZOR

		protected override void OnPaint(PaintEventArgs pevent)
		{
			base.OnPaint(pevent);

			if (hasFilters)
			{
				ControlPaint.DrawBorder(pevent.Graphics, ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
			}
		}

#endif

#if DEBUG
		public
#endif
 bool hasFilters;
	}
}
