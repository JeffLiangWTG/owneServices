using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class InspectionLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var krBag = OtherDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(170);

			layout.Include(0, captionRuler, krBag.InspectionDropEdit);
			layout.Include(0, captionRuler, krBag.DeliveryCompanyDropEdit);

			return layout;
		}
	}
}
