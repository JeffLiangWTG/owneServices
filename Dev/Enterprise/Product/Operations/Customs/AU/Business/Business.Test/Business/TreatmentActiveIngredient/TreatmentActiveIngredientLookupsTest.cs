using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TreatmentActiveIngredientLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSTreatmentActiveIngredient;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(codeType, "Desc", Core.Constants.CountryCodes.Australia);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Australia,
				codeType,
				"123", "123 Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Australia,
				codeType,
				"321", "321 Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var process = Factory.New<QuarantineExDocEstablishmentAndTime>();
			var collection = new TreatmentActiveIngredientCollection(process);
			var obj = collection.AddNew();
			AssertEquals("123, 321", obj.Lookups.CY_CodeList.CodesAsString);
		}
	}
}
