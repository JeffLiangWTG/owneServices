using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class RequestForGuaranteeInformationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new RequestForGuaranteeInformationWrapper(null));
	}

	public void TestJobReference()
	{
		AssertEquals("JobReference", "decRef", wrapper.JobReference);
	}

	public void TestFunctionCode()
	{
		AssertEquals("92", wrapper.FunctionCode);
	}

	public void TestDeclarant()
	{
		var orgImporter = Factory.New<OrgHeader>();
		AddCustomsCodeForTest(orgImporter, Core.Constants.CountryCodes.Netherlands, "100004064");

		declaration.JE_OH_Importer = orgImporter.PK;
		declaration.JE_PaymentMethod = DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;

		AssertEquals("Declarant - PaymentMethod A", "NL100004064", wrapper.Declarant);

		OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo)
		{
			var taxCode = organisation.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = country;
			taxCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			taxCode.OK_CustomsRegNo = customsRegNo;
			return taxCode;
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "decRef";
		wrapper = new RequestForGuaranteeInformationWrapper(declaration);
	}
	RequestForGuaranteeInformationWrapper wrapper;
	JobDeclaration declaration;
}
