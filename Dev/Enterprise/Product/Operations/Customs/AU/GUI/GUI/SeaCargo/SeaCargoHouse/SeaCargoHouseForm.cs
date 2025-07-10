using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoHouseForm : Declaration.GUI.CMRMessagingForm
	{
		public SeaCargoHouseForm(CusSCAHouse houseBill)
			: base(houseBill)
		{
		}

		CusSCAHouse HouseBill => (CusSCAHouse)BusinessEntity;

		public override string FormCaption => Declaration.GUI.Res.GetString("EA0C8E9F-9827-47CA-8855-C1BE8161430F", "Sea Cargo House {0}", HouseBill.CA_HouseBill);

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			WorkflowTabPage.Initialize(HouseBill);
		}

		protected override MenuItem GetMessagingMenu()
		{
			if (seaCargoMenu == null)
			{
				seaCargoMenu = new SeaCargoHouseMenu((CusSCAHouseMessageManager)Manager);
				seaCargoMenu.Text = Declaration.GUI.Res.GetString("88EC9D98-4757-4FAA-889F-9D876A8D39EF", "Sea Cargo");
			}
			return seaCargoMenu;
		}
		protected SeaCargoHouseMenu seaCargoMenu;

		protected override Business.MultiMessageManager GetManager()
		{
			return new CusSCAHouseMessageManager(HouseBill);
		}

		protected override bool AllowNew => false;
	}
}
