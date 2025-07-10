using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(TransportDetailsControlBag))]
	sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDetailsControlBag.Instance.HouseBillTextBox);
				yield return nameof(TransportDetailsControlBag.Instance.FreightCalcEdit);
				yield return nameof(TransportDetailsControlBag.Instance.StartDateDateEdit);
				yield return nameof(TransportDetailsControlBag.Instance.ArrivalDateDateEdit);
				yield return nameof(TransportDetailsControlBag.Instance.PortofLoadingCodeFindBox);
				yield return nameof(TransportDetailsControlBag.Instance.ForeignCityCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
	}
}
