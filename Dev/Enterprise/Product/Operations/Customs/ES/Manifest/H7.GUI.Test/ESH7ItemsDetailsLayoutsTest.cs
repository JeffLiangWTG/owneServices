using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESH7ItemsDetailsLayouts))]
	class ESH7ItemsDetailsLayoutsTest : LayoutsAbstractTest
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
				yield return (EUH7ItemDetailsCommonControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (EUH7ItemDetailsCommonControlBag.Instance.QuantityCalcEdit, ControlWidthClass.Auto);
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

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertCaption(EUH7ItemDetailsCommonControlBag.Instance.IntrinsicValueConvertToLocalCurrencyControl, expectedCaption: "Intrinsic Value");
				AssertCaption(EUH7ItemDetailsCommonControlBag.Instance.SupplementaryDropEdit, expectedCaption: "Suppl. Units");
				AssertCaption(EUH7ItemDetailsCommonControlBag.Instance.QuantityCalcEdit, expectedCaption: "Quantity");
			});

			void AssertCaption(ControlReference controlReference, string expectedCaption)
			{
				LayoutForTesting.TryGetCaption(controlReference, null, out var captionData);
				AssertEquals($"Caption for {controlReference.ControlName}", expectedCaption, captionData?.Caption);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>();
	}
}
