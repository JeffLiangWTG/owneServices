using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GDMBasicLayout))]
	sealed class GDMBasicLayoutTest : LayoutsAbstractTest
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
				yield return (GDMBasicControlBag.Instance.EffectiveDateDateEdit, ControlWidthClass.Medium);
				yield return (GDMBasicControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Auto);
				yield return (GDMBasicControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (GDMBasicControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (GDMBasicControlBag.Instance.PreferenceDropEdit, ControlWidthClass.Auto);
				yield return (GDMBasicControlBag.Instance.QuotaOrderNumberDropEdit, ControlWidthClass.Medium);
				yield return (GDMBasicControlBag.Instance.CustomsFirstQuantityCalcEdit, ControlWidthClass.Medium);
				yield return (GDMBasicControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (GDMBasicControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>();
	}
}
