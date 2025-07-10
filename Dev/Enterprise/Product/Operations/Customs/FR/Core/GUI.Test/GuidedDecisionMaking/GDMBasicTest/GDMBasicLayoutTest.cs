using System.Collections.Generic;
using Enterprise.Customs.FR.Business.GDM;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.GDM.Testing
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
				yield return (EU.GUI.GDMBasicControlBag.Instance.EffectiveDateDateEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.GDMBasicControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.GDMBasicControlBag.Instance.CountryOfOriginDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.GDMBasicControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.GDMBasicControlBag.Instance.PreferenceDropEdit, ControlWidthClass.Auto);
				yield return (GDMBasicControlBag.Instance.RegionOrTerritoryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.GDMBasicControlBag.Instance.QuotaOrderNumberDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.GDMBasicControlBag.Instance.CustomsFirstQuantityCalcEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.GDMBasicControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.GDMBasicControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.GDMBasicLayoutBuilder<GuidedDecisionMakingBasic>();
	}
}
