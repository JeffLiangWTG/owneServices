using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OrgSupBuyLinkTrnModeAddInfoBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCustomsOffices()
		{
			var universalDataHelper = new UniversalReferenceTestDataHelper(Factory);
			universalDataHelper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office Code");
			universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BJB", "BJB", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			universalDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "A00", "A00", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();
			var testItem = new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)Factory.New<OrgSupBuyLinkTrnMode>().AddInfo);
			testItem.ZO_CustomsOffice = "XXX";
			AssertHasMessageErrorContaining(testItem.ZO_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			testItem.ZO_CustomsOffice = "BJB";
			AssertNoNotifications(testItem.ZO_CustomsOfficeInfo);
			testItem.ZO_OfficeOfEntryExit = "XXX";
			AssertHasMessageErrorContaining(testItem.ZO_OfficeOfEntryExitInfo, ListValidation.InvalidCodeMessageError);
			testItem.ZO_OfficeOfEntryExit = "BJB";
			AssertNoNotifications(testItem.ZO_OfficeOfEntryExitInfo);
		}

		[TestDate(2018, 8, 13)]
		public void TestCheckCIQOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CIQPO", "CN CIQ Ports");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CIQPO", "CP01", "CN CIQ Port 01", new ZDateTime(2018, 8, 1), new ZDateTime(2018, 8, 31));
			Factory.Save();
			var testItem = new OrgSupBuyLinkTrnModeAddInfoBizObj((OrgSupBuyLinkTrnModeAddInfo)Factory.New<OrgSupBuyLinkTrnMode>().AddInfo);
			var targetInfo = testItem.ZO_CIQOfficeOfEntryExitInfo;
			testItem.ZO_CIQOfficeOfEntryExit = "X";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			testItem.ZO_CIQOfficeOfEntryExit = "CP01";
			AssertNoMessageErrors(targetInfo);
		}
	}
}
