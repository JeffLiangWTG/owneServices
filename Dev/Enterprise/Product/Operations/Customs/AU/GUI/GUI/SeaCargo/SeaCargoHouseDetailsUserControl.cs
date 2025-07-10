using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	/// <summary>
	/// Summary description for SeaCargoHouseDetailsUserControl.
	/// </summary>
	public partial class SeaCargoHouseDetailsUserControl : ZUserControl
	{
		public SeaCargoHouseDetailsUserControl()
		{
			InitializeComponent();
		}

		CusSCAHouse House => (CusSCAHouse)CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			this.overrideFreightDefaultsCheckBox.Visible = House?.OceanBill?.OverrideFreightDefaultsVisible ?? false;
		}
	}
}
