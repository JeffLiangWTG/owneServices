using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MessageSendingDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var common = DetailsControlBag.InstanceForMessageSendingObject;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(120);
			var ruler2 = layout.CreateRuler(530);

			layout.Include(ruler1, common.DutyReductionTypeDropEdit, ruler2, common.SpecificUseCheckBox);
			layout.Include(ruler1, common.DutyReductionCodeFindBox, ruler2, common.InstalmentCodeFindBox);
			layout.Include(ruler1, common.RemarkTextBox);

			return layout;
		}
	}
}
