using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class HouseBillDetailsUserControl : ZUserControl
	{
		public HouseBillDetailsUserControl()
		{
			InitializeComponent();
		}

		CusSCAHouse HouseBill => (CusSCAHouse)CurrentDataItem;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateForOceanBillUnpack();
		}

		public void UpdateForOceanBillUnpack()
		{
			var isUnpack = HouseBill?.OceanBill?.CB_MultiOBLUnpack ?? false;
			var labelText = isUnpack ? CMRSeaCargoUserControl.OceanBillLabelText : CMRSeaCargoUserControl.HouseBillLabelText;
			GroupBoxHouseBill.Text = labelText;
			HouseBillLabel.Text = labelText + ":";
		}

		void DetailsButton_Click(object sender, EventArgs e)
		{
			var customsInfo = HouseBill.UserFriendlyStatuses;
			if (!customsInfo.IsEmpty)
			{
				Globals.Message.ShowInformation(customsInfo, UserFriendlyStatusMessages.StatusMessageHeader);
			}
			else
			{
				Globals.Message.ShowInformation(UserFriendlyStatusMessages.StatusNotAvailable, UserFriendlyStatusMessages.StatusMessageHeader);
			}
		}
	}
}
