namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class LayoutsForTesting : IPanelLayoutProvider
	{
		public LayoutsForTesting(PanelLayout layout)
		{
			Layout = layout;
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
