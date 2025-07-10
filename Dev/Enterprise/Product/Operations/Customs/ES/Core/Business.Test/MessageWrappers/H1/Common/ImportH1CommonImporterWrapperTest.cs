using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonImporterWrapperTest : WrapperHelperTest<ImportH1CommonImporterWrapper>
{
	public void TestGetNewImportH1CommonImporterWrapper()
	{
		CombineAssertions(() =>
		{
			AssertNull("JobDocAddress null", ImportH1CommonImporterWrapper.New(null));

			var docAddress = Factory.New<JobDocAddress>();
			AssertNull("JobDocAddress with no org address null", ImportH1CommonImporterWrapper.New(docAddress));

			var address = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = address.PK;
			AssertNull("JobDocAddress with org address but no OrgHeader null", ImportH1CommonImporterWrapper.New(docAddress));

			var org = Factory.New<OrgHeader>();
			address.OA_OH = org.PK;
			AssertNotNull("JobDocAddress not null", ImportH1CommonImporterWrapper.New(docAddress));
		});
	}

	public void TestId()
	{
		wrapper = GetWrapper(jobDocAddress);
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

	public void TestName()
	{
		orgHeader.OH_FullName = OrgHeaderData.Name;
		AssertEquals("Expected empty Name", ZString.Empty, wrapper.Name);
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			AssertNull("Expected null Address when no id declared", wrapper.Address);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "33333333", "FR");
			var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456", "ES");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			wrapper = GetWrapper(jobDocAddress);
			var address = wrapper.Address;
			AssertNotNull("Expected filled Address when Id PAS (category is NAT)", wrapper.Address);
			AssertSame("Cached Address", wrapper.Address, address);
			AssertEquals("Expected filled Address correctly", "Address Other", address.Address);

			jobDocAddress.Address.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Address.Country);

			jobDocAddress.Address.OA_Address1 = "address1";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected address filled correctly.", "address1", wrapper.Address.Address);

			jobDocAddress.Address.OA_City = "city";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected city filled correctly.", "city", wrapper.Address.City);

			jobDocAddress.Address.OA_PostCode = "PostCode";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected post code filled correctly.", "PostCode", wrapper.Address.PostCode);

			var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			wrapper = GetWrapper(jobDocAddress);
			AssertNull("Expected null Address when Id is NIF", wrapper.Address);

			orgHeader.CustomsCodes.Remove(cusCode2);
			cusCode.OK_CustomsRegNo = "";
			wrapper = GetWrapper(jobDocAddress);
			AssertNull("Expected null Address when id declared is empty", wrapper.Address);

			orgHeader.OH_Category = OrgConstants.Category.Business;
			wrapper = GetWrapper(jobDocAddress);
			AssertNull("Expected null Address when Id is not the PAS (EORI when category is not NAT)", wrapper.Address);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Org Name";
		orgHeader.OH_Code = "TestOrg";

		var orgAddressMain = orgHeader.MainAddress;
		orgAddressMain.OA_Address1 = "Address Main";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address Other";

		jobDocAddress = declaration.SupplierDocumentaryAddress;
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		jobDocAddress.OrganisationPK = orgHeader.PK;

		wrapper = GetWrapper(jobDocAddress);
	}
	OrgHeader orgHeader;
	JobDocAddress jobDocAddress;
	ImportH1CommonImporterWrapper wrapper;

	ImportH1CommonImporterWrapper GetWrapper(JobDocAddress jobDocAddress) => ImportH1CommonImporterWrapper.New(jobDocAddress);

	protected override ImportH1CommonImporterWrapper GetProvider() => wrapper;
}
