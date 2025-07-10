using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTAEntryLineDetailsControlBag))]
	sealed class FTAEntryLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(FTAEntryLineDetailsControlBag.EntryLineNumberCalcEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.FTASeqNoCalcEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.COSplitOrderCalcEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.COTotalNetWeightCalcDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.CONetWeightCalcDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.CONoTextBox);
				yield return nameof(FTAEntryLineDetailsControlBag.COIssueDateTextBox);
				yield return nameof(FTAEntryLineDetailsControlBag.CountryOfOriginCodeFindBox);
				yield return nameof(FTAEntryLineDetailsControlBag.AssociatedCOCountryCodeFindBox);
				yield return nameof(FTAEntryLineDetailsControlBag.TarrifRateCalcEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.PreferenceCodeDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.TariffTextBox);
				yield return nameof(FTAEntryLineDetailsControlBag.FTACOProductTypeDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.ThirdCountryInvIssuedDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.ThirdCountryCodeCodeFindBox);
				yield return nameof(FTAEntryLineDetailsControlBag.COExporterNumberTextBox);
				yield return nameof(FTAEntryLineDetailsControlBag.COIssuingAgencyTypeDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.COIssueAgencyNameTextBox);
				yield return nameof(FTAEntryLineDetailsControlBag.SupportingDocTypeDropEdit);
				yield return nameof(FTAEntryLineDetailsControlBag.COIssuerTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => FTAEntryLineDetailsControlBag.Instance;
	}
}
