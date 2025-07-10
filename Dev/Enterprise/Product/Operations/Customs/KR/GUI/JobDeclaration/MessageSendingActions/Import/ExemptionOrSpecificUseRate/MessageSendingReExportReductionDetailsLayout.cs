using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MessageSendingReExportReductionDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = ReExportReductionDetailsControlBag.InstanceForMessageSendingObject;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(120);
			var ruler2 = layout.CreateRuler(351);

			layout.Include(ruler1, common.CustomsOfficeCodeFindBox, ruler2, common.DestCountryCodeFindBox);
			layout.Include(ruler1, common.EstimateDateEdit);

			return layout;
		}
	}
}
