using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
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
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportAndPackagingLayoutBuilder<Business.NctsDepartureMovementHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TransportAndPackagingControlBag.Instance.TransportMethodOfPaymentDropEdit, ControlWidthClass.Long);
				yield return (TransportAndPackagingControlBag.Instance.CarrierDocAddressControl, ControlWidthClass.Long);
			}
		}
	}
}
