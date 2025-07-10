using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CANoticeReasonCodesDescriptionHelperTest : TestCaseWithFactory
	{
		public void TestGetD4NoticesDescriptionFromCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeType("Test", "Test Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "0001", "0001 Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "0002", "0002 Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, codeType, "0003", "0003 Description", ZDateTime.Today.AddDays(5), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, codeType, "0004", "0004 Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, "Test", "0005", "0005 Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertEquals(ZString.Empty, CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, "0001"));
			AssertEquals("0002 Description", CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, "0002"));
			AssertEquals(ZString.Empty, CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, "0003"));
			AssertEquals(ZString.Empty, CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, "0004"));
			AssertEquals(ZString.Empty, CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, "0005"));
		}
	}
}
