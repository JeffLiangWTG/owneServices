using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ComponentUserControl : ZUserControl
	{
		public ComponentUserControl()
		{
			InitializeComponent();
		}

		internal void RemoveFromAvailableColumns(params string[] columnsNames)
		{
			if (columnsNames != null)
			{
				using (ComponentGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ComponentGrid.RemoveFromAvailableColumns(columnsNames);
					ComponentGrid.RefreshTableStyles();
				}
			}
		}

		internal void ReOrderColumns(params string[] columnsNames)
		{
			if (columnsNames != null)
			{
				using (ComponentGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ComponentGrid.ReOrderColumns(columnsNames);
					ComponentGrid.RefreshTableStyles();
				}
			}
		}

		internal void SetColumnCaption(string columnName, string caption)
		{
			if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(caption))
			{
				using (ComponentGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ComponentGrid.SetColumnCaption(columnName, caption);
					ComponentGrid.RefreshTableStyles();
				}
			}
		}

		internal void SetColumnWidth(string columnName, int width)
		{
			if (!string.IsNullOrEmpty(columnName) && width > 0)
			{
				using (ComponentGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ComponentGrid.SetColumnWidth(columnName, width);
					ComponentGrid.RefreshTableStyles();
				}
			}
		}
	}
}
