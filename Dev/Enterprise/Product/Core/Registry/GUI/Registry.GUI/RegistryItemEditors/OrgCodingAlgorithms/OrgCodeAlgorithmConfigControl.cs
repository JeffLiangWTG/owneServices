using System;
using System.Windows.Forms;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class OrgCodeAlgorithmConfigControl : RegistryBusinessObjectTemplateZUserControl
	{
		readonly OrgCodeAlgorithmType algorithmType;

		public OrgCodeAlgorithmConfigControl(OrgCodeAlgorithmType algorithmType)
		{
			this.algorithmType = algorithmType;
			InitializeComponent();
		}

		void RegenerateButton_Click(object sender, EventArgs e)
		{
			RegistryForm registryForm = FindForm() as RegistryForm;
			if (registryForm != null)
			{
				if (registryForm.HasChanges)
				{
					Globals.Message.ShowInformation(Res.GetString("f28a2daa-e902-481e-8a9e-f42d4bd9c99e", "Please save all changes first before regenerating organization codes."), Res.GetString("3a6f1a0a-b80d-416f-aca4-02234bb9c6d5", "Please Save First"));
				}
				else if (Globals.Message.ShowConfirmation(Res.GetString("52027d7a-0f89-4380-92aa-7c4c3ff58d93", "All organization codes that this algorithm applies to will be regenerated, including manually set codes. The process may take a few minutes. Do you wish to continue?"), Res.GetString("eec04006-0e29-474c-885a-e3de1af68b00", "Regenerate Codes"), Res.GetString("ddeb32b4-6cec-4035-ac88-262f64ed1041", "Yes"), MessageBoxIcon.Question) == DialogResult.OK)
				{
					using (OrgCodeUpdaterForm form = new OrgCodeUpdaterForm())
					{
						ZFormModaliser.Show(form, registryForm);
						form.UpdateOrgs(algorithmType);
					}
				}
			}
		}
	}
}
