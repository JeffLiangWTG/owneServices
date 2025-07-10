using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class PeriodForDischargeUserControl : EU.GUI.PeriodForDischargeUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("5D64CA79-238D-429D-88AD-38E634F1BEC0",
				"[Article 163 4/17] Period for Discharge");
			}
			return base.GroupBoxCaption();
		}
	}
}
