using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.GUI.Testing;

[TestedType(typeof(ITH7ItemDetailsLayouts))]
sealed class ITH7ItemDetailsLayoutsTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
