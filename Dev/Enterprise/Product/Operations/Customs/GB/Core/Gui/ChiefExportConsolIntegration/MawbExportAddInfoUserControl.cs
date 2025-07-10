using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirInventory
{
	public partial class MawbExportAddInfoUserControl : ZUserControl
	{
		public CustomsExportConsolIntegrationWrapper ConsolWrapper { get; set; }

		public MawbExportAddInfoUserControl()
		{
			InitializeComponent();
		}

		void MucrCalcButton_Click(object sender, System.EventArgs e)
		{
			ConsolWrapper?.CalculateMasterUCR(false);
		}
	}
}
