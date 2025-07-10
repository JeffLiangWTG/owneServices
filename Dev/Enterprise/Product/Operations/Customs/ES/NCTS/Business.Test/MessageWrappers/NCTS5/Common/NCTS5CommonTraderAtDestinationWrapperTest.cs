using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonTraderAtDestinationWrapperTest : WrapperHelperTest<NCTS5CommonTraderAtDestinationWrapper>
	{
		public void TestGetNewDeclarationAESExporterWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("Header null", GetWrapper(null));

				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("Header with no org address null", GetWrapper(docAddress));

				var address = Factory.New<OrgAddress>();
				docAddress.E2_OA_Address = address.PK;
				AssertNull("Header with org address but no OrgHeader null", GetWrapper(docAddress));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("Header not null", GetWrapper(docAddress));
			});
		}

		public void TestId()
		{
			wrapper = GetWrapper(docAddress);
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

			docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Org Name";
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = GetWrapper(docAddress);
		}
		JobDocAddress docAddress;
		OrgHeader orgHeader;
		NCTS5CommonTraderAtDestinationWrapper wrapper;

		NCTS5CommonTraderAtDestinationWrapper GetWrapper(JobDocAddress jobDocAddress) => NCTS5CommonTraderAtDestinationWrapper.New(jobDocAddress);

		protected override NCTS5CommonTraderAtDestinationWrapper GetProvider() => wrapper;
	}
}
