using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGrid))]
	sealed class Phase5HouseConsignmentDifferencesPreviousDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentDifferencesPreviousDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (PreviousDocumentControlBag.Instance.LineNoCalcEdit, ControlWidthClass.Auto);
				yield return (PreviousDocumentControlBag.Instance.StatusTextBox, ControlWidthClass.Long);
				yield return (PreviousDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (PreviousDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
			}
		}
	}
}
