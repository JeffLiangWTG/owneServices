using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class EntryDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceAmountCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.InvoiceAmountCalcDropEdit);
		}

		public void TestCircuitTextBox()
		{
			AssertType<ZTextBox>(control.CircuitTextBox);
		}

		public void TestCircuitCanTextBox()
		{
			AssertType<ZTextBox>(control.CircuitCanTextBox);
		}

		public void TestCSVClearanceTextBox()
		{
			AssertType<ZTextBox>(control.CSVClearanceTextBox);
		}

		public void TestVATDeferredCalcEdit()
		{
			AssertType<ZCalcEdit>(control.VATDeferredCalcEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		EntryDetailsUserControl control;
	}
}
