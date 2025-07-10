using System;
using CargoWiseNext.Infrastructure.Installations;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class WinzorUrlHandler
	{
		public static string GetWinzorHttpUrl(Uri serverBaseUri, string url)
		{
			string query = null;
			if (!string.IsNullOrEmpty(url))
			{
				query = UrlHandler.GetQueryStringTextFromUrl(url);
			}
			var uri = new UriBuilder(serverBaseUri) { Query = query }.Uri;
			var urlHandlerProvider = new UrlHandlerFromInstallerFilesHandler();
			var protocol = urlHandlerProvider.GetUrlHandler();
			return $"{protocol}:{uri}";
		}
	}
}
