using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AirCagoHouseBillPartiesUserControl : ZUserControl
	{
		public AirCagoHouseBillPartiesUserControl()
		{
			InitializeComponent();
			ConsigneezAddressControl.AllowOverlap(cS_OH_ConsigneeGuidFindBox);
			ConsignorzAddressControl.AllowOverlap(cS_OH_ConsignorGuidFindBox);
		}

		void SetControlVisibility()
		{
			ConsigneezAddressControl.Visible = true;
			ConsignorzAddressControl.Visible = true;
			cS_OH_ConsigneeGuidFindBox.Visible = false;
			cS_OH_ConsignorGuidFindBox.Visible = false;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			SetzAddressControlOrgListBindings(dataMember);
			base.SetDataBinding(dataSource, dataMember);
			SetControlVisibility();
		}

		void SetzAddressControlOrgListBindings(string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember) && dataMember != ".")
			{
				var prefix = dataMember + ".";
				ConsigneezAddressControl.CorrectBindToOrgList(prefix);
				ConsignorzAddressControl.CorrectBindToOrgList(prefix);
			}
		}
	}
}
