using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIOrganisationSecurityContainerControlForTest))]
	public class EDIOrganisationSecurityContainerControlTest : OrganisationSecurityContainerControlBaseTest
	{
		class EDIOrganisationSecurityContainerControlForTest : EDIOrganisationSecurityContainerControl
		{
			public EDIOrganisationSecurityContainerControlForTest()
				: base()
			{
				IsModifyLicence = true;
			}
		}

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new EDIOrganisationSecurityContainerControlForTest();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyLicence" }; }
		}

		protected override OrgHeader LoadAndSetupDEMOrgHeaderForTest()
		{
			EDIOrgHeader dEMORG = (EDIOrgHeader)base.LoadAndSetupDEMOrgHeaderForTest();
			dEMORG.CreateAndLoadLicenceForOrg();
			return dEMORG;
		}
	}
}
