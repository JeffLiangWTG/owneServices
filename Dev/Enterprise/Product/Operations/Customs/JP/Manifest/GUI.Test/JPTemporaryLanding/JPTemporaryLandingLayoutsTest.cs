using System.Collections.Generic;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPTemporaryLandingLayouts))]
	sealed class JPTemporaryLandingLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new JPTemporaryLandingLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (JPTemporaryLandingControlBag.Instance.TemporaryLandingReasonDropEdit, ControlWidthClass.Auto);
				yield return (JPTemporaryLandingControlBag.Instance.TemporaryLandingPeriodDaysCalcEdit, ControlWidthClass.Auto);
				yield return (JPTemporaryLandingControlBag.Instance.TemporaryLandingStartDateEdit, ControlWidthClass.Auto);
				yield return (JPTemporaryLandingControlBag.Instance.TemporaryLandingEndDateEdit, ControlWidthClass.Auto);
				yield return (JPTemporaryLandingControlBag.Instance.TemporaryLandingBondedTransportCodeDropEdit, ControlWidthClass.Auto);
				yield return (JPTemporaryLandingControlBag.Instance.GoodsLocationCodeFindBox, ControlWidthClass.Auto);
			}
		}
	}
}
