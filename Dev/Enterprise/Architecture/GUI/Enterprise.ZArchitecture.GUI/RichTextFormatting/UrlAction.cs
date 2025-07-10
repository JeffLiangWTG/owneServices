using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	internal sealed class UrlAction : HyperlinkAction<LogUrlLink>
	{
		public UrlAction(LogUrlLink hyperlink, string key)
			: base(hyperlink, key) { }

		public override void DoAction()
		{
			WebUrlLauncher.Launch(Hyperlink.Url != null ? Hyperlink.Url.OriginalString : null);
		}
	}
}

