using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CusEntryLineExtendedInformationQuantitiesLayout))]
sealed class CusEntryLineExtendedInformationQuantitiesLayoutTest : LayoutsAbstractTest
{
	public void TestCalcCustomsNetWeightVisibilityForImport() => AssertCalcCustomsNetWeightVisibility(CHJobMessageTypeList.Codes.Import, true);
	public void TestCalcCustomsNetWeightVisibilityForExport() => AssertCalcCustomsNetWeightVisibility(CHJobMessageTypeList.Codes.Export, false);
	public void TestCalcCustomsNetWeightVisibilityForExportDeclarationActivation() => AssertCalcCustomsNetWeightVisibility(CHJobMessageTypeList.Codes.ExportDeclarationActivation, false);

	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusEntryLineExtendedInformationQuantitiesLayoutBuilder<CusEntryLine>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CusEntryLineExtendedInformationQuantitiesControlBag.Instance.CalcCustomsNetWeightDropEdit, ControlWidthClass.Auto);
			yield return (CusEntryLineExtendedInformationQuantitiesControlBag.Instance.CalcAdditionalQuantityDropEdit, ControlWidthClass.Auto);
			yield return (CusEntryLineExtendedInformationQuantitiesControlBag.Instance.CalcNetWeightDropEdit, ControlWidthClass.Auto);
			yield return (CusEntryLineExtendedInformationQuantitiesControlBag.Instance.CalcGrossWeightDropEdit, ControlWidthClass.Auto);
		}
	}

	void AssertCalcCustomsNetWeightVisibility(string declarationType, bool expectedVisibility)
	{
		EntryLine.Declaration.JE_MessageType = declarationType;
		using (var form = new ZForm(EntryLine))
		using (var control = new CusEntryLineExtendedInformationsUserControl())
		{
			form.Controls.Add(control);
			form.SetDataBinding(EntryLine, ".");
			form.Show();

			var codeEditBox = control.Controls.Find("CalcCustomsNetWeightDropEdit", true).FirstOrDefault();
			AssertNotNull(nameof(codeEditBox), codeEditBox);
			AssertEquals(nameof(codeEditBox.Visible), expectedVisibility, codeEditBox.Visible);
		}
	}

	CusEntryLine EntryLine => entryLine ??= GetEntryLineForTest();
	CusEntryLine entryLine;

	CusEntryLine GetEntryLineForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		return entryLine;
	}
}
