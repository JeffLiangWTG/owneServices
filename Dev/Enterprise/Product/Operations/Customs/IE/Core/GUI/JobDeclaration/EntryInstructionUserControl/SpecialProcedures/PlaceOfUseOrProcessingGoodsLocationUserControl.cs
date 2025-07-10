using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class PlaceOfUseOrProcessingGoodsLocationUserControl : EU.GUI.PlaceOfUseOrProcessingGoodsLocationUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("BF8C2455-6D2F-4F9E-939C-700D0A808E5F",
				"[Article 163 4/9] Place(s) of Use or Processing");
			}
			return base.GroupBoxCaption();
		}
	}
}
