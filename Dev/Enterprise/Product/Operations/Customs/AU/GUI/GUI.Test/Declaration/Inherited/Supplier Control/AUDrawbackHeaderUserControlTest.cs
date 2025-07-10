using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUDrawbackHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceHeaderGridContext()
		{
			using (AUDrawbackHeaderUserControl control = new AUDrawbackHeaderUserControl())
			{
				AssertEquals("Context is set", nameof(Customs.GUI.DeclarationType.Drawback), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvoiceHeadersBoundGridColumns()
		{
			using (var form = new ZForm())
			using (var control = new AUDrawbackHeaderUserControl())
			{
				form.Controls.Add(control);
				control.InitializeGridLayout();
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_InvoiceNumber"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_OH_Supplier"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_OA_SupplierAddress"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("SupplierName"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_IncoTerm"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_InvoiceAmount"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_RX_NKInvoice_Currency"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_InvoiceCurrExRate"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("InvoiceLineTotal"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Calc_BalanceString"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_InvoiceDate"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_InvoiceCurrLandedCostExRate"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Calc_FOBAmount"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Calc_FOBCurrency"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Calc_CIFAmount"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Calc_CIFCurrency"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_OH_Buyer"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Volume"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_VolumeUQ"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Weight"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_WeightUQ"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_NetWeight"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_NetWeightUQ"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_PaymentNo"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_PaymentAmount"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_PaymentDate"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_PaymentExRate"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_CU_RelatedHouseBill"));
				AssertNotNull(control.InvoiceHeadersBoundGrid.GetColumnStyle("JZ_Calc_GroupInvoice"));
			}
		}

		public void TestBaseGroupChargesGridColumns()
		{
			using (var control = new AUDrawbackHeaderUserControl())
			{
				AssertNotNull(control.BaseGroupChargesGrid.GetColumnStyle("J7_IsCalculated"));
			}
		}
	}
}
