using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationAESExporterWrapperTest : WrapperHelperTest<DeclarationAESExporterWrapper>
{
	public void TestGetNewDeclarationAESExporterWrapper()
	{
		CombineAssertions(() =>
		{
			AssertNull("JobDocAddress null", DeclarationAESExporterWrapper.New(null));

			var docAddress = Factory.New<JobDocAddress>();
			AssertNull("JobDocAddress with no org address null", DeclarationAESExporterWrapper.New(docAddress));

			var address = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = address.PK;
			AssertNull("JobDocAddress with org address but no OrgHeader null", DeclarationAESExporterWrapper.New(docAddress));

			var org = Factory.New<OrgHeader>();
			address.OA_OH = org.PK;
			AssertNotNull("JobDocAddress not null", DeclarationAESExporterWrapper.New(docAddress));
		});
	}

	public void TestAddress()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

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

			jobDocAddress.Address.OA_RN_NKCountryCode = "RS";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.Address.Country);

			jobDocAddress.Address.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Address.Country);

			jobDocAddress.Address.OA_RN_NKCountryCode = "MQ";
			wrapper = GetWrapper(jobDocAddress);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.Address.Country);

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

		declaration = Factory.New<JobDeclaration>();

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
	DeclarationAESExporterWrapper wrapper;
	JobDeclaration declaration;

	DeclarationAESExporterWrapper GetWrapper(JobDocAddress jobDocAddress) => DeclarationAESExporterWrapper.New(jobDocAddress, declaration);

	protected override DeclarationAESExporterWrapper GetProvider() => wrapper;
}
