using System.Linq;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class CommercialInvoiceDetailsUserControlTest : TestCase
{
	public void TestPaymentMethodDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "PaymentMethodDropEdit").First());

	public void TestTotNoOfInvPagesTextBox() => AssertType<ZCalcEdit>(control.Find(c => c.Name == "TotNoOfInvPagesCalcEdit").First());

	public void TestValuationCodeDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "ValuationCodeDropEdit").First());

	public void TestInvoiceTypeDropEdit() => AssertType<ZDropEdit>(control.Find(c => c.Name == "InvoiceTypeDropEdit").First());

	public void TestAttestationNoTextBox() => AssertType<ZTextBox>(control.Find(c => c.Name == "AttestationNoTextBox").First());

	protected override void SetUp()
	{
		base.SetUp();
		control = new CommercialInvoiceDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	CommercialInvoiceDetailsUserControl control;
}
