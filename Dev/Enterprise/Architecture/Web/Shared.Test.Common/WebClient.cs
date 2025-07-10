using System;
using System.Linq;
#if NETFRAMEWORK
using System.Net;
#endif
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	public static class WebClient
	{
		public static ResponseWithContent HttpGet(string url, TimeSpan timeOutTimeSpan, CancellationToken cancellationToken)
		{
			ResponseWithContent result = null;

			Task.Run(async () =>
			{
				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(url);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					client.Timeout = timeOutTimeSpan;

					var response = await client.GetAsync(url, cancellationToken);
					result = new ResponseWithContent(response);
				}
			}, cancellationToken).Wait(cancellationToken);

			return result;
		}

#if NETFRAMEWORK
		public static ResponseWithContent WebGet(string webAddress, string webPage, int timeoutInSeconds = 120)
		{
			return WebGet($"http://{webAddress}/{webPage}", timeoutInSeconds);
		}

		public static ResponseWithContent WebGet(string url, int timeoutInSeconds = 120)
		{
			try
			{
				var request = WebRequest.CreateHttp(url);
				request.Method = "GET";
				request.ContentType = "text/html";
				request.Accept = "application/json";
				request.Timeout = timeoutInSeconds == Timeout.Infinite ? Timeout.Infinite : timeoutInSeconds * 1000;

				using (var response = request.GetResponse())
				{
					return new ResponseWithContent(url, response);
				}
			}
			catch (WebException ex)
			{
				return new ResponseWithContent(url, ex);
			}
			catch (Exception ex)
			{
				return new ResponseWithContent(url, ex);
			}
		}
#endif

		public static string ParseHtmlContent(this string html)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				return string.Empty;
			}

			try
			{
				var sb = new StringBuilder();
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				doc.DocumentNode.Descendants().Where(node => node.Name == "script" || node.Name == "style" ).ToList().ForEach(node => node.Remove());

				foreach (var node in doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']"))
				{
					var line = node.InnerText.Trim().Trim('"');
					if (!string.IsNullOrEmpty(line))
					{
						sb.AppendLine(line);
					}
				}

				sb.AppendLine();

				return sb.ToString();
			}
			catch
			{
				return html;
			}
		}
	}
}
