using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAErrorCodesDescriptionHelperTest : TestCaseWithFactory
	{
		public void TestGetDescriptionFromCode()
		{
			var helper = new CAErrorCodesDescriptionHelper();
			AssertEquals("Error Description1", helper.GetDescriptionFromCode(Factory, "1"));
			AssertEquals("French Description", "Erreur de description1", helper.GetFrenchDescriptionFromCode(Factory, "1"));
			AssertEquals("1 becomes 001", "Error Description1", helper.GetDescriptionFromCode(Factory, "001"));
			AssertEquals("1 becomes 001", "Erreur de description1", helper.GetFrenchDescriptionFromCode(Factory, "001"));

			AssertEquals("Error Description2", helper.GetDescriptionFromCode(Factory, "2"));
			AssertEquals("French Description", "Erreur de description2", helper.GetFrenchDescriptionFromCode(Factory, "2"));
		}

		public void TestGetDescriptionFromMsgNoCode()
		{
			var helper = new CAErrorCodesDescriptionHelper();
			AssertEquals("ENTRY REJECTED", helper.GetDescriptionFromMsgNoCode(Factory, "942991"));
			AssertEquals("ENTRY ACCEPTED", helper.GetDescriptionFromMsgNoCode(Factory, "942992"));
			AssertEquals("DUPLICATE TRAILER RECORD", helper.GetDescriptionFromMsgNoCode(Factory, "943009"));

			AssertEquals("Error Description1", helper.GetDescriptionFromMsgNoCode(Factory, "123"));
			AssertEquals("Error Description2", helper.GetDescriptionFromMsgNoCode(Factory, "456"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "1", "Error Description1", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "2", "Error Description2", startDate, endDate);
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code1, "FR", "Erreur de description1");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code2, "FR", "Erreur de description2");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, RefCusCodeListAttributeTypes.Codes.MessageNum, "123");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, RefCusCodeListAttributeTypes.Codes.MessageNum, "456");
			Factory.Save();
		}
	}
}
