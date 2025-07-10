using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(AdditionalInformationFieldsLayout))]
sealed class AdditionalInformationFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestControlVisibility_Export()
	{
		AdditionalInformation.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			AssertEquals("CodeDropEdit should be visible on a export Invoice Line", true, Layout.IsVisible(AdditionalInformationFieldsControlBag.Instance.CodeDropEdit, AdditionalInformation));
			AssertEquals("DescriptionTextBox should be visible on a export Invoice Line", true, Layout.IsVisible(AdditionalInformationFieldsControlBag.Instance.DescriptionTextBox, AdditionalInformation));
			AssertEquals("ReferenceNumberDropEdit should not be visible on a export Invoice Line", false, Layout.IsVisible(AdditionalInformationFieldsControlBag.Instance.ReferenceNumberDropEdit, AdditionalInformation));
		});
	}

	public void TestControlVisibility_Import()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CodeDropEdit should be visible on a export Invoice Line", true, Layout.IsVisible(AdditionalInformationFieldsControlBag.Instance.CodeDropEdit, AdditionalInformation));
			AssertEquals("DescriptionTextBox should be visible on a export Invoice Line", false, Layout.IsVisible(AdditionalInformationFieldsControlBag.Instance.DescriptionTextBox, AdditionalInformation));
			AssertEquals("ReferenceNumberDropEdit should not be visible on a export Invoice Line", true, Layout.IsVisible(AdditionalInformationFieldsControlBag.Instance.ReferenceNumberDropEdit, AdditionalInformation));
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
			yield return (AdditionalInformationFieldsControlBag.Instance.CodeDropEdit, ControlWidthClass.Auto);
			yield return (AdditionalInformationFieldsControlBag.Instance.ReferenceNumberDropEdit, ControlWidthClass.Auto);
			yield return (AdditionalInformationFieldsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
		}
	}

	AdditionalInformation AdditionalInformation
	{
		get
		{
			if (additionalInformation == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				additionalInformation = invoiceLine.AdditionalInformations.AddNew();
			}
			return additionalInformation;
		}
	}
	AdditionalInformation additionalInformation;

	public PanelLayout Layout => layout ?? (layout = new AdditionalInformationFieldsLayout().Layout);
	PanelLayout layout;

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInformationFieldsLayoutBuilder<AdditionalInformation>();
}
