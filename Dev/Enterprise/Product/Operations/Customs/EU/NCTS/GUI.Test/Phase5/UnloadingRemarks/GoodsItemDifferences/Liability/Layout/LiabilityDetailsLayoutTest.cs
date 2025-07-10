using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(LiabilityDetailsLayout))]
	sealed class LiabilityDetailsLayoutTest : LayoutsAbstractTest
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
				yield return (LiabilityDetailsControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (LiabilityDetailsControlBag.Instance.CommodityCodeTariffFindBox, ControlWidthClass.Long);
				yield return (LiabilityDetailsControlBag.Instance.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
				yield return (LiabilityDetailsControlBag.Instance.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
				yield return (LiabilityDetailsControlBag.Instance.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
				yield return (LiabilityDetailsControlBag.Instance.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
				yield return (LiabilityDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (LiabilityDetailsControlBag.Instance.FeesUserControl, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new LiabilityDetailsLayoutBuilder<Business.NctsArrivalCargoDesc>();

		protected override Type ExpectedGridUserControlType => typeof(LiabilityDetailsUserControl);
	}
}
