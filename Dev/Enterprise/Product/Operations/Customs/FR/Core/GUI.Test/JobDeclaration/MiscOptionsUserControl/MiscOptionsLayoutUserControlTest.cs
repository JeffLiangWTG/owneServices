using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing;

sealed class MiscOptionsLayoutUserControlTest : TestCase
{
	public void TestBVATDeferTypeDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.VATDeferTypeDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.ZG_VATDeferType), userControl.BindTo);
	});

	public void TestVatCanaDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.VatCanaDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.ZG_VATCANACode), userControl.BindTo);
	});

	public void TestVATDeferNumberTextBox() => CombineAssertions(() =>
	{
		var userControl = control.VATDeferNumberTextBox;
		AssertType<ZTextBox>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.ZG_VATDeferNumber), userControl.BindTo);
	});

	public void TestChargePaymentOrDestinationIDsDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.ChargePaymentOrDestinationIDsDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.ChargePaymentOrDestinationID), userControl.BindTo);
	});

	public void TestCustomsGuaranteeNumberDropEdit() => CombineAssertions(() =>
	{
		var userControl = control.CustomsGuaranteeNumberDropEdit;
		AssertType<ZDropEdit>("Type", userControl);
		AssertEquals("BindTo", nameof(JobDeclaration.JE_CustomsGuaranteeNumber), userControl.BindTo);
	});

	public void TestSupportingInformationUserControl()
	{
		using (var control = new MiscOptionsLayoutUserControl())
		{
			AssertType<SupportingInformationControl>(control.SupportingInformationUserControl);
		}
	}

	public void TestPaymentSeparatorUserControl()
	{
		using (var control = new MiscOptionsLayoutUserControl())
		{
			AssertType<SeparatorUserControl>(control.PaymentSeparatorUserControl);
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
