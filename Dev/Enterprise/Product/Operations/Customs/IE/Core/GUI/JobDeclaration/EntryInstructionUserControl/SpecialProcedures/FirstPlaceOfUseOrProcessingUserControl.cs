using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class FirstPlaceOfUseOrProcessingUserControl : EU.GUI.FirstPlaceOfUseOrProcessingUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("B4404410-E124-451E-AE4D-BBA9CC38B04B",
				"[Art. 163 4/5] First Place of Processing",
				"[Article 163 4/5] First Place of Processing");
			}
			return base.GroupBoxCaption();
		}
	}
}
