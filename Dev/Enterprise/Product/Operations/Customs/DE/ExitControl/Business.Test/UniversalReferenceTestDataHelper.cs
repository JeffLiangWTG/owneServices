using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	public class UniversalReferenceTestDataHelper : Universal.Testing.UniversalReferenceTestDataHelper
	{
		public UniversalReferenceTestDataHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void CreateCountryListOfAU_DE_FR(string codeType)
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var eurpoeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – Export Nationality");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
		}
	}
}
