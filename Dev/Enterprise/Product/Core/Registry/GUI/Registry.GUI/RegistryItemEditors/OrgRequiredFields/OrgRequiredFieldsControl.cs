using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgRequiredFieldsControl : ZUserControl
	{
		public OrgRequiredFieldsControl(bool isDebtorRequiredField = false)
		{
			InitializeComponent();
			this.RequirePhoneOrBusinessNoCheckEdit.Text = Res.GetString("e7f803a1-3b32-4ef5-872a-889c00eba154", "Require Phone or Business/{0}#: *", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			this.RequireBusinessNoCheckEdit.Text = Res.GetString("6e680bd1-6c14-48f4-904b-69dbe5d5cdb2", "Require Business/{0} #: *", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			this.RequireARContactCheckEdit.Visible = isDebtorRequiredField;
		}

		public void SetFields(OrgRequiredFields fields)
		{
			RequireAddressCheckEdit.Checked = fields.RequireAddress2;
			RequireBranchCheckEdit.Checked = fields.RequireBranch;
			RequireCityCheckEdit.Checked = fields.RequireCity;
			RequirePhoneNumberCheckEdit.Checked = fields.RequirePhoneNumber;
			RequirePhoneOrBusinessNoCheckEdit.Checked = fields.RequirePhoneOrBusinessNumber;
			RequireBusinessNoCheckEdit.Checked = fields.RequireBusinessNumber;
			RequireEmailCheckEdit.Checked = fields.RequireEmailAddress;
			RequireWebCheckEdit.Checked = fields.RequireWebAddress;
			RequireFaxNumberCheckEdit.Checked = fields.RequireFaxNumber;
			RequireFaxEmailOrWebCheckEdit.Checked = fields.RequireFaxEmailOrWeb;
			RequireARContactCheckEdit.Checked = fields.RequireARContact;
		}

		public OrgRequiredFields GetFields()
		{
			return new OrgRequiredFields(RequireAddressCheckEdit.Checked, RequireBranchCheckEdit.Checked, RequireCityCheckEdit.Checked, RequirePhoneNumberCheckEdit.Checked, RequireBusinessNoCheckEdit.Checked, RequirePhoneOrBusinessNoCheckEdit.Checked, RequireFaxNumberCheckEdit.Checked, RequireEmailCheckEdit.Checked, RequireWebCheckEdit.Checked, RequireFaxEmailOrWebCheckEdit.Checked, RequireARContactCheckEdit.Checked);
		}
	}
}
