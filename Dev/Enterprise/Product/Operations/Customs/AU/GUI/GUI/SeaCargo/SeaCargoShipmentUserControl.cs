using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoShipmentUserControl : ZUserControl
	{
		public SeaCargoShipmentUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			ChangeVisibility();
		}

		void ChangeVisibility()
		{
			if (CurrentDataItem is CusSCAHouse)
			{
				UnderbondAndPackingUserControlForCMR.Visible = true;
				HouseDetailsUsersControl.ParentBillLabel.Visible = true;
				HouseDetailsUsersControl.ParentBillTextBox.Visible = true;
				HouseDetailsUsersControl.ShippingLineGuidBoundFindBox.Visible = true;
				HouseDetailsUsersControl.ShippingLineLabel.Visible = true;
				HouseDetailsUsersControl.PrincipalIDLabel.Visible = true;
				HouseDetailsUsersControl.CB_PrincipalIDBoundTextBox.Visible = true;
			}
		}
	}
}
