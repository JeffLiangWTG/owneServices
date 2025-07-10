using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaDutiesUserControl : ZUserControl
	{
		public AsycudaDutiesUserControl()
		{
			InitializeComponent();
		}

		internal void SetControlVisibility(AsycudaManifestHeader manifestHeader)
		{
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader);
			if (provider != null && manifestHeader != null)
			{
				var taxesGridColumnOrders = provider.GetTaxesGridColumnsOrder()?.ToList();
				if (taxesGridColumnOrders?.Any() == true)
				{
					var columnsToBeRemoved = DutiesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(column => !taxesGridColumnOrders.Contains(column.ColumnName)).Select(column => column.ColumnName).ToList();

					DutiesGrid.RemoveFromAvailableColumns(columnsToBeRemoved.ToArray());

					taxesGridExtraColumnInfos = provider.GetTaxesGridExtraColumnInfos().ToArray();
					foreach (var columnInfo in taxesGridExtraColumnInfos)
					{
						if (!DutiesGrid.ColumnStyles.Contains(columnInfo))
						{
							DutiesGrid.ColumnStyles.Add(columnInfo);
						}
					}

					DutiesGrid.ReOrderColumns(taxesGridColumnOrders.ToArray());
					DutiesGrid.SetColumnVisible(true, taxesGridColumnOrders.ToArray());
				}
			}
		}
		ZGridColumnInfo[] taxesGridExtraColumnInfos;
	}
}
