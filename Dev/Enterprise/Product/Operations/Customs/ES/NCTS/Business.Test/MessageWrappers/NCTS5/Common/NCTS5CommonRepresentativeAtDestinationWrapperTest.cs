using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonRepresentativeAtDestinationWrapperTest : WrapperHelperTest<NCTS5CommonRepresentativeAtDestinationWrapper>
	{
		public void TestGetNewDeclarationAESExporterWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("Header null", GetWrapper(null));

				AssertNotNull("Header not null", GetWrapper(Factory.New<OrgHeader>()));
			});
		}

		public void TestId()
		{
			wrapper = GetWrapper(orgHeader);
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Id when no id declared", ZString.Empty, wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456");
				AssertEquals("Expected PAS Id when category is not NAT and EORI is empty", "ABC123456", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is not empty", "FR22222222", wrapper.Id);

				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected PAS Id when category is NAT and NIF is empty", "ABC123456", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				AssertEquals("Expected NIF Id when category is NAT", "NIF22222222", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Org Name";
			orgHeader.Addresses.AddNew();

			wrapper = GetWrapper(orgHeader);
		}
		OrgHeader orgHeader;
		NCTS5CommonRepresentativeAtDestinationWrapper wrapper;

		NCTS5CommonRepresentativeAtDestinationWrapper GetWrapper(OrgHeader orgHeader) => NCTS5CommonRepresentativeAtDestinationWrapper.New(orgHeader);

		protected override NCTS5CommonRepresentativeAtDestinationWrapper GetProvider() => wrapper;
	}
}
