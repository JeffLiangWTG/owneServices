using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class ExportEntryMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestListTypes()
		{
			CombineAssertions(() =>
			{
				AssertType<ExportEntryTypeList>("EntryTypeList", lookups.EntryTypeList);
				AssertType<ExportExitTypeList>("ExitTypeList", lookups.ExitTypeList);
				AssertType<ExportSecurityTypeList>("SecurityTypeList", lookups.SecurityTypeList);
				AssertType<EUCustomsOfficeCodeCollection>("ExitCustomsOfficeList", lookups.ExitCustomsOfficeList);
			});
		}

		public void TestExitTypeList()
		{
			AssertListHasCorrectElementsAndIsCached("ExitTypeList", x => x.ExitTypeList, "2, 4");
		}

		public void TestExitCustomsOfficeList()
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

			var coll = lookups.ExitCustomsOfficeList;
			coll.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "DE004324", "DE004325", "IT004324", "IT004325" }, coll.Select(x => x.ZZD_Code));
		}

		public void TestEntryTypeList()
		{
			AssertListHasCorrectElementsAndIsCached("Default", x => x.EntryTypeList, "AMD, CAN, DAT, ENT");
		}

		public void TestEntryTypeList_InvalidEntryStatus()
		{
			Entry.CH_EntryStatus = "11";

			AssertListHasCorrectElementsAndIsCached("Invalid Entry Status rejects EXT", x => x.EntryTypeList, "AMD, CAN, DAT, ENT");
		}

		public void TestEntryTypeList_ValidEntryStatus_EntryStatus500()
		{
			Entry.CH_EntryStatus = "500";

			AssertListHasCorrectElementsAndIsCached("EXT is included", x => x.EntryTypeList, "AMD, CAN, DAT, ENT, EXT");
		}

		public void TestEntryTypeList_ValidEntryStatus_EntryStatus501()
		{
			Entry.CH_EntryStatus = "501";

			AssertListHasCorrectElementsAndIsCached("EXT is included", x => x.EntryTypeList, "AMD, CAN, DAT, ENT, EXT");
		}

		public void TestEntryTypeList_ValidEntryStatus_EntryStatus502()
		{
			Entry.CH_EntryStatus = "502";

			AssertListHasCorrectElementsAndIsCached("EXT is included", x => x.EntryTypeList, "AMD, CAN, DAT, ENT, EXT");
		}

		public void TestEntryTypeList_WhenMRNIsNotEmptyAndEntryStatusIsEmpty()
		{
			Entry.CH_EntryStatus = ZString.Empty;
			action.MovementReferenceNumber = "19ZZ586600822952E8";
			AssertListHasCorrectElementsAndIsCached("DAT is excluded", x => x.EntryTypeList, "AMD, CAN, ENT, EXT");
		}

		public void TestSecurityTypeList()
		{
			AssertListHasCorrectElementsAndIsCached("SecurityTypeList", x => x.SecurityTypeList, "0, 2");
		}

		public void TestSecurityCodeList_EntryStyleIsCO()
		{
			Entry.Declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			AssertListHasCorrectElementsAndIsCached("Entry Style CO", x => x.SecurityTypeList, "0");
		}

		protected override void SetUp()
		{
			base.SetUp();

			action = new ExportEntryMessageSendingAction(Entry);
			lookups = new ExportEntryMessageSendingActionLookups(action);
		}
		ExportEntryMessageSendingAction action;
		ExportEntryMessageSendingActionLookups lookups;

		void AssertListHasCorrectElementsAndIsCached(string testCase, Func<ExportEntryMessageSendingActionLookups, CodeDescriptionPairList> listGetter, string expectedCodesAsString)
		{
			var list = listGetter.Invoke(lookups);
			CombineAssertions(() =>
			{
				AssertEquals(testCase + "->CodesAsString", expectedCodesAsString, list.CodesAsString);
				AssertSame(testCase + "->Cached", list, listGetter.Invoke(lookups));
			});
		}

		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					entry = declaration.CustomsEntryHeaders.AddNew();
				}
				return entry;
			}
		}
		CusEntryHeader entry;
	}
}
