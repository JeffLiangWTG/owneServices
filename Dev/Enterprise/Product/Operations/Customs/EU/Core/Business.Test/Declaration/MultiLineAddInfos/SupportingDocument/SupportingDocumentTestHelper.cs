using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public static class SupportingDocumentTestHelper
	{
		public static void SetupRefCusCodeList(BusinessObjectFactory factory)
		{
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;

			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "United Kingdom", eun);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(countryCode, new[] { importCodeType },
				"Y057", "Goods not requiring the presentation of a FLEGT import licence for timber",
				new Dictionary<string, string[]>
				{
					["LEVEL"] = new[] { "ITEM" },
					["StatementText"] = new[] { "Import licence not required" }
				},
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(countryCode, new[] { importCodeType, exportCodeType },
				"Y922", "Other than cats and dogs fur as mentioned by Regulation (EC) No 1523/2007",
				new Dictionary<string, string[]>
				{
					["LEVEL"] = new[] { "ITEM" },
					["StatementText"] = new[] { "Education and taxidermy only", "No cat or dog fur" }
				},
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}
	}
}
