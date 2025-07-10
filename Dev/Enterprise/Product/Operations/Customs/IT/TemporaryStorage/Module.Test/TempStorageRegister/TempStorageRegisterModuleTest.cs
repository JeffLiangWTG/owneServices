using CargoWise.Types;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

[TestedType(typeof(TempStorageRegisterModule))]
sealed class TempStorageRegisterModuleTest : ZModuleBasherTest
{
	public void TestReadOnlyProperties()
	{
		using var module = GetModule();
		AssertEquals("AllowNew", expected: false, module.AllowNew);
		AssertEquals("AllowEdit", expected: false, module.AllowEdit);
		AssertEquals("AllowDelete", expected: false, module.AllowDelete);
	}

	public void TestGetNewController()
	{
		using var module = GetModule();
		AssertType<TempStorageRegisterController>(module.GetNewController());
	}

	public void TestGetNewGridCollection()
	{
		using var module = GetModule();
		var collection = module.GridCollection;
		AssertType<CusTempStorageRegHeaderCollection>("Type", collection);

		var register = Factory.New<CusTempStorageRegHeader>();
		register.SRH_AppCode = ITConstants.TemporaryStorage.AppCodeTSR;
		Assert(register.MatchesFilter(collection.CompleteFilter));
	}

	public void TestGetNewFilterBusinessObject()
	{
		using var module = GetModule();
		AssertType<TempStorageRegisterFilterBusinessObject>(module.FilterBusinessObject);
	}

	public void TestGetNewFilterControl()
	{
		using var module = GetModule();
		using var filterControl = module.GetNewFilterControlForGrid();
		AssertType<TempStorageRegisterFilterStripControl>(filterControl);
	}

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStorageRegister;

	protected override string CountryCode => Core.Constants.CountryCodes.Italy;

	public override void TestModuleShowsAndCanSearch()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		header.SRH_Reference = "Reference";
		header.SRH_SystemCreateTimeUtc = ZDateTime.Now;
		Factory.Save();
		base.TestModuleShowsAndCanSearch();
	}
}
