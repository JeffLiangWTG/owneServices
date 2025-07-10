using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceEnterpriseModule))]
	public class LicenceDatabaseModuleForRegistrationTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => Modules.ClientModuleRegistration.LicenceEnterprise;

		public void TestLicenceDatabaseModule()
		{
			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				AssertEquals(form.LicenceDatabaseModuleExposed.ID, Modules.ClientModuleRegistration.LicenceDatabase);
				AssertEquals(form.LicenceDatabaseModuleExposed.SecurityCheckpoint, Env.Security.Organisation);
				AssertEquals(form.LicenceDatabaseModuleExposed.LicenceCheckPoint, Env.Licence.Core);
				AssertEquals(form.LicenceDatabaseModuleExposed.FilterBusinessObject, form.LicenceDatabaseFilterControlExposed.FilterBusinessObject);
				AssertEquals(form.LicenceDatabaseModuleExposed.GridCollection, form.LicenceDatabaseFilterControlExposed.GridCollection);
			}
		}

		class LicenceDatabaseRegistrationWizardFormForTest : LicenceDatabaseRegistrationWizardForm
		{
			public LicenceDatabaseRegistrationWizardFormForTest(LicenceDatabaseRegistrationWizard licenceDatabaseRegistrationWizard)
				: base(licenceDatabaseRegistrationWizard)
			{
			}

			public LicenceDatabaseModuleForRegistration LicenceDatabaseModuleExposed => LicenceDatabaseModule;

			public LicenceDatabaseRegistrationWizardFilterControl LicenceDatabaseFilterControlExposed => licenceDatabaseRegistrationWizardFilterControl;
		}
	}
}
