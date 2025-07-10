using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonDeclarantWrapperTest : WrapperHelperTest<AESCommonDeclarantWrapper>
	{
		public void TestGetNewAESCommonDeclarantWrapper()
		{
			CombineAssertions(() =>
			{
				orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = OrgHeaderData.Code;

				JobDeclaration declaration = null;
				AssertNull("Declaration null", AESCommonDeclarantWrapper.New(declaration));
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_OH_Supplier = ZGuid.Empty;
				declaration.JE_GB = ZGuid.Empty;
				AssertNull("Declaration no DeclarantAddress null", AESCommonDeclarantWrapper.New(declaration));
				var address = Factory.New<OrgAddress>();
				declaration.JE_OA_DeclarantAddress = address.PK;
				AssertNull("Declaration DeclarantAddress no Header null", AESCommonDeclarantWrapper.New(declaration));
				address.OA_OH = Factory.New<OrgHeader>().PK;
				AssertNotNull("Declaration not null", AESCommonDeclarantWrapper.New(declaration));
			});
		}

		public void TestIdForRepresentativeOrDeclarantOrExporter()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();

				var orgHeaderExporter = Factory.New<OrgHeader>();
				orgHeaderExporter.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF111111");
				var orgAddressExporter = Factory.New<OrgAddress>();
				orgAddressExporter.OA_OH = orgHeaderExporter.PK;

				var orgHeaderDeclarant = Factory.New<OrgHeader>();
				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
				var orgAddressDeclarant = Factory.New<OrgAddress>();
				orgAddressDeclarant.OA_OH = orgHeaderDeclarant.PK;

				var orgHeaderRepresent = Factory.New<OrgHeader>();
				orgHeaderRepresent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF333333");
				var orgAddressRepresent = Factory.New<OrgAddress>();
				orgAddressRepresent.OA_OH = orgHeaderRepresent.PK;

				declaration.JE_OH_Supplier = orgHeaderExporter.PK;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_OA_Representative = ZGuid.Empty;
				declaration.JE_GB = ZGuid.Empty;

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._2Direct;
				var wrapper = AESCommonDeclarantWrapper.New(declaration);
				AssertEquals("If JE_DeclarantType = 2 or 5 and representative is empty result is exporter", "ESNIF111111", wrapper.Id);

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				wrapper = AESCommonDeclarantWrapper.New(declaration);
				AssertEquals("If JE_DeclarantType = 2 or 5 and representative is not empty and declarant is empty and exporter is not empty the result is exporter", "ESNIF111111", wrapper.Id);

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
				declaration.JE_OH_Supplier = orgHeaderExporter.PK;
				wrapper = AESCommonDeclarantWrapper.New(declaration);
				AssertEquals("If JE_DeclarantType = 2 or 5 and representative is not empty and declarant is not empty and exporter is not empty the result is declarant", "ESNIF222222", wrapper.Id);

				declaration.JE_DeclarantType = ESRepresentationTypeList.Codes._3Indirect;
				declaration.JE_OA_Representative = ZGuid.Empty;
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				wrapper = AESCommonDeclarantWrapper.New(declaration);
				AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is empty and declarant is empty and exporter is not empty the result is exporter", "ESNIF111111", wrapper.Id);

				declaration.JE_OA_DeclarantAddress = orgAddressDeclarant.PK;
				wrapper = AESCommonDeclarantWrapper.New(declaration);
				AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is empty and declarant is not empty and exporter is not empty the result is declarant", "ESNIF222222", wrapper.Id);

				declaration.JE_OA_Representative = orgAddressRepresent.PK;
				wrapper = AESCommonDeclarantWrapper.New(declaration);
				AssertEquals("If JE_DeclarantType <> 2 or 5 and representative is not empty and declarant is not empty and exporter is not empty the result is representative", "ESNIF333333", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			wrapper = AESCommonDeclarantWrapper.New(declaration);
		}

		OrgHeader orgHeader;
		JobDeclaration declaration;
		AESCommonDeclarantWrapper wrapper;

		protected override AESCommonDeclarantWrapper GetProvider() => wrapper;
	}
}
