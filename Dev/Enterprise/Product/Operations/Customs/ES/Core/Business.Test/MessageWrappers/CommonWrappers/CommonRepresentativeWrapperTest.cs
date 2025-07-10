using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonRepresentativeWrapperTest : WrapperHelperTest<CommonRepresentativeWrapper>
{
	public void TestGetNewCommonRepresentativeWrapper()
	{
		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._1Auto;
			AssertNull("Declaration with JE_DeclarantType = 1, result null", CommonRepresentativeWrapper.New(declaration));

			declaration.JE_OA_Representative = orgAddress.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			AssertNotNull("Declaration with JE_DeclarantType = 2 with representant, result not null", CommonRepresentativeWrapper.New(declaration));

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
			AssertNull("Declaration with JE_DeclarantType = 3 with representant, result null", CommonRepresentativeWrapper.New(declaration));

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNotNull("JobDeclaration with representant is null and declarant not null and JE_DeclarantType = 2, result not null", CommonRepresentativeWrapper.New(declaration));

			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._5IndirectATC;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNull("JobDeclaration with representant and declarant is null and JE_DeclarantType = 5, result null", CommonRepresentativeWrapper.New(declaration));

			declaration.JE_OA_Representative = orgAddress.PK;
			AssertNotNull("JobDeclaration with representant is not null and declarant is null and JE_DeclarantType = 5, result not null", CommonRepresentativeWrapper.New(declaration));

			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			AssertNotNull("JobDeclaration with representant and declarant is not null and JE_DeclarantType = 2, result not null", CommonRepresentativeWrapper.New(declaration));

			AssertNull("JobDeclaration without DeclarationType", CommonRepresentativeWrapper.New(Factory.New<JobDeclaration>()));

			AssertNull("JobDeclaration with null", CommonRepresentativeWrapper.New(null));
		});
	}

	public void TestStatus()
	{
		AssertEquals("Expected filled Status with fixed value 2", "2", wrapper.Status);
	}

	public void TestIdForRepresentativeOrDeclarantWhenRepresentativeTypeEqualTo2Or5()
	{
		CombineAssertions(() =>
		{
			var orgHeaderDeclarant = Factory.New<OrgHeader>();
			orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF111111");
			var orgAddressDeclarant = Factory.New<OrgAddress>();
			orgAddressDeclarant.OA_OH = orgHeaderDeclarant.PK;

			declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
			declaration.JE_GB = ZGuid.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			var wrapper = CommonRepresentativeWrapper.New(declaration);

			AssertEquals("Only with declarant, the values is declarant when JE_DeclarantType = 2", "ESNIF111111", wrapper.Id);

			var orgHeaderRepresent = Factory.New<OrgHeader>();
			orgHeaderRepresent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			var orgAddressRepresent = Factory.New<OrgAddress>();
			orgAddressRepresent.OA_OH = orgHeaderRepresent.PK;

			declaration.JE_OA_Representative = orgAddressRepresent.PK;
			declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
			wrapper = CommonRepresentativeWrapper.New(declaration);
			AssertEquals("With exporter and representative, the values is representative when JE_DeclarantType = 2", "ESNIF222222", wrapper.Id);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgAddress.OA_OH = orgHeader.PK;

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_Representative = orgAddress.PK;
		declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;

		wrapper = CommonRepresentativeWrapper.New(declaration);
	}

	OrgAddress orgAddress;
	CommonRepresentativeWrapper wrapper;
	JobDeclaration declaration;

	protected override CommonRepresentativeWrapper GetProvider() => wrapper;
}
