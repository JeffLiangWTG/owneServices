using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(SumARegisterModule))]
sealed class SumARegisterModuleTest : ZModuleBasherTest
{
	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.SumARegister;

	protected override string CountryCode => string.Empty;

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
