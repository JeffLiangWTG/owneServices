using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI.WebLauncher;
using Enterprise.ZArchitecture.Modules;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	public static class WebUrlLauncher
	{
		[ThreadSafe]
		static readonly IWebUrlLaunchValidator webUrlLaunchValidator = new WebUrlLaunchValidator();

		[ThreadSafe]
		static readonly IWebUrlEdientParser webUrlEdientParser = new WebUrlEdientParser();

		public static void Launch(string url)
		{
			bool launched = false;
			try
			{
				webUrlLaunchValidator.ValidateUrl(url);
			}
			catch (WebUrlValidationException)
			{
				return;
			}

			if (webUrlEdientParser.TryParse(url, out var result))
			{
				url = result;
			}

			// Shortcut and attempt to launch the URL against the current Enterprise application.
			if (url.StartsWith(UrlHandler.EdiUrlPrefix))
			{
				try
				{
					launched = EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				}
				catch
				{
					// Don't care why it failed, it did so we will use fallback/original method.
					launched = false;
				}
			}

			if (!launched)
			{
				var contextForm = WinzorDispatcher.Current?.CurrentContext?.Form;
				contextForm?.InvokeRenderDispatcher(async () =>
				{
					await (contextForm.GetJSInterop<IWindowJSInterop>().OpenUrlAsync(url) ?? Task.CompletedTask);
					launched = true;
				});
			}

#if DEBUG
			LastUrlLaunched = url;
#endif
		}

		public static void Launch(string url, string docType, string businessObjectPK, string parentType)
		{
		}

		public static bool IsRemote => false;

#if DEBUG
		public static string LastUrlLaunched { get; private set; }

		public static void ClearLastUrlLaunched()
		{
			LastUrlLaunched = string.Empty;
		}
#endif
	}
}
