using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class GuaranteeForEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondTypesList()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR004323", "France OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR004324", "France OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, CusAuthorizationHeaderTypeList.Codes.ComprehensiveGuarantee);
			Factory.Save();

			var guarantee = Factory.New<CusEntryInstruction>().Guarantees.AddNew();
			var officeCodeList = guarantee.Lookups.OfficeCodeList;
			officeCodeList.Load();
			AssertContainsExactElementsInAnyOrder("Only office code with GUA ROlE should be retrieve in France.", new string[] { "FR004323" }, officeCodeList.Select(x => x.ZZD_Code));
		}
	}
}
