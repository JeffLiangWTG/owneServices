using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class UnitsExtensionTest : TestCaseWithFactory
	{
		public void TestConvertCargoWiseToES_PKAndGFNotInDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;

			var mapType = helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Universal.MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(mapType.ZZP_MapType, "KGM", "KG", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), countryCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				var cw1Value = ZString.Empty;
				AssertEquals("When ZString is empty there is no conversion to any value", ZString.Empty, cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "ZZ";
				AssertEquals("When ZString does not have a valid value there is no conversion to any value", "ZZ", cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "PK";
				AssertEquals("When ZString does not have a valid value in database but is PK, method returns conversion to KN", "KN", cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "KGM";
				AssertEquals("When ZString has a valid value in database, method returns a valid conversion", "KG", cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "GF";
				AssertEquals("When ZString does not have a valid value in database but is GF, method returns conversion to KN", "KN", cw1Value.ConvertCargoWiseToES(Factory));
			});
		}

		public void TestConvertCargoWiseToES_PKAndGFInDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;

			var mapType = helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Universal.MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(mapType.ZZP_MapType, "KGM", "KG", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), countryCode);
			helper.CreateCusMap(mapType.ZZP_MapType, "PK", "AA", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), countryCode);
			helper.CreateCusMap(mapType.ZZP_MapType, "GF", "BB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), countryCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				var cw1Value = ZString.Empty;
				AssertEquals("When ZString is empty there is no conversion to any value", ZString.Empty, cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "ZZ";
				AssertEquals("When ZString does not have a valid value there is no conversion to any value", "ZZ", cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "KGM";
				AssertEquals("When ZString has a valid value in database, method returns a valid conversion", "KG", cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "PK";
				AssertEquals("When ZString has a valid value in database for PK, method returns the database conversion", "AA", cw1Value.ConvertCargoWiseToES(Factory));

				cw1Value = "GF";
				AssertEquals("When ZString has a valid value in database for GF, method returns the database conversion", "BB", cw1Value.ConvertCargoWiseToES(Factory));
			});
		}

		public void TestConvertESToCargoWise()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;

			var mapType = helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Universal.MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(mapType.ZZP_MapType, "KGM", "KG", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), countryCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				var esValue = ZString.Empty;
				AssertEquals("When ZString is empty there is no conversion to any value", ZString.Empty, esValue.ConvertESToCargoWise(Factory));

				esValue = "ZZ";
				AssertEquals("When ZString does not have a valid value there is no conversion to any value", "ZZ", esValue.ConvertESToCargoWise(Factory));

				esValue = "KG";
				AssertEquals("When ZString have a valid value, it returns a valid conversion", "KGM", esValue.ConvertESToCargoWise(Factory));
			});
		}
	}
}
