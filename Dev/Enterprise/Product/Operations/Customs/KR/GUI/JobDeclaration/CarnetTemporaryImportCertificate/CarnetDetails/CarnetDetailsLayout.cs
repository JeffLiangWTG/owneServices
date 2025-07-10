using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CarnetDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = CarnetDetailsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(435);

			layout.Include(ruler1, common.CarnetCertificateNoTextBox, ruler2, common.EffectiveToDateDateEdit);
			layout.Include(ruler1, common.CarnetUseDropEdit);
			layout.Include(ruler1, common.RepresentativeProductNameLongTextControl);
			layout.Include(ruler1, common.CargoManagementNoTextBox);
			layout.Include(ruler1, common.HouseBillTextBox, ruler2, common.HouseBillSplitDropEdit);
			layout.Include(ruler1, common.TotalWeightCalcDropEdit, ruler2, common.TotalQtyCalcEdit);
			layout.Include(ruler1, common.NoPackagesCalcDropEdit, ruler2, common.TotalAmountConvertToLocalCurrencyControl);

			return layout;
		}
	}
}
