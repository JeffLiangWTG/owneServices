using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public class PrimaryOwnerOfGoodsUserControl : EU.GUI.PrimaryOwnerOfGoodsUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("FE440F4C-2D04-4EE7-9701-A619317833FE",
				"[Art. 163 3/8] Primary Owner of Goods",
				"[Article 163 3/8] Primary Owner of Goods");
			}
			return base.GroupBoxCaption();
		}
	}
}
