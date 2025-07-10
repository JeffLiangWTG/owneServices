using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIsNctsPreviousDocumentPhase5LookupsType()
		{
			AssertEquals(true, typeof(NctsPreviousDocumentPhase5Lookups).IsAssignableFrom(lookups.GetType()));
		}

		public void TestSubTypeList_N337()
		{
			document.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			CombineAssertions(() =>
			{
				var list = lookups.SubTypeList;
				AssertEquals("CodesAsString", "REG", list.CodesAsString);
				AssertSame("Cached", list, lookups.SubTypeList);
			});
		}

		public void TestSubTypeList_N337_IsOfficeAIR()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var airCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE185104", "SHYZAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var airAttribute = helper.CreateCusCodeListAttribute(airCodeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
			helper.CreateTransportModeForCusCodeAttribute(airAttribute.PK, RefTransportModeList.Codes.AIR);
			Factory.Save();

			var customOffice = document.Parent.Header.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			customOffice.CY_Data = "DE185104";

			document.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
			CombineAssertions(() =>
			{
				var list = lookups.SubTypeList;
				AssertEquals("CodesAsString", "AWB, REG, ULD", list.CodesAsString);
				AssertSame("Cached", list, lookups.SubTypeList);
			});
		}

		public void TestUnitOfQuantityList()
		{
			SetupCUSUQCusCodeList();

			document.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var list = lookups.UnitOfQuantityList;

			CombineAssertions(() =>
			{
				AssertEquals("List", "UQ1, UQ2", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnitOfQuantityList);
			});
		}

		public void TestUnitOfQuantity2List()
		{
			SetupCUSUQCusCodeList();

			document.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var list = lookups.UnitOfQuantity2List;

			CombineAssertions(() =>
			{
				AssertEquals("List", "UQ1, UQ2", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnitOfQuantity2List);
			});
		}

		void SetupCUSUQCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
				"Customs Declaration Units of Quantity");

			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "de", eun);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ1", "UQ1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ2", "UQ2",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ2", "UQ2",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			helper.CreateNewOrGetExistingCusCodeList("EUN",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ3", "UQ3",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			document = goodsItem.PreviousDocuments.AddNew();
			lookups = new NctsPreviousDocumentLookups(document);
		}
		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousDocument document;
		NctsPreviousDocumentLookups lookups;
	}
}
