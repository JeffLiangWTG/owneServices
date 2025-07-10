using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FTADetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public FTADetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new FTADetailsLayoutBuilder();
			var bag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(bag.LawCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.CustomsDisbursementBillDropEdit, ControlWidthClass.Auto);
			return builder.Build();
		}
	}
}
