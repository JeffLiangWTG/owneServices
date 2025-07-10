using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Semaphores.Common;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public class EnterpriseUrlHandlerService
	{
		public static EnterpriseUrlHandlerService Instance { get; } = new EnterpriseUrlHandlerService();

		internal static QueryString GetEnterpriseUrlQueryString(string url)
		{
			return UrlHandlerServiceUtils.GetVerifiedQueryString(url, s => UrlHandlerServiceUtils.VerifyLicenceKey(s) ?? UrlHandlerServiceUtils.VerifyUserOrLoginIfRequested(s));
		}

		public static void RegisterUrlHandler(UrlHandler handler)
		{
			urlHandlers.Add(handler);
		}

		public static void UnregisterUrlHandler(UrlHandler handler)
		{
			urlHandlers.Remove(handler);
		}

		public static UrlHandler[] UrlHandlers => urlHandlers.ToArray();

		readonly static List<UrlHandler> urlHandlers = new List<UrlHandler>()
		{
			ShowEditFormUrlHandler.Instance,
			ShowNewFormUrlHandler.Instance,
			ShowViewFormUrlHandler.Instance,
			ShowDeleteFormUrlHandler.Instance,
			ShowModuleUrlHandler.Instance,
			ShowStorageDocUrlHandler.Instance
		};

		public bool ExecuteUrl(string url, bool waitForAppToStart)
		{
			var queryString = new QueryString { UrlEncodeNameAndValue = true, EscapeAmpersandAndEquals = true };
			queryString.Deserialize(UrlHandler.GetQueryStringTextFromUrl(url));

			return (from handler in UrlHandlers
					where handler.CanHandle(queryString)
					select handler.Handle(queryString)).FirstOrDefault();
		}

		public bool ExecuteUrlForWindowPersister(string url, bool waitForAppToStart) => ExecuteUrl(url, waitForAppToStart);
	}
}
