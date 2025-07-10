using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportFTAEntryLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsTabPage()
		{
			using (var userControl = new ImportFTAEntryLineDetailsUserControl())
			{
				var detailsPanel = userControl.FindSingle<DynamicLayoutPanel>("FTAEntryLineDetailsPanel");
				AssertNotNull(detailsPanel.FindSingle<ZCalcEdit>(nameof(FTAEntryLineDetailsControlBag.EntryLineNumberCalcEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZCalcEdit>(nameof(FTAEntryLineDetailsControlBag.FTASeqNoCalcEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZCalcEdit>(nameof(FTAEntryLineDetailsControlBag.COSplitOrderCalcEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZCalcDropEdit>(nameof(FTAEntryLineDetailsControlBag.COTotalNetWeightCalcDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZCalcDropEdit>(nameof(FTAEntryLineDetailsControlBag.CONetWeightCalcDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZTextBox>(nameof(FTAEntryLineDetailsControlBag.CONoTextBox)));
				AssertNotNull(detailsPanel.FindSingle<ZDateEdit>(nameof(FTAEntryLineDetailsControlBag.COIssueDateTextBox)));
				AssertNotNull(detailsPanel.FindSingle<ZCodeFindBox>(nameof(FTAEntryLineDetailsControlBag.CountryOfOriginCodeFindBox)));
				AssertNotNull(detailsPanel.FindSingle<ZCodeFindBox>(nameof(FTAEntryLineDetailsControlBag.AssociatedCOCountryCodeFindBox)));
				AssertNotNull(detailsPanel.FindSingle<ZCalcEdit>(nameof(FTAEntryLineDetailsControlBag.TarrifRateCalcEdit)));

				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(FTAEntryLineDetailsControlBag.PreferenceCodeDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZTextBox>(nameof(FTAEntryLineDetailsControlBag.TariffTextBox)));
				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(FTAEntryLineDetailsControlBag.FTACOProductTypeDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(FTAEntryLineDetailsControlBag.ThirdCountryInvIssuedDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZCodeFindBox>(nameof(FTAEntryLineDetailsControlBag.ThirdCountryCodeCodeFindBox)));
				AssertNotNull(detailsPanel.FindSingle<ZTextBox>(nameof(FTAEntryLineDetailsControlBag.COExporterNumberTextBox)));
				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(FTAEntryLineDetailsControlBag.COIssuingAgencyTypeDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZTextBox>(nameof(FTAEntryLineDetailsControlBag.COIssueAgencyNameTextBox)));
				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(FTAEntryLineDetailsControlBag.SupportingDocTypeDropEdit)));
				AssertNotNull(detailsPanel.FindSingle<ZDropEdit>(nameof(FTAEntryLineDetailsControlBag.COIssuerTypeDropEdit)));
			}
		}
	}
}
