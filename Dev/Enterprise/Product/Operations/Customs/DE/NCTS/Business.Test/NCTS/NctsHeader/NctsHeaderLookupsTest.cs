using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEventFlagList_Default()
		{
			CombineAssertions(() =>
			{
				var list = header.Lookups.EventFlagList;
				AssertEquals("CodeAsString", "Y, N", list.CodesAsString);
				AssertSame(list, header.Lookups.EventFlagList);
			});
		}

		public void TestEventFlagList_HasLogYes()
		{
			header.Logs.AddNew(Events.MiscellaneousEvent, "EventFlag=Y");
			CombineAssertions(() =>
			{
				var list = header.Lookups.EventFlagList;
				AssertEquals("CodeAsString", "Y, C", list.CodesAsString);
				AssertSame(list, header.Lookups.EventFlagList);
			});
		}

		public void TestEventFlagList_HasLogYesButCancelled()
		{
			header.Logs.AddNew(Events.MiscellaneousEvent, "EventFlag=Y").Cancel();
			CombineAssertions(() =>
			{
				var list = header.Lookups.EventFlagList;
				AssertEquals("CodeAsString", "Y, C", list.CodesAsString);
				AssertSame(list, header.Lookups.EventFlagList);
			});
		}

		public void TestDestinationCustomsOfficeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000001", "DE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000002", "DE000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR009999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var list = header.Lookups.DestinationCustomsOfficeCodeList;
				list.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "DE000001", "DE000002", "DE000003" }, list.Select(x => x.ZZD_Code));
			}
		}

		public void TestNctsMessageStatusList_Departure()
		{
			header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "ACC, ACK, ERR, FAL, INV, SNT, MDN", header.Lookups.NctsMessageStatusList.CodesAsString);
				AssertSame("Cached", header.Lookups.NctsMessageStatusList, header.Lookups.NctsMessageStatusList);
			});
		}

		public void TestNctsMessageStatusList_Arrival()
		{
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "ACC, ACK, ERR, FAL, INV, SNT, MAN", header.Lookups.NctsMessageStatusList.CodesAsString);
				AssertSame("Cached", header.Lookups.NctsMessageStatusList, header.Lookups.NctsMessageStatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
