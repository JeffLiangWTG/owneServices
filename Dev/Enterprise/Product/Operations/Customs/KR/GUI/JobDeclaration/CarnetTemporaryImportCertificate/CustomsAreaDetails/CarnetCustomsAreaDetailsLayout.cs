using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CarnetCustomsAreaDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = CarnetCustomsAreaDetailsControlBag.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(435);

			layout.Include(ruler1, common.CustomsOfficeCodeFindBox, ruler2, common.DepartmentCodeFindBox);
			layout.Include(ruler1, common.BondedAreaCodeFindBox);

			return layout;
		}
	}
}
