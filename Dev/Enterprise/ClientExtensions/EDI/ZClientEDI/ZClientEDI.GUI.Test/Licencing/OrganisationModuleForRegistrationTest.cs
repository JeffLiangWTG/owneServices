using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceEnterpriseModule))]
	public class OrganisationModuleForRegistrationTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => Modules.ClientModuleRegistration.LicenceEnterprise;

		public void TestOrganisationModule()
		{
			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				AssertEquals(form.OrganisationModuleExposed.ID, ModuleIDs.Organisation);
				AssertEquals(form.OrganisationModuleExposed.SecurityCheckpoint, Env.Security.Organisation);
				AssertEquals(form.OrganisationModuleExposed.LicenceCheckPoint, Env.Licence.Core);
				AssertEquals(form.OrganisationModuleExposed.FilterBusinessObject, form.OrganisationWizardFilterControlExposed.FilterBusinessObject);
				AssertEquals(form.OrganisationModuleExposed.GridCollection, form.OrganisationWizardFilterControlExposed.GridCollection);
			}
		}

		class LicenceDatabaseRegistrationWizardFormForTest : LicenceDatabaseRegistrationWizardForm
		{
			public LicenceDatabaseRegistrationWizardFormForTest(LicenceDatabaseRegistrationWizard licenceDatabaseRegistrationWizard)
				: base(licenceDatabaseRegistrationWizard)
			{
			}

			public OrganisationModuleForRegistration OrganisationModuleExposed => OrganisationModule;

			public OrganisationWizardFilterControl OrganisationWizardFilterControlExposed => organisationWizardFilterControl;
		}
	}
}
