using Enterprise.Core.Forms;
using Enterprise.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class WebServicesRegistryGridControl : RegistryZUserControl
	{
		public WebServicesRegistryGridControl()
		{
			InitializeComponent();
			WebServicesConfigItemsGrid.AllowSorting = false;

			if (!Env.CurrentUser.IsSupportUser)
			{
				RemoveColumnCore("IsAutoManaged");
				RemoveColumnCore("NumberofServerClusters");
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			WebServicesConfigItemsGrid.ReadOnly = readOnly;
		}

		void RemoveColumnCore(string columnName)
		{
			for (int i = WebServicesConfigItemsGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				ZGridColumnInfo columnInfo = (ZGridColumnInfo)WebServicesConfigItemsGrid.ColumnStyles[i];
				if (columnInfo.ColumnName == columnName)
				{
					WebServicesConfigItemsGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}
	}
}
