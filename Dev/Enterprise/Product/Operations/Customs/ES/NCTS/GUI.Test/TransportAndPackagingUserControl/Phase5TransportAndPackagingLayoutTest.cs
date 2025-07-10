using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TransportAndPackagingLayout))]
	sealed class Phase5TransportAndPackagingLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportAndPackagingLayoutBuilder<Business.NctsDepartureMovementHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				var commonBag = TransportAndPackagingControlBag.Instance;
				yield return (commonBag.TransportMethodOfPaymentDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				var commonBag = TransportAndPackagingControlBag.Instance;
				yield return (commonBag.CarrierDocAddressControl, ControlWidthClass.LongNoCaption);
			}
		}

		public void TestAddControlBehaviour()
		{
			AssertEquals("ZDocAddressControl", true, LayoutForTesting.HasBehaviourByBehaviourType(TransportAndPackagingControlBag.Instance.CarrierDocAddressControl, typeof(DocAddressControlDisplayModeCompactWithOverrideBehaviour)));
		}
	}
}
