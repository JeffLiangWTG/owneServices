using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExpeditionImporterWrapperTest : WrapperHelperTest<ExpeditionImporterWrapper>
	{
		public void TestImporterId()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode;
			orgHeader.OH_FullName = OrgHeaderData.Name;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			var orgAddressMain = orgHeader.MainAddress;
			orgAddressMain.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			CombineAssertions(() =>
			{
				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;

				wrapper = ExpeditionImporterWrapper.New(declaration.ImporterDocumentaryAddress);
				AssertEquals("Expected Empty Id if Importer country is not Spain", ZString.Empty, wrapper.Id);

				orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

				wrapper = ExpeditionImporterWrapper.New(declaration.ImporterDocumentaryAddress);
				AssertEquals("Expected Id if Importer country is Spain", "NIF22222222", wrapper.Id);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode;
			orgHeader.OH_FullName = OrgHeaderData.Name;
			orgHeader.Addresses.AddNew();

			declaration.JE_OH_Importer = orgHeader.PK;

			wrapper = ExpeditionImporterWrapper.New(declaration.ImporterDocumentaryAddress);
		}

		JobDeclaration declaration;
		OrgHeader orgHeader;
		ExpeditionImporterWrapper wrapper;

		protected override ExpeditionImporterWrapper GetProvider() => wrapper;
	}
}
