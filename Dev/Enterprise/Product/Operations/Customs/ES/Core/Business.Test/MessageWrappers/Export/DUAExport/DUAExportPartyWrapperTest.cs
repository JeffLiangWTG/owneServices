using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DUAExportPartyWrapperTest : WrapperHelperTest<DUAExportPartyWrapper>
	{
		public void TestGetNewDUAExportAddressInformationWrapperIfNotNull()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Header not null", DUAExportPartyWrapper.New(Factory.New<OrgHeader>()));
				AssertNull("OrgHeader null", DUAExportPartyWrapper.New(null));
			});
		}

		public void TestOrganizationCodeQualifier()
		{
			CombineAssertions(() =>
			{
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected P OrganizationCodeQualifier", "P", wrapper.OrganizationCodeQualifier);

				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected empty OrganizationCodeQualifier for Category not NAT", ZString.Empty, wrapper.OrganizationCodeQualifier);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = OrgHeaderData.Code;
			wrapper = DUAExportPartyWrapper.New(orgHeader);
		}

		OrgHeader orgHeader;
		DUAExportPartyWrapper wrapper;

		protected override DUAExportPartyWrapper GetProvider() => wrapper;
	}
}
