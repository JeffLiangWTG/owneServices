using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(IncidentDetailsControlBag))]
	sealed class IncidentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(IncidentDetailsControlBag.IncidentCodeDropEdit);
				yield return nameof(IncidentDetailsControlBag.InformationTextBox);
				yield return nameof(IncidentDetailsControlBag.EndorsementDateEdit);
				yield return nameof(IncidentDetailsControlBag.EndorsementAuthorityTextBox);
				yield return nameof(IncidentDetailsControlBag.EndorsementCountryCodeDropEdit);
				yield return nameof(IncidentDetailsControlBag.EndorsementPlaceTextBox);
				yield return nameof(IncidentDetailsControlBag.LocationOfGoodsUserControl);
				yield return nameof(IncidentDetailsControlBag.EventCountryCodeDropEdit);
				yield return nameof(IncidentDetailsControlBag.TransportMeansGroupUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => IncidentDetailsControlBag.Instance;
	}
}
