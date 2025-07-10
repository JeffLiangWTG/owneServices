using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumARegisterModule))]
	public class SumARegisterModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.SumARegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		public override void TestModuleShowsAndCanSearch()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_AppCode = "SUM";
			header.SRH_Reference = "Reference";
			header.SRH_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}
	}
}
