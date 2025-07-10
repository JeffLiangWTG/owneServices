using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public class IdentificationOfGoodsUserControl : EU.GUI.IdentificationOfGoodsUserControl
	{
		protected override ResourceStringData GetIdentificationofGoodsGroupSubBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("EC978098-1039-4581-A8C8-25A359E97EA4",
				"[Art. 163 5/8] Identification of Goods",
				"[Article 163 5/8] Identification of Goods");
			}
			return base.GetIdentificationofGoodsGroupSubBoxCaption();
		}

		protected override ResourceStringData GetProcessedProductsGroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("4DF55A66-6E92-436B-99B1-18700AF3BF35",
				"[Art. 163 5/7] Processed Products",
				"[Article 163 5/7] Processed Products");
			}
			return base.GetProcessedProductsGroupBoxCaption();
		}
	}
}
