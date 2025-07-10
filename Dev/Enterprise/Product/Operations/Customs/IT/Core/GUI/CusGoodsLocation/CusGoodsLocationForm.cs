using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public partial class CusGoodsLocationForm : EU.GUI.CusGoodsLocationForm
{
	public CusGoodsLocationForm(ICusGoodsLocationProvider provider) : base(provider)
	{
		this.provider = provider;
	}

	readonly ICusGoodsLocationProvider provider;

	void ClearFieldsButton_Click(object sender, System.EventArgs e)
	{
		provider.ClearGoodsLocation();
	}
}
