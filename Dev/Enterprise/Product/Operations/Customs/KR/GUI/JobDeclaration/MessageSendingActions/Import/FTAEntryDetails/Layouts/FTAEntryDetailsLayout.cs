using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FTAEntryDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = FTAEntryDetailsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(160);

			layout.Include(0, ruler1, common.EntryNumberTextBox);
			layout.Include(0, ruler1, common.LawCodeDropEdit);
			layout.Include(0, ruler1, common.DepartureDateEdit);
			layout.Include(0, ruler1, common.DeparturePortCodeFindBox);
			layout.Include(0, ruler1, common.DepartureCountryCodeFindBox);
			layout.Include(0, ruler1, common.ManufacturerAddressControl);
			layout.Include(0, ruler1, common.ManufacturerAreaPostcodeTextBox);

			layout.AddColumn();
			layout.Include(1, ruler1, common.CustomsDisbursementBillNoTextBox);
			layout.Include(1, ruler1, common.TransshipmentYNDropEdit);
			layout.Include(1, ruler1, common.TransshipmentDateEdit);
			layout.Include(1, ruler1, common.TransshipmentPortCodeFindBox);
			layout.Include(1, ruler1, common.TransshipmentCountryCodeFindBox);
			layout.Include(1, ruler1, common.ImporterAddressControl);
			layout.Include(1, ruler1, common.ExporterAddressControl);

			return layout;
		}
	}
}
