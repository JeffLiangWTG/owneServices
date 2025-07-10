using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirConsignmentUserControlMawb : ZUserControl
	{
		public CcsukAirConsignmentUserControlMawb()
		{
			InitializeComponent();
			ccsukMasterControl1.ChangeParentFromHawbToMawb();
			ccsukAirportsAndPartiesControl1.AllowOutsideOfParent();
			ccsukAirportsAndPartiesControl1.AllowOverlap(ccsukMasterControl1);
		}

		public CcsukAirConsignmentUserControlMawb(CusMAWB cusMawb)
			: this()
		{
			ccsukMainMasterUserControlHelpers1.SetCusMawb(cusMawb);
		}
	}
}
