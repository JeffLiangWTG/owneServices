using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var list = lookups.MessageStatusList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Codes", "ACK, WAR, SNT, REJ, RKN, RCV, QUE, PRS, PPS, PND, FAL, ERR, DCD, CAN, WDW, LNK, CAP", list.CodesAsString);
				AssertSame("Should be cached", list, lookups.MessageStatusList);
			});
		}

		public void TestEntryStatusList()
		{
			var list = lookups.EntryStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("AAD, AAR, ACK, ALT, CAN, CHG, COM, CON, CRM, DEL, DIV, ERJ, ERR, EVT, EXP, ICL, INC, INR, INT, MAN, REG, REJ, REM, ROR, SHR, SNT, SPL, STA, SYN", list.CodesAsString);
				AssertSame("Should be cached", list, lookups.EntryStatusList);
			});
		}

		public void TestPaymentPartyList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.PaymentPartyList;
				AssertEquals("CodesAsString", "A, B, C, D", list.CodesAsString);
				AssertSame("Cached", list, lookups.PaymentPartyList);
			});
		}

		public void TestDeclarantTypeList()
		{
			CombineAssertions(() =>
			{
				var list = (CodeDescriptionPairList)lookups.DeclarantTypeList;
				AssertEquals("CodesAsString", "1, 2", list.CodesAsString);
				AssertSame("Cached", list, lookups.DeclarantTypeList);
			});
		}

		public void TestMessageSubType_DestinationType_List()
		{
			CombineAssertions(() =>
			{
				var list = lookups.MessageSubTypeList;
				AssertEquals("DestinationType CodesAsString", "1, 2, 3, 4, 5, 6, 8, 9, 10, 11, 7", list.CodesAsString);
				AssertSame("Cached", list, lookups.MessageSubTypeList);
			});
		}

		public void TestTransportTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.TransportTypeList;
				AssertEquals("CodesAsString", "AIR, FIX, IWT, MAI, OTH, RAI, ROA, SEA", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportTypeList);
			});
		}

		public void TestJourneyTimeUnitList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.JourneyTimeUnitList;
				AssertEquals("CodesAsString", "D, H", list.CodesAsString);
				AssertSame("Cached", list, lookups.JourneyTimeUnitList);
			});
		}

		public void TestConsigneesList()
		{
			AssertType<ConsigneeCollection>(lookups.ConsigneesList);
		}

		public void TestOwnersList()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.OwnersList);
		}

		public void TestCarrierAgentList()
		{
			AssertType<ShippingProviderCollection>(lookups.CarrierAgentList);
		}

		public void TestTransporterList()
		{
			AssertType<ShippingProviderCollection>(lookups.TransporterList);
		}

		public void TestDispatchWarehouseList()
		{
			AssertType<WarehouseClientCollection>(lookups.DispatchWarehouseList);
		}

		public void TestDestinationWarehouseList()
		{
			AssertType<WarehouseClientCollection>(lookups.DestinationWarehouseList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new EMCSJobDeclarationLookups(Factory.New<EMCSJobDeclaration>());
		}

		EMCSJobDeclarationLookups lookups;
	}
}
