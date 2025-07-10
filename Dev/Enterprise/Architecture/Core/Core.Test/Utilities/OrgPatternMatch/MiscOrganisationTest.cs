namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class MiscOrganisationTest : CreateSystemOrgScriptsTest
	{
		protected override CreateSystemOrgScripts Scripts
		{
			get { return new CreateMISCOrgScripts(); }
		}
	}
}
