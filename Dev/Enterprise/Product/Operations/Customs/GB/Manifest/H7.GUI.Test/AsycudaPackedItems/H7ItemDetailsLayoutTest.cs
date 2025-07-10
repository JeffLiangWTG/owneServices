using System.Collections.Generic;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(H7ItemDetailsLayout))]
	sealed class H7ItemDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EUH7ItemDetailsCommonControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.IntrinsicValueConvertToLocalCurrencyControl, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.SupplementaryDropEdit, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EUH7ItemDetailsCommonControlBag.Instance.CustomEntriesSeparatorUserControl, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.CustomEntriesGrid, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>();
	}
}
