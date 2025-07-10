using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class GuaranteeAccessCodesSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOfficeOfGuaranteeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var codeIEDUB100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeIEDUB101 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB101", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeGB000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB101.PK, RefCusCodeListAttributeTypes.Codes.ROLE, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeGB000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, CusPermitHeaderApplicationCodeList.Codes.Guarantee);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				var lookups = new GuaranteeAccessCodesSendingObject(cusGuaranteeHeader).Lookups;
				var officeCodeList = lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertEquals(1, officeCodeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "IEDUB100" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			}
		}
	}
}
