using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISProducerCodeForm : ZChildForm
	{
		public AQISProducerCodeForm(IAQIS aQIS) : base(aQIS)
		{
			fAQIS = aQIS;
			zGrid1.GridId = "GridLayoutLJyjvt06xHjWp3458eWURQ==";
			var zCodeFindBoxColumnStyleInfo = (ZCodeFindBoxColumnStyleInfo)zGrid1.GetColumnStyle(nameof(AQISProducerCode.Code));
			zCodeFindBoxColumnStyleInfo.ModuleID = AQISProducerCodeLookups.AQISProducerModuleId;
		}

		readonly IAQIS fAQIS;

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			fAQIS.AddInfo.ReBuildAQISProducerCodes();
			Close();
		}
	}
}
