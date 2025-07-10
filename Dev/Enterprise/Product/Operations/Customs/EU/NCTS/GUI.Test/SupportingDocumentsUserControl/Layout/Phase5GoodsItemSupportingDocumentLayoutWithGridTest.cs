using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemSupportingDocumentLayoutWithGrid))]
	sealed class Phase5GoodsItemSupportingDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(GoodsItemSupportingDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentLayoutBuilder<Business.NctsSupportingDocument>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SupportingDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (SupportingDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (SupportingDocumentControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
				yield return (SupportingDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
			}
		}
	}
}
