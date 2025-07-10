using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(CCF15AWrapper))]
	class CCF15ADeclarationWrapperTest : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CCF15AWrapper>
	{
		public void TestDeclarantTIN_IsDepartureFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR12345678900001", wrapper.DeclarantTIN);
		}

		public void TestPrincipalTIN()
		{
			var organisation = Factory.New<OrgHeader>();
			header.Principal.OrganisationPK = organisation.PK;
			header.Principal.Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			header.Principal.Organisation.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			AssertEquals("FR12345678900001", wrapper.PrincipalTIN);
		}

		public void TestAgreementNumber()
		{
			var org = DeclarationWrapperHelperTest.CreateOrgHeaderWithDTA(Factory, "CC0002");
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CC0002", wrapper.AgreementNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Departure);
			wrapper = new CCF15AWrapper(header);
		}
		CCF15AWrapper wrapper;
		NctsHeader header;

		protected override CCF15AWrapper GetProvider() => wrapper;
	}
}
