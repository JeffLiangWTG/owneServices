using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class BillOfDischargeUserControl : EU.GUI.BillOfDischargeUserControl
	{
		protected override ResourceStringData GroupBoxCaption()
		{
			if (this.BindingSource?.DataSource is JobDeclaration jobDeclaration && jobDeclaration.IsUCC5AndIsImport)
			{
				return Res.GetData("8291D5EA-3CF9-402F-B208-ED21DF4285E4",
				"[Art. 163 4/18] Bill of Discharge",
				"[Article 163 4/18] Bill of Discharge");
			}
			return base.GroupBoxCaption();
		}
	}
}
