using CargoWise.ComponentModel;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class InvoiceLineBottomPanelUserControlTest : TestCase
	{
		public void TestTotalBondedWhsQuantityCalcEdit()
		{
			var totalBondedWhsQuantityCalcEdit = control.TotalBondedWhsQuantityCalcEdit;
			AssertType<ZCalcEdit>(totalBondedWhsQuantityCalcEdit);
			AssertEquals("totalBondedWhsQuantityCalcEdit Should be readonly", true, totalBondedWhsQuantityCalcEdit.ReadOnly);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.TotalBondedWhsQuantity), false, x => x.DecimalPlaces == 3);
		}

		public void TestTotalCustomsQuantityCalcEdit()
		{
			var totalCustomsQuantityCalcEdit = control.TotalCustomsQuantityCalcEdit;
			AssertType<ZCalcEdit>(control.TotalCustomsQuantityCalcEdit);
			AssertEquals("totalCustomsQuantityCalcEdit Should be readonly", true, totalCustomsQuantityCalcEdit.ReadOnly);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.TotalCustomsQuantity), false, x => x.DecimalPlaces == 6);
		}

		public void TestTotalWeightCalcEdit()
		{
			var totalWeightCalcEdit = control.TotalWeightCalcEdit;
			AssertType<ZCalcEdit>(control.TotalWeightCalcEdit);
			AssertEquals("totalWeightCalcEdit Should be readonly", true, totalWeightCalcEdit.ReadOnly);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.TotalWeight), false, x => x.DecimalPlaces == 3);
		}

		public void TestTotalLinePriceCalcEdit()
		{
			var totalLinePriceCalcEdit = control.TotalLinePriceCalcEdit;
			AssertType<ZCalcEdit>(control.TotalLinePriceCalcEdit);
			AssertEquals("totalLinePriceCalcEdit Should be readonly", true, totalLinePriceCalcEdit.ReadOnly);
		}

		InvoiceLineBottomPanelUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineBottomPanelUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
