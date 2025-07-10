using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class QueryOnGuaranteeSendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQueryIdentifierList()
		{
			var helper = new EU.NCTS.Business.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ, "NCTS Query Identifier");
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
				code: "100",
				description: "Description 1",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: UniversalReferenceConstants.RefCusCodeListTypes.Codes.NGUAQ,
				code: "200",
				description: "Another Description 2",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);

			Factory.Save();

			var lookups = new QueryOnGuaranteeSendingAction(Factory.New<NctsHeader>()).Lookups;
			var queryIdentifiers = lookups.QueryIdentifier;
			AssertContainsExactElementsInAnyOrder(new[] { "100", "200" }, queryIdentifiers.Cast<ICodeDescription>().Select(x => x.Code));
		}
	}
}
