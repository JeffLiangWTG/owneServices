using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class RequestForVatPartyInformationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new RequestForVatPartyInformationWrapper(null));
	}

	public void TestJobReference()
	{
		AssertEquals("JobReference", "decRef", wrapper.JobReference);
	}

	public void TestFunctionCode()
	{
		AssertEquals("90", wrapper.FunctionCode);
	}

	public void TestConsignee()
	{
		var orgHeaderControllingAgent = Factory.New<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderControllingAgent, Core.Constants.CountryCodes.Netherlands, "7654321");

		declaration.JE_OH_ControllingAgent = orgHeaderControllingAgent.PK;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;

		AssertEquals("Consignee", "NL7654321", wrapper.Consignee);
	}

	public void TestDomesticDutyTaxParty()
	{
		var orgHeaderDeferment = Factory.New<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderDeferment, Core.Constants.CountryCodes.Netherlands, "123456789", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands));

		declaration.DefermentPartyDocAddress.OrganisationPK = orgHeaderDeferment.PK;

		AssertEquals("DomesticDutyTaxParty", "NL123456789", wrapper.DomesticDutyTaxParty);
	}

	OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType = null)
	{
		var taxCode = organisation.CustomsCodes.AddNew();
		taxCode.OK_RN_NKCodeCountry = country;
		taxCode.OK_CodeType = codeType ?? OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		taxCode.OK_CustomsRegNo = customsRegNo;
		return taxCode;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "decRef";
		wrapper = new RequestForVatPartyInformationWrapper(declaration);
	}
	RequestForVatPartyInformationWrapper wrapper;
	JobDeclaration declaration;
}
