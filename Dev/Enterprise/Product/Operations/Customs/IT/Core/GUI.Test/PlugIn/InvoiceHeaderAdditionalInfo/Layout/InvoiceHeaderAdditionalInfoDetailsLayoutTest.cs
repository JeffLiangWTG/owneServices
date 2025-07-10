using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(InvoiceHeaderAdditionalInfoDetailsLayout))]
sealed class InvoiceHeaderAdditionalInfoDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInformationDetailsLayoutBuilder();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (AdditionalInformationDetailsControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
			yield return (AdditionalInformationDetailsControlBag.Instance.FullTypeCodeFindBox, ControlWidthClass.Long);
			yield return (AdditionalInformationDetailsControlBag.Instance.ReferenceTextBox, ControlWidthClass.Auto);
			yield return (InvoiceHeaderAdditionalInfoDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Auto);
		}
	}
}
