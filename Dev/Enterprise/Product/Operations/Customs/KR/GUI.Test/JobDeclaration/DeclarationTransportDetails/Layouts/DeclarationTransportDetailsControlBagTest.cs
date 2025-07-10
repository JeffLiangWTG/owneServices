using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DeclarationTransportDetailsControlBag))]
	sealed class DeclarationTransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.TransshipmentPortCodeFindBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.TransshipmentDateEdit);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.VesselCountryCodeFindBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.CarrierKRCCodeFindBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.VoyageFlightNumberTextBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.FolioNumberTextBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.PortOfLoadingCodeFindBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.ExportDateEdit);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.IATALoadPortDropEdit);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.VoyageDurationCalcEdit);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.RadioCallSignTextBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.MRNNumberTextBox);
				yield return nameof(DeclarationTransportDetailsControlBag.Instance.MRNTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationTransportDetailsControlBag.Instance;
	}
}
