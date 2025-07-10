using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CIQProductQualificationsLookupTesting : BusinessObjectLookupsTestCase
	{
		public void TestUntranslatableProductQualifications()
		{
			var li = Factory.GetCachedValue<ProductQualificationCodeList>();
			Assert("CIQ Product qualifications should be untranslatable", li is UntranslatableCodeDescriptionPairList);
		}

		public void TestCodeList()
		{
			var tetItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var testItem = tetItems.InvoiceLine.CIQProductQualifications.AddNew();
			var testList = testItem.Lookups.CodeList;
			AssertEquals(31, testList.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "103", "203", "401", "404", "417", "425", "426", "429", "518", "522", "526", "527", "528", "529", "530", "602", "614", "615", "616", "617", "618", "619", "620", "621", "622", "623", "624", "626", "627", "628", "630" }, testList.Cast<CodeDescriptionPair>().Select(x => x.Code));
			tetItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testList = testItem.Lookups.CodeList;
			AssertEquals(60, testList.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "105", "106", "107", "108", "109", "110", "111", "112", "113", "114", "115", "116", "117", "203", "325", "328", "330", "331", "332", "401", "402", "408", "409", "410", "411", "412", "416", "422", "423", "424", "428", "429", "430", "516", "517", "519", "522", "523", "524", "526", "527", "528", "529", "530", "601", "603", "604", "605", "606", "607", "608", "609", "610", "611", "612", "613", "629", "630", "800", "900" }, testList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public void TestUnitOfMeasurementList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "018", "Unit 1", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "074", "Unit 2", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "101", "Unit 3", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testItem = testItems.InvoiceLine.CIQProductQualifications.AddNew();
			var testList = testItem.Lookups.UnitOfMeasurementList;
			AssertEquals(3, testList.Count);
			Assert(testList.ContainsCode("018"));
			Assert(testList.ContainsCode("074"));
			Assert(testList.ContainsCode("101"));
		}
	}
}
