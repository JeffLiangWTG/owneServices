using System;
using System.Collections.Generic;
using System.Linq;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	abstract class RequestHandler
	{
		public abstract Uri Uri { get; }

		public string Handle(IHttpRequestInfo request)
		{
			return HandleCore(request);
		}

		protected abstract string HandleCore(IHttpRequestInfo request);

		internal class UriComparer : IComparer<Uri>
		{
			public int Compare(Uri x, Uri y)
			{
				return Uri.Compare(x, y, UriComponents.Path, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
			}
		}

		internal static class RequestInfoHelper
		{
			public static string[] QueryCsvString(IHttpRequestInfo request, string name)
			{
				return request?.QueryString[name]?
							.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
							.Where(x => !string.IsNullOrEmpty(x)).ToArray()
						?? Array.Empty<string>();
			}

			public static bool? QueryBool(IHttpRequestInfo request, string name)
			{
				return bool.TryParse(request?.QueryString[name], out var val) ? val : null;
			}

			public static uint QueryUInt(IHttpRequestInfo request, string name, uint defaultValue)
			{
				return uint.TryParse(request?.QueryString[name], out var val) ? val : defaultValue;
			}

			public static TimeSpan? QueryTimeSpanFromSeconds(IHttpRequestInfo request, string name)
			{
				return uint.TryParse(request?.QueryString[name], out var val) ? TimeSpan.FromSeconds(val) : null;
			}

			public static string QueryString(IHttpRequestInfo request, string name)
			{
				return string.IsNullOrEmpty(request?.QueryString[name])
					? string.Empty
					: request.QueryString[name];
			}
		}
	}
}
