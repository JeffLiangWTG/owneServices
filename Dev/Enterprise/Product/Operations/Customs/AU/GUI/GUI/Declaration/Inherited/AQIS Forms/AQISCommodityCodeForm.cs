using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISCommodityCodeForm : ZChildForm
	{
		public AQISCommodityCodeForm(IAQIS aQIS) : base(aQIS)
		{
			fAQIS = aQIS;
			zGrid1.GridId = "GridLayoutcg+K4aS7mI14HUJQMhx++A==";
		}

		readonly IAQIS fAQIS;

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			fAQIS.AddInfo.ReBuildAQISCommodityCodes();
			Close();
		}
	}
}
