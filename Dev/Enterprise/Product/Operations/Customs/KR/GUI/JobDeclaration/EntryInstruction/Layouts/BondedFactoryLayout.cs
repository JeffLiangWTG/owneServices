using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class BondedFactoryLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public BondedFactoryLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new BondedFactoryLayoutBuilder();
			var bag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(bag.UseTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(bag.UseDateEdit, ControlWidthClass.Auto);
			return builder.Build();
		}
	}
}
