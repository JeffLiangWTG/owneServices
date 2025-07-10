using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FTAEntryLineDetailsControlBag : ControlBag
	{
		[ThreadStatic]
		static FTAEntryLineDetailsControlBag instance;

		public static FTAEntryLineDetailsControlBag Instance => instance ?? (instance = new FTAEntryLineDetailsControlBag());
		protected override Control CreateTemplate() => new FTAEntryLineDetailsUserControl();

		public FTAEntryLineDetailsControlBag()
		{
			EntryLineNumberCalcEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.EntryLineNumberCalcEdit));
			FTASeqNoCalcEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.FTASeqNoCalcEdit));
			COSplitOrderCalcEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COSplitOrderCalcEdit));
			COTotalNetWeightCalcDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COTotalNetWeightCalcDropEdit));
			CONetWeightCalcDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.CONetWeightCalcDropEdit));
			CONoTextBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.CONoTextBox));
			COIssueDateTextBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COIssueDateTextBox));
			CountryOfOriginCodeFindBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.CountryOfOriginCodeFindBox));
			AssociatedCOCountryCodeFindBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.AssociatedCOCountryCodeFindBox));
			TarrifRateCalcEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.TarrifRateCalcEdit));

			PreferenceCodeDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.PreferenceCodeDropEdit));
			TariffTextBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.TariffTextBox));
			FTACOProductTypeDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.FTACOProductTypeDropEdit));
			ThirdCountryInvIssuedDropEdit  = RegisterControl(nameof(FTAEntryLineDetailsUserControl.ThirdCountryInvIssuedDropEdit));
			ThirdCountryCodeCodeFindBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.ThirdCountryCodeCodeFindBox));
			COExporterNumberTextBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COExporterNumberTextBox));
			COIssuingAgencyTypeDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COIssuingAgencyTypeDropEdit));
			COIssueAgencyNameTextBox = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COIssueAgencyNameTextBox));
			SupportingDocTypeDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.SupportingDocTypeDropEdit));
			COIssuerTypeDropEdit = RegisterControl(nameof(FTAEntryLineDetailsUserControl.COIssuerTypeDropEdit));
		}

		public ControlReference EntryLineNumberCalcEdit { get; }
		public ControlReference FTASeqNoCalcEdit { get; }
		public ControlReference COSplitOrderCalcEdit { get; }
		public ControlReference COTotalNetWeightCalcDropEdit { get; }
		public ControlReference CONetWeightCalcDropEdit { get; }
		public ControlReference CONoTextBox { get; }
		public ControlReference COIssueDateTextBox { get; }
		public ControlReference CountryOfOriginCodeFindBox { get; }
		public ControlReference AssociatedCOCountryCodeFindBox { get; }
		public ControlReference TarrifRateCalcEdit { get; }
		public ControlReference PreferenceCodeDropEdit { get; }
		public ControlReference TariffTextBox { get; }
		public ControlReference FTACOProductTypeDropEdit { get; }
		public ControlReference ThirdCountryInvIssuedDropEdit  { get; }
		public ControlReference ThirdCountryCodeCodeFindBox { get; }
		public ControlReference COExporterNumberTextBox { get; }
		public ControlReference COIssuingAgencyTypeDropEdit { get; }
		public ControlReference COIssueAgencyNameTextBox { get; }
		public ControlReference SupportingDocTypeDropEdit { get; }
		public ControlReference COIssuerTypeDropEdit { get; }
	}
}
