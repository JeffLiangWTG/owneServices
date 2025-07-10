using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using GlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAMA_CustomsProfile()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "45", "AA", "11-001");
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "67", "BB", "22-001");
		currentCompany.Factory.Save();

		var header = Factory.New<TemporaryStorageHeader>();

		header.AMA_CustomsProfile = "";
		AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

		header.AMA_CustomsProfile = "98";
		AssertHasMessageError(header.AMA_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

		header.AMA_CustomsProfile = "45";
		AssertNoNotifications(header.AMA_CustomsProfileInfo);
	}

	public void TestCheckAMA_OA_Representative()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			tempHeader.AMA_OA_Representative = ZGuid.Empty;
			tempHeader.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageErrorContaining("Representative is mandatory.", tempHeader.AMA_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);

			tempHeader.AMA_OA_Representative = ZGuid.NewZGuid();
			tempHeader.Validation.ValidateAMA_OA_Representative();
			AssertNoMessageErrorContaining("Representative should not have message error when not empty.", tempHeader.AMA_OA_RepresentativeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestTransportType()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			tempHeader.TransportType = ZString.Empty;
			tempHeader.Validation.ValidateTransportType();
			AssertHasErrorContaining(tempHeader.TransportTypeInfo, MandatoryValidation.MustBeEntered);

			tempHeader.TransportType = "10";
			tempHeader.Validation.ValidateTransportType();
			AssertNoErrorContaining(tempHeader.TransportTypeInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestCheckPresentationCustomsOffice_MandatoryValidation()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			tempHeader.PresentationCustomsOffice = ZString.Empty;
			tempHeader.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageErrorContaining("Presentation Customs office is empty", tempHeader.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			tempHeader.PresentationCustomsOffice = "Test12";
			tempHeader.Validation.ValidateAMA_OA_Representative();
			AssertNoMessageErrorContaining("Presentation Customs office is filled", tempHeader.PresentationCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckDeclarantAndRepresentativeAreDifferent()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var orgAddress = GetTestOrgAddressWithEORI();
		tempHeader.AMA_OA_Declarant = orgAddress.PK;
		tempHeader.AMA_OA_Representative = orgAddress.PK;

		AssertNoNotifications(tempHeader.AMA_OA_DeclarantInfo);
	}

	public void TestCheckRepresentativeAndDeclarantAreDifferent()
	{
		var tempHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var orgAddress = GetTestOrgAddressWithEORI();
		tempHeader.AMA_OA_Declarant = orgAddress.PK;
		tempHeader.AMA_OA_Representative = orgAddress.PK;

		AssertNoNotifications(tempHeader.AMA_OA_RepresentativeInfo);
	}

	public void TestCheckAMA_AgentType()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

		header.AMA_AgentType = ZString.Empty;
		AssertHasErrorContaining(header.AMA_AgentTypeInfo, MandatoryValidation.MustBeEntered);

		header.AMA_AgentType = "XXX";
		AssertHasErrorContaining(header.AMA_AgentTypeInfo, ListValidation.InvalidCodeError);

		header.AMA_AgentType = "DIR";
		AssertNoNotifications(header.AMA_AgentTypeInfo);
	}

	OrgAddress GetTestOrgAddressWithEORI()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		var orgCusCode = orgHeader.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = "EOR";
		orgCusCode.OK_CustomsRegNo = "IT0001";
		return orgAddress;
	}
}
