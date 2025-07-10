using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.Shared.Testing
{
	class PortConverterTests : TestCaseWithFactory
	{
		public void TestIataToChief()
		{
			AssertEquals("LSA", PortConverter.IataToChief("XX1", Factory, "AIR"));
			AssertEquals("111", PortConverter.IataToChief("XX1", Factory, "SEA"));
			AssertEquals("XX2", PortConverter.IataToChief("XX2", Factory, "AIR"));
			AssertEquals("STN", PortConverter.IataToChief("XX2", Factory, "SEA"));
		}

		public void TestIataToUnloco()
		{
			AssertEquals("FRANT", PortConverter.IataToUnloco("XAT", Factory));
			AssertEquals("GBSTN", PortConverter.IataToUnloco("STN", Factory));
			AssertEquals("AUSYD", PortConverter.IataToUnloco("SYD", Factory));
		}

		public void TestUnlocoToChief()
		{
			AssertEquals("LSA", PortConverter.UnlocoToChief("QQ111", Factory, "AIR"));
			AssertEquals("111", PortConverter.UnlocoToChief("QQ111", Factory, "SEA"));
			AssertEquals("LSB", PortConverter.UnlocoToChief("QQ111", Factory, "ROR"));
			AssertEquals("111", PortConverter.UnlocoToChief("QQ111", Factory, "ROA"));
			AssertEquals("111", PortConverter.UnlocoToChief("QQ111", Factory, "RAI"));

			AssertEquals("XX2", PortConverter.UnlocoToChief("QQ222", Factory, "AIR"));
			AssertEquals("STN", PortConverter.UnlocoToChief("QQ222", Factory, "SEA"));
			AssertEquals("STN", PortConverter.UnlocoToChief("QQ222", Factory, "ROR"));
			AssertEquals("STN", PortConverter.UnlocoToChief("QQ222", Factory, "ROA"));
			AssertEquals("222", PortConverter.UnlocoToChief("QQ222", Factory, "RAI"));
		}

		public void TestUnlocoToIata()
		{
			AssertEquals("XAT", PortConverter.UnlocoToIata("FRANT", Factory));
			AssertEquals("STN", PortConverter.UnlocoToIata("GBSTN", Factory));
			AssertEquals("SYD", PortConverter.UnlocoToIata("AUSYD", Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var fakeStansted = Factory.New<RefUNLOCO>();
			fakeStansted.RL_Code = "QQ111";
			fakeStansted.RL_IATA = "XX1";
			var fakeSouthampton = Factory.New<RefUNLOCO>();
			fakeSouthampton.RL_Code = "QQ222";
			fakeSouthampton.RL_IATA = "XX2";
			var stanstedAirMap = fakeStansted.RefLocoMaps.AddNew();
			stanstedAirMap.RY_LocalPortCode = "LSA";
			stanstedAirMap.RY_RL_NKLocoPort = fakeStansted.RL_Code;
			stanstedAirMap.RY_SystemUsage = "AIR";
			stanstedAirMap.RY_RN = Core.Constants.CountryGuids.UnitedKingdom;
			var rorMap = fakeStansted.RefLocoMaps.AddNew();
			rorMap.RY_LocalPortCode = "LSB";
			rorMap.RY_RL_NKLocoPort = fakeStansted.RL_Code;
			rorMap.RY_SystemUsage = "ROR";
			rorMap.RY_RN = Core.Constants.CountryGuids.UnitedKingdom;

			var southamponSeaMap = fakeSouthampton.RefLocoMaps.AddNew();
			southamponSeaMap.RY_LocalPortCode = "STN";
			southamponSeaMap.RY_RL_NKLocoPort = fakeSouthampton.RL_Code;
			southamponSeaMap.RY_SystemUsage = "SEA";
			southamponSeaMap.RY_RN = Core.Constants.CountryGuids.UnitedKingdom;
		}
	}
}
