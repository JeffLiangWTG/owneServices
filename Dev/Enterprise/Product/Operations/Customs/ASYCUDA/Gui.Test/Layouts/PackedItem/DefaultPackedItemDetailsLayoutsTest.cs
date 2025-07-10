using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(DefaultPackedItemDetailsLayouts))]
	sealed class DefaultPackedItemDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new PackedItemDetailsLayoutBuilder<AsycudaPack>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Auto);
				yield return (CommonPackedItemDetailsControlBag.Instance.PackStatusTextBox, ControlWidthClass.Auto);
				yield return (CommonPackedItemDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomsValueCalcEdit, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.TaxAmountCalcEdit, ControlWidthClass.Medium);
				yield return (CommonPackedItemDetailsControlBag.Instance.DutyAmountCalcEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomEntriesSeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonPackedItemDetailsControlBag.Instance.CustomEntriesGrid, ControlWidthClass.Long);
			}
		}
	}
}
