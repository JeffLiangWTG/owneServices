using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5ArrivalDeclarationDetailsLayout : IPanelLayoutProvider
	{
		public Phase5ArrivalDeclarationDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new ArrivalDeclarationDetailsLayoutBuilder<Business.NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.StatusDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PhaseDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.SeparatorLabel, ControlWidthClass.Auto);
			builder.Add(commonBag.ReleaseDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.AcceptanceDateEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
