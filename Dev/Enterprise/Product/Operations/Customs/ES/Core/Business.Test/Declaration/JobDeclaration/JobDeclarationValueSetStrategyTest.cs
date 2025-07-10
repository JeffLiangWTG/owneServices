using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class JobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSetBox14Representation()
		{
			var localCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, jobDeclaration.Country.Code);
			var impAddInfo = EUOrgImpAddInfo.Get(importerIndirect, localCountry.Code);
			var impCompanyAddress = importerIndirect.Addresses.AddNewMainAddress();
			impCompanyAddress.OA_Code = "A";
			impAddInfo.ZO_Box14UseIndirectRepresentation = true;

			var impAddInfo2 = EUOrgImpAddInfo.Get(importerNoIndirect, localCountry.Code);
			var impCompanyAddress2 = importerNoIndirect.Addresses.AddNewMainAddress();
			impCompanyAddress2.OA_Code = "B";
			impAddInfo2.ZO_Box14UseIndirectRepresentation = false;

			jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = impCompanyAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Rep use indirect Representation don't change Declarant Type", ZString.Empty, jobDeclaration.JE_DeclarantType);
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = impCompanyAddress2.PK;
				AssertEquals("Rep doesn't use indirect Representation don't change Declarant Type", ZString.Empty, jobDeclaration.JE_DeclarantType);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			jobDeclaration.JE_DeclarantType = ZString.Empty;
			importerIndirect = Factory.NewWithValidTestData<OrgHeader>();
			importerIndirect.OH_Code = "COD";
			importerNoIndirect = Factory.NewWithValidTestData<OrgHeader>();
			importerNoIndirect.OH_Code = "DOC";
		}

		JobDeclaration jobDeclaration;
		OrgHeader importerIndirect;
		OrgHeader importerNoIndirect;
	}
}
