using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(EntryHeaderModuleForTest))]
sealed class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
{
	public void TestHasActions() => AssertEquals(true, Module.HasActions);

	public void TestAllowNew() => AssertEquals(false, Module.AllowNew);

	public void TestAllowDelete() => AssertEquals(false, Module.AllowDelete);

	public void TestGetNewController() => AssertType<EntryHeaderController>(Module.GetNewController());

	public void TestGetNewFilterControl() => AssertType<EntryHeaderFilterUserControl>(FilterControl);

	public void TestGetNewFilterBusinessObject() => AssertType<EntryHeaderFilterBusinessObject>(Module.GetNewFilterBusinessObject());

	EntryHeaderModuleForTest Module => module ??= new EntryHeaderModuleForTest();
	EntryHeaderModuleForTest module;

	ZArchitecture.GUI.IFilterControl FilterControl => filterControl ??= Module.GetNewFilterControl();
	ZArchitecture.GUI.IFilterControl filterControl;

	protected override void TearDown()
	{
		filterControl?.Dispose();
		module?.Dispose();
		base.TearDown();
	}

	class EntryHeaderModuleForTest : EntryHeaderModule
	{
		public new ZArchitecture.GUI.IFilterControl GetNewFilterControl() => base.GetNewFilterControl();

		public new FilterBusinessObject GetNewFilterBusinessObject() => base.GetNewFilterBusinessObject();
	}
}
