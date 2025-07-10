using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(PartPivotSupportingDocumentFieldsLayout))]
sealed class PartPivotSupportingDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentFieldsLayoutBuilder<Business.Declaration.SupportingDocument>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.CodeCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.YearOfIssueTextBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.CountryCodeCodeFindBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.IssuingAuthorityTextBox, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.QuantityCalcDropEdit, ControlWidthClass.Long);
			yield return (SupportingDocumentFieldsControlBag.Instance.ValueCalcFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance.DateOfExpiryDateEdit, ControlWidthClass.Auto);
			yield return (SupportingDocumentFieldsControlBag.Instance.AvailabilityDropEdit, ControlWidthClass.Auto);
		}
	}
}
