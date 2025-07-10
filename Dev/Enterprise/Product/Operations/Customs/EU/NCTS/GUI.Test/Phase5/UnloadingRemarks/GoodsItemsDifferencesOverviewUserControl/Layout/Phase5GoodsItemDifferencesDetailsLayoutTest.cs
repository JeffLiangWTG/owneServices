using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDifferencesDetailsLayout))]
	sealed class Phase5GoodsItemDifferencesDetailsLayoutTest : LayoutsAbstractTest
	{
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
				yield return (Phase5GoodsItemDifferencesDetailsControlBag.Instance.SequenceNumberTextBox, ControlWidthClass.Auto);
				yield return (Phase5GoodsItemDifferencesDetailsControlBag.Instance.ItemNumberTextBox, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new Phase5GoodsItemDifferencesDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new Phase5GoodsItemDifferencesDetailsLayoutBuilder<Business.NctsArrivalCargoDesc>();

		PanelLayout layout;
	}
}
