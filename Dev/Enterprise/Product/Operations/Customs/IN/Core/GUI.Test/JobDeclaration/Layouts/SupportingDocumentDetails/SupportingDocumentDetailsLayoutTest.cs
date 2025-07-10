using System;
using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SupportingDocumentDetailsLayout))]
sealed class SupportingDocumentDetailsLayoutTest : LayoutsAbstractTest
{
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
			yield return (commonBag.ImageReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (commonBag.DocumentTypeCodeFindBox, ControlWidthClass.Long);
			yield return (commonBag.IssuingPartyGroupBox, ControlWidthClass.LongControl);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentDetailsLayoutBuilder<SupportingDocument>();

	SupportingDocumentDetailsControlBag commonBag => SupportingDocumentDetailsControlBag.Instance;

	protected override Type ExpectedGridUserControlType => typeof(SupportingDocumentGridControl);
}
