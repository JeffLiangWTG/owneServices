using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISPermitNumberForm : ZChildForm
	{
		public AQISPermitNumberForm(IAQIS aQIS) : base(aQIS)
		{
			fAQIS = aQIS;
			zGrid1.GridId = "GridLayoutTLMvr7AiGn/fR12HLjl/WQ==";
		}

		readonly IAQIS fAQIS;

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			fAQIS.AddInfo.ReBuildAQISPermitIds();
			Close();
		}
	}
}
