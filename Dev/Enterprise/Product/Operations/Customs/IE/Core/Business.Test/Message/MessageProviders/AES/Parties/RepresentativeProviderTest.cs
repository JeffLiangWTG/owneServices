using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class RepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<RepresentativeProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null - OrgAddress", RepresentativeProvider.New((OrgAddress)null, ZString.Empty));
				AssertNull("Null - JobDocAddress", RepresentativeProvider.New((JobDocAddress)null, ZString.Empty));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestStatus()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("1", GetProvider().Status);

			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("2", GetProvider().Status);

			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			AssertEquals("3", GetProvider().Status);
		}

		public void TestId()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Germany);
			AssertEquals("DEREG222", Provider.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG333", Core.Constants.CountryCodes.Ireland);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG444", Core.Constants.CountryCodes.Swaziland);
			AssertEquals("IEREG333", GetProvider().Id);
		}

		public void TestContact()
		{
			orgContact.OC_ContactName = "TestContact";
			AssertEquals("Contact", "TestContact", Provider.Contact.Name);
		}

		protected override RepresentativeProvider GetProvider() => RepresentativeProvider.New(orgAddress, jobDeclaration.JE_DeclarantType);

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();

			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

			orgHeader = Factory.New<OrgHeader>();

			orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			jobDeclaration.JE_OA_Representative = orgAddress.PK;

			orgContact = Factory.New<OrgContact>();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_OA_OrgAddress = orgAddress.PK;
		}

		protected JobDeclaration jobDeclaration;
		protected OrgHeader orgHeader;
		protected OrgAddress orgAddress;
		protected OrgContact orgContact;
	}
}
