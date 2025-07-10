using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class OfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAtLeastOneCADOfficeTypeExists()
		{
			AssertAtLeastOneOfficeTypeExists(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch, $"At least one office of type {OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch} is required");
		}

		public void TestAtLeastOneCAAOfficeTypeExists()
		{
			emcsJob.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			AssertAtLeastOneOfficeTypeExists(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch, $"At least one office of type {OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch} is required");
		}

		void AssertAtLeastOneOfficeTypeExists(string code, string messageError)
		{
			CombineAssertions(() =>
			{
				var customsOffices = emcsJob.CustomsOffices;
				var office = customsOffices[0];
				office.CY_Code = "XXX";
				AssertHasMessageError("error for code no exists", office.CY_CodeInfo, messageError);

				var office2 = customsOffices.AddNew();
				office2.CY_Code = code;
				customsOffices.RunPreSaveValidation();
				AssertNoMessageError("one code exists", office.CY_CodeInfo, messageError);
			});
		}

		protected override void SetUp()
		{
			emcsJob = Factory.New<EMCSJobDeclaration>();
			base.SetUp();
		}

		EMCSJobDeclaration emcsJob;
	}
}
