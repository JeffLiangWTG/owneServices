using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentsWithGridLayout))]
	sealed class SupportingDocumentsWithGridLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumn;
				yield return SecondColumn;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentDetailsBuilder();

		protected override Type ExpectedGridUserControlType => typeof(SupportingDocumentGridControl);

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
		{
			get
			{
				yield return (SupportingDocumentDetailsControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
				yield return (SupportingDocumentDetailsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (SupportingDocumentDetailsControlBag.Instance.EDocGuidDropEditGuidDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumn
		{
			get
			{
				yield return (SupportingDocumentDetailsControlBag.Instance.StatusTextBox, ControlWidthClass.Long);
				yield return (SupportingDocumentDetailsControlBag.Instance.AdditionalDescriptionTextBox, ControlWidthClass.Long);
				yield return (SupportingDocumentDetailsControlBag.Instance.CustomsDocIDTextBox, ControlWidthClass.Long);
			}
		}
	}
}
