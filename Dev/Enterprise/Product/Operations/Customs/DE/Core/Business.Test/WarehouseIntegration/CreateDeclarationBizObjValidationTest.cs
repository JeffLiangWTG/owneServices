using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CreateDeclarationBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, string.Empty, "40", "78", "", "DES2", "IMP", group: "EZL");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bizObj.CPCInfo, "10", "40");
		}

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

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bizObj.CustomsOfficeInfo, "FAKE", "DE004323");
		}

		public void TestCheckDeclarantsReference()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(bizObj.DeclarantsReferenceInfo);
		}

		public void TestCheckDeclarantsReference_WarehouseOrder()
		{
			var createDeclarationBizObj = new CreateDeclarationBizObj(true);
			createDeclarationBizObj.DeclarantsReference = ZString.Empty;

			ValidationTestHelper.AssertFieldIsNotMandatory(createDeclarationBizObj.DeclarantsReferenceInfo);
		}

		public void TestCheckDeclarationType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bizObj.DeclarationTypeInfo, "XX", "AZ");
		}

		protected override void SetUp()
		{
			base.SetUp();

			bizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: false);
		}

		CreateDeclarationBizObj bizObj;
	}
}
