using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public partial class LocationOfGoodsUserControl : EU.GUI.LocationOfGoodsUserControl
{
	public LocationOfGoodsUserControl()
	{
		InitializeComponent();
	}

	protected override EU.GUI.CusGoodsLocationForm GetCusGoodsLocationForm(EU.Business.ICusGoodsLocationProvider provider)
		=> new CusGoodsLocationForm((ICusGoodsLocationProvider)provider);
}
