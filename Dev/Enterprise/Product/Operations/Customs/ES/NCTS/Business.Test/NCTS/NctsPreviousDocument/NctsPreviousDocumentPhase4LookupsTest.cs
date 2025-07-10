using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsPreviousDocumentPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previousDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			helper.CreateNewOrGetExistingCusCodeType(previousDocumentType, "Previous Documents Transit NCTS (BOX40)");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previousDocumentType, "T1");
			Create_CusCodeList_RefData(helper, Core.Constants.CountryCodes.Spain, previousDocumentType, "AA");
			Create_CusCodeList_RefData(helper, Core.Constants.CountryCodes.Spain, previousDocumentType, "BB");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			var codeListLookup = (CodeDescriptionPairList)lookups.CodeList;
			AssertContainsExactElementsInAnyOrder("Only gets Spain's previous document types", new string[] { "AA", "BB" }, codeListLookup.GetAllCodes());
		}

		public void TestNotAttributeLevel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previousDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", eun);
			helper.CreateCusCodeType(previousDocumentType, "Previous Documents Transit NCTS (BOX40)");
			helper.CreateCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Spain, previousDocumentType, "AA", "AAAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributes.Names.Level, "ValueAA");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Spain, previousDocumentType, "BB", "BBBBBB", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Name", "ValueBB");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Spain, previousDocumentType, "CC", "CCCCCC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Name", "ValueCC");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Spain, previousDocumentType, "DD", "DDDDDD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeListAttributes.Names.Level, "ValueDD");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, previousDocumentType, "EE", "EEEEEE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var codeListLookup = (CodeDescriptionPairList)lookups.CodeList;
			AssertContainsExactElementsInAnyOrder("Two with attribute level", new string[] { "BB", "CC", "EE" }, codeListLookup.GetAllCodes());
		}

		void Create_CusCodeList_RefData(UniversalReferenceTestDataHelper helper, string countryCode, string refDataType, string code)
		{
			var dateMIN = ZDateTime.Today.AddMonths(-1);
			var dateMAX = ZDateTime.Today.AddMonths(3);

			helper.CreateCusCodeList(countryCode, refDataType, code, dateMIN, dateMAX);
		}

		public void TestSubTypeList()
		{
			var subTypeList = lookups.SubTypeList;
			AssertEquals("SubTypeList type", typeof(PreviousDocumentClassList), subTypeList.GetType());
			Assert(!subTypeList.ContainsCode("P"));
			var testList = subTypeList;
			AssertEquals(3, testList.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
			lookups = new NctsPreviousDocumentPhase4Lookups(previousDocument);
		}

		NctsPreviousDocument previousDocument;
		NctsPreviousDocumentPhase4Lookups lookups;
	}
}
