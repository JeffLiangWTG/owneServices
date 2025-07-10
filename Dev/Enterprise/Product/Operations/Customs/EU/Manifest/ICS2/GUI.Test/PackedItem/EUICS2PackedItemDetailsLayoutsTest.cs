using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2PackedItemDetailsLayouts))]
	sealed class EUICS2PackedItemDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PackedItemDetailsLayoutBuilder<Business.AsycudaPack>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Long);
				yield return (EUICS2PackedItemDetailsControlBag.Instance.CusCodeFindBox, ControlWidthClass.Auto);
				yield return (EUICS2PackedItemDetailsControlBag.Instance.PostalValueCalcFindBox, ControlWidthClass.Long);
				yield return (EUICS2PackedItemDetailsControlBag.Instance.TypeOfGoodsDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomEntriesSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomEntriesGrid, ControlWidthClass.Long);
			}
		}
	}
}
