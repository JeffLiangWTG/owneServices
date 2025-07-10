using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	/// <summary>
	/// Summary description for CMRDepotShipmentUserControl.
	/// </summary>
	public partial class CMRDepotShipmentUserControl : ZUserControl
	{
		public CMRDepotShipmentUserControl(ModuleIdentifier moduleID)
		{
			InitializeComponent();
			this.zGuidFindBox2.ModuleID = moduleID;
			this.zGuidFindBox1.ModuleID = moduleID;
		}
	}
}
