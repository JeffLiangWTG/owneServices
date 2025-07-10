using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISEntityIdForm : ZChildForm
	{
		public AQISEntityIdForm(IAQIS aQIS) : base(aQIS)
		{
			fAQIS = aQIS;
			zGrid1.GridId = "GridLayoutMZx1QjgxFru6vgbjS0WTuw==";
		}

		readonly IAQIS fAQIS;

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			fAQIS.AddInfo.ReBuildAQISEntityIds();
			Close();
		}
	}
}
