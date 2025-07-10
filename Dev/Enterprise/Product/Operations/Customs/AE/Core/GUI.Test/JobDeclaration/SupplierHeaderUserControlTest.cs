using System.Collections.Generic;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(SupplierHeaderUserControl))]
sealed class SupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<SupplierHeaderUserControl, JobDeclaration>
{
	protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

	public void TestChargeColumns()
	{
		var declaration = JobDeclaration.New(Factory);
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
			form.CustomsBrokerageUserControl.SupplierHeaderUserControl.ChargesTabControl.SelectedTab = form.CustomsBrokerageUserControl.SupplierHeaderUserControl.ApportionedTabPage;
			var apportionedGrid = ((SupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl).ApportionedChargesGrid;
			var isDutiableColumn = apportionedGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsDutiable];
			var isGSTApplicableColumn = apportionedGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsGSTApplicable];

			CombineAssertions(() =>
			{
				AssertEquals("J7_IsDutiable column removed", null, isDutiableColumn);
				AssertEquals("J7_IsGSTApplicable column renamed", "Dutiable", isGSTApplicableColumn.ColumnStyle.HeaderText);

				var chargeGrid = ((SupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl).InvoiceChargesGrid;
				isDutiableColumn = chargeGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsDutiable];
				isGSTApplicableColumn = chargeGrid.Columns[BaseInvoiceLineCharge.Schema.J7_IsGSTApplicable];
				AssertEquals("J7_IsDutiable column removed", null, isDutiableColumn);
				AssertEquals("J7_IsGSTApplicable column renamed", "Dutiable", isGSTApplicableColumn.ColumnStyle.HeaderText);
			});
		}
	}
}
