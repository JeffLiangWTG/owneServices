using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EXDOCAqisPlaceCollection))]
	sealed class EXDOCAqisPlaceCollectionTest : EXDOCRefCodeCollectionTest<EXDOCAqisPlaceCollection>
	{
		public void TestCreateRelationshipFilter()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("AQISP", "AQISP");

			var darwin = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AQISP", "DR2", "DARWIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.QuarantineRegion, "");
			refHelper.CreateCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Dairy, "");
			refHelper.CreateCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Eggs, "");
			refHelper.CreateCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Fish, "");
			refHelper.CreateCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.GrainsAndPlants, "");
			refHelper.CreateCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Horticulture, "");

			var canberra = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AQISP", "CBR", "CANBERRA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.QuarantineRegion, "");
			refHelper.CreateCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.Dairy, "");
			refHelper.CreateCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.InedibleMeat, "");
			refHelper.CreateCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.Meat, "");
			refHelper.CreateCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.SkinsAndHides, "");
			refHelper.CreateCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.Wool, "");

			var redcliffs = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "AQISP", "RED", "RED CLIFFS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateCusCodeListAttribute(redcliffs.PK, EXDOCCommodityCodeAttributes.Codes.QuarantineOffice, "");
			refHelper.CreateCusCodeListAttribute(redcliffs.PK, EXDOCCommodityCodeAttributes.Codes.State, "VIC");

			Factory.Save();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var header = helper.Header1.QuarantineExDocHeader;

			var collection = new EXDOCAqisPlaceCollection(header);
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			collection.Load();
			AssertEquals("Collection is all codes", 3, collection.Count);

			header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			collection.Load();
			AssertEquals("Collection is only the Quarantine Region codes", 2, collection.Count);
		}
	}
}
