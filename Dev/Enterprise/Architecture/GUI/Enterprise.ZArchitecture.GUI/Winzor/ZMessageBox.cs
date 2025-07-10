using System.Threading.Tasks;
using CargoWise.Windows.UI.JSInterop;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZMessageBox
	{
		protected override string ClassName => (NoResString)"zmessagebox";

		protected override void OnBeforeRender()
		{
			base.OnBeforeRender();
			EnableTranslationFeedbackHighlight = TranslationFeedbackManager.IsTranslationFeedbackAccessible();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				await (GetJSInterop<IControlHighlightJSInterop>()?.InitializeAsync(EnableTranslationFeedbackHighlight) ?? Task.CompletedTask);
			}
		}
	}
}
