using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.GUI.PlugIn
{
	public class ImportInvoiceLineOrganizationsUserControl
		: EU.GUI.PlugIn.ImportInvoiceLineOrganizationsUserControl
	{
		public ImportInvoiceLineOrganizationsUserControl()
		{
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			InitControls();
		}

		public void InitControls()
		{
			if (BindingSource.Current is JobDeclaration declaration && declaration.IsUCC5AndIsImport)
			{
				ConsignorAddressControl.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("F2FD97F7-4291-4454-B56E-56481DDAE0A7", "Exporter");

				ConsigneeAddressControl.Visible = false;
				BuyerDocAddressControl.Visible = true;
				BuyerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 44, true);
				SellerDocAddressControl.Visible = true;
				SellerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 70, true);
				SupplyChainActorReferencesUserControl.Visible = false;
			}
		}
	}
}
