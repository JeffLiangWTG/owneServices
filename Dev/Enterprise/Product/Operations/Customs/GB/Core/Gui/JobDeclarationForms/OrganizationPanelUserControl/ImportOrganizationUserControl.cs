using System;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.GUI
{
	public partial class ImportOrganizationUserControl : EU.GUI.ImportOrganizationUserControl
	{
		public ImportOrganizationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetCaptions();
		}

		public void SetCaptions()
		{
			RepresentativeAddressControl.CaptionResourceString = Declaration?.CaptionResourceStringForProperty(nameof(JobDeclaration.JE_OA_Representative));
			SellerAddressControl.CaptionResourceString = Declaration?.CaptionResourceStringForProperty(nameof(JobDeclaration.JE_OA_SellerAddress));
			ManufacturerAddressControl.CaptionResourceString = Declaration?.CaptionResourceStringForProperty(nameof(JobDeclaration.JE_OA_ManufacturerAddress));
			DeclarantOfficeAddressControl.CaptionResourceString = Declaration?.CaptionResourceStringForProperty(nameof(JobDeclaration.JE_OA_DeclarantAddress));
			SupervisingOfficeDocAddress.CaptionResourceString = Declaration?.CaptionResourceStringForProperty(nameof(JobDeclaration.SupervisingOfficeDocAddress));
		}

		void SpoffSetterButton_Click(object sender, EventArgs e)
		{
			Declaration.SetSupervisingOfficeFromDeclarant();
		}

		JobDeclaration Declaration => ((JobDeclaration)CurrentDataItem);
	}
}
