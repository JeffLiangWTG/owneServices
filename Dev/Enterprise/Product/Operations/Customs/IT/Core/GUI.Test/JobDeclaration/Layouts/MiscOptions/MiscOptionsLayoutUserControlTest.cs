using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MiscOptionsLayoutUserControlTest : TestCase
{
	public void TestBadgeCodeDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.BadgeCodeDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.JE_CustomsProfile), userControl.BindTo);
	});

	public void TestSubscriberDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.SubscriberDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.JE_GS_NKCusAgent), userControl.BindTo);
	});

	public void TestPreClearingCheckBox() => CombineAssertions(() =>
	{
		var userControl = control.PreClearingCheckBox;
		AssertType<ZCheckBox>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.ZG_PreClearing), userControl.BindTo);
	});

	public void TestDefermentAccountNumberDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.DefermentAccountNumberDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.JE_DefermentAccountNumber), userControl.BindTo);
	});

	public void TestSupportingInformationUserControl()
	{
		using (var control = new MiscOptionsLayoutUserControl())
		{
			AssertType<SupportingInformationControl>(control.SupportingInformationUserControl);
		}
	}

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

