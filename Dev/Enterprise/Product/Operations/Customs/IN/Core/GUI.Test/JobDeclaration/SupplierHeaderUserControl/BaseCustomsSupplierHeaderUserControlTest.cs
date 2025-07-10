using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(BaseCustomsSupplierHeaderUserControl))]
sealed class BaseCustomsSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<BaseCustomsSupplierHeaderUserControl, JobDeclaration>
{
	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "IncoTermExplainButton" });

	public void TestIncoTermPlaceColumnCaption()
	{
		using var control = new BaseCustomsSupplierHeaderUserControl();
		var incoTermPlaceColumn = control.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTermPlace);
		CombineAssertions(() =>
		{
			AssertEquals("IncoTerm Place short Caption", incoTermPlaceColumn.CaptionResourceString.ShortCaption, "Place");
			AssertEquals("IncoTerm Place medium Caption", incoTermPlaceColumn.CaptionResourceString.MediumCaption, "Inco Place");
			AssertEquals("IncoTerm Place long Caption", incoTermPlaceColumn.CaptionResourceString.Caption, "Incoterm Place");
		});
	}

	public void TestFindIncoTermDropEdit()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.Invoices.AddNew();

		using (var form = new JobDeclarationForm(declaration))
		using (var control = new ExportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var dropEdit = control.Controls.Find("IncoTermDropEdit", true);
			AssertEquals("Should be able to find IncoTermDropEdit", true, dropEdit.Any());
		}
	}

	public void TestDynamicControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.Invoices.AddNew();

		using var form = new JobDeclarationForm(declaration);
		using var control = new ExportSupplierHeaderUserControl();
		form.Controls.Add(control);
		form.Show();

		control.InvoiceTabControl.SelectTab("SupportingDocumentTabPage");
		AssertEquals(typeof(LayoutSupportingDocumentsUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentUserControl").UserControlType);
	}
}
