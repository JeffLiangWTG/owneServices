using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentAdditionalDocumentLayoutWithGrid))]
	sealed class HouseConsignmentAdditionalDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override Type ExpectedGridUserControlType => typeof(HouseConsignmentAdditionalDocumentGridUserControl);

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalDocumentLayoutBuilder<Business.NctsBillAdditionalDocument>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (AdditionalDocumentControlBag.Instance.KindDropEdit, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (AdditionalDocumentControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			}
		}
	}
}
