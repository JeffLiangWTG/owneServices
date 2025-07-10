using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(HouseConsignmentDetailsControlBag))]
	sealed class HouseConsignmentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(HouseConsignmentDetailsControlBag.CountryOfDispatchDropEdit);
				yield return nameof(HouseConsignmentDetailsControlBag.CountryOfDestinationDropEdit);
				yield return nameof(HouseConsignmentDetailsControlBag.GrossWeightCalcDropEdit);
				yield return nameof(HouseConsignmentDetailsControlBag.TransportMoPDropEdit);
				yield return nameof(HouseConsignmentDetailsControlBag.ReferenceNumberUCRTextBox);
				yield return nameof(HouseConsignmentDetailsControlBag.ConsignorDocAddressControl);
				yield return nameof(HouseConsignmentDetailsControlBag.ConsigneeDocAddressControl);
				yield return nameof(HouseConsignmentDetailsControlBag.LinePriceCurrencyDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => HouseConsignmentDetailsControlBag.Instance;
	}
}
