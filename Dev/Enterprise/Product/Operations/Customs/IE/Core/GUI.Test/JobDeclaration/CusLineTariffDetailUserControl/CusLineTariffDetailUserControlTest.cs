using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class CusLineTariffDetailUserControlTest : TestCaseWithFactory
	{
		public void TestCusLineTariffDetailGrid()
		{
			using (var control = new CusLineTariffDetailUserControl())
			{
				var grid = control.FindSingle<ZGrid>("CusLineTariffDetailGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("CusLineTariffDetailGrid", grid);
					AssertEquals("CusLineTariffDetailGrid is visible", true, grid.Visible);
					AssertEquals("CusLineTariffDetailGrid is bound to JobComInvoiceLine.CusLineTariffDetails", "CusLineTariffDetails", grid.BindTo);
				});
			}
		}

		public void TestGridColumnOrder()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			using (var form = new ZForm())
			using (var control = new CusLineTariffDetailUserControl())
			{
				form.Controls.Add(control);
				form.SetDataBinding(invoiceLine, null);
				form.Show();
				var grid = control.FindSingle<ZGrid>("CusLineTariffDetailGrid");
				var visibleColumns = grid.DefaultColumns.Where(x => x.IsVisible).ToArray();

				CombineAssertions(() =>
				{
					for (var i = 0; i < visibleColumns.Length; i++)
					{
						var (columnName, columnType) = OrderedColumns[i];
						AssertEquals($"The visible column on index:{i} should have this name {columnName}", columnName, visibleColumns[i].ColumnName);
						AssertEquals($"The visible column on index:{i} should have this type {columnType}", columnType, visibleColumns[i].ColumnStyle.GetType());
					}
				});
			}
		}

		public void TestCaptionRenderingEnabled()
		{
			using (var control = new CusLineTariffDetailUserControl())
			{
				AssertEquals("Rendering of captions should be available in the user control", true, control.CaptionRenderingEnabled);
			}
		}

		(string columnName, Type columnType)[] OrderedColumns => new[]
		{
			(CusLineTariffDetail.Schema.BZ_Type, typeof(ZDropEditColumnStyle)),
			(CusLineTariffDetail.Schema.TypeDescription, typeof(ZTextBoxColumnStyle)),
			(CusLineTariffDetail.Schema.BZ_Tariff, typeof(ZCodeFindBoxColumnStyle)),
			(CusLineTariffDetail.Schema.ExciseReferenceNumberDescription, typeof(ZTextBoxColumnStyle)),
			(CusLineTariffDetail.Schema.RateFormula, typeof(ZTextBoxColumnStyle)),
			(CusLineTariffDetail.Schema.BZ_Qty1, typeof(ZCalcEditColumnStyle)),
			(CusLineTariffDetail.Schema.BZ_UQ1, typeof(ZTextBoxColumnStyle)),
			(CusLineTariffDetail.Schema.BZ_Qty2, typeof(ZCalcEditColumnStyle)),
			(CusLineTariffDetail.Schema.BZ_UQ2, typeof(ZTextBoxColumnStyle)),
			(CusLineTariffDetail.Schema.ZG_MethodOfPayment, typeof(ZDropEditColumnStyle)),
			(CusLineTariffDetail.Schema.PaymentMethodDescription, typeof(ZTextBoxColumnStyle)),
		};
	}
}
