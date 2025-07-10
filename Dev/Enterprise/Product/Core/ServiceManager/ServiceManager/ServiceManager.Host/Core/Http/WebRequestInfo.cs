using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class WebRequestInfo : IHttpRequestInfo
	{
		public string Body { get; set; }
		public long ContentLength { get; }
		public string ContentType { get; }
		public string HttpMethod { get; }
		public Uri Uri { get; set; }
		public NameValueCollection QueryString { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "HttpMethod {0}", HttpMethod));
			sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Url {0}", Uri));
			sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "ContentType {0}", ContentType));
			sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "ContentLength {0}", ContentLength));
			sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Body {0}", Body));
			return sb.ToString();
		}

		public WebRequestInfo(HttpListenerRequest request) : this()
		{
			HttpMethod = request.HttpMethod;
			Uri = request.Url;
			QueryString = request.QueryString;

			if (request.HasEntityBody)
			{
				var encoding = request.ContentEncoding;
				Stream bodyStream = null; // https://msdn.microsoft.com/library/ms182334.aspx
				try
				{
					bodyStream = request.InputStream;
					using (var streamReader = new StreamReader(bodyStream, encoding))
					{
						bodyStream = null;
						if (request.ContentType != null)
						{
							ContentType = request.ContentType;
						}

						ContentLength = request.ContentLength64;
						Body = streamReader.ReadToEnd();
					}
				}
				finally
				{
					bodyStream?.Dispose();
				}
			}
		}

		internal WebRequestInfo()
		{
		}
	}
}