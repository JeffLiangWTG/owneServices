using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class COOIssueLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = COOandFTAControlBag.InstanceForInvoicerHeader;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(130);

			layout.Include(0, ruler1, common.COReferenceNumberTextBox);
			layout.Include(0, ruler1, common.COIssuingCountryCodeFindBox);
			layout.Include(0, ruler1, common.COIssueDateEdit);
			layout.Include(0, ruler1, common.COSplitYNDropEdit);

			layout.AddColumn();
			layout.Include(1, ruler1, common.IssuingAgencyNameTextBox);
			layout.Include(1, ruler1, common.IssuingAreaNameTextBox);
			layout.Include(1, ruler1, common.IssuingPersonNameTextBox);
			layout.Include(1, ruler1, common.COCodeDropEdit);

			return layout;
		}
	}
}
