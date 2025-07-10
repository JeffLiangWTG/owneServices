using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MessageSendingDutyReductionDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = DutyReductionDetailsControlBag.InstanceForMessageSendingObject;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(120);
			var ruler2 = layout.CreateRuler(305);

			layout.Include(ruler1, common.GroupNumberDropEdit);
			layout.Include(ruler1, common.SeqNumberTextBox, ruler2, common.ItemNumberTextBox);

			return layout;
		}
	}
}
