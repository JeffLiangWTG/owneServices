using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FTAEntryLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public FTAEntryLineDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new FTAEntryLineDetailsLayoutBuilder();
			var bag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(bag.EntryLineNumberCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.FTASeqNoCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.COSplitOrderCalcEdit, ControlWidthClass.Auto);
			builder.Add(bag.COTotalNetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.CONetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.CONoTextBox, ControlWidthClass.Auto);
			builder.Add(bag.COIssueDateTextBox, ControlWidthClass.Auto);
			builder.Add(bag.CountryOfOriginCodeFindBox, ControlWidthClass.Auto);
			builder.Add(bag.AssociatedCOCountryCodeFindBox, ControlWidthClass.Auto);
			builder.Add(bag.TarrifRateCalcEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(bag.PreferenceCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.TariffTextBox, ControlWidthClass.Auto);
			builder.Add(bag.FTACOProductTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.ThirdCountryInvIssuedDropEdit , ControlWidthClass.Auto);
			builder.Add(bag.ThirdCountryCodeCodeFindBox, ControlWidthClass.Auto);
			builder.Add(bag.COExporterNumberTextBox, ControlWidthClass.Auto);
			builder.Add(bag.COIssuingAgencyTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.COIssueAgencyNameTextBox, ControlWidthClass.Auto);
			builder.Add(bag.SupportingDocTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.COIssuerTypeDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
