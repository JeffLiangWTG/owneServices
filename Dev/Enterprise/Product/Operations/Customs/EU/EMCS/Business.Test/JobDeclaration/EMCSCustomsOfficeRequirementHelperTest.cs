using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSCustomsOfficeRequirementHelperTest : TestCaseWithFactory
	{
		public void TestRightRoleIsSetForCustomsOfficeOfEMCS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var office1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT135000", "VICENZA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var office2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE009453", "Ludwigsburg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var office3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE005853", "Baden-Baden", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(office1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXC");
			helper.CreateNewOrGetExistingCusCodeListAttribute(office2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXC");
			helper.CreateNewOrGetExistingCusCodeListAttribute(office3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			var dec = Factory.New<EMCSJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("MainOffice is defaulted", 1, dec.CustomsOffices.Count);
				var officeCodeList = dec.CustomsOffices[0].Lookups.OfficeCodeList;
				officeCodeList.Load();
				AssertEquals("Office1", true, officeCodeList.Contains(office1.PK));
				AssertEquals("Office2", true, officeCodeList.Contains(office2.PK));
				AssertEquals("Office3", false, officeCodeList.Contains(office3.PK));
			});
		}
	}
}
