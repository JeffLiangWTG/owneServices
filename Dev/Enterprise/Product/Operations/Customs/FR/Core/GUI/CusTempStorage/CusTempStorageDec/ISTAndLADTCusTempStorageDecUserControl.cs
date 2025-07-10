using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using SupportingDocumentsUserControl = Enterprise.Customs.FR.GUI.PlugIn.SupportingDocumentsUserControl;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class ISTAndLADTCusTempStorageDecUserControl : CusTempStorageDecUserControl
	{
		public ISTAndLADTCusTempStorageDecUserControl()
		{
			InitializeComponent();
			UpdateLinesGridColumns();
			UpdateLineItemsGridColumns();
			SupportingDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupportingDocumentsUserControl());
		}

		void InitSupportingDocumentsUserControl()
		{
			SupportingDocumentsUserControl.UserControlType = typeof(SupportingDocumentsUserControl);
			SupportingDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				var control = SupportingDocumentsUserControl.HostedControl as SupportingDocumentsUserControl;
				if (control != null)
				{
					EU.GUI.PlugIn.SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "CusTempStorageDec", "ISTAndLADTCusTempStorageDec");
				}
			};
		}

		void UpdateLinesGridColumns()
		{
			var locationOfGoodsColumnIndex = LinesGrid.ColumnStyles.Count;
			var existingLocationOfGoodsColumn = LinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == CusTempStorageLine.Schema.TSL_LocationOfGoods);
			if (existingLocationOfGoodsColumn != null)
			{
				locationOfGoodsColumnIndex = LinesGrid.ColumnStyles.IndexOf(existingLocationOfGoodsColumn);
				LinesGrid.ColumnStyles.Remove(existingLocationOfGoodsColumn);
			}
			LinesGrid.ColumnStyles.Insert(locationOfGoodsColumnIndex, new ZDropEditColumnStyleInfo
			{
				ColumnName = CusTempStorageLine.Schema.TSL_LocationOfGoods,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CharacterCasing = CharacterCasing.Upper
			});
		}

		void UpdateLineItemsGridColumns()
		{
			var commodityCodeColumnIndex = LineItemsGrid.ColumnStyles.Count;
			var existingCommodityCodeColumn = LineItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == CusTempStorageLineItem.Schema.TSI_CommodityCode);
			if (existingCommodityCodeColumn != null)
			{
				commodityCodeColumnIndex = LineItemsGrid.ColumnStyles.IndexOf(existingCommodityCodeColumn);
				LineItemsGrid.ColumnStyles.Remove(existingCommodityCodeColumn);
			}
			LineItemsGrid.ColumnStyles.Insert(commodityCodeColumnIndex, new Universal.GUI.TariffColumnStyleInfo
			{
				GetDataGrouping = () => Core.Constants.CountryCodes.France,
				TariffType = Universal.Constants.TariffTypes.Import,
				ColumnName = CusTempStorageLineItem.Schema.TSI_CommodityCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(226)
			});
		}

		public void updateVisibilityOfDDTNumberTextBox(bool isVisible)
		{
			this.DDTNumberTextBox.Visible = isVisible;
		}
	}
}
