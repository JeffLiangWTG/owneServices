using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSJobDeclarationLookupsTest : EU.EMCS.Business.Testing.EMCSJobDeclarationLookupsTest
	{
		public void TestCertificateIdentifierList()
		{
			var idList = emcsTestDeclaration.Lookups.Credentials;
			AssertEquals("Credentials should have 0 items", 0, idList.Count);

			TestHelper.CreateEMCSCredential(Factory, GlbCompany.CurrentCompany.PK, "EM1", "123456789", PasswordTypesList.Codes.CDS);
			TestHelper.CreateEMCSCredential(Factory, GlbCompany.CurrentCompany.PK, "EM2", "123456789", PasswordTypesList.Codes.CDS);
			idList = emcsTestDeclaration.Lookups.Credentials;
			AssertEquals("Credentials should have 0 items as cache not updated", 0, idList.Count);
			Factory.ClearCachedValue<CodeDescriptionPairList>($"EMCSCredentialCollection-{GlbCompany.CurrentCompany.PK.ToGuid()}");
			idList = emcsTestDeclaration.Lookups.Credentials;
			AssertEquals("Credentials should have 2 items", 2, idList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsTestDeclaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration emcsTestDeclaration;
	}
}
