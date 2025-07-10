using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class HouseBillPartiesUserControl : ZUserControl
	{
		public HouseBillPartiesUserControl()
		{
			InitializeComponent();
			ConsigneezAddressControl.AllowOverlap(CA_OH_ConsigneeBoundGuidFindBox);
			ConsignorzAddressControl.AllowOverlap(CA_OH_ConsignorBoundGuidFindBox);
		}

		void SetControlVisibility()
		{
			ConsigneezAddressControl.Visible = true;
			ConsignorzAddressControl.Visible = true;
			CA_OH_ConsigneeBoundGuidFindBox.Visible = false;
			CA_OH_ConsignorBoundGuidFindBox.Visible = false;
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
