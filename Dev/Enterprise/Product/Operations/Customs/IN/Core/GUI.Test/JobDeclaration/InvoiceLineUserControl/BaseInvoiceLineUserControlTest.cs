using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(BaseInvoiceLineUserControl))]
sealed class BaseInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestTariffColumnNotExists_UniversalTariffFalse()
	{
		using (var control = new BaseInvoiceLineUserControl())
		{
			AssertNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
		}
	}

	public void TestDynamicControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var header = declaration.Invoices.AddNew();
		header.InvoiceLines.AddNew();

		using var form = new JobDeclarationForm(declaration);
		using var control = new BaseInvoiceLineUserControl();
		form.Controls.Add(control);
		form.Show();

		control.LineDetailTabControl.SelectTab("SupportingDocumentTabPage");
		AssertEquals(typeof(LayoutSupportingDocumentsUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentUserControl").UserControlType);
	}
}
