using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class CommonBillPartiesUserControl : ZUserControl
	{
		public CommonBillPartiesUserControl()
		{
			InitializeComponent();
		}

		void ConvertShipperToOrganizationButton_Click(object sender, EventArgs e)
		{
			ShipperAddressControl.ShowEditOrViewForm();
		}

		void ConvertConsigneeToOrganizationButton_Click(object sender, EventArgs e)
		{
			ConsigneeAddressControl.ShowEditOrViewForm();
		}

		void ConvertNotifyPartyToOrganizationButton_Click(object sender, EventArgs e)
		{
			NotifyPartyAddressControl.ShowEditOrViewForm();
		}

		void ConvertBuyerToOrganizationButton_Click(object sender, EventArgs e)
		{
			BuyerAddressControl.ShowEditOrViewForm();
		}

		void ConvertSellerToOrganizationButton_Click(object sender, EventArgs e)
		{
			SellerAddressControl.ShowEditOrViewForm();
		}
	}
}
