using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPCertificatesUserControl : ZUserControl
	{
		public RFPCertificatesUserControl()
		{
			InitializeComponent();
		}

		internal void SetupVisibility(ZBool isNEXDOCSActive)
		{
			QL_CommercialProductDescriptionTextBox.Visible = !isNEXDOCSActive;
			QL_HealthCertificateDescriptionTextBox.Visible = !isNEXDOCSActive;
			QL_ImportAuthorityCodeTextBox.Visible = !isNEXDOCSActive;
			QL_SendHCDescCheckBox.Visible = !isNEXDOCSActive;

			QL_AdditionalProductDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, isNEXDOCSActive ? 88 : 71, true);
			CopyMeatInspectionDescriptionFromLineButton.Visible = isNEXDOCSActive;

			QL_MeatInspectionDescriptionTextBox.CaptionResourceString = isNEXDOCSActive ? MeatInspectionDescriptionCaptionForNEXDOCS : MeatInspectionDescriptionCaption;
			QL_MeatInspectionDescriptionTextBox.UpdateCaption();
		}

		void CopyMeatInspectionDescriptionFromLineButton_Click(object sender, EventArgs e)
		{
			CurrentInvoiceLine?.CopyDescriptionToQuarantineLine();
		}

		protected JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentDataItem;
		ResourceStringData MeatInspectionDescriptionCaption => Res.GetData("2705A95B-F09B-4A19-82F6-EA55A1CA463E", "Line Item (Inspection Description)");
		ResourceStringData MeatInspectionDescriptionCaptionForNEXDOCS => Res.GetData("B626D17F-E7D9-4C3B-8E52-963CAD35B96C", englishCaption: "Manual Certificate Product Description", englishFullDescription: "Text entered here will override the default Product description sent in the message.");
	}
}
