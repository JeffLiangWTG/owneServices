using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentPreviousDocumentLayoutWithGrid))]
	sealed class HouseConsignmentPreviousDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentPreviousDocumentsGridUserControl);

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentLayoutBuilder<Business.CommonPreviousDocument>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (PreviousDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (PreviousDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
			}
		}
	}
}
