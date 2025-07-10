using System.Drawing;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IThemePreviewProvider
	{
		Image GetPreview(IDocBuilderTheme theme);
	}
}