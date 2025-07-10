using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class FRUnloadingItemDifferencesTabUserControl : UnloadingItemDifferencesTabUserControl
	{
		public FRUnloadingItemDifferencesTabUserControl()
		{
			InitializeComponent();
			InitializeGrids();
		}

		void InitializeGrids()
		{
			using (UnloadedGoodsItemsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var isMissingColumn = UnloadedGoodsItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == nameof(EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc.IsMissing));
				if (isMissingColumn != null)
				{
					UnloadedGoodsItemsGrid.ColumnStyles.Remove(isMissingColumn);
				}
			}
		}
	}
}
