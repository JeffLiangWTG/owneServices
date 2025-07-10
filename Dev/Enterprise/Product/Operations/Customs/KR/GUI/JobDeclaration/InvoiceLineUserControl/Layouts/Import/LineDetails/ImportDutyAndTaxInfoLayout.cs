using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ImportDutyAndTaxInfoLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = ImportInvoiceLineDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(185);
			layout.Include(ruler1, common.DutyRateTypeDropEdit);
			layout.Include(ruler1, common.MinMaxDutyDropEdit);
			layout.Include(ruler1, common.PreferenceCodeDropEdit, layout.CreateRuler(280), common.DutyRateCalcEdit);
			layout.Include(ruler1, common.DutyCodeDropEdit, layout.CreateRuler(235), common.AddDutyRateCalcEdit);
			layout.Include(ruler1, common.DutyReductionCodeFindBox, layout.CreateRuler(325), common.DutyReductionRateCalcEdit);
			layout.Include(ruler1, common.InstalmentCodeFindBox);
			layout.Include(ruler1, common.SpecificUsePermitNoTextBox, layout.CreateRuler(365), common.SpecificUseCheckBox);
			layout.Include(ruler1, common.DomesticTaxCodeFindBox, layout.CreateRuler(300), common.DomesticTaxTypeDropEdit);
			layout.Include(ruler1, common.DomesticTaxRateCalcEdit);
			layout.Include(ruler1, common.DomesticTaxExemptionCodeFindBox);
			layout.Include(ruler1, common.DomesticTaxBaseQtyOrPriceCalcEdit);
			layout.Include(ruler1, common.VATTypeDropEdit);
			layout.Include(ruler1, common.VATReductionCodeFindBox);
			layout.Include(ruler1, common.EducationTaxTypeDropEdit);
			layout.Include(ruler1, common.AgricultureTaxTypeDropEdit);

			return layout;
		}
	}
}
