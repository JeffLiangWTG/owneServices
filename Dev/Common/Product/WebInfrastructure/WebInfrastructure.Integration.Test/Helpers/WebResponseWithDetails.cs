using System;
using System.IO;
using System.Net;
using static System.FormattableString;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	class WebResponseWithDetails
	{
		public WebResponseWithDetails(string webAddress, string webPage, WebResponse response)
		{
			Status = Ok;
			WebAddress = webAddress;
			WebPage = webPage;
			using var reader = new StreamReader(response.GetResponseStream() ?? throw new NotSupportedException("response.GetResponseStream() returned null."));
			Details = reader.ReadToEnd().ParseHtmlContent();
		}

		public WebResponseWithDetails(string webAddress, string webPage, string content)
		{
			Status = Ok;
			WebAddress = webAddress;
			WebPage = webPage;
			Details = content.ParseHtmlContent();
		}

		public WebResponseWithDetails(string webAddress, string webPage, WebException exception)
		{
			Status = Exception;
			WebAddress = webAddress;
			WebPage = webPage;

			var response = exception.Response;
			if (response == null)
			{
				Details = $"(no response details): {exception}";
			}
			else
			{
				try
				{
					using var stream = response.GetResponseStream();

					if (stream == null)
					{
						Details = $"{exception}";
					}
					else
					{
						using var reader = new StreamReader(stream);
						Details = reader.ReadToEnd().ParseHtmlContent();
					}
				}
				catch (Exception ex)
				{
					Details = $"{exception}; {ex}";
				}
			}
		}

		public WebResponseWithDetails(string webAddress, string webPage, Exception exception)
		{
			Status = Exception;
			WebAddress = webAddress;
			WebPage = webPage;
			Details = $"{SitePage} had returned exception with no response details: {exception}";
		}

		public string SitePage => Invariant($"{Path.GetFileName(WebAddress)}/{WebPage}");
		public string WebAddress { get; }
		public string WebPage { get; }
		public string Details { get; }
		public string Status { get; }

		public bool IsOk => Status == Ok;

		public override string ToString() => Invariant($"[{SitePage}:{Status}], {Details}");

		public const string Ok = "OK";
		public const string Exception = "Exception";
	}
}
