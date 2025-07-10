using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Module.Testing
{
	[TestedType(typeof(MailItemTemplateModule))]
	public class MailItemTemplateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.MailItemTemplate;
		}

		public void TestCheckpoints()
		{
			using (MailItemTemplateModule module = new MailItemTemplateModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.EmailTemplates, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
			}
		}
	}
}
