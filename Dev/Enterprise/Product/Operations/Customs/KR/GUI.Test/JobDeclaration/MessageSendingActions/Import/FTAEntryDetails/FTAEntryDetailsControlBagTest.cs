using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTAEntryDetailsControlBag))]
	sealed class FTAEntryDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(FTAEntryDetailsControlBag.Instance.EntryNumberTextBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.LawCodeDropEdit);
				yield return nameof(FTAEntryDetailsControlBag.Instance.DepartureDateEdit);
				yield return nameof(FTAEntryDetailsControlBag.Instance.DeparturePortCodeFindBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.DepartureCountryCodeFindBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.ManufacturerAddressControl);
				yield return nameof(FTAEntryDetailsControlBag.Instance.ManufacturerAreaPostcodeTextBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.CustomsDisbursementBillNoTextBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.TransshipmentYNDropEdit);
				yield return nameof(FTAEntryDetailsControlBag.Instance.TransshipmentDateEdit);
				yield return nameof(FTAEntryDetailsControlBag.Instance.TransshipmentPortCodeFindBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.TransshipmentCountryCodeFindBox);
				yield return nameof(FTAEntryDetailsControlBag.Instance.ImporterAddressControl);
				yield return nameof(FTAEntryDetailsControlBag.Instance.ExporterAddressControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => FTAEntryDetailsControlBag.Instance;
	}
}
