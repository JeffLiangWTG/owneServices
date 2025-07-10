using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class OfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList_IMP()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var lookups1 = new ImportOfficeCodeLookups(declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation));
				var lookups2 = new ImportOfficeCodeLookups(declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation));
				var list1 = lookups1.CY_CodeList;
				var list2 = lookups2.CY_CodeList;
				AssertEquals("Valid Codes when declaration.JE_MessageType is IMP", "DSC, PRE, SVO", list1.CodesAsString);
				AssertSame("Cached", list1, list2);
			});
		}

		public void TestOfficeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeList = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IE000001",
				description: "Customs Office IE000001",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeList.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			var lookups = new OfficeCodeLookups(Factory.New<JobDeclaration>().CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit));
			var officeCodeList = lookups.OfficeCodeList;
			officeCodeList.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "IE000001" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}
	}
}
