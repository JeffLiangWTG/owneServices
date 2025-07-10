using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNctsMessageStatusList()
		{
			AssertEquals("List values", "ACC, ACK, ERR, FAL, INV, SNT", lookups.NctsMessageStatusList.CodesAsString);
		}

		public void TestOrganisations()
		{
			AssertType<OrgHeaderCollection>("Type", lookups.Organisations);
		}

		public void TestBondedWarehouseCollection()
		{
			AssertType<BondedWarehouseCollection>("Type", lookups.BondedWarehouseCollection);
		}

		public void TestSpecificCircumstanceIndicatorList()
		{
			var list = lookups.SpecificCircumstanceIndicatorList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "E, A, D, C, B", list.CodesAsString);
				AssertSame("Cached", list, lookups.SpecificCircumstanceIndicatorList);
			});
		}

		public void TestNctsSpecificCircumstanceIndicatorList()
		{
			var list = lookups.NctsSpecificCircumstanceIndicatorList;
			AssertEquals("Phase5 List should be empty", string.Empty, list.CodesAsString);
		}

		public void TestRepresentatives()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.Representatives);

			var defaultFilter = lookups.Representatives.FilterBusinessObjectDefaults["Category:Property"];

			AssertNotNull("Representatives default filter must contain Category", defaultFilter);
			AssertEquals("Category default filter must be NAT", (ZString)OrgConstants.Category.NaturalPersonIndividual, defaultFilter.Value);
		}

		public void TestTransportChargesModeOfPaymentList()
		{
			var list = lookups.TransportChargesModeOfPaymentList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "A, B, C, D, H, Y, Z", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportChargesModeOfPaymentList);
			});
		}

		public void TestNctsControlResultList()
		{
			var list = lookups.NctsControlResultList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "A1, A2, A3, A4, A5, B1", list.CodesAsString);
				AssertSame("Cached", list, lookups.NctsControlResultList);
			});
		}

		public void TestSealTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.SealTypeList;
				AssertSame("Cached", Factory.GetCachedValue<SealTypeList>(), list);
				AssertEquals("CON, PAC", list.CodesAsString);
			});
		}

		public void TestModeOfTransportList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.ModeOfTransportList;
				AssertEquals("Values", "1, 2, 3, 4, 5, 7, 8, 9", list.CodesAsString);
				AssertSame("Cached", list, lookups.ModeOfTransportList);
			});
		}

		public void TestBorderModeOfTransportList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.BorderModeOfTransportList;
				AssertEquals("Values", "1, 2, 3, 4, 5, 7, 8, 9", list.CodesAsString);
				AssertSame("Cached", list, lookups.BorderModeOfTransportList);
			});
		}

		public void TestTransportAtBorderTypeOfIdList()
		{
			AssertEquals(string.Empty, lookups.TransportAtBorderTypeOfIdList.CodesAsString);
		}

		public void TestAdditionalDeclarationTypeList()
		{
			AssertEquals("Values", string.Empty, lookups.AdditionalDeclarationTypeList.CodesAsString);
		}

		public void TestLocationOfGoodsCodeList()
		{
			AssertEquals(string.Empty, lookups.LocationOfGoodsCodeList.CodesAsString);
		}

		public void TestTypeOfSecurityList()
		{
			AssertEquals(string.Empty, lookups.TypeOfSecurityList.CodesAsString);
		}

		public void TestTransportAtDepartureTypeOfIdList()
		{
			AssertEquals(string.Empty, lookups.TransportAtDepartureTypeOfIdList.CodesAsString);
		}

		public void TestOfficeCodeList()
		{
			AssertEquals("Border offices not used in Phase4", 0, lookups.OfficeCodeList.Count);
		}

		public void TestTOLCarrierNationalityList()
		{
			AssertType<RefCountryCollection>(lookups.TOLCarrierNationalityList);
		}

		public void TestForeignDestPortCodes()
		{
			AssertEquals(string.Empty, ((CodeDescriptionPairList)lookups.ForeignDestPortCodes).CodesAsString);
		}

		public void TestPortOfPresentationCodes()
		{
			AssertEquals(string.Empty, ((CodeDescriptionPairList)lookups.PortOfPresentationCodes).CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = new NctsDepartureMovementHeaderPhase4Lookups(nctsHeader.MovementHeader);
		}
		NctsHeader nctsHeader;
		NctsDepartureMovementHeaderPhase4Lookups lookups;
	}
}
