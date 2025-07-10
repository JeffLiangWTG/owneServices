using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsControlBag))]
	sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentDetailsControlBag.Instance.WeightCalcDropEdit);
				yield return nameof(ShipmentDetailsControlBag.Instance.VolumeCalcDropEdit);
				yield return nameof(ShipmentDetailsControlBag.Instance.OriginCodeFindBox);
				yield return nameof(ShipmentDetailsControlBag.Instance.EstimatedDepartureDateEdit);
				yield return nameof(ShipmentDetailsControlBag.Instance.FinalDestinationCodeFindBox);
				yield return nameof(ShipmentDetailsControlBag.Instance.EstimatedArrivalDateEdit);
				yield return nameof(ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
	}
}
