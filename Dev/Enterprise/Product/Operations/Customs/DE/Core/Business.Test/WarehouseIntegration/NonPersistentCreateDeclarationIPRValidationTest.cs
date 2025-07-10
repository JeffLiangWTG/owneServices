using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class NonPersistentCreateDeclarationIPRValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCustomsOffice()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			Factory.Save();

			createDeclarationIPR.DeclarationType = "AZ";
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(createDeclarationIPR.CustomsOfficeInfo, "FAKE", "DE004323");

			createDeclarationIPR.DeclarationType = "AVABR";
			ValidationTestHelper.AssertFieldIsNotMandatory(createDeclarationIPR.CustomsOfficeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(createDeclarationIPR.CustomsOfficeInfo, "FAKE", "DE004323");
		}

		public void TestCheckDeclarantsReference()
		{
			createDeclarationIPR.DeclarationType = "AZ";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(createDeclarationIPR.DeclarantsReferenceInfo);

			createDeclarationIPR.DeclarationType = "AVABR";
			ValidationTestHelper.AssertFieldIsNotMandatory(createDeclarationIPR.DeclarantsReferenceInfo);
		}

		public void TestCheckCustomsDeadline()
		{
			createDeclarationIPR.DeclarationType = "AVABR";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(createDeclarationIPR.CustomsDeadlineInfo);

			createDeclarationIPR.DeclarationType = "AZ";
			ValidationTestHelper.AssertFieldIsNotMandatory(createDeclarationIPR.CustomsDeadlineInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			createDeclarationIPR = new CreateDeclarationIPR();
		}

		CreateDeclarationIPR createDeclarationIPR;
	}
}
