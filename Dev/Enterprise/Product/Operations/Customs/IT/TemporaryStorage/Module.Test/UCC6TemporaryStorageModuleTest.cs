using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

[TestedType(typeof(UCC6TemporaryStorageModule))]
sealed class UCC6TemporaryStorageModuleTest : ZModuleBasherTest
{
	public void TestFilterBusinessObject()
	{
		using var module = new UCC6TemporaryStorageModule();
		AssertType<UCC6TemporaryStorageFilterStripBusinessObject>(module.FilterBusinessObject);
	}

	public void TestNewFilterControl()
	{
		using var uCC6TemporaryStorageModule = new UCC6TemporaryStorageModuleForTest();
		var uCC6TemporaryStorageFilterControl = uCC6TemporaryStorageModule.GetNewFilterControl_Exposed();
		AssertType<UCC6TemporaryStorageFilterControl>(uCC6TemporaryStorageFilterControl);
		uCC6TemporaryStorageFilterControl.Dispose();
	}

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.UCC6TemporaryStorage;

	protected override string CountryCode => Core.Constants.CountryCodes.Italy;
}

class UCC6TemporaryStorageModuleForTest : UCC6TemporaryStorageModule
{
	public IFilterControl GetNewFilterControl_Exposed() => base.GetNewFilterControl();
}
