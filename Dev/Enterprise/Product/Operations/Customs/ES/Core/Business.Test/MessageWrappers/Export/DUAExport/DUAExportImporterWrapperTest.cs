using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUAExportImporterWrapperTest : WrapperHelperTest<DUAExportImporterWrapper>
	{
		public void TestImporterId()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = HeaderData.ImporterCode;
				orgHeader.OH_FullName = OrgHeaderData.Name;
				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var orgAddress = orgHeader.MainAddress;
				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

				declaration.JE_OH_Importer = orgHeader.PK;

				wrapper = DUAExportImporterWrapper.New(declaration.Importer);
				AssertEquals("Expected Empty Id if Importer country is not Spain", ZString.Empty, wrapper.Id);

				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

				wrapper = DUAExportImporterWrapper.New(declaration.Importer);
				AssertEquals("Expected Id if Importer country is Spain", "NIF22222222", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode;
			orgHeader.OH_FullName = OrgHeaderData.Name;
			orgHeader.Addresses.AddNew();

			declaration.JE_OH_Importer = orgHeader.PK;

			wrapper = DUAExportImporterWrapper.New(declaration.Importer);
		}

		JobDeclaration declaration;
		OrgHeader orgHeader;
		DUAExportImporterWrapper wrapper;

		protected override DUAExportImporterWrapper GetProvider() => wrapper;
	}
}
