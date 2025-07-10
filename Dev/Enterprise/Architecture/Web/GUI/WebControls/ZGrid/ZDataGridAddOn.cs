using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZDataGridAddOn : CompositeControl
	{
		#region Constructors

		public ZDataGridAddOn()
		{
		}

		#endregion

		#region Properties

		public ZDataGrid Grid
		{
			get { return grid; }
			set
			{
				if (grid != null)
				{
					try
					{
						grid.AfterCreateChildControls -= OnAfterCreateChildControls;
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
				}
				grid = value;
				grid.AfterCreateChildControls += OnAfterCreateChildControls;
				OnGridChange();
			}
		}

		#endregion

		#region Overrides

		public override void RenderControl(HtmlTextWriter writer)
		{
		}

		#endregion

		#region Implementation

		protected virtual void OnGridChange()
		{
		}

		protected virtual void OnAfterCreateChildControls(object sender, EventArgs e)
		{
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (Grid != null)
			{
				RegisterServices();
				RegisterIncludedScripts();
				foreach (DataGridItem item in Grid.Items)
				{
					AddScripts(item);
				}
			}
		}

		protected virtual void RegisterIncludedScripts()
		{
		}

		protected virtual void RegisterServices()
		{
		}

		protected virtual void AddScripts(DataGridItem item)
		{
		}

		ZDataGrid grid;

		#endregion
	}
}
