using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.JSInterop;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Winzor;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZForm
	{
		[JSInvokable]
		public override async Task OnBrowserSizeChangedAsync(int jsBrowserWidth, int jsBrowserHeight)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				ResizeComplete?.Invoke(this, EventArgs.Empty);
			});
			await base.OnBrowserSizeChangedAsync(jsBrowserWidth, jsBrowserHeight);
		}

		protected override async Task DoDropFilesAsync(Control target, BrowserFile[] files, int clientX, int clientY, long maximumFileSize)
		{
			var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(files);
			maximumFileSize = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value * FileCommonContent.FromMbToByte;
			await base.DoDropFilesAsync(target, validFiles, clientX, clientY, maximumFileSize);
			if (!string.IsNullOrEmpty(message))
			{
				await ZApplication.GetOpenForms().Last().InvokeWinzorDispatcherAsync(() => Globals.Message.ShowWarning(message));
			}
		}

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
