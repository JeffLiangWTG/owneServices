using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ReExportReductionDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = ReExportReductionDetailsControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(120);
			var ruler2 = layout.CreateRuler(320);

			layout.Include(ruler1, common.CustomsOfficeCodeFindBox, ruler2, common.DestCountryCodeFindBox);
			layout.Include(ruler1, common.EstimateDateEdit);

			return layout;
		}
	}
}
