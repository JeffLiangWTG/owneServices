using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemPreviousDocumentLayoutWithGrid))]
	sealed class GoodsItemPreviousDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (PreviousDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Auto);
				yield return (PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
				yield return (PreviousDocumentControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
				yield return (PreviousDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Auto);
			}
		}
	}
}
