using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	class EMCSJobDeclarationLookupsTest : EU.EMCS.Business.Testing.EMCSJobDeclarationLookupsTest
	{
		public void TestCertificateIdentifierList()
		{
			var idList = emcsTestDeclaration.Lookups.CertificateIdentifierList;
			AssertEquals("CertificateIdentifierList should have 0 items", 0, idList.Count);

			var certificateIdentifier = Factory.New<EMCSGlbCompanyCredential>();
			AssertEquals("Default value - code", PasswordTypesList.Codes.IEM, certificateIdentifier.GP_PasswordType);
			AssertEquals("Default value - company", GlbCompany.CurrentCompany.PK, certificateIdentifier.GP_GC);
			AssertEquals("Default value - status", PasswordStatusList.Codes.Invalid, certificateIdentifier.GP_PasswordStatus);

			certificateIdentifier.GP_MailBoxID = "DUBLIN WAREHOUSE 1";
			certificateIdentifier.GP_CertificateSerialNumber = "A0492387J";
			certificateIdentifier.GP_GC = GlbCompany.CurrentCompany.PK;
			certificateIdentifier.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var certificateIdentifier2 = Factory.New<EMCSGlbCompanyCredential>();
			certificateIdentifier2.GP_MailBoxID = "KILKENNY WAREHOUSE";
			certificateIdentifier2.GP_CertificateSerialNumber = "754927U38TZ";
			certificateIdentifier2.GP_GC = GlbCompany.CurrentCompany.PK;
			certificateIdentifier2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var otherGlbPasswordType = Factory.New<GlbExternalPassword>();
			otherGlbPasswordType.GP_PasswordType = "SGC";
			Factory.Save();

			var filter = emcsTestDeclaration.Lookups.CertificateIdentifierList.CompleteFilter;
			Assert(certificateIdentifier.MatchesFilter(filter));

			filter = emcsTestDeclaration.Lookups.CertificateIdentifierList.CompleteFilter;
			Assert(certificateIdentifier2.MatchesFilter(filter));

			filter = emcsTestDeclaration.Lookups.CertificateIdentifierList.CompleteFilter;
			Assert(!otherGlbPasswordType.MatchesFilter(filter));
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsTestDeclaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration emcsTestDeclaration;
	}
}
