using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class MakeProcesureCodesLessCrappy : TestCaseWithFactory
	{
		const string URL = "https://www.gov.uk/guidance/requested-and-previous-procedure-codes-for-de-110-of-cds";
		const string OutputFolder = @"C" + ":" + @"\Dev\Enterprise\Product\Operations\Customs\GB\Core\CDS.Test\DevTools\ProcedureHTML\Results";
		const string FilenameIndex = "_index.html";
		const string DelimiterForEachProcedure = @"<h3 id=""code\d\d\d\d"">(\d\d \d\d)</h3>";
		const string StartOfFooter = @"<div class=""app-c-published-dates "">";
		const string HeaderTwoForListOfProceduresInNextBlock = "<h2 id=\"codes";

		[DeveloperOnlyTest]
		public void TestMakeFriendlyPages()
		{
			var html = DownloadHtml();
			var pairs = SplitIntoSectionsAndTrimHeaderAndFooter(html);
			foreach (var pair in pairs)
			{
				WriteSectionTodisk(pair);
			}
			WriteIndexPage(pairs);
			Assert("This unit test is not supposed to assert anything, nor pass, it's just supposed to be a way of executing the tool to retrieve and parse the procedure.  Dev only failure.", condition: false);
		}

		void WriteIndexPage(List<(string procedure, string text)> pairs)
		{
			var indexPageFullFilename = Path.Combine(OutputFolder, FilenameIndex);
			var sb = new StringBuilder();
			sb.Append("<html> <ul> ");
			foreach (var (procedure, _) in pairs)
			{
				sb.AppendFormat("<li><a href='{0}.htm'>{0}</li>", procedure);
			}
			sb.Append(" </ul> </html>");
			File.WriteAllText(indexPageFullFilename, sb.ToString());
		}

		void WriteSectionTodisk((string procedure, string text) pair)
		{
			File.WriteAllText(Path.Combine(OutputFolder, pair.procedure + ".htm"), "<h2>" + pair.procedure + "</h2>" + pair.text);
		}

		List<(string procedure, string text)> SplitIntoSectionsAndTrimHeaderAndFooter(string html)
		{
			var sections = Regex.Split(html, DelimiterForEachProcedure); // delimited has a grouping match around the procedure code, therefore it is included in the list of matches, alternating with the blocks of text

			// Remove footer from last element
			var last = sections.Last();
			var lastAndFooter = Regex.Split(last, StartOfFooter);
			var copy = sections.ToList();
			copy.RemoveAt(sections.Length - 1);
			copy.Add(lastAndFooter[0]);

			// Get rid of header
			copy.RemoveAt(0);

			var pairs = new List<(string procedure, string text)>();
			for (int i = 1; i < copy.Count; i++)
			{
				if (i % 2 == 0)
				{
					var procedureCodeFromPreviousElement = copy[i - 2];
					var fullTextFromCurrentElement = copy[i - 1];
					if (fullTextFromCurrentElement.Contains(HeaderTwoForListOfProceduresInNextBlock))
					{
						// The last procedure in a series will be followed by an introduction to the next series; strip that
						fullTextFromCurrentElement = Regex.Split(fullTextFromCurrentElement, HeaderTwoForListOfProceduresInNextBlock)[0];
					}
					pairs.Add((procedureCodeFromPreviousElement, fullTextFromCurrentElement));
				}
			}
			return pairs;
		}

#if NET48
		string DownloadHtml() => DownloadHtmlNet48();
#else
		string DownloadHtml() => DownloadHtmlAsync(URL).GetAwaiter().GetResult();
#endif

#if NET48
		string DownloadHtmlNet48()
		{
			var request = (HttpWebRequest)WebRequest.Create(URL);
			request.Timeout = 10000;
			request.Proxy = WebRequest.DefaultWebProxy;
			request.Proxy.Credentials = CredentialCache.DefaultCredentials;

			try
			{
				using var response = (HttpWebResponse)request.GetResponse();
				using var reader = new StreamReader(response.GetResponseStream());
				return reader.ReadToEnd();
			}
			catch (WebException)
			{
				return string.Empty;
			}
		}
#else
		static readonly System.Net.Http.HttpClient _httpClient = CreateHttpClient();

		static System.Net.Http.HttpClient CreateHttpClient()
		{
			var handler = new System.Net.Http.HttpClientHandler
			{
				Proxy = WebRequest.DefaultWebProxy,
				UseProxy = true,
				UseDefaultCredentials = true
			};

			return new System.Net.Http.HttpClient(handler)
			{
				Timeout = System.TimeSpan.FromSeconds(10)
			};
		}

		public async System.Threading.Tasks.Task<string> DownloadHtmlAsync(string url)
		{
			try
			{
				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode(); // Throws if not 2xx
				return await response.Content.ReadAsStringAsync();
			}
			catch (System.Net.Http.HttpRequestException)
			{
				return string.Empty;
			}
		}
#endif
	}
}
