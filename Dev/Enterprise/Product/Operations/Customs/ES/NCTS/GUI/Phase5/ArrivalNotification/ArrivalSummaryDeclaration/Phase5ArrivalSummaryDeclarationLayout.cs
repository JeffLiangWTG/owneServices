using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class Phase5ArrivalSummaryDeclarationLayout : IPanelLayoutProvider
	{
		public Phase5ArrivalSummaryDeclarationLayout()
		{
			Layout = CreateLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new Phase5ArrivalSummaryDeclarationLayoutBuilder<NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SummaryTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PreviousSummaryDeclarationTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.G4PreviousDocumentGroupBox, ControlWidthClass.LongNoCaption);

			builder.SetVisibility(commonBag.PreviousSummaryDeclarationTextBox, h => h.ESNctsHeader.CEN_SummaryType == SummaryTypeList.Codes.PreviousSummaryDeclarationDeclared, h => h.ESNctsHeader.CEN_SummaryTypeInfo);
			builder.SetVisibility(commonBag.G4PreviousDocumentGroupBox, h => h.ESNctsHeader.CEN_SummaryType == SummaryTypeList.Codes.PreviousG4Declared, h => h.ESNctsHeader.CEN_SummaryTypeInfo);

			return builder.Build();
		}
	}
}
