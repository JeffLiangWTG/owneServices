using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IPageViewBuildService
	{
		IPageView CreatePageView(IPage page, IPageViewPresenter presenter, bool isReadOnly);
	}
}