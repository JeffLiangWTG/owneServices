using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSOfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOfficeCodeListForDispatchOfficeIfDeclarationIsConsolidatedDocument()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DEBER001", "BERLIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.UniversalReferenceConstants.CustomsOfficeAttributes.Excise);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.UniversalReferenceConstants.CustomsOfficeAttributes.Excise);
			Factory.Save();

			var declaration = Factory.New<EMCSJobDeclaration>();
			var officeCode = declaration.CustomsOffices.AddNew();

			CombineAssertions(() =>
			{
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
				var officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder("No ConsolidatedDocument, office role 'DIS' selected: office codes", new[] { "DEBER001", "IEDUB100" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

				declaration.SetConsolidatedDocument();
				officeCodeList = officeCode.Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertContainsExactElementsInAnyOrder("Is ConsolidatedDocument, office role 'DIS' selected: office codes", new[] { "DEBER001" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});
		}
	}
}
