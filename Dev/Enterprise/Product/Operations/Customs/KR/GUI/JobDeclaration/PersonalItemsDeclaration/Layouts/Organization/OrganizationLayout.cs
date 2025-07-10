using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class OrganizationLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = OrganizationControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(100);

			layout.Include(ruler1, common.ForeignCarrierGuidFindBox);
			layout.Include(ruler1, common.DomesticCarrierGuidFindBox);

			return layout;
		}
	}
}
