using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class ConditionsAndTermsUserControl : EU.GUI.ConditionsAndTermsUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration)
			{
				return Res.GetData("672FAEB4-DFC3-4426-956E-489CAD5053EB",
			   "[Art. 163 6/2] Conditions and Terms",
			   "[Article 163 6/2] Conditions and Terms");
			}
			return base.GroupBoxCaption();
		}
	}
}
