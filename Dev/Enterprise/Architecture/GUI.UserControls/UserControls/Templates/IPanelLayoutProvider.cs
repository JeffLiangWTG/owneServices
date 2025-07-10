using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IPanelLayoutProvider
	{
		PanelLayout Layout { get; }
	}

	public interface IPanelLayoutProviderWithExtensions : IPanelLayoutProvider
	{
		IReadOnlyCollection<ILayoutExtension> Extensions { get; }
	}
}
