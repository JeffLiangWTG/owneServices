using System;
using System.IO;
using System.Net;
using System.Net.Http;
using static System.FormattableString;

#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	public class ResponseWithContent
	{
		public ResponseWithContent(HttpResponse httpResponse)
		{
			StatusCode = (HttpStatusCode)httpResponse.StatusCode;
			Status = StatusCode == HttpStatusCode.OK ? Ok : Exception;
#if NETFRAMEWORK
			Content = httpResponse.Output.ToString();
#else
			Content = httpResponse.Body.ToString();
#endif
		}

		public ResponseWithContent(HttpResponseMessage response)
		{
			Status = response.IsSuccessStatusCode ? Ok : Exception;
			StatusCode = response.StatusCode;
			Content = response.Content?.ReadAsStringAsync().GetAwaiter().GetResult().ParseHtmlContent();
		}

		public ResponseWithContent(string url, WebResponse response)
		{
			Status = Ok;
			Url = url;

			if (response is HttpWebResponse webResponse)
			{
				StatusCode = webResponse.StatusCode;
			}

			using (var reader = new StreamReader(response.GetResponseStream() ?? throw new NotSupportedException("response.GetResponseStream() returned null.")))
			{
				Content = reader.ReadToEnd().ParseHtmlContent();
			}
		}

		public ResponseWithContent(string url, WebException exception)
		{
			Status = Exception;
			Url = url;
			StatusCode = HttpStatusCode.Unused;

			var response = exception.Response;
			if (response == null)
			{
				Content = $"(no response details): {exception}";
			}
			else
			{
				try
				{
					using (var stream = response.GetResponseStream())
					{
						if (stream == null)
						{
							Content = $"{exception}";
						}
						else
						{
							using (var reader = new StreamReader(stream))
							{
								Content = reader.ReadToEnd().ParseHtmlContent();
							}
						}
					}
				}
				catch (Exception ex)
				{
					Content = $"{exception}; {ex}";
				}
			}
		}

		public ResponseWithContent(string url, Exception exception)
		{
			Status = Exception;
			Url = url;
			StatusCode = HttpStatusCode.Unused;
			Content = $"{SitePage} had returned exception with no response details: {exception}";
		}

		public string SitePage => Invariant($"{Path.GetFileName(Url)}");
		public string Url { get; }
		public string Content { get; }
		public string Status { get; }
		public HttpStatusCode StatusCode { get; }

		public bool IsOk => Status == Ok;

		public override string ToString() => Invariant($"[{SitePage}:{Status}], {Content}");

		public const string Ok = "OK";
		public const string Exception = "Exception";
	}
}
