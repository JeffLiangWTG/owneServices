using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class PostClearanceAgencyLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var krBag = OtherDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(krBag);

			var captionRuler = layout.CreateRuler(170);

			layout.Include(0, captionRuler, krBag.Agency1CodeFindBox);
			layout.Include(0, captionRuler, krBag.Agency2CodeFindBox);
			layout.Include(0, captionRuler, krBag.Agency3CodeFindBox);

			return layout;
		}
	}
}
