using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(NCTS.Phase5GoodsItemPreviousDocumentLayoutWithGrid))]
	sealed class Phase5GoodsItemPreviousDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(GoodsItemPreviousDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new NCTS.PreviousDocumentLayoutBuilder<NctsPreviousDocument>();

		protected override int ControlBagCount => 2;

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (GUI.NCTS.PreviousDocumentControlBag.Instance.ReferenceNumberCodeFindBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.NumOfPackagesDropEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.QuantityDropEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Long);
			}
		}
	}
}
