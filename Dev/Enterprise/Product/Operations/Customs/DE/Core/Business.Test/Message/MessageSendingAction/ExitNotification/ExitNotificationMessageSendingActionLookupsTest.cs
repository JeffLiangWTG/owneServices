using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitNotificationMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestListTypes()
		{
			CombineAssertions(() =>
			{
				AssertType<CustomsOfficeCodeCollection>("ExitCustomsOfficeList", Lookups.IntendedExitCustomsOfficeList);
				AssertType<CodeDescriptionPairList>("MessageTypeList", Lookups.MessageTypeList);
			});
		}

		public void TestMessageTypeList()
		{
			var list = Lookups.MessageTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "NOT", list.CodesAsString);
				AssertSame("Cached", list, lookups.MessageTypeList);
			});
		}

		public void TestIntendedExitCustomsOfficeList()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004324", "GERMAN OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004325", "GERMAN OFFICE3", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);

			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004323", "Italy OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004324", "Italy OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExitInland);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004325", "Italy OFFICE3", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			var coll = Lookups.IntendedExitCustomsOfficeList;
			coll.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "DE004325" }, coll.Select(x => x.ZZD_Code));
		}

		ExitNotificationMessageSendingAction Action => action ?? (action = new ExitNotificationMessageSendingAction(Entry));
		ExitNotificationMessageSendingAction action;

		ExitNotificationMessageSendingActionLookups Lookups => lookups ?? (lookups = new ExitNotificationMessageSendingActionLookups(Action));
		ExitNotificationMessageSendingActionLookups lookups;

		CusExitControlHeader Header => header ?? (header = Factory.New<CusExitControlHeader>());
		CusExitControlHeader header;

		CusExitDetail Entry => entry ?? (entry = Header.CusExitDetails.AddNew());
		CusExitDetail entry;
	}
}
