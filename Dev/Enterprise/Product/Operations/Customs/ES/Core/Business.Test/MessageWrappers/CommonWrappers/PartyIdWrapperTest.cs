using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PartyIdWrapperTest : WrapperHelperTest<PartyIdWrapper>
	{
		public void TestGetNewPartyIdWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("Header null", PartyIdWrapper.New((OrgHeader)null));
				var org = Factory.New<OrgHeader>();
				AssertNotNull("Header not null", PartyIdWrapper.New(Factory.New<OrgHeader>()));

				AssertNull("OrgAddress null", PartyIdWrapper.New((OrgAddress)null));
				var address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", PartyIdWrapper.New(address));

				AssertNull("JobDocAddress null", PartyIdWrapper.New((JobDocAddress)null));
				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", PartyIdWrapper.New(docAddress));
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", PartyIdWrapper.New(docAddress));
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", PartyIdWrapper.New(address));
				AssertNotNull("JobDocAddress not null", PartyIdWrapper.New(docAddress));
			});
		}

		public void TestId()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Id", ZString.Empty, wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", wrapper.Id);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.Id);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", wrapper.Id);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			wrapper = PartyIdWrapper.New(orgHeader);
		}
		OrgHeader orgHeader;
		PartyIdWrapper wrapper;

		protected override PartyIdWrapper GetProvider() => wrapper;
	}
}
