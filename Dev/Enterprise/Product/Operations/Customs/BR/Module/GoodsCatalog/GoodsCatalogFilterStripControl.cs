using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Module
{
	public partial class GoodsCatalogFilterStripControl : Customs.Module.GoodsCatalogFilterStripControl
	{
		public GoodsCatalogFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			AddGridColumns();
		}

		protected void AddGridColumns()
		{
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("6230CDE6-6CDA-4964-96F8-499E995BC800", "Local Part Numbers"),
					ColumnName = CusGoodsCatalog.Schema.LocalPartNumbersConcatenated,
					IsReadOnly = true,
					IsVisible = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				}
			);
		}
	}
}
