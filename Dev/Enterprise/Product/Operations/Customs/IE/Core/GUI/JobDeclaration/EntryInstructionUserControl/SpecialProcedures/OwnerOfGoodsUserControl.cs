using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class OwnerOfGoodsUserControl : EU.GUI.OwnerOfGoodsUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("F29FF561-BDBA-4DF2-881D-9BC2B78C291F",
				"[Art. 163 3/8] Owner of Goods",
				"[Article 163 3/8] Owner of Goods");
			}
			return base.GroupBoxCaption();
		}
	}
}
