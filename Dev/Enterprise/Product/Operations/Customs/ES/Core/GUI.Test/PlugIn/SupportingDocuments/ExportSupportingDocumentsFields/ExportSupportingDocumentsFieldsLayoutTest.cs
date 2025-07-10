using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ExportSupportingDocumentsFieldsLayout))]
sealed class ExportSupportingDocumentsFieldsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}
 
	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.CodeCodeFindBox, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.StatusDropEdit, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.QuantityAndUnitUserControl, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.SecondQuantityAndUnitUserControl, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.ValueAndCurrencyUserControl, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.DateOfIssueAndExpiryUserControl, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
			yield return (ExportSupportingDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
		}
	}
}
