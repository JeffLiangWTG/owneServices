using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentSupportingDocumentLayoutWithGrid))]
sealed class HouseConsignmentSupportingDocumentLayoutWithGridTest : LayoutsAbstractTest
{
	public void TestYearOfIssueTextBoxCaption()
	{
		var layoutWithGridProvider = (IPanelLayoutWithGridProvider)new HouseConsignmentSupportingDocumentLayoutWithGrid();
		var layout = layoutWithGridProvider.Layout;

		layout.TryGetCaption(SupportingDocumentControlBag.Instance.YearOfIssueTextBox, Factory.New<NctsSupportingDocument>(), out var captionData);
		AssertEquals("Caption for YearOfIssueTextBox", "Year of Issue", captionData?.Caption);
	}

	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentSupportingDocumentsGridUserControl);

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentLayoutBuilder<NctsSupportingDocument>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.SupportingDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.SupportingDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (SupportingDocumentControlBag.Instance.YearOfIssueTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.SupportingDocumentControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.SupportingDocumentControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.SupportingDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
		}
	}
}
