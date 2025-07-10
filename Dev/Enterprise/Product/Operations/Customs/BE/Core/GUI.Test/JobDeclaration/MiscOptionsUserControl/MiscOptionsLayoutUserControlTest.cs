using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class MiscOptionsLayoutUserControlTest : TestCase
{
	public void TestVATDeferTypeDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.VATDeferTypeDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.ZG_VATDeferType), userControl.BindTo);
	});

	protected override void SetUp()
	{
		base.SetUp();
		control = new MiscOptionsLayoutUserControl();
	}
	MiscOptionsLayoutUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
