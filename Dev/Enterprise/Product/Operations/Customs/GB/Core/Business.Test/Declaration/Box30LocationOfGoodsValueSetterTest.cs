using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class Box30LocationOfGoodsValueSetterTest : TestCaseWithFactory
	{
		public void TestSetBox30NotInList()
		{
			var fakeStansted = Factory.New<RefUNLOCO>();
			fakeStansted.RL_Code = "QQ111";
			fakeStansted.RL_IATA = "XX1";
			var stanstedAirMap = fakeStansted.RefLocoMaps.AddNew();
			stanstedAirMap.RY_LocalPortCode = "LSA";
			stanstedAirMap.RY_RL_NKLocoPort = fakeStansted.RL_Code;
			stanstedAirMap.RY_SystemUsage = "AIR";
			stanstedAirMap.RY_RN = Core.Constants.CountryGuids.UnitedKingdom;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_RL_NKPortOfArrival = "QQ111";
			AssertEquals("", declaration.JE_LocationOfGoods);
		}

		public void TestSetBox30InList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Port, "port");
			var codelist = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "LSA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist.PK, "Type", "CA3");
			Factory.Save();

			var fakeStansted = Factory.New<RefUNLOCO>();
			fakeStansted.RL_Code = "QQ111";
			fakeStansted.RL_IATA = "XX1";
			var stanstedAirMap = fakeStansted.RefLocoMaps.AddNew();
			stanstedAirMap.RY_LocalPortCode = "LSA";
			stanstedAirMap.RY_RL_NKLocoPort = fakeStansted.RL_Code;
			stanstedAirMap.RY_SystemUsage = "AIR";
			stanstedAirMap.RY_RN = Core.Constants.CountryGuids.UnitedKingdom;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_RL_NKPortOfArrival = "QQ111";
			AssertEquals("LSA", declaration.JE_LocationOfGoods);
		}
	}
}
