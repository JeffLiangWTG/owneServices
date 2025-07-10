using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IdentityTenant.GUI
{
	public partial class EdiIdentityTenantForm : ZTemplateForm
	{
		public EdiIdentityTenantForm(EdiIdentityTenant tenant)
			: base(tenant)
		{
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		EdiIdentityTenant Tenant => (EdiIdentityTenant)base.BusinessEntity;

		protected override ContinueWithSave ValidateAndSave()
		{
			if (OnboardingCheckBox.Checked && Tenant.OnboardingEdiIdentityTenant != null)
			{
				var dialogResult = Globals.Message.Show(
					(NoResString)$"When you set this Onboarding to true, the Onboarding of tenant '{Tenant.OnboardingEdiIdentityTenant.IDT_TenantId}' will automatically be set to false. Only one Onboarding can be active at a time.",
					(NoResString)"Tenant",
					MessageBoxButtons.YesNo,
					DialogResult.No);
				if (dialogResult == DialogResult.No)
				{
					return ContinueWithSave.No;
				}
			}

			return base.ValidateAndSave();
		}
	}
}
