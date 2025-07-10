using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public class InvoiceLineChargesUserControlTest : TestCaseWithFactory
{
	public void TestInvoiceLineChargesGridColumns()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new InvoiceLineChargesUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var index = 0;
			var invoiceChargesGridColumnStyle = userControl.ChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_ChargeType, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.ChargeCodeDescription, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_Amount, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_RX_NKCurrency, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_IsDutiable, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_IsGSTApplicable, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_Percentage, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.IsJ7_ExchangeRateUserEnterable, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_ExchangeRate, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, BaseInvoiceLineCharge.Schema.J7_ExchangeRateDate, index++);
			});
		}
	}

	public void TestInvoiceLineChargesGridColumnsLabels()
	{
		using (var form = new ZForm(declaration))
		using (var userControl = new InvoiceLineChargesUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var invoiceChargesGridColumns = userControl.ChargesGrid.Columns;

			CombineAssertions(() =>
			{
				AssertEquals("Code", invoiceChargesGridColumns[0].ColumnStyle.HeaderText);
				AssertEquals("Desc.", invoiceChargesGridColumns[1].ColumnStyle.HeaderText);
				AssertEquals("Amount", invoiceChargesGridColumns[2].ColumnStyle.HeaderText);
				AssertEquals("Curr.", invoiceChargesGridColumns[3].ColumnStyle.HeaderText);
				AssertEquals("Dutiable", invoiceChargesGridColumns[4].ColumnStyle.HeaderText);
				AssertEquals("Statistical Value Applicable", invoiceChargesGridColumns[5].ColumnStyle.HeaderText);
				AssertEquals("VAT Apply", invoiceChargesGridColumns[6].ColumnStyle.HeaderText);
				AssertEquals("Included In Invoice", invoiceChargesGridColumns[7].ColumnStyle.HeaderText);
				AssertEquals("% of Line Price", invoiceChargesGridColumns[8].ColumnStyle.HeaderText);
				AssertEquals("Fixed Rate", invoiceChargesGridColumns[9].ColumnStyle.HeaderText);
				AssertEquals("Exchange Rate", invoiceChargesGridColumns[10].ColumnStyle.HeaderText);
				AssertEquals("Exchange Rate Date", invoiceChargesGridColumns[11].ColumnStyle.HeaderText);
				AssertEquals("Included In Line", invoiceChargesGridColumns[12].ColumnStyle.HeaderText);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
	}
	JobDeclaration declaration;
}
