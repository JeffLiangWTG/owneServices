using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class InvoiceLineUserControlTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<InvoiceLineUserControl>
{
	public void TestTabPages() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		using var form = new JobDeclarationForm(declaration);
		using var control = new InvoiceLineUserControl();
		control.JobDeclaration = declaration;
		form.Controls.Add(control);
		form.Show();

		var expectedTabPagesInOrder = new List<ZTabPage>()
		{
			control.LineDetailTabControl.FindSingle<ZTabPage>("NewLineDetailsTabPage"),
			control.LineChargesTabPage,
			control.LineDetailTabControl.FindSingle<ZTabPage>("VehiclesTabPage"),
			control.LineDetailTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage")
		};
		foreach (var expectedTabPage in expectedTabPagesInOrder)
		{
			AssertEquals($"{expectedTabPage.Name} is visible", true, expectedTabPage.TabVisible);
		}

		AssertContainsExactElementsInExactOrder(expectedTabPagesInOrder, control.LineDetailTabControl.TabPages.Cast<ZTabPage>().ToList());
	});

	public void TestNewColumnsInGrid() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		using var form = new JobDeclarationForm(declaration);
		form.Show();
		form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
		var control = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

		AssertType<ZDropEditColumnStyleInfo>(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewUsed));
	});

	protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;
}
